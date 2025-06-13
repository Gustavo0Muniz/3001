using UnityEngine;
using UnityEngine.Events;

public class EventTriggerArea : MonoBehaviour
{
    public UnityEvent onPlayerEnter;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            onPlayerEnter?.Invoke();
            // Opcional: Desativar o trigger ap�s o primeiro uso
             GetComponent<Collider2D>().enabled = false;
        }
    }
}
