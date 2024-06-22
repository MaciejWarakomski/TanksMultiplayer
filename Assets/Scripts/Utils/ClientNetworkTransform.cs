using Unity.Netcode.Components;

namespace Utils
{
    public class ClientNetworkTransform : NetworkTransform
    {
        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            CanCommitToTransform = IsOwner;
        }

        protected override void Update()
        {
            CanCommitToTransform = IsOwner;
            base.Update();
            if (CanCommitToTransform && NetworkManager && (NetworkManager.IsConnectedClient || NetworkManager.IsListening))
            {
                TryCommitTransformToServer(transform, NetworkManager.LocalTime.Time);
            }
        }

        protected override bool OnIsServerAuthoritative() => false;
    }
}