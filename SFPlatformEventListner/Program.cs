using CometD.NetCore.Client;
using CometD.NetCore.Client.Transport;
using CometD.NetCore.Bayeux.Client;
using CometD.NetCore.Bayeux;
using System.Text.Json;
using System.Collections.Specialized;
using SFPlatformEventListener;
using System.Configuration;

internal class Program
{
    /// <summary>
    /// A class to handle incoming platform events.
    /// </summary>
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
    public class OAuthTokenResponse
    {
        public string access_token { get; set; }
        public string instance_url { get; set; }
    }

    /// <summary>
    /// A class to handle OAuth token injection for CometD messages.
    /// </summary>
    public class OAuthExtension : IExtension
    {
        private readonly string _accessToken;

        /// <summary>
        /// Initializes a new instance of the OAuthExtension class.
        /// </summary>
        /// <param name="accessToken">The OAuth access token to be used in the CometD messages.</param>
        public OAuthExtension(string accessToken)
        {
            _accessToken = accessToken;
        }

        public Task<bool> Incoming(IClientSession session, IMutableMessage message)
        {
            return Task.FromResult(true);
        }

        public Task<bool> Outgoing(IClientSession session, IMutableMessage message)
        {
            if (message.Channel.Equals("/meta/handshake"))
            {
                message["ext"] = new Dictionary<string, object> { { "access_token", _accessToken } };
            }
            return Task.FromResult(true);
        }

        public bool Receive(IClientSession session, IMutableMessage message)
        {
            return true;
        }

        public bool ReceiveMeta(IClientSession session, IMutableMessage message)
        {
            return true;
        }

        public bool Send(IClientSession session, IMutableMessage message)
        {
            throw new NotImplementedException();
        }

        public bool SendMeta(IClientSession session, IMutableMessage message)
        {
            if (message.Channel.Equals("/meta/handshake"))
            {
                message["ext"] = new Dictionary<string, object> { { "access_token", _accessToken } };
            }
            return true;
        }
    }

    /// <summary>
    /// The main entry point of the application. It handles authentication, CometD connection, and event listening.
    /// </summary>
    /// <param name="args">Command-line arguments.</param>
    /// 
    static async Task Main(string[] args)
    {
        string clientId = ConfigurationManager.AppSettings["clientId"];
        string clientSecret = ConfigurationManager.AppSettings["clientSecret"];
        string username = ConfigurationManager.AppSettings["username"];
        string password = ConfigurationManager.AppSettings["password"];
        string securityToken = ConfigurationManager.AppSettings["securityToken"];
        string channel = $"/event/{ConfigurationManager.AppSettings["channel"]}";
        string tokenUrl = ConfigurationManager.AppSettings["tokenUrl"];
        string cometdVersion = ConfigurationManager.AppSettings["cometdVersion"];

        using (var client = new HttpClient())
        {
            var requestContent = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("grant_type", "password"),
                new KeyValuePair<string, string>("client_id", clientId),
                new KeyValuePair<string, string>("client_secret", clientSecret),
                new KeyValuePair<string, string>("username", username),
                new KeyValuePair<string, string>("password", password + securityToken)
            });

            HttpResponseMessage response = await client.PostAsync(tokenUrl, requestContent);
            response.EnsureSuccessStatusCode();

            string responseBody = await response.Content.ReadAsStringAsync();
            var tokenResponse = JsonSerializer.Deserialize<OAuthTokenResponse>(responseBody);
            string accessToken = tokenResponse.access_token;
            string instanceUrl = tokenResponse.instance_url;

            var transportOptions = new Dictionary<string, object>();
            transportOptions.Add("headers", new Dictionary<string, string>
            {
                { "Authorization", "Bearer " + accessToken }
            });

            NameValueCollection nvc = new NameValueCollection();
            nvc.Add("Authorization", "Bearer " + accessToken);

            var transport = new LongPollingTransport(transportOptions,nvc);
            var clientBayeux = new BayeuxClient($"{instanceUrl}/cometd/{cometdVersion}", transport);

            clientBayeux.AddExtension(new OAuthExtension(accessToken));

            clientBayeux.Handshake();
            if (clientBayeux.WaitFor(10000, new List<BayeuxClient.State> { BayeuxClient.State.CONNECTED }) == BayeuxClient.State.CONNECTED)
            {
                var clientChannel = clientBayeux.GetChannel(channel);

                var listner = new MessageListener();

                clientChannel.Subscribe(listner);

                listner.PlatformEventReceived += Listner_PlatformEventReceived; ;

                while (true)
                {
                    await Task.Delay(1000);
                }
            }
            else
            {
                Console.WriteLine("Failed to connect to CometD server.");
            }
        }
    }

    /// <summary>
    /// Event handler for platform events received on the CometD channel.
    /// </summary>
    /// <param name="sender">The sender of the event.</param>
    /// <param name="e">The event arguments containing the message.</param>
    private static void Listner_PlatformEventReceived(object? sender, PlatformEventReceivedEventArgs e)
    {
        Console.WriteLine(e.Message);
    }
}