// SoundWave.cs (Modificado: Ignora paredes, a menos que stopOnWallHit seja true)
using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(Rigidbody2D), typeof(CircleCollider2D))] // Ou outro collider
public class SoundWave : MonoBehaviour
{
    private Rigidbody2D rb;
    private CircleCollider2D circleCollider; // Ou o tipo de collider que você usar

    private Vector2 direction;
    private float speed;
    private float growthRate;
    private float damage;
    private float maxLifetime;

    private float currentLifetime = 0f;
    private Vector3 initialScale;
    private List<Collider2D> hitEnemies = new List<Collider2D>(); // Lista para evitar dano múltiplo no mesmo inimigo

    public string enemyTag = "Enemy";
    // public string wallTag = "Wall"; // Tag verificada apenas se stopOnWallHit for true
    public GameObject hitEffectPrefab; // Efeito ao atingir algo (opcional)
    public bool stopOnWallHit = false; // <<< ALTERADO PARA FALSE POR PADRÃO - A onda NÃO para ao bater na parede
    public bool stopOnEnemyHit = false; // A onda para ao bater no primeiro inimigo?

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        circleCollider = GetComponent<CircleCollider2D>();
        initialScale = transform.localScale;
        rb.gravityScale = 0; // Garante que não tenha gravidade
        circleCollider.isTrigger = true; // Para detectar colisões sem física de impacto
    }

    // Método chamado pelo PlayerController para passar os parâmetros
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

        // Destroi o objeto após o tempo máximo de vida
        Destroy(gameObject, maxLifetime);
    }

    void Update()
    {
        // Crescimento da Onda
        transform.localScale += Vector3.one * growthRate * Time.deltaTime;
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        // Verifica se colidiu com inimigo
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
        // Verifica se colidiu com parede/obstáculo APENAS se stopOnWallHit for true
        else if (stopOnWallHit && collision.CompareTag("Wall")) // <<< CONDIÇÃO ADICIONADA
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

