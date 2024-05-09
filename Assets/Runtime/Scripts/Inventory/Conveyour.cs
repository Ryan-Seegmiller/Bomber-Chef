using Collectible;
using UnityEngine;

public class Conveyour : MonoBehaviour
{
    public float conveyourSpeed;
    public const int conveyourID = 367;

    private void Start()
    {
        conveyourSpeed = GameManager.instance.conveyourSpeed;
    }
    private void OnTriggerStay(Collider other)
    {
        if(GameManager.instance.gameState != GameState.Playing) { return; }
        if (other.TryGetComponent(out Rigidbody rb))
        {
            if(rb.velocity.magnitude < conveyourSpeed)
            {
                rb.AddForce(transform.forward * (conveyourSpeed - rb.velocity.magnitude) * 10, ForceMode.Force);
            }
            if (other.TryGetComponent(out ItemHelper item))
            {
                item.ResetTime();
            }
        }
    }
}
