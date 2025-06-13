using UnityEngine;
using System.Collections;

public class TrashCollectible : MonoBehaviour
{
    [Header("Configurações Visuais")]
    [SerializeField] private float glowRadius = 3f; 
    [SerializeField] private float glowIntensity = 1.5f; 
    [SerializeField] private Color glowColor = Color.yellow; 
    [SerializeField] private GameObject interactionPrompt; 

    [Header("Efeitos")]
    [SerializeField] private GameObject collectEffect;
    [SerializeField] private AudioClip collectSound; 

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

        if (interactionPrompt != null)
        {
            interactionPrompt.SetActive(false);
        }
    }

    private void Update()
    {
        if (playerInRange && Input.GetKeyDown(KeyCode.E))
        {
            CollectTrash();
        }

        UpdateGlow();
    }

    private void UpdateGlow()
    {
        if (playerTransform == null || spriteRenderer == null)
            return;

        float distance = Vector2.Distance(transform.position, playerTransform.position);

        if (distance <= glowRadius)
        {
            float glowFactor = 1f - (distance / glowRadius);
            float currentIntensity = 1f + (glowIntensity - 1f) * glowFactor;

            spriteRenderer.color = originalColor * currentIntensity;

            if (interactionPrompt != null)
            {
                interactionPrompt.SetActive(distance <= glowRadius * 0.5f);
            }
        }
        else
        {
            spriteRenderer.color = originalColor;

            if (interactionPrompt != null)
            {
                interactionPrompt.SetActive(false);
            }
        }
    }

    private void CollectTrash()
    {
        if (EnvironmentalManager.Instance != null)
        {
            EnvironmentalManager.Instance.CollectTrash();
        }
        else
        {
            Debug.LogError("TrashCollectible: EnvironmentalManager não encontrado!", this);
        }

        if (collectEffect != null)
        {
            Instantiate(collectEffect, transform.position, Quaternion.identity);
        }

        if (collectSound != null)
        {
            AudioSource.PlayClipAtPoint(collectSound, transform.position);
        }

        Destroy(gameObject);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;
            playerTransform = other.transform;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;

            if (spriteRenderer != null)
            {
                spriteRenderer.color = originalColor;
            }

            if (interactionPrompt != null)
            {
                interactionPrompt.SetActive(false);
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, glowRadius);
    }
}
