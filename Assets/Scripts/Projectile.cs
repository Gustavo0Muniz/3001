// Projectile.cs (Modificado: Ignora paredes)
using UnityEngine;

public class Projectile : MonoBehaviour
{
    public float damage = 10f;
    public float lifeTime = 3f; // Tempo até se destruir sozinho
    public string enemyTag = "Enemy"; // Tag dos inimigos
    // public string wallTag = "Wall"; // Tag não é mais usada para destruição
    public GameObject hitEffectPrefab; // Efeito ao atingir algo (opcional)

    void Start()
    {
        // Destroi o projétil após o tempo de vida definido
        Destroy(gameObject, lifeTime);
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        // Verifica se colidiu com inimigo
        if (collision.CompareTag(enemyTag))
        {
            // Tenta encontrar o script Inimigo (adapte o nome se for diferente)
            Inimigo enemy = collision.GetComponent<Inimigo>();
            if (enemy != null)
            {
                enemy.ReceberDano(damage); // Chama a função de dano no inimigo
            }
            else
            {
                Debug.LogWarning("Objeto com tag '" + enemyTag + "' não tem script Inimigo.", collision.gameObject);
            }
            // Destroi o projétil ao atingir um inimigo
            HitTarget(collision.ClosestPoint(transform.position));
        }
        // --- REMOVIDO: Verificação de colisão com parede ---
        // else if (collision.CompareTag(wallTag))
        // {
        //     HitTarget(collision.ClosestPoint(transform.position));
        // }
        // Adicione outras tags se necessário (ex: "DestructibleObject")
        // else if (collision.CompareTag("DestructibleObject")) { ... }
    }

    void HitTarget(Vector2 hitPosition)
    {
        // Instancia efeito de hit (se houver)
        if (hitEffectPrefab != null)
        {
            Instantiate(hitEffectPrefab, hitPosition, Quaternion.identity);
        }
        Destroy(gameObject); // Destroi o projétil
    }
}

