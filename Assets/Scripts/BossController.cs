// BossController.cs (v3 - Remove Hit Trigger, Adiciona Dash Down com par�metro Int 'DashState')
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(Rigidbody2D), typeof(Animator))]
public class BossController : MonoBehaviour
{
    private Rigidbody2D _rb;
    private Animator _animator;

    [Header("Movimento (Controlado por IA)")]
    public float moveSpeed = 5f;
    public float runSpeed = 8f;
    private float _currentSpeed;
    private Vector2 _targetDirection = Vector2.zero;
    private Vector2 _lastMoveDirection = Vector2.right;
    private bool _isMoving = false;
    private bool _isRunning = false;

    [Header("Dash (Controlado por IA)")]
    public float dashSpeed = 15f;
    public float dashDuration = 0.3f;
    public float dashCooldown = 2.0f;
    public int damageOnDash = 10;
    public float dashDetectionRadius = 1.0f;
    public LayerMask playerLayerMask;
    public Transform dashAttackPoint;
    private bool _isDashing = false;
    private float _dashTimer = 0f;
    private float _dashCooldownTimer = 0f;
    private Vector2 _dashDirection = Vector2.right;
    private List<Collider2D> _playersHitThisDash = new List<Collider2D>();
    // <<< ADICIONADO: Constantes para o estado do dash no Animator >>>
    private const int DASH_STATE_NONE = 0;
    private const int DASH_STATE_HORIZONTAL = 1;
    private const int DASH_STATE_DOWN = 2;

    [Header("Vida e Dano")]
    public float maxHealth = 500f;
    public float currentHealth;
    public bool isInvincible = false;
    public float invincibilityDuration = 0.5f;
    public GameObject deathEffectPrefab;
    public bool isDead = false;
    // public BossHealthBar healthBar;

    [Header("Outros (IA)")]
    public Transform playerTarget;

    private int _bossLayer;
    private int _playerLayerValue = -1;

    void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _animator = GetComponent<Animator>();
        _currentSpeed = moveSpeed;
        currentHealth = maxHealth;
        _lastMoveDirection = (Random.value > 0.5f) ? Vector2.right : Vector2.left;

        _bossLayer = gameObject.layer;
        if (playerLayerMask.value != 0)
        {
            for (int i = 0; i < 32; i++)
            {
                if ((playerLayerMask.value & (1 << i)) != 0)
                {
                    _playerLayerValue = i;
                    break;
                }
            }
            if (_playerLayerValue == -1) Debug.LogError("BossController: LayerMask do Jogador (playerLayerMask) n�o configurada corretamente!", this);
        }
        else
        {
            Debug.LogError("BossController: LayerMask do Jogador (playerLayerMask) n�o est� configurada no Inspector!", this);
        }

        if (dashAttackPoint == null)
        {
            dashAttackPoint = transform;
            Debug.LogWarning("BossController: Ponto de Ataque do Dash (dashAttackPoint) n�o configurado, usando o transform do Boss.", this);
        }
        FindPlayer();
    }

    void OnEnable()
    {
        isDead = false;
        currentHealth = maxHealth;
        isInvincible = false;
        _isDashing = false;
        _targetDirection = Vector2.zero;
        if (_rb != null) _rb.linearVelocity = Vector2.zero;
        if (_animator != null)
        {
            _animator.SetInteger("Movimento", 0);
            _animator.SetInteger("DashState", DASH_STATE_NONE); // Garante que come�a sem dash
        }
        // UpdateHealthUI(); 
    }

    void Update()
    {
        if (isDead) return;

        // --- L�GICA DA IA ENTRARIA AQUI ---
        // A IA definiria _targetDirection, _isMoving, _isRunning 
        // e chamaria TryStartDash(Vector2 direction) com a dire��o desejada (ex: Vector2.down)
        if (playerTarget != null && !_isDashing)
        {
            _targetDirection = (playerTarget.position - transform.position).normalized;
            _isMoving = _targetDirection.sqrMagnitude > 0.01f;
        }
        else
        {
            _targetDirection = Vector2.zero;
            _isMoving = false;
            _isRunning = false;
        }
        // --- FIM DA L�GICA DA IA (Exemplo) ---

        HandleCooldowns();
        UpdateAnimation(); // <<< ATEN��O: UpdateAnimation agora N�O controla mais a anima��o de dash >>>
        FlipBasedOnTargetDirection();
    }

    void FixedUpdate()
    {
        if (isDead) return;
        ApplyMovement();
    }

    void HandleCooldowns()
    {
        if (_dashCooldownTimer > 0)
        {
            _dashCooldownTimer -= Time.deltaTime;
        }
    }

    void ApplyMovement()
    {
        if (_isDashing)
        {
            _rb.linearVelocity = _dashDirection * dashSpeed;
            _dashTimer -= Time.fixedDeltaTime;
            if (_dashTimer <= 0)
            {
                EndDash();
            }
        }
        else if (_isMoving)
        {
            _currentSpeed = _isRunning ? runSpeed : moveSpeed;
            _rb.linearVelocity = _targetDirection * _currentSpeed;
            if (_targetDirection.sqrMagnitude > 0.01f)
            {
                _lastMoveDirection = _targetDirection;
            }
        }
        else
        {
            _rb.linearVelocity = Vector2.zero;
        }
    }

    // <<< MODIFICADO: N�o controla mais anima��o de dash, apenas movimento >>>
    void UpdateAnimation()
    {
        // Anima��es de Dash s�o controladas em StartDash/EndDash via par�metro "DashState"
        if (_isDashing) return;

        if (_isMoving)
        {
            bool isHorizontal = Mathf.Abs(_targetDirection.x) >= Mathf.Abs(_targetDirection.y);
            if (_isRunning)
            {
                _animator.SetInteger("Movimento", isHorizontal ? 3 : (_targetDirection.y > 0 ? 6 : 7));
            }
            else
            {
                _animator.SetInteger("Movimento", isHorizontal ? 1 : (_targetDirection.y > 0 ? 4 : 5));
            }
        }
        else
        {
            _animator.SetInteger("Movimento", 0); // Idle
        }
    }
    // --- FIM DA MODIFICA��O ---

    void FlipBasedOnTargetDirection()
    {
        if (_isDashing) return; // N�o vira durante o dash

        float horizontalDirection = _targetDirection.x;
        if (!_isMoving && Mathf.Abs(_lastMoveDirection.x) > 0.01f)
        {
            horizontalDirection = _lastMoveDirection.x;
        }

        if (Mathf.Abs(horizontalDirection) > 0.01f)
        {
            transform.eulerAngles = new Vector2(0f, horizontalDirection > 0 ? 0f : 180f);
        }
    }

    // <<< MODIFICADO: Aceita dire��o do dash como par�metro >>>
    // A IA deve chamar esta fun��o com a dire��o desejada (ex: Vector2.down ou Vector2.left/right)
    public bool TryStartDash(Vector2 direction)
    {
        if (!_isDashing && _dashCooldownTimer <= 0)
        {
            // Normaliza a dire��o para garantir consist�ncia
            Vector2 normalizedDirection = direction.normalized;

            // Decide se � dash horizontal ou para baixo (prioriza baixo se houver componente Y)
            bool isDashDown = Mathf.Abs(normalizedDirection.y) > Mathf.Abs(normalizedDirection.x) && normalizedDirection.y < -0.1f;
            bool isDashHorizontal = !isDashDown; // Assume horizontal se n�o for para baixo

            // Define a dire��o real do dash
            if (isDashDown)
            {
                _dashDirection = Vector2.down;
                StartDash(DASH_STATE_DOWN); // Inicia com estado Dash Down
            }
            else // Dash Horizontal
            {
                // Usa a dire��o que o boss est� virado para o dash horizontal
                _dashDirection = (transform.eulerAngles.y == 0f) ? Vector2.right : Vector2.left;
                StartDash(DASH_STATE_HORIZONTAL); // Inicia com estado Dash Horizontal
            }
            return true;
        }
        return false;
    }
    // --- FIM DA MODIFICA��O ---

    // <<< MODIFICADO: Aceita o estado do dash para o Animator >>>
    void StartDash(int dashState)
    {
        _isDashing = true;
        _dashTimer = dashDuration;
        _dashCooldownTimer = dashCooldown;
        _playersHitThisDash.Clear();

        // Define o estado da anima��o de Dash
        _animator.SetInteger("DashState", dashState);
        // Garante que o par�metro de movimento n�o interfira
        _animator.SetInteger("Movimento", 0);

        if (_playerLayerValue != -1)
        {
            Physics2D.IgnoreLayerCollision(_bossLayer, _playerLayerValue, true);
        }
        DetectAndDamagePlayer();
    }
    // --- FIM DA MODIFICA��O ---

    void DetectAndDamagePlayer()
    {
        Vector2 attackPoint = (dashAttackPoint != null) ? (Vector2)dashAttackPoint.position : (Vector2)transform.position;
        Collider2D[] playersDetected = Physics2D.OverlapCircleAll(attackPoint, dashDetectionRadius, playerLayerMask);

        foreach (Collider2D playerCollider in playersDetected)
        {
            if (playerCollider == null || _playersHitThisDash.Contains(playerCollider)) continue;
            HeartSystem_Universal playerHealth = playerCollider.GetComponent<HeartSystem_Universal>();
            if (playerHealth != null)
            {
                Debug.Log("Boss atingiu jogador com dash!");
                playerHealth.TakeDamage(damageOnDash);
                _playersHitThisDash.Add(playerCollider);
            }
        }
    }

    void EndDash()
    {
        _isDashing = false;
        _rb.linearVelocity = Vector2.zero;
        if (_playerLayerValue != -1)
        {
            Physics2D.IgnoreLayerCollision(_bossLayer, _playerLayerValue, false);
        }
        // <<< ADICIONADO: Reseta o estado do dash no Animator >>>
        _animator.SetInteger("DashState", DASH_STATE_NONE);
        // For�a atualiza��o da anima��o de movimento/idle
        UpdateAnimation();
    }

    public void TakeDamage(float damageAmount)
    {
        if (isInvincible || isDead) return;
        currentHealth -= damageAmount;
        Debug.Log(gameObject.name + " tomou " + damageAmount + " de dano. Vida restante: " + currentHealth);
        // UpdateHealthUI();
        if (currentHealth <= 0)
        {
            currentHealth = 0;
            Die();
        }
        else
        {
            StartCoroutine(InvincibilityCoroutine());
            // <<< REMOVIDO: Trigger de Hit n�o � mais chamado >>>
            // _animator.SetTrigger("Hit"); 
        }
    }

    IEnumerator InvincibilityCoroutine()
    {
        isInvincible = true;
        yield return new WaitForSeconds(invincibilityDuration);
        isInvincible = false;
    }

    void Die()
    {
        if (isDead) return;
        isDead = true;
        Debug.Log(gameObject.name + " morreu!");
        _rb.linearVelocity = Vector2.zero;
        _isMoving = false;
        _isDashing = false;
        this.enabled = false;
        _animator.SetTrigger("Die"); // Mant�m o trigger de morte
        Collider2D[] colliders = GetComponents<Collider2D>();
        foreach (Collider2D col in colliders) col.enabled = false;
        if (deathEffectPrefab != null)
        {
            Instantiate(deathEffectPrefab, transform.position, Quaternion.identity);
        }
        GameEventManager eventManager = FindObjectOfType<GameEventManager>();
        if (eventManager != null)
        {
            // eventManager.HandleBossDefeated(); 
        }
        else
        {
            Debug.LogWarning("N�o foi poss�vel encontrar GameEventManager para notificar morte do Boss.");
        }
        // Destroy(gameObject, 3f);
    }

    public void FindPlayer()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            playerTarget = playerObj.transform;
            Debug.Log("Boss encontrou o jogador: " + playerObj.name);
        }
        else
        {
            Debug.LogWarning("Boss n�o conseguiu encontrar o jogador com a tag 'Player'.");
            playerTarget = null;
        }
    }

    /*
    void UpdateHealthUI()
    {
        if (healthBar != null)
        {
            healthBar.UpdateHealth(currentHealth, maxHealth);
        }
    }
    */
}

