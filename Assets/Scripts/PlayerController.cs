using UnityEngine;
using System.Collections.Generic; // Necessário para List<Collider2D>

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
    private Vector2 _lastMoveDirection; // Última direção de input válida
    private Vector2 _dashAttackDirection; // Direção do dash/ataque (sempre horizontal)
    private bool _isAttack = false;
    private bool _isRunning = false;
    private bool _isHorizontalMove = true; // True se o movimento atual ou a intenção é horizontal
    private bool _isDashing = false;
    private float _dashTimer = 0f;
    private float _dashCooldownTimer = 0f;

    [Header("Configurações de Ataque/Dash com Dano")]
    public float danoDoDash = 25f;
    public float raioDeteccaoDash = 0.7f;
    public LayerMask camadaDoInimigo;
    public Transform pontoDeAtaqueDash;
    private List<Collider2D> _inimigosAtingidosNoDash;
    private int _playerLayer;
    private int _enemyLayerValue = -1;

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
        _lastMoveDirection = Vector2.right;
        _dashAttackDirection = Vector2.right; // Padrão inicial
        _inimigosAtingidosNoDash = new List<Collider2D>();

        _playerLayer = gameObject.layer;
        if (camadaDoInimigo.value != 0)
        {
            int layerValue = camadaDoInimigo.value;
            int layerNumber = 0;
            while ((layerValue & 1) == 0 && layerNumber < 31)
            {
                layerValue >>= 1;
                layerNumber++;
            }
            if ((layerValue & 1) == 1)
            {
                _enemyLayerValue = layerNumber;
            }
            else
            {
                Debug.LogError("Camada do Inimigo não configurada corretamente ou está vazia!", this);
            }
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
        if (_dashCooldownTimer > 0)
        {
            _dashCooldownTimer -= Time.deltaTime;
        }

        if (!_isDashing) // Captura input de direção apenas se não estiver em dash
        {
            _playerDirection = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
            if (_playerDirection.sqrMagnitude > 0.01f) // Se houve input significativo
            {
                _lastMoveDirection = _playerDirection.normalized;
                // Determina se o movimento pretendido é mais horizontal
                _isHorizontalMove = Mathf.Abs(_playerDirection.x) > Mathf.Abs(_playerDirection.y);
            }
            else
            {
                // Se não há input, considera que a intenção de movimento horizontal depende da última direção horizontal
                // ou assume horizontal se estava parado e _lastMoveDirection já é horizontal.
                // Para o dash, _isHorizontalMove será verificado no HandleAttackInput com base na _playerDirection atual.
                // Se parado, _playerDirection é (0,0), então _isHorizontalMove pode não ser o esperado.
                // Vamos recalcular _isHorizontalMove baseado em _lastMoveDirection se não houver input atual.
                if (Mathf.Abs(_lastMoveDirection.x) > Mathf.Abs(_lastMoveDirection.y))
                {
                    _isHorizontalMove = true;
                }
                else
                {
                    _isHorizontalMove = false;
                }
            }
        }

        // A animação de movimento normal só atualiza se não estiver atacando/dando dash
        if (!_isAttack && !_isDashing)
        {
            UpdateMovementAnimation();
        }

        // Flip é feito baseado na última direção de movimento, apenas se não estiver em dash
        if (!_isDashing)
        {
            FlipBasedOnLastMoveDirection();
        }

        PlayerRun();
        HandleAttackInput();
    }

    void BotBehaviorUpdate()
    {
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

        if (!_isAttack && !_isDashing)
        {
            UpdateMovementAnimation();
        }
        if (!_isDashing)
        {
            FlipBasedOnLastMoveDirection();
        }
    }

    void FixedUpdate()
    {
        if (_isDashing)
        {
            _playerRigidbody2D.linearVelocity = _dashAttackDirection * _dashSpeed;
            _dashTimer -= Time.fixedDeltaTime;
            if (_dashTimer <= 0)
            {
                EndDash();
            }
        }
        else if (!_isAttack)
        {
            _playerRigidbody2D.linearVelocity = _playerDirection.normalized * _playerSpeed;
        }
        else if (_isAttack && !_isDashing)
        {
            _playerRigidbody2D.linearVelocity = Vector2.zero;
        }
    }

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
        // _isHorizontalMove é atualizado em PlayerInputUpdate com base no input.
        // Aqui, usamos o _playerDirection para as animações de walk/run.
        if (_playerDirection.sqrMagnitude > 0.01f) // Se há input de movimento
        {
            bool currentMoveIsHorizontal = Mathf.Abs(_playerDirection.x) > Mathf.Abs(_playerDirection.y);
            if (_isRunning)
            {
                _playerAnimator.SetInteger("Movimento", currentMoveIsHorizontal ? 3 : (_playerDirection.y > 0 ? 6 : 7));
            }
            else
            {
                _playerAnimator.SetInteger("Movimento", currentMoveIsHorizontal ? 1 : (_playerDirection.y > 0 ? 4 : 5));
            }
        }
        else // Sem input de movimento, animação de Idle
        {
            _playerAnimator.SetInteger("Movimento", 0);
        }
    }

    // Renomeado para ser mais específico
    void FlipBasedOnLastMoveDirection()
    {
        if (Mathf.Abs(_lastMoveDirection.x) > 0.01f) // Apenas flipa se houver uma direção horizontal em _lastMoveDirection
        {
            if (_lastMoveDirection.x > 0.01f)
            {
                transform.eulerAngles = new Vector2(0f, 0f);
            }
            else if (_lastMoveDirection.x < -0.01f)
            {
                transform.eulerAngles = new Vector2(0f, 180f);
            }
        }
        // Se _lastMoveDirection.x é (próximo de) zero, não flipa, mantém a orientação atual.
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

    void HandleAttackInput()
    {
        // Verifica se o input atual (_playerDirection) é predominantemente horizontal OU se não há input (parado)
        // Se parado, o dash será na direção que o player está olhando (_lastMoveDirection ou flip)
        bool canDashHorizontally = false;
        if (_playerDirection.sqrMagnitude > 0.01f) // Se há input
        {
            canDashHorizontally = Mathf.Abs(_playerDirection.x) > Mathf.Abs(_playerDirection.y);
        }
        else // Se parado, permite dash horizontal baseado na direção que está olhando
        {
            canDashHorizontally = true;
        }

        if ((Input.GetKeyDown(KeyCode.LeftControl) || Input.GetMouseButtonDown(0)) &&
            !_isDashing &&
            _dashCooldownTimer <= 0 &&
            canDashHorizontally) // Condição para dash/ataque horizontal
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

        // Determina a direção do dash (sempre horizontal)
        if (Mathf.Abs(_lastMoveDirection.x) > 0.01f) // Se _lastMoveDirection tem componente horizontal
        {
            _dashAttackDirection = new Vector2(Mathf.Sign(_lastMoveDirection.x), 0f).normalized;
        }
        else // Se _lastMoveDirection é vertical ou zero, usa a direção do flip atual do personagem
        {
            _dashAttackDirection = (transform.eulerAngles.y == 0f) ? Vector2.right : Vector2.left;
        }

        // Ativa a animação de ataque/dash (Movimento = 2, conforme script original do usuário)
        _playerAnimator.SetInteger("Movimento", 2);

        _inimigosAtingidosNoDash.Clear();

        if (_enemyLayerValue != -1)
        {
            Physics2D.IgnoreLayerCollision(_playerLayer, _enemyLayerValue, true);
        }

        Vector2 attackPoint = (pontoDeAtaqueDash != null) ? (Vector2)pontoDeAtaqueDash.position : (Vector2)transform.position;
        // Para o OverlapCircle, a direção do dash é importante para o posicionamento do ponto de ataque se ele for um offset
        // Se pontoDeAtaqueDash é o próprio transform, a direção do dash não afeta o centro do círculo aqui.
        // Se o pontoDeAtaqueDash for um filho à frente do player, ele já estará na direção correta devido ao Flip.
        Collider2D[] inimigosDetectados = Physics2D.OverlapCircleAll(attackPoint, raioDeteccaoDash, camadaDoInimigo);

        foreach (Collider2D inimigoCollider in inimigosDetectados)
        {
            Inimigo inimigo = inimigoCollider.GetComponent<Inimigo>();
            if (inimigo != null && !_inimigosAtingidosNoDash.Contains(inimigoCollider))
            {
                Debug.Log("PlayerController: Atacando " + inimigo.name + " com dash.");
                inimigo.ReceberDano(danoDoDash);
                _inimigosAtingidosNoDash.Add(inimigoCollider);
            }
        }
    }

    void EndDash()
    {
        _isDashing = false;
        _isAttack = false;
        _playerRigidbody2D.linearVelocity = Vector2.zero;

        if (_enemyLayerValue != -1)
        {
            Physics2D.IgnoreLayerCollision(_playerLayer, _enemyLayerValue, false);
        }
        // A animação será atualizada por UpdateMovementAnimation() no próximo Update se não houver novo dash/ataque
    }
}

