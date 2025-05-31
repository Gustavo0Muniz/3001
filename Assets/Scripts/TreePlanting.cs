using UnityEngine;
using System.Collections;

public class TreePlanting : MonoBehaviour
{
    [Header("Configurações de Plantio")]
    [SerializeField] private GameObject treePrefab; // Prefab da árvore a ser plantada
    [SerializeField] private float plantingCooldown = 2f; // Tempo mínimo entre plantios
    [SerializeField] private float minDistanceBetweenTrees = 3f; // Distância mínima entre árvores

    [Header("Animação de Crescimento")]
    [SerializeField] private Sprite[] treeGrowthStages; // Sprites dos estágios de crescimento
    [SerializeField] private float growthStageTime = 0.5f; // Tempo entre cada estágio

    [Header("Efeitos")]
    [SerializeField] private GameObject plantEffect; // Efeito opcional de partículas ao plantar
    [SerializeField] private AudioClip plantSound; // Som opcional ao plantar

    private float lastPlantTime = -10f; // Inicializado para permitir plantar imediatamente
    private Transform playerTransform;
    private LayerMask treeLayer; // Layer para verificar colisões com outras árvores

    private void Start()
    {
        playerTransform = transform;

        // Configura a layer para detecção de árvores
        treeLayer = LayerMask.GetMask("Tree");

        if (treePrefab == null)
        {
            Debug.LogError("TreePlanting: Prefab da árvore não configurado!", this);
            enabled = false;
        }
    }

    private void Update()
    {
        // Verifica se o jogador pressionou a tecla O para plantar
        if (Input.GetKeyDown(KeyCode.O) && CanPlant())
        {
            PlantTree();
        }
    }

    private bool CanPlant()
    {
        // Verifica o cooldown
        if (Time.time - lastPlantTime < plantingCooldown)
            return false;

        // Verifica se há espaço suficiente (não há outras árvores muito próximas)
        Collider2D[] nearbyTrees = Physics2D.OverlapCircleAll(playerTransform.position, minDistanceBetweenTrees, treeLayer);
        if (nearbyTrees.Length > 0)
        {
            Debug.Log("Não é possível plantar aqui. Muito próximo de outra árvore.");
            return false;
        }

        return true;
    }

    private void PlantTree()
    {
        // Atualiza o tempo do último plantio
        lastPlantTime = Time.time;

        // Cria a árvore na posição do jogador
        Vector3 plantPosition = playerTransform.position;
        GameObject newTree = Instantiate(treePrefab, plantPosition, Quaternion.identity);

        // Inicia a animação de crescimento se houver sprites configurados
        if (treeGrowthStages != null && treeGrowthStages.Length > 0)
        {
            StartCoroutine(AnimateTreeGrowth(newTree.GetComponent<SpriteRenderer>()));
        }

        // Notifica o EnvironmentalManager sobre o plantio
        if (EnvironmentalManager.Instance != null)
        {
            EnvironmentalManager.Instance.PlantTree();
        }
        else
        {
            Debug.LogError("TreePlanting: EnvironmentalManager não encontrado!", this);
        }

        // Reproduz efeito de partículas, se configurado
        if (plantEffect != null)
        {
            Instantiate(plantEffect, plantPosition, Quaternion.identity);
        }

        // Reproduz som, se configurado
        if (plantSound != null)
        {
            AudioSource.PlayClipAtPoint(plantSound, plantPosition);
        }
    }

    private IEnumerator AnimateTreeGrowth(SpriteRenderer treeSprite)
    {
        if (treeSprite == null)
            yield break;

        // Começa com o primeiro estágio (menor)
        treeSprite.sprite = treeGrowthStages[0];

        // Anima através de todos os estágios
        for (int i = 1; i < treeGrowthStages.Length; i++)
        {
            yield return new WaitForSeconds(growthStageTime);
            treeSprite.sprite = treeGrowthStages[i];

            // Opcional: Ajusta a escala ou outras propriedades para cada estágio
            float growthFactor = (float)i / (treeGrowthStages.Length - 1);
            treeSprite.transform.localScale = Vector3.Lerp(
                new Vector3(0.5f, 0.5f, 1f),
                Vector3.one,
                growthFactor
            );
        }
    }

    // Método para visualizar o raio de plantio no editor
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, minDistanceBetweenTrees);
    }
}
