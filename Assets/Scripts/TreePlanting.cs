using UnityEngine;
using System.Collections;

public class TreePlanting : MonoBehaviour
{
    [Header("Configura��es de Plantio")]
    [SerializeField] private GameObject treePrefab; // Prefab da �rvore a ser plantada
    [SerializeField] private float plantingCooldown = 2f; // Tempo m�nimo entre plantios
    [SerializeField] private float minDistanceBetweenTrees = 3f; // Dist�ncia m�nima entre �rvores

    [Header("Anima��o de Crescimento")]
    [SerializeField] private Sprite[] treeGrowthStages; // Sprites dos est�gios de crescimento
    [SerializeField] private float growthStageTime = 0.5f; // Tempo entre cada est�gio

    [Header("Efeitos")]
    [SerializeField] private GameObject plantEffect; // Efeito opcional de part�culas ao plantar
    [SerializeField] private AudioClip plantSound; // Som opcional ao plantar

    private float lastPlantTime = -10f; // Inicializado para permitir plantar imediatamente
    private Transform playerTransform;
    private LayerMask treeLayer; // Layer para verificar colis�es com outras �rvores

    private void Start()
    {
        playerTransform = transform;

        // Configura a layer para detec��o de �rvores
        treeLayer = LayerMask.GetMask("Tree");

        if (treePrefab == null)
        {
            Debug.LogError("TreePlanting: Prefab da �rvore n�o configurado!", this);
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

        // Verifica se h� espa�o suficiente (n�o h� outras �rvores muito pr�ximas)
        Collider2D[] nearbyTrees = Physics2D.OverlapCircleAll(playerTransform.position, minDistanceBetweenTrees, treeLayer);
        if (nearbyTrees.Length > 0)
        {
            Debug.Log("N�o � poss�vel plantar aqui. Muito pr�ximo de outra �rvore.");
            return false;
        }

        return true;
    }

    private void PlantTree()
    {
        // Atualiza o tempo do �ltimo plantio
        lastPlantTime = Time.time;

        // Cria a �rvore na posi��o do jogador
        Vector3 plantPosition = playerTransform.position;
        GameObject newTree = Instantiate(treePrefab, plantPosition, Quaternion.identity);

        // Inicia a anima��o de crescimento se houver sprites configurados
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
            Debug.LogError("TreePlanting: EnvironmentalManager n�o encontrado!", this);
        }

        // Reproduz efeito de part�culas, se configurado
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

        // Come�a com o primeiro est�gio (menor)
        treeSprite.sprite = treeGrowthStages[0];

        // Anima atrav�s de todos os est�gios
        for (int i = 1; i < treeGrowthStages.Length; i++)
        {
            yield return new WaitForSeconds(growthStageTime);
            treeSprite.sprite = treeGrowthStages[i];

            // Opcional: Ajusta a escala ou outras propriedades para cada est�gio
            float growthFactor = (float)i / (treeGrowthStages.Length - 1);
            treeSprite.transform.localScale = Vector3.Lerp(
                new Vector3(0.5f, 0.5f, 1f),
                Vector3.one,
                growthFactor
            );
        }
    }

    // M�todo para visualizar o raio de plantio no editor
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, minDistanceBetweenTrees);
    }
}
