using System;
using System.Collections.Concurrent;
using System.Windows.Threading;

namespace Tidal.Helpers
{
    /// <summary>
    /// Implements a stack of messages, each with a time-to-live attached to
    /// them.
    /// </summary>
    /// <remarks>
    /// When a message is added, it will be pushed atop the current
    /// top-of-stack, meaning that it will be displayed instead of the old
    /// message. When it's time for that message to go away, the previous
    /// topmost message will be displayed, resuming its countdown till it too is
    /// removed.
    /// </remarks>
    class InfoDisplayStack : IDisposable
    {
        class DisplayMessage
        {
            public string message;
            public DateTime timeToRemove;
        }

        private readonly DispatcherTimer timer;
        private readonly ConcurrentStack<DisplayMessage> messageStack;

        /// <summary>
        ///   Create a new instance of <see cref="InfoDisplayStack"/>.
        /// </summary>
        /// <param name="displayAction">
        ///   The action to perform on the string in an added message.
        /// </param>
        /// <param name="clearAction">
        ///   The action to perform when a message is complete. Defaults to
        ///   invoking <see cref="DisplayAction"/> with <see
        ///   cref="String.Empty"/>.
        /// </param>
        public InfoDisplayStack(Action<string> displayAction, Action clearAction = null)
        {
            messageStack = new ConcurrentStack<DisplayMessage>();
            DisplayAction = displayAction;
            ClearAction = clearAction is null ? (() => DisplayAction(string.Empty)) : clearAction;

            timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(1),
            };
            timer.Tick += Timer_Tick;
            timer.Start();
        }

        public void ClearAll()
        {
            messageStack.Clear();
            DisplayAction.Invoke(string.Empty);
        }

        /// <summary>
        /// Add a new message to be displayed (or whatever your Action was at
        /// instance creation). The action will be invoked after adding the
        /// message.
        /// </summary>
        /// <remarks>
        /// When a new message is added to the stack, all of the other items
        /// currently on the stack will have their times adjusted so that
        /// they'll still be displayed the amount they requested.
        /// </remarks>
        /// <param name="infoMsg">
        /// A message to pass to the action.
        /// </param>
        /// <param name="displayTime">
        /// When this time elapses, the message is popped from the stack and the
        /// <see cref="DisplayAction"/> is invoked with an empty string.
        /// </param>
        public void AddMessage(string infoMsg, TimeSpan displayTime)
        {
            // Anything on the stack automatically gets its time
            // extended by the amount being requested by the new item
            foreach (var msg in messageStack)
            {
                msg.timeToRemove.Add(displayTime);
            }

            // Create a new message, push it onto the stack and display it
            var message = new DisplayMessage
            {
                message = infoMsg,
                timeToRemove = DateTime.Now.Add(displayTime),
            };
            messageStack.Push(message);
            DisplayAction(infoMsg);
        }

        /// <summary>
        /// Action to be performed on the message string.
        /// </summary>
        public Action<string> DisplayAction { get; set; }

        public Action ClearAction { get; set; }

        private void Timer_Tick(object sender, EventArgs e)
        {
            // If there isn't anything to display, exit
            if (messageStack.Count == 0)
                return;

            // Conversely, if the item being displayed isn't done
            // being shown, also exit
            if (messageStack.TryPeek(out var tos) && tos.timeToRemove > DateTime.Now)
                return;

            // Get rid of whatever is showing...
            ClearAction();

            // Pop items off the stack that have timed out (shouldn't be any,
            // but check anyway)
            while (messageStack.TryPeek(out tos) && tos.timeToRemove <= DateTime.Now)
                messageStack.TryPop(out _);

            // Finally, if anything is left on the stack, display the topmost
            // item
            if (messageStack.TryPeek(out tos))
            {
                DisplayAction(tos.message);
            }
        }

        public void Dispose()
        {
            timer.Stop();
            messageStack.Clear();
        }
    }
}
