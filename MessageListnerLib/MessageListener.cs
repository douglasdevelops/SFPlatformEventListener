using CometD.NetCore.Bayeux.Client;
using CometD.NetCore.Bayeux;

namespace MessageListnerLib
{
    public class MessageListener : IMessageListener
    {
        /// <summary>
        /// Event triggered when a platform event message is received.
        /// </summary>
        public event EventHandler<PlatformEventReceivedEventArgs> PlatformEventReceived;

        /// <summary>
        /// Invokes the PlatformEventReceived event with the received message.
        /// </summary>
        /// <param name="e">The event arguments containing the message.</param>
        protected virtual void OnMessageReceived(PlatformEventReceivedEventArgs e)
        {
            PlatformEventReceived?.Invoke(this, e);
        }

        /// <summary>
        /// Method called when a message is received on the CometD channel.
        /// </summary>
        /// <param name="channel">The CometD channel where the message was received.</param>
        /// <param name="message">The message received on the channel.</param>
        public void OnMessage(IClientSessionChannel channel, IMessage message)
        {
            var args = new PlatformEventReceivedEventArgs(message.Json);
            OnMessageReceived(args);
        }
    }
}