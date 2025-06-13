using UnityEngine;


[RequireComponent(typeof(Collider2D))]
public class FinalDialogueTrigger : MonoBehaviour
{
    private bool triggered = false;
    private Collider2D triggerCollider;

    void Awake()
    {
        triggerCollider = GetComponent<Collider2D>();
        triggerCollider.isTrigger = true;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!triggered && other.CompareTag("Player"))
        {
            triggered = true;
            Debug.Log("FinalDialogueTrigger: Jogador entrou! Tentando iniciar diálogo final via GameEventManager.");

            GameEventManager gameEventManager = FindObjectOfType<GameEventManager>();
            if (gameEventManager != null)
            {
                gameEventManager.StartFinalDialogueSequence();
            }
            else
            {
                Debug.LogError("FinalDialogueTrigger: GameEventManager não encontrado na cena!");
            }

            gameObject.SetActive(false);
        }
    }

    void OnDrawGizmos()
    {
        if (triggerCollider == null) triggerCollider = GetComponent<Collider2D>();
        Gizmos.color = new Color(0, 1, 1, 0.3f); 
        if (triggerCollider is BoxCollider2D box)
        {
            Gizmos.DrawCube(transform.position + (Vector3)box.offset, box.size);
        }
        else if (triggerCollider is CircleCollider2D circle)
        {
            Gizmos.DrawSphere(transform.position + (Vector3)circle.offset, circle.radius);
        }
    }
}

