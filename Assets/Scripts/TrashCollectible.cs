using UnityEngine;
using System.Collections;

public class TrashCollectible : MonoBehaviour
{
    [Header("Configurações Visuais")]
    [SerializeField] private float glowRadius = 3f; // Raio em que o lixo começa a brilhar
    [SerializeField] private float glowIntensity = 1.5f; // Intensidade máxima do brilho
    [SerializeField] private Color glowColor = Color.yellow; // Cor do brilho
    [SerializeField] private GameObject interactionPrompt; // Objeto com texto "Aperte E para coletar"

    [Header("Efeitos")]
    [SerializeField] private GameObject collectEffect; // Efeito opcional de partículas ao coletar
    [SerializeField] private AudioClip collectSound; // Som opcional ao coletar

    private SpriteRenderer spriteRenderer;
    private Color originalColor;
    private Material material;
    private bool playerInRange = false;
    private Transform playerTransform;

    private void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            originalColor = spriteRenderer.color;
            material = spriteRenderer.material;
        }
        else
        {
            Debug.LogWarning("TrashCollectible: SpriteRenderer não encontrado!", this);
        }

        // Desativa o prompt de interação no início
        if (interactionPrompt != null)
        {
            interactionPrompt.SetActive(false);
        }
    }

    private void Update()
    {
        // Verifica se o jogador está no alcance e pressionou E
        if (playerInRange && Input.GetKeyDown(KeyCode.E))
        {
            CollectTrash();
        }

        // Atualiza o brilho baseado na distância do jogador
        UpdateGlow();
    }

    private void UpdateGlow()
    {
        if (playerTransform == null || spriteRenderer == null)
            return;

        float distance = Vector2.Distance(transform.position, playerTransform.position);

        // Calcula a intensidade do brilho baseado na distância
        if (distance <= glowRadius)
        {
            // Quanto mais perto, mais brilhante
            float glowFactor = 1f - (distance / glowRadius);
            float currentIntensity = 1f + (glowIntensity - 1f) * glowFactor;

            // Aplica o brilho
            spriteRenderer.color = originalColor * currentIntensity;

            // Mostra o prompt de interação quando estiver bem próximo
            if (interactionPrompt != null)
            {
                interactionPrompt.SetActive(distance <= glowRadius * 0.5f);
            }
        }
        else
        {
            // Restaura a cor original quando fora do alcance
            spriteRenderer.color = originalColor;

            if (interactionPrompt != null)
            {
                interactionPrompt.SetActive(false);
            }
        }
    }

    private void CollectTrash()
    {
        // Notifica o EnvironmentalManager sobre a coleta
        if (EnvironmentalManager.Instance != null)
        {
            EnvironmentalManager.Instance.CollectTrash();
        }
        else
        {
            Debug.LogError("TrashCollectible: EnvironmentalManager não encontrado!", this);
        }

        // Reproduz efeito de partículas, se configurado
        if (collectEffect != null)
        {
            Instantiate(collectEffect, transform.position, Quaternion.identity);
        }

        // Reproduz som, se configurado
        if (collectSound != null)
        {
            AudioSource.PlayClipAtPoint(collectSound, transform.position);
        }

        // Destrói o objeto de lixo
        Destroy(gameObject);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Verifica se o objeto que entrou no trigger é o jogador
        if (other.CompareTag("Player"))
        {
            playerInRange = true;
            playerTransform = other.transform;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        // Verifica se o objeto que saiu do trigger é o jogador
        if (other.CompareTag("Player"))
        {
            playerInRange = false;

            // Restaura a cor original
            if (spriteRenderer != null)
            {
                spriteRenderer.color = originalColor;
            }

            // Esconde o prompt de interação
            if (interactionPrompt != null)
            {
                interactionPrompt.SetActive(false);
            }
        }
    }

    // Método para visualizar o raio de detecção no editor
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, glowRadius);
    }
}
