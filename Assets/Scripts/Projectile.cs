// Projectile.cs (Modificado: Ignora paredes)
using UnityEngine;

public class Projectile : MonoBehaviour
{
    public float damage = 10f;
    public float lifeTime = 3f; // Tempo at� se destruir sozinho
    public string enemyTag = "Enemy"; // Tag dos inimigos
    // public string wallTag = "Wall"; // Tag n�o � mais usada para destrui��o
    public GameObject hitEffectPrefab; // Efeito ao atingir algo (opcional)

    void Start()
    {
        // Destroi o proj�til ap�s o tempo de vida definido
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
                enemy.ReceberDano(damage); // Chama a fun��o de dano no inimigo
            }
            else
            {
                Debug.LogWarning("Objeto com tag '" + enemyTag + "' n�o tem script Inimigo.", collision.gameObject);
            }
            // Destroi o proj�til ao atingir um inimigo
            HitTarget(collision.ClosestPoint(transform.position));
        }
        // --- REMOVIDO: Verifica��o de colis�o com parede ---
        // else if (collision.CompareTag(wallTag))
        // {
        //     HitTarget(collision.ClosestPoint(transform.position));
        // }
        // Adicione outras tags se necess�rio (ex: "DestructibleObject")
        // else if (collision.CompareTag("DestructibleObject")) { ... }
    }

    void HitTarget(Vector2 hitPosition)
    {
        // Instancia efeito de hit (se houver)
        if (hitEffectPrefab != null)
        {
            Instantiate(hitEffectPrefab, hitPosition, Quaternion.identity);
        }
        Destroy(gameObject); // Destroi o proj�til
    }
}

