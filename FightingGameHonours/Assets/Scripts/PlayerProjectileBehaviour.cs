using UnityEngine;

public class PlayerProjectileBehaviour : MonoBehaviour
{
    //Projectile Behaviour Script
    public float Speed = 4f;

    void Update()
    {
        transform.position += transform.right * Time.deltaTime * Speed;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Wall"))
        {
            // Handle collision with wall
            Debug.Log("Hit wall");
            Destroy(gameObject);
        }
        else if (collision.gameObject.CompareTag("Enemy"))
        {
            EnemyMovement enemyMovement = collision.gameObject.GetComponent<EnemyMovement>();
            // Handle collision with enemy opponent
            Debug.Log("Hit Enemy");
            Destroy(gameObject);
            enemyMovement.Balled(100);
        }
        else if (collision.gameObject.CompareTag("Ball"))
        {
            // Handle collision with ball
            Debug.Log("Hit Ball");
            Destroy(gameObject);
        }
    }
}
