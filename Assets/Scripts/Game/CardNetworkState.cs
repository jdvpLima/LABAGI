using Unity.Netcode;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class CardNetworkState : NetworkBehaviour
{
    public NetworkVariable<ulong> OwnerId = new NetworkVariable<ulong>();
    public NetworkVariable<int> CardValue = new NetworkVariable<int>();

    private XRGrabInteractable grab;

    private void Awake()
    {
        grab = GetComponent<XRGrabInteractable>();

        grab.selectEntered.AddListener(OnGrab);
    }

    public override void OnNetworkSpawn()
    {
        if (IsServer && CardValue.Value == 0)
            CardValue.Value = Random.Range(1, 14);
    }

    [ServerRpc(RequireOwnership = false)]
    public void SetOwnerServerRpc(ulong id)
    {
        OwnerId.Value = id;
    }

    private void OnGrab(SelectEnterEventArgs args)
    {
        var interactorObject = args.interactorObject;
        var playerObject = interactorObject.transform.root.GetComponentInParent<NetworkObject>();

        if (playerObject == null)
        {
            DenyGrab();
            return;
        }

        ulong grabbingClient = playerObject.OwnerClientId;

        if (grabbingClient != OwnerId.Value)
            DenyGrab();
    }

    private void DenyGrab()
    {
        grab.interactionManager.CancelInteractableSelection((IXRSelectInteractable)grab);
    }
}
