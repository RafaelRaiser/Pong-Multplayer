using UnityEngine;
using Unity.Netcode;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerPaddle : NetworkBehaviour
{
    [Header("Movement")]
    public float speed = 8f;
    public float maxY = 4.5f;

    private Rigidbody2D rb;

    // input replicado ao servidor
    private NetworkVariable<float> inputY = new NetworkVariable<float>(
        0f,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    public override void OnNetworkSpawn()
    {
        // apenas o servidor aplica a física do paddle
        rb.simulated = IsServer;
    }

    private void Update()
    {
        if (!IsOwner) return;

        float y = 0f;
        if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow)) y = 1f;
        else if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow)) y = -1f;

        // envia o input ao servidor (leve, simples)
        SubmitInputServerRpc(y);
    }

    [ServerRpc]
    private void SubmitInputServerRpc(float y, ServerRpcParams rpcParams = default)
    {
        inputY.Value = Mathf.Clamp(y, -1f, 1f);
    }

    private void FixedUpdate()
    {
        if (!IsServer) return;

        if (Mathf.Abs(inputY.Value) > 0f)
        {
            Vector2 newPos = rb.position + Vector2.up * inputY.Value * speed * Time.fixedDeltaTime;
            newPos.y = Mathf.Clamp(newPos.y, -maxY, maxY);
            rb.MovePosition(newPos);
        }
    }

    // util para o GameManager resetar (servidor)
    public void ResetPositionServer()
    {
        if (!IsServer) return;
        rb.linearVelocity = Vector2.zero;
        rb.position = new Vector2(rb.position.x, 0f);
    }
}
