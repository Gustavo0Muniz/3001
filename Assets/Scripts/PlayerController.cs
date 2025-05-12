using UnityEngine;


public class PlayerController : MonoBehaviour
{
    private Rigidbody2D _playerRigidbody2D;
    private Animator _playerAnimator;

    public float _playerSpeed;
    private float _playerInitialSpeed;
    public float _playerRunSpeed;
    public float _dashSpeed = 10f;
    public float _dashDuration = 0.2f;
    public float _dashCooldown = 0.5f;

    private Vector2 _playerDirection;
    private Vector2 _lastMoveDirection;
    private bool _isAttack = false;
    private bool _isRunning = false;
    private bool _isHorizontalMove = true;
    private bool _isDashing = false;
    private float _dashTimer = 0f;
    private float _dashCooldownTimer = 0f;

    // Variáveis para controle de bot
    public bool isActive = true;
    public float followDistance = 2f;
    public float shootingRange = 5f;
    public GameObject projectilePrefab;
    public float fireRate = 1f;
    private float nextFireTime = 0f;

    void Start()
    {
        _playerRigidbody2D = GetComponent<Rigidbody2D>();
        _playerAnimator = GetComponent<Animator>();
        _playerInitialSpeed = _playerSpeed;
        _lastMoveDirection = Vector2.right; // Direção padrão inicial
    }

    void Update()
    {
        if (isActive)
        {
            PlayerInputUpdate();
        }
        else
        {
            BotBehaviorUpdate();
        }
    }

    void PlayerInputUpdate()
    {
        // Atualiza timers
        if (_dashCooldownTimer > 0)
        {
            _dashCooldownTimer -= Time.deltaTime;
        }

        // Captura input apenas se não estiver em dash
        if (!_isDashing)
        {
            _playerDirection = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));

            // Guarda a última direção de movimento válida
            if (_playerDirection.sqrMagnitude > 0)
            {
                _lastMoveDirection = _playerDirection.normalized;
            }
        }

        if (!_isAttack && !_isDashing)
        {
            UpdateMovementAnimation();
        }

        if (_isHorizontalMove && !_isDashing)
        {
            Flip();
        }

        PlayerRun();
        OnAttack();

        if (_isAttack)
        {
            _playerAnimator.SetInteger("Movimento", 2);
        }
    }

    void BotBehaviorUpdate()
    {
        // Seguir o personagem ativo
        GameObject activePlayer = FindActivePlayer();
        if (activePlayer != null && activePlayer != gameObject)
        {
            float distance = Vector2.Distance(transform.position, activePlayer.transform.position);

            if (distance > followDistance)
            {
                _playerDirection = (activePlayer.transform.position - transform.position).normalized;
                _lastMoveDirection = _playerDirection;
            }
            else
            {
                _playerDirection = Vector2.zero;
            }
        }

        // Atirar em inimigos próximos
        GameObject nearestEnemy = FindNearestEnemy();
        if (nearestEnemy != null)
        {
            float distanceToEnemy = Vector2.Distance(transform.position, nearestEnemy.transform.position);

            if (distanceToEnemy <= shootingRange && Time.time >= nextFireTime)
            {
                ShootAt(nearestEnemy.transform.position);
                nextFireTime = Time.time + 1f / fireRate;
            }
        }

        // Atualizar animações do bot
        if (!_isAttack && !_isDashing)
        {
            UpdateMovementAnimation();
        }

        if (_isHorizontalMove && !_isDashing)
        {
            Flip();
        }
    }

    void FixedUpdate()
    {
        if (_isDashing)
        {
            // Movimento durante o dash
            _playerRigidbody2D.linearVelocity = _lastMoveDirection * _dashSpeed;

            // Atualiza timer do dash
            _dashTimer -= Time.fixedDeltaTime;
            if (_dashTimer <= 0)
            {
                EndDash();
            }
        }
        else if (!_isAttack)
        {
            // Movimento normal ou do bot
            _playerRigidbody2D.linearVelocity = _playerDirection.normalized * _playerSpeed;
        }
    }

    // Métodos auxiliares para o comportamento do bot
    GameObject FindActivePlayer()
    {
        PlayerController[] players = FindObjectsOfType<PlayerController>();
        foreach (PlayerController player in players)
        {
            if (player.isActive && player != this)
            {
                return player.gameObject;
            }
        }
        return null;
    }

    GameObject FindNearestEnemy()
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        GameObject nearest = null;
        float minDistance = Mathf.Infinity;

        foreach (GameObject enemy in enemies)
        {
            float distance = Vector2.Distance(transform.position, enemy.transform.position);
            if (distance < minDistance)
            {
                minDistance = distance;
                nearest = enemy;
            }
        }

        return nearest;
    }

    void ShootAt(Vector2 targetPosition)
    {
        Vector2 direction = (targetPosition - (Vector2)transform.position).normalized;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        Instantiate(projectilePrefab, transform.position, Quaternion.Euler(0, 0, angle));
    }

    void UpdateMovementAnimation()
    {
        if (_playerDirection.sqrMagnitude > 0)
        {
            _isHorizontalMove = Mathf.Abs(_playerDirection.x) > Mathf.Abs(_playerDirection.y);

            if (_isRunning)
            {
                if (_isHorizontalMove)
                {
                    _playerAnimator.SetInteger("Movimento", 3); // Run horizontal
                }
                else
                {
                    _playerAnimator.SetInteger("Movimento", _playerDirection.y > 0 ? 6 : 7); // 6 = runup, 7 = rundown
                }
            }
            else
            {
                if (_isHorizontalMove)
                {
                    _playerAnimator.SetInteger("Movimento", 1); // Walk horizontal
                }
                else
                {
                    _playerAnimator.SetInteger("Movimento", _playerDirection.y > 0 ? 4 : 5); // 4 = walkup, 5 = walkdown
                }
            }
        }
        else
        {
            _playerAnimator.SetInteger("Movimento", 0); // Idle
            _isHorizontalMove = true;
        }
    }

    void Flip()
    {
        if (_playerDirection.x > 0)
        {
            transform.eulerAngles = new Vector2(0f, 0f); // Direita
        }
        else if (_playerDirection.x < 0)
        {
            transform.eulerAngles = new Vector2(0f, 180f); // Esquerda
        }
    }

    void PlayerRun()
    {
        if (Input.GetKeyDown(KeyCode.LeftShift) && !_isDashing)
        {
            _playerSpeed = _playerRunSpeed;
            _isRunning = true;
        }
        if (Input.GetKeyUp(KeyCode.LeftShift) && !_isDashing)
        {
            _playerSpeed = _playerInitialSpeed;
            _isRunning = false;
        }
    }

    void OnAttack()
    {
        if ((Input.GetKeyDown(KeyCode.LeftControl) || Input.GetMouseButtonDown(0)) &&
            !_isDashing &&
            _dashCooldownTimer <= 0 &&
            _isHorizontalMove) // <-- Nova condição adicionada
        {
            StartDash();
        }
    }

    void StartDash()
    {
        _isAttack = true;
        _isDashing = true;
        _dashTimer = _dashDuration;
        _dashCooldownTimer = _dashCooldown;
        _playerAnimator.SetTrigger("Dash"); // Adicione um trigger de dash no seu Animator se quiser uma anima��o espec�fica
    }

    void EndDash()
    {
        _isDashing = false;
        _isAttack = false;
        _playerRigidbody2D.linearVelocity = Vector2.zero;
    }
    
}