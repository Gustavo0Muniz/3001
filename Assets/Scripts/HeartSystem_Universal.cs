using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class HeartSystem_Universal : MonoBehaviour
{
    

    [Header("Vida")]
    public int maxHealth = 5;
    [HideInInspector]
    public int currentHealth;
    public bool isInvincible = false;
    public float invincibilityDuration = 1f;
    public bool IsDead { get { return currentHealth <= 0; } } 

    [Header("UI Config")]
    public Image[] coracao;
    public Sprite cheio;
    public Sprite vazio;

    private bool uiInitialized = false;

    void Awake()
    {
        currentHealth = maxHealth;
        isInvincible = false;
    }

    void Start()
    {
        InitializeUI();
    }

    void OnEnable()
    {
       
        if (uiInitialized) UpdateHealthUI();
        else InitializeUI();
    }
    void InitializeUI()
    {
        if (coracao == null || coracao.Length == 0 || cheio == null || vazio == null)
        {
            Debug.LogWarning("HeartSystem_Universal: Configuração da UI incompleta (Array Coracao ou Sprites Cheio/Vazio).", this.gameObject);
            uiInitialized = false;
            return;
        }
        uiInitialized = true;
        UpdateHealthUI();
    }
    public void TakeDamage(int damageAmount)
    {
        if (isInvincible || IsDead) return; 

        currentHealth -= damageAmount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth); 

        Debug.Log(gameObject.name + " tomou " + damageAmount + " de dano via HeartSystem_Universal. Vida restante: " + currentHealth);

        UpdateHealthUI();

        if (currentHealth <= 0)
        {
           
            Debug.Log("HeartSystem_Universal detectou morte para " + gameObject.name);
        }
        else
        {
  
            StartCoroutine(InvincibilityCoroutine());
        }
    }

    void UpdateHealthUI()
    {
        if (!uiInitialized) return; 

        for (int i = 0; i < coracao.Length; i++)
        {
 
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
                coracao[i].enabled = false;
            }
        }
    }

    IEnumerator InvincibilityCoroutine()
    {
        isInvincible = true;
        yield return new WaitForSeconds(invincibilityDuration);
        isInvincible = false;
    }

    public void Heal(int healAmount)
    {
        if (IsDead) return; 

        currentHealth += healAmount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
        UpdateHealthUI();
        Debug.Log(gameObject.name + " curou " + healAmount + ". Vida atual: " + currentHealth);
    }

    public void SetMaxHealth(int newMaxHealth, bool restoreFullHealth = true)
    {
        maxHealth = Mathf.Max(1, newMaxHealth);
        if (restoreFullHealth)
        {
            currentHealth = maxHealth;
        }
        else
        {
            currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
        }
        InitializeUI(); 
    }
}

