using UnityEngine;



[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
public class AllyBulletScript : MonoBehaviour
{
    [Header("Configura��es da Bala")]
    public float speed = 10f;     
    public int damage = 15;       
    public float lifeTime = 5f;     
    public string enemyTag = "Enemy"; 
    private Rigidbody2D rb;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        GetComponent<Collider2D>().isTrigger = true;

        rb.linearVelocity = transform.right * speed;

        Destroy(gameObject, lifeTime);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.CompareTag(enemyTag))
        {
            Inimigo enemyScript = other.gameObject.GetComponent<Inimigo>();

            if (enemyScript != null)
            {
                Debug.Log(gameObject.name + " atingiu " + other.gameObject.name + " causando " + damage + " de dano.");
                enemyScript.ReceberDano(damage);
                Destroy(gameObject);
            }
            else
            {
                Debug.LogWarning("AllyBullet atingiu um objeto Enemy (" + other.gameObject.name + ") sem o script 'Inimigo'!", other.gameObject);
                Destroy(gameObject);
            }
        }
      
    }

   
}

