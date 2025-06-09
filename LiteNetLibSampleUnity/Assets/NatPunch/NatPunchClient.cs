using UnityEngine;

namespace NatPunchSample
{
    public class NatPunchClient : MonoBehaviour
    {
        public string HostAddress = "127.0.0.1";
        public int HostPort = 5000;
        public string Token = "room";
        private NatPunchCore _core;

        private void Start()
        {
            _core = new NatPunchCore(false, 0);
            _core.OnPeerConnected += peer => Debug.Log($"Connected to host {peer.EndPoint}");
            _core.SendNatIntroduceRequest(HostAddress, HostPort, Token);
            Debug.Log("NatPunch client started");
        }

        private void Update()
        {
            _core?.PollEvents();
        }

        private void OnDestroy()
        {
            _core?.Stop();
        }
    }
}
