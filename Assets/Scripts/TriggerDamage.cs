// TriggerDamage.cs (v2 - Corrigido para usar HeartSystem_Universal.TakeDamage)
using UnityEngine;

public class TriggerDamage : MonoBehaviour
{
    // Opcional: Definir quanto dano este trigger causa
    public int damageAmount = 1;
    public string playerTag = "Player"; // Tag do objeto que deve receber dano

    // Removido: public HeartSystem_Universal heart; // Não precisa mais de referência pré-definida

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // Verifica se o objeto que colidiu tem a tag correta
        if (collision.gameObject.CompareTag(playerTag))
        {
            // Tenta pegar o componente HeartSystem_Universal do objeto que colidiu
            HeartSystem_Universal healthSystem = collision.gameObject.GetComponent<HeartSystem_Universal>();

            // Se encontrou o sistema de vida, aplica o dano
            if (healthSystem != null)
            {
                Debug.Log(gameObject.name + " colidiu com " + collision.gameObject.name + ". Aplicando " + damageAmount + " de dano.");
                healthSystem.TakeDamage(damageAmount);

                // Opcional: Destruir este objeto após causar dano?
                // Destroy(gameObject);
            }
            else
            {
                Debug.LogWarning("TriggerDamage colidiu com objeto com tag '" + playerTag + "', mas ele não tem HeartSystem_Universal.", collision.gameObject);
            }

            // --- REMOVIDO: Linha antiga que causava erro ---
            // heart.vida--; 
        }
    }

    // Considerar usar OnTriggerEnter2D se este objeto for um Trigger Collider
    /*
    private void OnTriggerEnter2D(Collider2D other)
    {
        if(other.CompareTag(playerTag))
        {
            HeartSystem_Universal healthSystem = other.GetComponent<HeartSystem_Universal>();
            if (healthSystem != null)
            {
                healthSystem.TakeDamage(damageAmount);
                // Destroy(gameObject);
            }
        }
    }
    */
}

