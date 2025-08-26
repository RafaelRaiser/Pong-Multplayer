using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.UI;

public class GameManager : NetworkBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("UI")]
    public Text player1ScoreText;
    public Text player2ScoreText;

    [Header("Prefabs")]
    public NetworkObject ballPrefab;

    [Header("Spawn X")]
    public float leftX = -8.5f;
    public float rightX = 8.5f;

    private NetworkVariable<int> p1Score = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    private NetworkVariable<int> p2Score = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    private NetworkObject currentBall;
    private readonly List<ulong> playerOrder = new List<ulong>(2);

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public override void OnNetworkSpawn()
    {
        p1Score.OnValueChanged += (_, v) => { if (player1ScoreText) player1ScoreText.text = v.ToString(); };
        p2Score.OnValueChanged += (_, v) => { if (player2ScoreText) player2ScoreText.text = v.ToString(); };

        if (IsServer)
        {
            NetworkManager.OnClientConnectedCallback += OnClientConnected;
            NetworkManager.OnClientDisconnectCallback += OnClientDisconnected;
        }
    }

    protected override void OnDestroy()
    {
        if (IsServer && NetworkManager != null)
        {
            NetworkManager.OnClientConnectedCallback -= OnClientConnected;
            NetworkManager.OnClientDisconnectCallback -= OnClientDisconnected;
        }
        base.OnDestroy();
    }

    private void OnClientConnected(ulong clientId)
    {
        if (!playerOrder.Contains(clientId)) playerOrder.Add(clientId);
        StartCoroutine(TryStartMatch());
    }

    private void OnClientDisconnected(ulong clientId)
    {
        playerOrder.Remove(clientId);
        // opcional: resetar ou pausar jogo
        ResetGameServer();
    }

    private IEnumerator TryStartMatch()
    {
        // espera até termos 2 jogadores (pode ajustar para aceitar mais)
        while (playerOrder.Count < 2 ||
               !NetworkManager.ConnectedClients.ContainsKey(playerOrder[0]) ||
               !NetworkManager.ConnectedClients.ContainsKey(playerOrder[1]) ||
               NetworkManager.ConnectedClients[playerOrder[0]].PlayerObject == null ||
               NetworkManager.ConnectedClients[playerOrder[1]].PlayerObject == null)
        {
            yield return null;
        }

        // posiciona paddles
        var p0 = NetworkManager.ConnectedClients[playerOrder[0]].PlayerObject;
        var p1 = NetworkManager.ConnectedClients[playerOrder[1]].PlayerObject;

        if (p0 != null) p0.transform.position = new Vector2(leftX, 0f);
        if (p1 != null) p1.transform.position = new Vector2(rightX, 0f);

        ResetGameServer();
    }

    [ServerRpc(RequireOwnership = false)]
    public void RequestNewRoundServerRpc(ServerRpcParams rpcParams = default)
    {
        // quem chamar ServerRpc pede uma nova rodada; server executa
        NewRoundServer();
    }

    private void ResetGameServer()
    {
        // feito no servidor
        if (!IsServer) return;
        p1Score.Value = 0;
        p2Score.Value = 0;
        NewRoundServer();
    }

    private void NewRoundServer()
    {
        if (!IsServer) return;

        // reposiciona paddles
        foreach (var kv in NetworkManager.ConnectedClients)
        {
            var playerObj = kv.Value.PlayerObject;
            if (playerObj == null) continue;
            var rb = playerObj.GetComponent<Rigidbody2D>();
            bool isLeft = kv.Key == playerOrder[0];
            if (rb != null)
            {
                rb.linearVelocity = Vector2.zero;
                rb.position = new Vector2(isLeft ? leftX : rightX, 0f);
            }
            else
            {
                playerObj.transform.position = new Vector2(isLeft ? leftX : rightX, 0f);
            }
        }

        // respawn da bola
        if (currentBall != null && currentBall.IsSpawned)
        {
            currentBall.Despawn(true);
            currentBall = null;
        }

        currentBall = Instantiate(ballPrefab);
        currentBall.Spawn(true);

        StartCoroutine(StartBallAfterDelay());
    }

    private IEnumerator StartBallAfterDelay()
    {
        yield return new WaitForSeconds(1f);
        if (currentBall != null && currentBall.TryGetComponent<Ball>(out var nb))
        {
            // chame o método do servidor diretamente (evita RPC se já estamos no servidor)
            nb.AddStartingForce();
        }
    }

    // chamados pelo ScoringZone (servidor)
    public void Player1Scored()
    {
        if (!IsServer) return;
        p1Score.Value++;
        NewRoundServer();
    }

    public void Player2Scored()
    {
        if (!IsServer) return;
        p2Score.Value++;
        NewRoundServer();
    }
}
