using CometD.NetCore.Client.Transport;
using CometD.NetCore.Client;
using MessageListnerLib;
using System.Collections.Concurrent;
using System.Collections.Specialized;
using static SFEventListenerWeb.EventMessage;
using Newtonsoft.Json.Linq;
using System.Net.Http.Headers;

namespace SFEventListenerWeb.Services
{
    public class EventService : IEventService
    {
        private readonly ConcurrentDictionary<string, List<EventMessage>> _eventMessages = new ConcurrentDictionary<string, List<EventMessage>>();
        private readonly ConcurrentDictionary<string, List<EventReceivedHandler>> _eventHandlers = new ConcurrentDictionary<string, List<EventReceivedHandler>>();

        public async Task<List<string>> GetPlatformEventsAsync(string instanceUrl, string accessToken)
        {
            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

                string metadataUrl = $"{instanceUrl}/services/data/v55.0/sobjects/";

                HttpResponseMessage response = await client.GetAsync(metadataUrl);
                response.EnsureSuccessStatusCode();

                string responseContent = await response.Content.ReadAsStringAsync();

                JObject jsonResponse = JObject.Parse(responseContent);

                var platformEvents = jsonResponse["sobjects"]
                    .Where(obj => obj["custom"].Value<bool>() && obj["name"].ToString().EndsWith("__e"))
                    .Select(obj => obj["name"].ToString())
                    .ToList();

                return platformEvents;
            }
        }


        public void Subscribe(string eventName, string accessToken, string instanceUrl, EventReceivedHandler handler)
        {
            if (!_eventMessages.ContainsKey(eventName))
            {
                _eventMessages[eventName] = new List<EventMessage>();
            }

            if (!_eventHandlers.ContainsKey(eventName))
            {
                _eventHandlers[eventName] = new List<EventReceivedHandler>();
            }

            _eventHandlers[eventName].Add(handler);

            var transportOptions = new Dictionary<string, object>();

            transportOptions.Add("headers", new Dictionary<string, string>
            {
                { "Authorization", "Bearer " + accessToken }
            });

            NameValueCollection nvc = new NameValueCollection();
            nvc.Add("Authorization", "Bearer " + accessToken);

            var transport = new LongPollingTransport(transportOptions, nvc);
            var clientBayeux = new BayeuxClient($"{instanceUrl}/cometd/50.0", transport);

            clientBayeux.AddExtension(new OAuthExtension(accessToken));

            try
            {
                clientBayeux.Handshake();
                if (clientBayeux.WaitFor(10000, new List<BayeuxClient.State> { BayeuxClient.State.CONNECTED }) == BayeuxClient.State.CONNECTED)
                {
                    var clientChannel = clientBayeux.GetChannel($"/event/{eventName}");
                    var listener = new MessageListener();
                    clientChannel.Subscribe(listener);

                    listener.PlatformEventReceived += Listener_PlatformEventReceived;
                }
                else
                {
                    Console.WriteLine("Failed to connect to CometD server.");
                }
            }
            finally
            {
            }
        }

        private void Listener_PlatformEventReceived(object? sender, PlatformEventReceivedEventArgs e)
        {
            JObject jsonObject = JObject.Parse(e.Message);

            string channel = jsonObject["channel"].ToString();

            string eventName = channel.Split(new string[] { "/event/" }, StringSplitOptions.None)[1];

            foreach (var handler in _eventHandlers[eventName])
            {
                handler?.Invoke(new EventMessage() { Message = e.Message, Timestamp = DateTime.Now, EventName = eventName });
            }
        }

        public void Unsubscribe(string eventName, EventReceivedHandler handler)
        {
            if (_eventHandlers.ContainsKey(eventName))
            {
                _eventHandlers[eventName].Remove(handler);
            }
        }

        public void AddMessage(EventMessage message)
        {
            if (_eventMessages.ContainsKey(message.EventName))
            {
                _eventMessages[message.EventName].Add(message);

                if (_eventHandlers.ContainsKey(message.EventName))
                {
                    foreach (var handler in _eventHandlers[message.EventName])
                    {
                        handler?.Invoke(message);
                    }
                }
            }
        }

        public List<EventMessage> GetMessages()
        {
            return _eventMessages.SelectMany(kvp => kvp.Value).ToList();
        }
    }
}
