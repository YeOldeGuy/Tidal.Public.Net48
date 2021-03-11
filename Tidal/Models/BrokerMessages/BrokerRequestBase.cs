using System;
using System.Threading.Tasks;
using Tidal.Client;
using Tidal.Client.Exceptions;
using Tidal.Helpers;
using Tidal.Models.Messages;
using Tidal.Services.Abstract;

namespace Tidal.Models.BrokerMessages
{
    /// <summary>
    /// Basis of all requests of the <see cref="IBrokerService"/> to make
    /// requests asynchronously to the host.
    /// </summary>
    /// <remarks>
    /// Each implementor must override the <see cref="Invoke(IClient)"/> method
    /// to do the actual work of communicating with the <see cref="IClient"/>.
    /// </remarks>
    internal abstract class BrokerRequestBase
    {
        private readonly IMessenger messenger;

        protected BrokerRequestBase()
        {
            messenger = ServiceResolver.Resolve<IMessenger>();
        }

        protected IMessenger Messenger => messenger;

        /// <summary>
        /// Send a request to the <see cref="IClient"/> instance and if
        /// appropriate, create an appropriate response and broadcast that.
        /// </summary>
        /// <param name="client">An instance of the <see cref="IClient"/>.</param>
        public abstract Task Invoke(IClient client);

        /// <summary>
        ///   Does the work of <see cref="Invoke(IClient)"/>, wrapping the call
        ///   in some error checking.
        /// </summary>
        /// <param name="rpc">
        ///   The delegate doing the work of calling the client. The call is
        ///   asynchronous.
        /// </param>
        protected virtual async Task InvokeWrapper(Func<Task> rpc)
        {
            try
            {
                await rpc();
            }
            catch (ClientRPCException ex)
            {
                messenger.Send(new WarningMessage(ex.Message, ex.Header));
            }
            catch (ClientAuthorizationException ex)
            {
                messenger.Send(new FatalMessage(ex.Message));
            }
            catch (ClientTimeoutException ex)
            {
                messenger.Send(new FatalMessage(ex.Message));
            }
            catch (ClientException ex)
            {
                messenger.Send(new FatalMessage(ex.Message));
            }
        }
    }
}
