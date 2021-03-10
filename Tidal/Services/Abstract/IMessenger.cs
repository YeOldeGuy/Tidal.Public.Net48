using System;
using Prism.Events;

namespace Tidal.Services.Abstract
{
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
        void Send<T>(T message);

        /// <summary>
        /// Tells the message system to invoke the <paramref name="action"/>
        /// when the message of type <typeparamref name="T"/> is sent.
        /// </summary>
        /// <typeparam name="T">
        ///   The type of the message.
        /// </typeparam>
        /// <param name="owner">The owning object</param>
        /// <param name="action">An <see cref="Action"/> to invoke when the message is sent.</param>
        /// <param name="threadOption">One of the <see cref="ThreadOption"/> values.</param>
        /// <returns>A token to make disposal of the action easier.</returns>
        SubscriptionToken Subscribe<T>(Action<T> action, ThreadOption threadOption = ThreadOption.UIThread);

        /// <summary>
        /// Tell the message system to stop sending messages. Normally, this is
        /// not used. Use <c>MySubscriptionToken.Dispose()</c> instead.
        /// </summary>
        /// <typeparam name="T">The type of the message.</typeparam>
        /// <param name="token">
        ///   A <see cref="SubscriptionToken"/> value, returned from a call to
        ///   <see cref="Subscribe{T}(Action{T}, ThreadOption)"/>
        /// </param>
        void Unsubscribe<T>(SubscriptionToken token);
    }
}
