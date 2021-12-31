using Prism.Events;
using System;
using Tidal.Services.Abstract;

namespace Tidal.Services.Actual
{
    internal class Messenger : IMessenger
    {
        private static IEventAggregator aggregator;

        public Messenger()
        {
            aggregator = aggregator ?? new EventAggregator();
        }

        public void Send<T>(T message)
            where T : class
        {
            aggregator.GetEvent<PubSubEvent<T>>().Publish(message);
        }

        public SubscriptionToken Subscribe<T>(Action<T> action, ThreadOption threadOption = ThreadOption.UIThread)
            where T : class
        {
            var token = aggregator.GetEvent<PubSubEvent<T>>().Subscribe(action, threadOption);
            return token;
        }

        public void Unsubscribe<T>(SubscriptionToken token)
            where T : class
        {
            aggregator.GetEvent<PubSubEvent<T>>().Unsubscribe(token);
        }
    }
}
