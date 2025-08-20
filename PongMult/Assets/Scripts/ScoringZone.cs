using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(BoxCollider2D))]
public class ScoringZone : MonoBehaviour
{
    public UnityEvent scoreTrigger;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.TryGetComponent(out Ball _))
        {
            scoreTrigger.Invoke();
        }
    }
}
