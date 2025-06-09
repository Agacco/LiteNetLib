using UnityEngine;

namespace NatPunchSample
{
    public class NatPunchHost : MonoBehaviour
    {
        public int Port = 5000;
        public string Token = "room";
        private NatPunchCore _core;

        private void Start()
        {
            _core = new NatPunchCore(true, Port);
            _core.OnPeerConnected += peer => Debug.Log($"Client connected {peer.EndPoint}");
            Debug.Log("NatPunch host started");
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
