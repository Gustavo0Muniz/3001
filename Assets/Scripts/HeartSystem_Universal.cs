// HeartSystem_Universal.cs (Funciona com PlayerController ou HenryController)
using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class HeartSystem_Universal : MonoBehaviour
{
    // Não precisa mais de referência direta ao controlador aqui
    // private PlayerController playerControllerRef;
    // private HenryController henryControllerRef;

    [Header("Vida")]
    public int maxHealth = 5;
    [HideInInspector] // Esconde no inspector, mas é pública para leitura
    public int currentHealth;
    public bool isInvincible = false;
    public float invincibilityDuration = 1f;
    public bool IsDead { get { return currentHealth <= 0; } } // Propriedade para verificar se está morto

    [Header("UI Config")]
    public Image[] coracao;
    public Sprite cheio;
    public Sprite vazio;

    private bool uiInitialized = false;

    void Awake()
    {
        // Inicializa a vida aqui
        currentHealth = maxHealth;
        isInvincible = false;
    }

    void Start()
    {
        // Tenta inicializar a UI no Start
        InitializeUI();
    }

    void OnEnable()
    {
        // Garante que a vida seja resetada se o objeto for reativado
        // (Pode ser melhor controlar isso externamente, ex: ao respawnar)
        // currentHealth = maxHealth;
        // isInvincible = false;
        // Atualiza a UI ao ser reativado
        if (uiInitialized) UpdateHealthUI();
        else InitializeUI();
    }

    // Tenta configurar a UI
    void InitializeUI()
    {
        if (coracao == null || coracao.Length == 0 || cheio == null || vazio == null)
        {
            Debug.LogWarning("HeartSystem_Universal: Configuração da UI incompleta (Array Coracao ou Sprites Cheio/Vazio).", this.gameObject);
            uiInitialized = false;
            return;
        }
        uiInitialized = true;
        UpdateHealthUI(); // Atualiza a UI com os valores iniciais
    }

    // Método chamado para aplicar dano (por inimigos, projéteis, etc.)
    public void TakeDamage(int damageAmount)
    {
        if (isInvincible || IsDead) return; // Não toma dano se invencível ou morto

        currentHealth -= damageAmount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth); // Garante que a vida não fique negativa

        Debug.Log(gameObject.name + " tomou " + damageAmount + " de dano via HeartSystem_Universal. Vida restante: " + currentHealth);

        // Atualiza a UI imediatamente após tomar dano
        UpdateHealthUI();

        if (currentHealth <= 0)
        {
            // Apenas registra a morte. O controlador (Player/Henry) deve verificar IsDead e reagir.
            Debug.Log("HeartSystem_Universal detectou morte para " + gameObject.name);
        }
        else
        {
            // Inicia período de invencibilidade
            StartCoroutine(InvincibilityCoroutine());
            // O controlador pode ser responsável pelo feedback visual
        }
    }

    // Atualiza a interface gráfica dos corações
    void UpdateHealthUI()
    {
        if (!uiInitialized) return; // Não atualiza se a UI não estiver pronta

        for (int i = 0; i < coracao.Length; i++)
        {
            // Garante que o índice está dentro dos limites da vida máxima real
            if (i < maxHealth)
            {
                coracao[i].enabled = true;
                if (i < currentHealth)
                {
                    coracao[i].sprite = cheio;
                }
                else
                {
                    coracao[i].sprite = vazio;
                }
            }
            else
            {
                // Esconde corações extras se maxHealth for menor que o array
                coracao[i].enabled = false;
            }
        }
    }

    // Coroutine de invencibilidade
    IEnumerator InvincibilityCoroutine()
    {
        isInvincible = true;
        // O controlador (Player/Henry) pode iniciar feedback visual aqui
        yield return new WaitForSeconds(invincibilityDuration);
        isInvincible = false;
        // O controlador pode parar o feedback visual aqui
    }

    // Método para curar (opcional)
    public void Heal(int healAmount)
    {
        if (IsDead) return; // Não pode curar se estiver morto

        currentHealth += healAmount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
        UpdateHealthUI();
        Debug.Log(gameObject.name + " curou " + healAmount + ". Vida atual: " + currentHealth);
    }

    // Método para definir a vida máxima (opcional, útil para upgrades)
    public void SetMaxHealth(int newMaxHealth, bool restoreFullHealth = true)
    {
        maxHealth = Mathf.Max(1, newMaxHealth); // Garante pelo menos 1 de vida máxima
        if (restoreFullHealth)
        {
            currentHealth = maxHealth;
        }
        else
        {
            currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth); // Ajusta vida atual se necessário
        }
        InitializeUI(); // Reajusta a UI para a nova vida máxima
    }
}

