using UnityEngine;

public class TriggerDamage : MonoBehaviour
{
    public int damageAmount = 1;
    public string playerTag = "Player"; 


    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag(playerTag))
        {
            HeartSystem_Universal healthSystem = collision.gameObject.GetComponent<HeartSystem_Universal>();

            if (healthSystem != null)
            {
                Debug.Log(gameObject.name + " colidiu com " + collision.gameObject.name + ". Aplicando " + damageAmount + " de dano.");
                healthSystem.TakeDamage(damageAmount);

              
            }
            else
            {
                Debug.LogWarning("TriggerDamage colidiu com objeto com tag '" + playerTag + "', mas ele não tem HeartSystem_Universal.", collision.gameObject);
            }

           
        }
    }

    
}

