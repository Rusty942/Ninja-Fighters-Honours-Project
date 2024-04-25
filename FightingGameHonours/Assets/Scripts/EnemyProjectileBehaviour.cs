using UnityEngine;

public class EnemyProjectileBehaviour : MonoBehaviour
{
    //Projectile Behaviour Script
    public float Speed = 4f;

    void Update()
    {
        transform.position -= transform.right * Time.deltaTime * Speed;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Wall"))
        {
            // Handle collision with wall
            Debug.Log("Hit wall");
            Destroy(gameObject);
        }
        else if (collision.gameObject.CompareTag("Pl"))
        {
            PlayerMovement playerMovement = collision.gameObject.GetComponent<PlayerMovement>();
            // Handle collision with enemy opponent
            Debug.Log("Hit Enemy");
            Destroy(gameObject);
            playerMovement.Balled(100);
        }
        else if (collision.gameObject.CompareTag("Ball"))
        {
            // Handle collision with ball
            Debug.Log("Hit Ball");
            Destroy(gameObject);
        }
    }
}