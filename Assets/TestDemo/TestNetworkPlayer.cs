using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

public class TestNetworkPlayer : NetworkBehaviour
{
    private void Start()
    {
        if (IsOwner)
            GetComponent<MeshRenderer>().material.color = Color.green;
        else
            GetComponent<MeshRenderer>().material.color = Color.gray;
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        Debug.Log($"[TestNetworkPlayer] Spawned! ClientId: {NetworkManager.Singleton.LocalClientId}, IsOwner: {IsOwner}");
    }

    private void Update()
    {
        if (!IsOwner) return;

        Vector2 input = Keyboard.current != null ? GetInputFromKeyboard() : Vector2.zero;

        // Simple horizontal movement
        transform.Translate(input.x * Time.deltaTime * 5f, 0, 0);
    }

    private Vector2 GetInputFromKeyboard()
    {
        float moveX = 0;

        if (Keyboard.current.aKey.isPressed)
            moveX -= 1f;

        if (Keyboard.current.dKey.isPressed)
            moveX += 1f;

        return new Vector2(moveX, 0);
    }
}
