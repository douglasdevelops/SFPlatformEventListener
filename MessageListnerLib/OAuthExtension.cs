using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CometD.NetCore.Client;
using CometD.NetCore.Client.Transport;
using CometD.NetCore.Bayeux.Client;
using CometD.NetCore.Bayeux;

namespace MessageListnerLib
{
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
}
