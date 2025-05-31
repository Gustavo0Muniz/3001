using UnityEngine;
using UnityEngine.Events;

// Gerencia os valores globais de Lixo e Reciclagem e eventos relacionados.
public class EnvironmentalManager : MonoBehaviour
{
    // Singleton para fácil acesso
    public static EnvironmentalManager Instance { get; private set; }

    [Header("Barras de Status")]
    [SerializeField] private float maxTrash = 100f;
    [SerializeField] private float currentTrash = 50f; // Começa com algum lixo
    [SerializeField] private float maxRecycling = 100f;
    [SerializeField] private float currentRecycling = 0f;

    [Header("Valores por Ação")]
    [SerializeField] private float trashDecreaseOnCollect = 10f;
    [SerializeField] private float recyclingIncreaseOnCollect = 5f;
    [SerializeField] private float recyclingIncreaseOnPlant = 15f;

    [Header("Eventos")]
    // Eventos para a UI atualizar as barras
    public UnityEvent<float, float> OnTrashUpdated; // (current, max)
    public UnityEvent<float, float> OnRecyclingUpdated; // (current, max)

    // Eventos para outras lógicas do jogo
    public UnityEvent OnRecyclingHalfFull; // Para desbloquear Draco
    public UnityEvent OnRecyclingFull; // Para fim de jogo?
    public UnityEvent OnTrashCollected; // Para spawnar inimigos / danificar boss
    public UnityEvent OnTreePlanted; // Apenas para feedback, se necessário

    private bool recyclingHalfFullReached = false;

    void Awake()
    {
        // Configuração do Singleton
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
            // DontDestroyOnLoad(gameObject); // Descomente se precisar persistir entre cenas
        }
    }

    void Start()
    {
        // Garante que a UI seja atualizada no início
        UpdateTrashUI();
        UpdateRecyclingUI();
    }

    // Chamado quando o jogador coleta lixo
    public void CollectTrash()
    {
        currentTrash -= trashDecreaseOnCollect;
        currentTrash = Mathf.Clamp(currentTrash, 0, maxTrash);

        currentRecycling += recyclingIncreaseOnCollect;
        currentRecycling = Mathf.Clamp(currentRecycling, 0, maxRecycling);

        Debug.Log($"Lixo Coletado! Lixo: {currentTrash}/{maxTrash}, Reciclagem: {currentRecycling}/{maxRecycling}");

        UpdateTrashUI();
        UpdateRecyclingUI();

        OnTrashCollected?.Invoke(); // Notifica outros sistemas (Boss, Inimigos)
        CheckRecyclingMilestones();
    }

    // Chamado quando o jogador planta uma árvore
    public void PlantTree()
    {
        currentRecycling += recyclingIncreaseOnPlant;
        currentRecycling = Mathf.Clamp(currentRecycling, 0, maxRecycling);

        Debug.Log($"Árvore Plantada! Reciclagem: {currentRecycling}/{maxRecycling}");

        UpdateRecyclingUI();

        OnTreePlanted?.Invoke();
        CheckRecyclingMilestones();
    }

    // Verifica marcos importantes da barra de reciclagem
    void CheckRecyclingMilestones()
    {
        // Verifica se chegou na metade (e ainda não tinha chegado antes)
        if (!recyclingHalfFullReached && currentRecycling >= maxRecycling / 2f)
        {
            recyclingHalfFullReached = true;
            Debug.Log("Barra de Reciclagem na metade! Desbloqueando Draco...");
            OnRecyclingHalfFull?.Invoke(); // Notifica para desbloquear Draco
        }

        // Verifica se chegou ao máximo
        if (currentRecycling >= maxRecycling)
        {
            Debug.Log("Barra de Reciclagem Cheia! Fim de jogo?");
            OnRecyclingFull?.Invoke(); // Notifica fim de jogo ou próximo passo
        }
    }

    // Funções para atualizar a UI
    void UpdateTrashUI()
    {
        OnTrashUpdated?.Invoke(currentTrash, maxTrash);
    }

    void UpdateRecyclingUI()
    {
        OnRecyclingUpdated?.Invoke(currentRecycling, maxRecycling);
    }

    // Funções para obter os valores atuais (se necessário por outros scripts)
    public float GetCurrentTrash() => currentTrash;
    public float GetMaxTrash() => maxTrash;
    public float GetCurrentRecycling() => currentRecycling;
    public float GetMaxRecycling() => maxRecycling;
}

