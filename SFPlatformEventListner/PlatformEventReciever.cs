using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SFPlatformEventListener
{
    /// <summary>
    /// This class is responsible for receiving platform events and triggering the corresponding event.
    /// </summary>
    public class PlatformEventReciever
    {
        /// <summary>
        /// Event triggered when a platform event is received.
        /// </summary>
        public event EventHandler<PlatformEventReceivedEventArgs> PlatformEventReceived;

        /// <summary>
        /// Method to invoke the PlatformEventReceived event with the given event arguments.
        /// </summary>
        /// <param name="e">The event arguments containing the received message.</param>
        protected virtual void OnPlatformEventReceived(PlatformEventReceivedEventArgs e)
        {
            PlatformEventReceived?.Invoke(this, e);
        }

        /// <summary>
        /// Method to receive a message and trigger the PlatformEventReceived event.
        /// </summary>
        /// <param name="message">The message to be processed.</param>
        public void ReceiveMessage(string message)
        {
            OnPlatformEventReceived(new PlatformEventReceivedEventArgs(message));
        }
    }

    /// <summary>
    /// Event arguments used to convey platform event data.
    /// </summary>
    public class PlatformEventReceivedEventArgs : EventArgs
    {
        /// <summary>
        /// Gets the received message.
        /// </summary>
        public string Message { get; }

        /// <summary>
        /// Initializes a new instance of the PlatformEventReceivedEventArgs class.
        /// </summary>
        /// <param name="message">The message received from the platform event.</param>
        public PlatformEventReceivedEventArgs(string message)
        {
            var jsonObject = JsonConvert.DeserializeObject(message);
            string prettyJson = JsonConvert.SerializeObject(jsonObject, Formatting.Indented);
            Message = message;
        }
    }
}
