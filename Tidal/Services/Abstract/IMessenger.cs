using Prism.Events;
using System;

namespace Tidal.Services.Abstract
{
    /// <summary>
    /// An easier-to-use interface to the rather baroque Prism publish/subscribe
    /// event aggregator.
    /// </summary>
    public interface IMessenger
    {
        /// <summary>
        ///   Sends a message to all subscribers. Unless the subscriber used
        ///   <see cref="ThreadOption.BackgroundThread"/>, this call will be
        ///   synchronous. There can be subscriptions of both types, sync and
        ///   async, to the same event.
        /// </summary>
        /// <typeparam name="T">
        ///   The type of the message, any reference type.
        /// </typeparam>
        /// <param name="message">
        ///   A message instance of type <typeparamref name="T"/>.
        /// </param>
        void Send<T>(T message)
            where T : class;

        /// <summary>
        /// Attach a delegate to the specified message type. When a publisher
        /// sends out a matching message, this action will be invoked.
        /// </summary>
        /// <typeparam name="T">
        ///   The type of the message.
        /// </typeparam>
        /// <param name="owner">The owning object</param>
        /// <param name="action">An <see cref="Action"/> to invoke when the message is sent.</param>
        /// <param name="threadOption">One of the <see cref="ThreadOption"/> values.</param>
        /// <returns>A token to make disposal of the action easier.</returns>
        SubscriptionToken Subscribe<T>(Action<T> action, ThreadOption threadOption = ThreadOption.UIThread)
            where T : class;

        /// <summary>
        /// Tell the message system to stop sending messages. Normally, this is
        /// not used. Use <c>MySubscriptionToken.Dispose()</c> instead.
        /// </summary>
        /// <typeparam name="T">The type of the message.</typeparam>
        /// <param name="token">
        ///   A <see cref="SubscriptionToken"/> value, returned from a call to
        ///   <see cref="Subscribe{T}(Action{T}, ThreadOption)"/>
        /// </param>
        void Unsubscribe<T>(SubscriptionToken token)
            where T : class;
    }
}
