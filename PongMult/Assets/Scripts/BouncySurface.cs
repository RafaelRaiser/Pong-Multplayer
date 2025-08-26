using UnityEngine;
using Unity.Netcode;

[RequireComponent(typeof(BoxCollider2D))]
public class BouncySurface : MonoBehaviour
{
    public enum ForceType { Additive, Multiplicative }
    public ForceType forceType = ForceType.Additive;
    public float bounceStrength = 1f;

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (!NetworkManager.Singleton || !NetworkManager.Singleton.IsServer) return;
        if (collision.gameObject.TryGetComponent<Ball>(out var ball))
        {
            var rb = ball.GetComponent<Rigidbody2D>();
            if (rb == null) return;

            switch (forceType)
            {
                case ForceType.Additive:
                    rb.velocity = rb.velocity.normalized * Mathf.Min(rb.velocity.magnitude + bounceStrength, 20f);
                    break;
                case ForceType.Multiplicative:
                    rb.velocity *= bounceStrength;
                    break;
            }
        }
    }
}
