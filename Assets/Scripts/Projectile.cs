using UnityEngine;

public class Projectile : MonoBehaviour
{
    public float damage = 10f;
    public float lifeTime = 3f; 
    public string enemyTag = "Enemy"; 
    public GameObject hitEffectPrefab; 

    void Start()
    {
        Destroy(gameObject, lifeTime);
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag(enemyTag))
        {
            Inimigo enemy = collision.GetComponent<Inimigo>();
            if (enemy != null)
            {
                enemy.ReceberDano(damage); 
            }
            else
            {
                Debug.LogWarning("Objeto com tag '" + enemyTag + "' não tem script Inimigo.", collision.gameObject);
            }
            HitTarget(collision.ClosestPoint(transform.position));
        }
     
    }

    void HitTarget(Vector2 hitPosition)
    {
        if (hitEffectPrefab != null)
        {
            Instantiate(hitEffectPrefab, hitPosition, Quaternion.identity);
        }
        Destroy(gameObject); 
    }
}

