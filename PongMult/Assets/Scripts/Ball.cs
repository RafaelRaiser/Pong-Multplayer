using UnityEngine;
using Unity.Netcode;

[RequireComponent(typeof(Rigidbody2D))]
public class Ball : NetworkBehaviour
{
    private Rigidbody2D rb;

    [Header("Speed")]
    public float baseSpeed = 6f;
    public float maxSpeed = 14f;
    public float currentSpeed { get; private set; }

    private void Awake()
    {
        // cache do componente (evita allocations repetidas)
        rb = GetComponent<Rigidbody2D>();
    }

    public override void OnNetworkSpawn()
    {
        // física da bola somente no servidor/host
        rb.simulated = IsServer;
        if (IsServer)
        {
            ResetPosition();
        }
    }

    // método público que o servidor chama para iniciar rodada
    public void AddStartingForce()
    {
        if (!IsServer) return;

        float x = Random.value < 0.5f ? -1f : 1f;
        float y = Random.value < 0.5f ? Random.Range(-0.8f, -0.3f) : Random.Range(0.3f, 0.8f);
        Vector2 dir = new Vector2(x, y).normalized;
        rb.AddForce(dir * baseSpeed, ForceMode2D.Impulse);
        currentSpeed = baseSpeed;
    }

    // opcional: cliente pode pedir ao servidor para começar (não necessário se o servidor o chamar)
    [ServerRpc(RequireOwnership = false)]
    public void StartRoundServerRpc(ServerRpcParams rpcParams = default)
    {
        AddStartingForce();
    }

    public void ResetPosition()
    {
        // use linearVelocity (Unity 6.x)
        rb.linearVelocity = Vector2.zero;
        rb.position = Vector2.zero;
        currentSpeed = baseSpeed;
    }

    private void FixedUpdate()
    {
        if (!IsServer) return;
        if (rb.linearVelocity.sqrMagnitude == 0f) return;

        Vector2 dir = rb.linearVelocity.normalized;
        float speed = Mathf.Min(rb.linearVelocity.magnitude, maxSpeed);
        currentSpeed = speed;
        rb.linearVelocity = dir * speed;
    }
}
