using UnityEngine;
using UnityEngine.Events;

public class EnvironmentalManager : MonoBehaviour
{
    public static EnvironmentalManager Instance { get; private set; }

    [Header("Barras de Status")]
    [SerializeField] private float maxTrash = 100f;
    [SerializeField] private float currentTrash = 50f;
    [SerializeField] private float maxRecycling = 100f;
    [SerializeField] private float currentRecycling = 0f;

    [Header("Valores por Ação")]
    [SerializeField] private float trashDecreaseOnCollect = 10f;
    [SerializeField] private float recyclingIncreaseOnCollect = 5f;
    [SerializeField] private float recyclingIncreaseOnPlant = 15f;

    [Header("Eventos")]
    public UnityEvent<float, float> OnTrashUpdated; 
    public UnityEvent<float, float> OnRecyclingUpdated; 

    public UnityEvent OnRecyclingHalfFull; 
    public UnityEvent OnRecyclingFull; 
    public UnityEvent OnTrashCollected; 
    public UnityEvent OnTreePlanted; 

    private bool recyclingHalfFullReached = false;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }
    }

    void Start()
    {
        UpdateTrashUI();
        UpdateRecyclingUI();
    }

    public void CollectTrash()
    {
        currentTrash -= trashDecreaseOnCollect;
        currentTrash = Mathf.Clamp(currentTrash, 0, maxTrash);

        currentRecycling += recyclingIncreaseOnCollect;
        currentRecycling = Mathf.Clamp(currentRecycling, 0, maxRecycling);

        Debug.Log($"Lixo Coletado! Lixo: {currentTrash}/{maxTrash}, Reciclagem: {currentRecycling}/{maxRecycling}");

        UpdateTrashUI();
        UpdateRecyclingUI();

        OnTrashCollected?.Invoke(); 
        CheckRecyclingMilestones();
    }

    public void PlantTree()
    {
        currentRecycling += recyclingIncreaseOnPlant;
        currentRecycling = Mathf.Clamp(currentRecycling, 0, maxRecycling);

        Debug.Log($"Árvore Plantada! Reciclagem: {currentRecycling}/{maxRecycling}");

        UpdateRecyclingUI();

        OnTreePlanted?.Invoke();
        CheckRecyclingMilestones();
    }

    void CheckRecyclingMilestones()
    {
        if (!recyclingHalfFullReached && currentRecycling >= maxRecycling / 2f)
        {
            recyclingHalfFullReached = true;
            Debug.Log("Barra de Reciclagem na metade! Desbloqueando Draco...");
            OnRecyclingHalfFull?.Invoke(); 
        }

        if (currentRecycling >= maxRecycling)
        {
            Debug.Log("Barra de Reciclagem Cheia! Fim de jogo?");
            OnRecyclingFull?.Invoke(); 
        }
    }

    void UpdateTrashUI()
    {
        OnTrashUpdated?.Invoke(currentTrash, maxTrash);
    }

    void UpdateRecyclingUI()
    {
        OnRecyclingUpdated?.Invoke(currentRecycling, maxRecycling);
    }

    public float GetCurrentTrash() => currentTrash;
    public float GetMaxTrash() => maxTrash;
    public float GetCurrentRecycling() => currentRecycling;
    public float GetMaxRecycling() => maxRecycling;
}

