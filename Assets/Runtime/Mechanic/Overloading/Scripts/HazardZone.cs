using UnityEngine;

/// <summary>
/// Any object with this component causes a loss when the truck touches it.
/// Use it for side barriers, heavy obstacles, pits, and out-of-bounds triggers.
/// </summary>
public sealed class HazardZone : MonoBehaviour
{
    [SerializeField] private GameManager gameManager;
    [SerializeField] private string failReason = "The truck hit a hazard.";

    private void Awake()
    {
        if (gameManager == null)
        {
            gameManager = FindAnyObjectByType<GameManager>();
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.GetComponentInParent<TruckController>() != null)
        {
            gameManager.Lose(failReason);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponentInParent<TruckController>() != null)
        {
            gameManager.Lose(failReason);
        }
    }
}
