using UnityEngine;

// Script para controlar o proj�til disparado por um Aliado (ex: Draco)
[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
public class AllyBulletScript : MonoBehaviour
{
    [Header("Configura��es da Bala")]
    public float speed = 10f;      // Velocidade de movimento da bala
    public int damage = 15;        // Dano que a bala causa
    public float lifeTime = 5f;      // Tempo em segundos antes da bala se autodestruir
    public string enemyTag = "Enemy"; // Tag dos objetos que a bala deve atingir

    private Rigidbody2D rb;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        // Garante que o Collider � um Trigger para detectar colis�es sem f�sica de impacto
        GetComponent<Collider2D>().isTrigger = true;

        // Aplica velocidade inicial na dire��o para onde a bala est� apontando
        // (Assume que o AllyController j� rotacionou a bala corretamente ao instanciar)
        rb.linearVelocity = transform.right * speed;

        // Destroi a bala ap�s o tempo de vida
        Destroy(gameObject, lifeTime);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        // Verifica se colidiu com um objeto com a tag de inimigo
        if (other.gameObject.CompareTag(enemyTag))
        {
            // Tenta obter o componente de script do inimigo (ajuste o nome se necess�rio)
            Inimigo enemyScript = other.gameObject.GetComponent<Inimigo>();

            // Verifica se o script do inimigo foi encontrado
            if (enemyScript != null)
            {
                // Aplica o dano ao inimigo
                Debug.Log(gameObject.name + " atingiu " + other.gameObject.name + " causando " + damage + " de dano.");
                enemyScript.ReceberDano(damage);
                // Destroi a bala ap�s causar dano
                Destroy(gameObject);
            }
            else
            {
                // Loga um aviso se o objeto "Enemy" n�o tiver o script esperado
                Debug.LogWarning("AllyBullet atingiu um objeto Enemy (" + other.gameObject.name + ") sem o script 'Inimigo'!", other.gameObject);
                // Destroi a bala mesmo assim
                Destroy(gameObject);
            }
        }
        // Adicionar outras colis�es que destroem a bala (ex: paredes)
        // else if (other.gameObject.CompareTag("Environment"))
        // {
        //     Destroy(gameObject);
        // }
    }

    // Opcional: Se precisar de l�gica no Update (raro para balas simples)
    // void Update()
    // {
    // }
}

