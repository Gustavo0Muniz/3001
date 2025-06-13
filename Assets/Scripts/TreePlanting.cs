using UnityEngine;
using System.Collections;

public class TreePlanting : MonoBehaviour
{
    [Header("Configurações de Plantio")]
    [SerializeField] private GameObject treePrefab; 
    [SerializeField] private float plantingCooldown = 2f; 
    [SerializeField] private float minDistanceBetweenTrees = 3f; 

    [Header("Animação de Crescimento")]
    [SerializeField] private Sprite[] treeGrowthStages; 
    [SerializeField] private float growthStageTime = 0.5f; 

    [Header("Efeitos")]
    [SerializeField] private GameObject plantEffect; 
    [SerializeField] private AudioClip plantSound; 

    private float lastPlantTime = -10f; 
    private Transform playerTransform;
    private LayerMask treeLayer; 

    private void Start()
    {
        playerTransform = transform;

   
        treeLayer = LayerMask.GetMask("Tree");

        if (treePrefab == null)
        {
            Debug.LogError("TreePlanting: Prefab da árvore não configurado!", this);
            enabled = false;
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.O) && CanPlant())
        {
            PlantTree();
        }
    }

    private bool CanPlant()
    {
        if (Time.time - lastPlantTime < plantingCooldown)
            return false;

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
        lastPlantTime = Time.time;

        Vector3 plantPosition = playerTransform.position;
        GameObject newTree = Instantiate(treePrefab, plantPosition, Quaternion.identity);

        if (treeGrowthStages != null && treeGrowthStages.Length > 0)
        {
            StartCoroutine(AnimateTreeGrowth(newTree.GetComponent<SpriteRenderer>()));
        }

        if (EnvironmentalManager.Instance != null)
        {
            EnvironmentalManager.Instance.PlantTree();
        }
        else
        {
            Debug.LogError("TreePlanting: EnvironmentalManager não encontrado!", this);
        }

        if (plantEffect != null)
        {
            Instantiate(plantEffect, plantPosition, Quaternion.identity);
        }

        if (plantSound != null)
        {
            AudioSource.PlayClipAtPoint(plantSound, plantPosition);
        }
    }

    private IEnumerator AnimateTreeGrowth(SpriteRenderer treeSprite)
    {
        if (treeSprite == null)
            yield break;

        treeSprite.sprite = treeGrowthStages[0];

        for (int i = 1; i < treeGrowthStages.Length; i++)
        {
            yield return new WaitForSeconds(growthStageTime);
            treeSprite.sprite = treeGrowthStages[i];

            float growthFactor = (float)i / (treeGrowthStages.Length - 1);
            treeSprite.transform.localScale = Vector3.Lerp(
                new Vector3(0.5f, 0.5f, 1f),
                Vector3.one,
                growthFactor
            );
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, minDistanceBetweenTrees);
    }
}
