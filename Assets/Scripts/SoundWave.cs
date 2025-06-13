using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(Rigidbody2D), typeof(CircleCollider2D))] 
public class SoundWave : MonoBehaviour
{
    private Rigidbody2D rb;
    private CircleCollider2D circleCollider; 

    private Vector2 direction;
    private float speed;
    private float growthRate;
    private float damage;
    private float maxLifetime;

    private float currentLifetime = 0f;
    private Vector3 initialScale;
    private List<Collider2D> hitEnemies = new List<Collider2D>();

    public string enemyTag = "Enemy";
   
    public GameObject hitEffectPrefab; 
    public bool stopOnWallHit = false;
    public bool stopOnEnemyHit = false; 

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        circleCollider = GetComponent<CircleCollider2D>();
        initialScale = transform.localScale;
        rb.gravityScale = 0;
        circleCollider.isTrigger = true; 
    }

    public void Initialize(Vector2 dir, float spd, float growth, float dmg, float life)
    {
        direction = dir.normalized;
        speed = spd;
        growthRate = growth;
        damage = dmg;
        maxLifetime = life;

        if (rb != null)
        {
            rb.linearVelocity = direction * speed;
        }
        else
        {
            Debug.LogError("Rigidbody2D não encontrado na Onda Sonora!", this.gameObject);
        }

        Destroy(gameObject, maxLifetime);
    }

    void Update()
    {
        transform.localScale += Vector3.one * growthRate * Time.deltaTime;
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag(enemyTag))
        {
            if (!hitEnemies.Contains(collision))
            {
                Inimigo enemy = collision.GetComponent<Inimigo>();
                if (enemy != null)
                {
                    enemy.ReceberDano(damage);
                    hitEnemies.Add(collision);
                    Debug.Log("Onda Sonora atingiu: " + collision.name);
                }
                else
                {
                    Debug.LogWarning("Objeto com tag '" + enemyTag + "' não tem script Inimigo.", collision.gameObject);
                }

                if (stopOnEnemyHit)
                {
                    HitTarget(collision.ClosestPoint(transform.position));
                }
            }
        }
        else if (stopOnWallHit && collision.CompareTag("Wall")) 
        {
            HitTarget(collision.ClosestPoint(transform.position));
        }
    }

    void HitTarget(Vector2 hitPosition)
    {
        if (hitEffectPrefab != null)
        {
            Instantiate(hitEffectPrefab, hitPosition, Quaternion.identity);
        }
        DestroySelf();
    }

    void DestroySelf()
    {
        if (rb != null) rb.linearVelocity = Vector2.zero;
        Destroy(gameObject);
    }
}

