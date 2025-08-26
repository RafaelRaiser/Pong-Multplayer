using UnityEngine;
using Unity.Netcode;

[RequireComponent(typeof(BoxCollider2D))]
public class ScoringZone : MonoBehaviour
{
    public bool isLeftGoal = true; // se true => quem pontua é o player da direita

    private void OnTriggerEnter2D(Collider2D other)
    {
        // só o servidor processa pontuação (a bola só colide no servidor se rb.simulated = IsServer)
        if (!NetworkManager.Singleton || !NetworkManager.Singleton.IsServer) return;

        // use TryGetComponent para evitar allocations
        if (other.TryGetComponent<Ball>(out _))
        {
            if (GameManager.Instance == null) return;

            if (isLeftGoal)
                GameManager.Instance.Player2Scored();
            else
                GameManager.Instance.Player1Scored();
        }
    }
}
