using UnityEngine;

// Este script deve ser colocado em um GameObject com um Collider2D marcado como 'Is Trigger'.
// Posicione este GameObject onde o jogador deve chegar para iniciar o diálogo final.
[RequireComponent(typeof(Collider2D))]
public class FinalDialogueTrigger : MonoBehaviour
{
    private bool triggered = false;
    private Collider2D triggerCollider;

    void Awake()
    {
        triggerCollider = GetComponent<Collider2D>();
        // Garante que é um trigger
        triggerCollider.isTrigger = true;
        // O GameObject que contém este script deve começar desativado na cena
        // e ser ativado pelo GameEventManager quando a reciclagem encher.
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Verifica se já foi acionado e se quem entrou é o jogador
        if (!triggered && other.CompareTag("Player"))
        {
            triggered = true;
            Debug.Log("FinalDialogueTrigger: Jogador entrou! Tentando iniciar diálogo final via GameEventManager.");

            // Encontra o GameEventManager na cena
            GameEventManager gameEventManager = FindObjectOfType<GameEventManager>();
            if (gameEventManager != null)
            {
                // Chama a função pública no GameEventManager para iniciar a sequência
                gameEventManager.StartFinalDialogueSequence();
            }
            else
            {
                Debug.LogError("FinalDialogueTrigger: GameEventManager não encontrado na cena!");
            }

            // Desativa o GameObject do trigger para não ser acionado novamente
            gameObject.SetActive(false);
        }
    }

    // Opcional: Desenha um Gizmo no editor para visualizar a área do trigger
    void OnDrawGizmos()
    {
        if (triggerCollider == null) triggerCollider = GetComponent<Collider2D>();
        Gizmos.color = new Color(0, 1, 1, 0.3f); // Ciano transparente
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

