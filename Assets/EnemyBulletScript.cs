// EnemyBulletScript.cs (v4 - Modificado para usar HeartSystem_Universal.TakeDamage)
using UnityEngine;

public class EnemyBulletScript : MonoBehaviour
{
    private Rigidbody2D rb;
    public float force;
    private float timer;
    public int damageAmount = 1;
    public string playerTag = "Player"; // Tag do jogador (Henry ou outro Player)

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        GameObject playerObject = GameObject.FindGameObjectWithTag(playerTag);

        if (playerObject != null)
        {
            Vector3 direction = playerObject.transform.position - transform.position;
            rb.linearVelocity = new Vector2(direction.x, direction.y).normalized * force;
            // Rota��o assumindo sprite aponta para CIMA
            float rot = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90f;
            transform.rotation = Quaternion.Euler(0, 0, rot);
        }
        else
        {
            Debug.LogWarning("EnemyBulletScript: Jogador com tag '" + playerTag + "' n�o encontrado no Start.");
            Destroy(gameObject, 0.1f);
        }
    }

    void Update()
    {
        timer += Time.deltaTime;
        if (timer > 10)
        {
            Destroy(gameObject);
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        // Verifica se colidiu com o objeto que tem a tag do jogador
        if (other.gameObject.CompareTag(playerTag))
        {
            Debug.Log("Proj�til inimigo colidiu com: " + other.name);
            // --- MODIFICADO: Tenta pegar HeartSystem_Universal e chamar TakeDamage nele --- 
            HeartSystem_Universal healthSystem = other.gameObject.GetComponent<HeartSystem_Universal>();

            if (healthSystem != null)
            {
                Debug.Log("Aplicando " + damageAmount + " de dano via HeartSystem_Universal.");
                healthSystem.TakeDamage(damageAmount);
                Destroy(gameObject); // Destroi o proj�til ap�s tentar aplicar dano
            }
            // --- FIM DA MODIFICA��O ---
            else
            {
                Debug.LogWarning("Objeto com tag '" + playerTag + "' n�o tem o script HeartSystem_Universal. Dano n�o aplicado.", other.gameObject);
                // Considerar destruir o proj�til mesmo se n�o achar o script de vida?
                // Destroy(gameObject);
            }
        }
        // Opcional: Adicionar l�gica para colidir com paredes/cen�rio?
        // else if (other.gameObject.CompareTag("Wall"))
        // {
        //     Destroy(gameObject);
        // }
    }
}

