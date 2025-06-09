using System;
using System.Collections.Generic;
using System.Net;
using LiteNetLib;
using LiteNetLib.Utils;

namespace NatPunchSample
{
    internal sealed class WaitPeer
    {
        public IPEndPoint Internal;
        public IPEndPoint External;
    }

    public class NatPunchCore : INatPunchListener
    {
        private const string ConnectionKey = "nat_test";
        private readonly EventBasedNetListener _listener;
        private readonly EventBasedNatPunchListener _natListener;
        private readonly NetManager _netManager;
        private readonly Dictionary<string, WaitPeer> _waiting = new Dictionary<string, WaitPeer>();

        public event Action<NetPeer> OnPeerConnected;
        public event Action<NetPeer> OnPeerDisconnected;
        public event Action OnNatIntroductionSuccess;

        public NatPunchCore(bool isHost, int port)
        {
            _listener = new EventBasedNetListener();
            _natListener = new EventBasedNatPunchListener();

            _netManager = new NetManager(_listener)
            {
                NatPunchEnabled = true,
                IPv6Enabled = true
            };

            if (isHost)
            {
                _netManager.Start(port);
                _netManager.NatPunchModule.Init(this);
            }
            else
            {
                _netManager.Start();
                _netManager.NatPunchModule.Init(_natListener);
                _natListener.NatIntroductionSuccess += (point, type, token) =>
                {
                    _netManager.Connect(point, ConnectionKey);
                    OnNatIntroductionSuccess?.Invoke();
                };
            }

            _listener.PeerConnectedEvent += peer => OnPeerConnected?.Invoke(peer);
            _listener.PeerDisconnectedEvent += (peer, info) => OnPeerDisconnected?.Invoke(peer);
            _listener.ConnectionRequestEvent += request => request.AcceptIfKey(ConnectionKey);
        }

        public void SendNatIntroduceRequest(string host, int port, string token)
        {
            _netManager.NatPunchModule.SendNatIntroduceRequest(host, port, token);
        }

        public void PollEvents()
        {
            _netManager.NatPunchModule.PollEvents();
            _netManager.PollEvents();
        }

        public void Stop()
        {
            _netManager.Stop();
        }

        void INatPunchListener.OnNatIntroductionRequest(IPEndPoint localEndPoint, IPEndPoint remoteEndPoint, string token)
        {
            if (_waiting.TryGetValue(token, out var peer))
            {
                _netManager.NatPunchModule.NatIntroduce(peer.Internal, peer.External, localEndPoint, remoteEndPoint, token);
                _waiting.Remove(token);
            }
            else
            {
                _waiting[token] = new WaitPeer { Internal = localEndPoint, External = remoteEndPoint };
            }
        }

        void INatPunchListener.OnNatIntroductionSuccess(IPEndPoint targetEndPoint, NatAddressType type, string token)
        {
            // only clients handle this through EventBasedNatPunchListener
        }
    }
}
