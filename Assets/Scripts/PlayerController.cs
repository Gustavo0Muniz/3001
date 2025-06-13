using UnityEngine;
using System.Collections.Generic;
using System.Collections; 

[RequireComponent(typeof(Rigidbody2D), typeof(Animator), typeof(HeartSystem_Universal))]
public class PlayerController : MonoBehaviour
{
    private Rigidbody2D _playerRigidbody2D;
    private Animator _playerAnimator;
    private HeartSystem_Universal _heartSystem;

    [Header("Movimento")]
    public float _playerSpeed;
    private float _playerInitialSpeed;
    public float _playerRunSpeed;

    [Header("Dash/Ataque")]
    public float _dashSpeed = 10f;
    public float _dashDuration = 0.2f;
    public float _dashCooldown = 0.5f;
    public float danoDoDash = 25f;
    public float raioDeteccaoDash = 0.7f;
    public LayerMask camadaDoInimigo;
    public Transform pontoDeAtaqueDash;
    public string deathAnimationStateName = "Player_die"; 
    private Vector2 _playerDirection;
    private Vector2 _lastMoveDirection = Vector2.right;
    private Vector2 _dashAttackDirection;
    private bool _isAttack = false;
    private bool _isRunning = false;
    private bool _isDashing = false;
    private float _dashTimer = 0f;
    private float _dashCooldownTimer = 0f;
    private List<Collider2D> _inimigosAtingidosNoDash = new List<Collider2D>();
    private int _playerLayer;
    private int _enemyLayerValue = -1;
    private bool isDead = false;

    void Awake()
    {
        _playerRigidbody2D = GetComponent<Rigidbody2D>();
        _playerAnimator = GetComponent<Animator>();
        _heartSystem = GetComponent<HeartSystem_Universal>();

        _playerInitialSpeed = _playerSpeed;
        _lastMoveDirection = Vector2.right;
        _dashAttackDirection = Vector2.right;
        isDead = false;

        if (_heartSystem == null) Debug.LogError("HeartSystem_Universal não encontrado no GameObject!", this);

        _playerLayer = gameObject.layer;
        if (camadaDoInimigo.value != 0)
        {
            for (int i = 0; i < 32; i++)
            {
                if ((camadaDoInimigo.value & (1 << i)) != 0)
                {
                    _enemyLayerValue = i;
                    break;
                }
            }
            if (_enemyLayerValue == -1) Debug.LogError("Camada do Inimigo não configurada corretamente!", this);
        }
        else
        {
            Debug.LogError("Camada do Inimigo (camadaDoInimigo) não está configurada no Inspector!", this);
        }

        if (pontoDeAtaqueDash == null)
        {
            pontoDeAtaqueDash = transform;
            Debug.LogWarning("PontoDeAtaqueDash não configurado, usando o transform do Player.", this);
        }
    }

    void OnEnable()
    {
        _isDashing = false;
        _isAttack = false;
        _isRunning = false;
        _playerSpeed = _playerInitialSpeed;
        _playerDirection = Vector2.zero;
        if (_playerRigidbody2D != null) _playerRigidbody2D.linearVelocity = Vector2.zero;
        if (_playerAnimator != null)
        {
            _playerAnimator.SetInteger("Movimento", 0);
            _playerAnimator.SetBool("IsDead", false);
        }
        isDead = false;
        this.enabled = true;
        Collider2D playerCollider = GetComponent<Collider2D>();
        if (playerCollider != null) playerCollider.enabled = true;
        if (_playerRigidbody2D != null) _playerRigidbody2D.simulated = true;
    }

    void OnDisable()
    {
        if (!isDead)
        {
            _isDashing = false;
            _isAttack = false;
            _isRunning = false;
            _playerDirection = Vector2.zero;
            if (_playerRigidbody2D != null) _playerRigidbody2D.linearVelocity = Vector2.zero;
            if (_playerAnimator != null) _playerAnimator.SetInteger("Movimento", 0);
        }
    }


    void Update()
    {
        if (!isDead && _heartSystem != null && _heartSystem.IsDead)
        {
            TriggerDeathSequence();
        }

        if (isDead || !enabled) return;

        HandleInput();
        UpdateAnimation();
        FlipBasedOnLastMoveDirection();
    }

   
    void FixedUpdate()
    {
        if (isDead || !enabled) return;
        ApplyMovement();
    }

    void HandleInput()
    {
        if (_dashCooldownTimer > 0) _dashCooldownTimer -= Time.deltaTime;
        if (!_isDashing)
        {
            _playerDirection = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
            if (_playerDirection.sqrMagnitude > 0.01f) _lastMoveDirection = _playerDirection.normalized;
        }
        else _playerDirection = Vector2.zero;
        HandleRunInput();
        HandleAttackInput();
    }

    void HandleRunInput()
    {
        if (Input.GetKeyDown(KeyCode.LeftShift) && !_isDashing) { _playerSpeed = _playerRunSpeed; _isRunning = true; }
        if (Input.GetKeyUp(KeyCode.LeftShift) && !_isDashing) { _playerSpeed = _playerInitialSpeed; _isRunning = false; }
        if (!Input.GetKey(KeyCode.LeftShift) && _isRunning) { _playerSpeed = _playerInitialSpeed; _isRunning = false; }
    }

    void HandleAttackInput()
    {
        bool canDashHorizontally = true;
        if (_playerDirection.sqrMagnitude > 0.01f) canDashHorizontally = Mathf.Abs(_playerDirection.x) >= Mathf.Abs(_playerDirection.y);
        if ((Input.GetKeyDown(KeyCode.LeftControl) || Input.GetMouseButtonDown(0)) && !_isDashing && _dashCooldownTimer <= 0 && canDashHorizontally)
        {
            StartDash();
        }
    }

    void ApplyMovement()
    {
        if (_isDashing)
        {
            _playerRigidbody2D.linearVelocity = _dashAttackDirection * _dashSpeed;
            _dashTimer -= Time.fixedDeltaTime;
            if (_dashTimer <= 0) EndDash();
        }
        else if (!_isAttack)
        {
            _playerRigidbody2D.linearVelocity = _playerDirection.normalized * _playerSpeed;
        }
        else if (_isAttack)
        {
            _playerRigidbody2D.linearVelocity = Vector2.zero;
        }
    }

    void UpdateAnimation()
    {
        if (_isDashing || isDead) return;
        if (_playerDirection.sqrMagnitude > 0.01f)
        {
            bool currentMoveIsHorizontal = Mathf.Abs(_playerDirection.x) >= Mathf.Abs(_playerDirection.y);
            if (_isRunning) _playerAnimator.SetInteger("Movimento", currentMoveIsHorizontal ? 3 : (_playerDirection.y > 0 ? 6 : 7));
            else _playerAnimator.SetInteger("Movimento", currentMoveIsHorizontal ? 1 : (_playerDirection.y > 0 ? 4 : 5));
        }
        else _playerAnimator.SetInteger("Movimento", 0);
    }

    void FlipBasedOnLastMoveDirection()
    {
        if (isDead) return;
        if (Mathf.Abs(_lastMoveDirection.x) > 0.01f)
        {
            transform.eulerAngles = new Vector2(0f, _lastMoveDirection.x > 0 ? 0f : 180f);
        }
    }

    void StartDash()
    {
        _isAttack = true;
        _isDashing = true;
        _dashTimer = _dashDuration;
        _dashCooldownTimer = _dashCooldown;
        _dashAttackDirection = (transform.eulerAngles.y == 0f) ? Vector2.right : Vector2.left;
        _playerAnimator.SetInteger("Movimento", 2);
        _inimigosAtingidosNoDash.Clear();
        if (_enemyLayerValue != -1) Physics2D.IgnoreLayerCollision(_playerLayer, _enemyLayerValue, true);
        DetectAndDamageEnemies();
    }

    void DetectAndDamageEnemies()
    {
        Vector2 attackPoint = (pontoDeAtaqueDash != null) ? (Vector2)pontoDeAtaqueDash.position : (Vector2)transform.position;
        Collider2D[] inimigosDetectados = Physics2D.OverlapCircleAll(attackPoint, raioDeteccaoDash, camadaDoInimigo);
        foreach (Collider2D inimigoCollider in inimigosDetectados)
        {
            if (inimigoCollider == null || _inimigosAtingidosNoDash.Contains(inimigoCollider)) continue;
            HeartSystem_Universal enemyHealth = inimigoCollider.GetComponent<HeartSystem_Universal>();
            if (enemyHealth != null)
            {
                enemyHealth.TakeDamage((int)danoDoDash);
                _inimigosAtingidosNoDash.Add(inimigoCollider);
            }
            else
            {
        
            }
        }
    }

    void EndDash()
    {
        _isDashing = false;
        _isAttack = false;
        _playerRigidbody2D.linearVelocity = Vector2.zero;
        if (_enemyLayerValue != -1) Physics2D.IgnoreLayerCollision(_playerLayer, _enemyLayerValue, false);
        UpdateAnimation();
    }

    void TriggerDeathSequence()
    {
        if (isDead) return;
        isDead = true;
        Debug.Log(gameObject.name + " iniciando sequência de morte (detectado por PlayerController)!");

        _playerRigidbody2D.linearVelocity = Vector2.zero;
        _playerRigidbody2D.simulated = false;
        this.enabled = false;
        Collider2D playerCollider = GetComponent<Collider2D>();
        if (playerCollider != null) playerCollider.enabled = false;

        if (_playerAnimator != null)
        {
            _playerAnimator.SetBool("IsDead", true);
            StartCoroutine(DestroyAfterAnimation());
        }
        else
        {
            Debug.LogError("Animator não encontrado para animação de morte! Destruindo imediatamente.", gameObject);
            Destroy(gameObject);
        }
    }

    IEnumerator DestroyAfterAnimation()
    {
        yield return new WaitForSeconds(0.1f);
        float waitTime = 1.5f;
        try
        {
            if (_playerAnimator != null && _playerAnimator.GetCurrentAnimatorStateInfo(0).IsName(deathAnimationStateName))
            {
                waitTime = _playerAnimator.GetCurrentAnimatorStateInfo(0).length;
                Debug.Log("Esperando " + waitTime + " segundos pela animação de morte (" + deathAnimationStateName + ").");
            }
            else
            {
                Debug.LogWarning("Estado de animação de morte (" + deathAnimationStateName + ") não encontrado ou não está tocando. Usando tempo fixo: " + waitTime + "s.");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError("Erro ao obter duração da animação de morte: " + e.Message + ". Usando tempo fixo: " + waitTime + "s.");
        }
        yield return new WaitForSeconds(waitTime);
        Debug.Log("Animação de morte concluída (ou tempo esgotado). Destruindo " + gameObject.name);
        Destroy(gameObject);
    }
}

