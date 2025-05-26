using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(Animator))]
public class AllyController : MonoBehaviour
{
    private Rigidbody2D _allyRigidbody2D;
    private Animator _allyAnimator;

    [Header("Configurações de IA")]
    public string playerTag = "Player"; // Tag para identificar os jogadores a seguir
    public float moveSpeed = 3f;
    public float stopDistance = 2f;
    public float shootingRange = 5f;
    public GameObject projectilePrefab;
    public float fireRate = 1f;
    private float nextFireTime = 0f;

    private Transform targetToFollow = null;
    private Vector2 _moveDirection;
    private Vector2 _lastMoveDirection = Vector2.right;

    private float timeSinceLastSearch = 0f;
    private const float searchInterval = 1.0f; // Buscar a cada 1 segundo se sem alvo

    // Flags de log (mantidas para tiro)
    private bool loggedNoPrefab = false;
    private bool loggedNoEnemyFound = false;
    private bool loggedEnemyOutOfRange = false;
    private bool loggedCooldown = false;

    void Awake()
    {
        _allyRigidbody2D = GetComponent<Rigidbody2D>();
        _allyAnimator = GetComponent<Animator>();
        Debug.Log("AllyController Awake: " + gameObject.name);
    }

    void OnEnable()
    {
        Debug.Log("AllyController Enabled: " + gameObject.name);
        targetToFollow = null; // Garante que buscará um novo alvo ao ser ativado
        FindTargetToFollow(); // Tenta encontrar imediatamente
        _moveDirection = Vector2.zero;
        if (_allyRigidbody2D != null) _allyRigidbody2D.linearVelocity = Vector2.zero;
        // Resetar logs
        loggedNoPrefab = false;
        loggedNoEnemyFound = false;
        loggedEnemyOutOfRange = false;
        loggedCooldown = false;
    }

    void OnDisable()
    {
        Debug.Log("AllyController Disabled: " + gameObject.name);
        _moveDirection = Vector2.zero;
        if (_allyRigidbody2D != null) _allyRigidbody2D.linearVelocity = Vector2.zero;
        if (_allyAnimator != null) _allyAnimator.SetInteger("Movimento", 0);
        targetToFollow = null;
    }

    void Update()
    {
        if (!enabled) return;

        ValidateCurrentTarget();

        if (targetToFollow == null)
        {
            timeSinceLastSearch += Time.deltaTime;
            if (timeSinceLastSearch >= searchInterval)
            {
                FindTargetToFollow();
                timeSinceLastSearch = 0f;
            }
        }

        if (targetToFollow != null)
        {
            CalculateMovement();
            HandleShooting(); // A lógica de tiro continua independente do alvo seguido
            UpdateAnimation();
            FlipBasedOnMovement();
        }
        else
        {
            // Garante estado Idle se não houver alvo
            if (_allyAnimator != null) _allyAnimator.SetInteger("Movimento", 0);
            _moveDirection = Vector2.zero;
        }
    }

    void FixedUpdate()
    {
        if (!enabled || targetToFollow == null)
        {
            if (_allyRigidbody2D != null) _allyRigidbody2D.linearVelocity = Vector2.zero;
            return;
        }

        if (_moveDirection.sqrMagnitude > 0.01f)
        {
            if (_allyRigidbody2D != null) _allyRigidbody2D.linearVelocity = _moveDirection.normalized * moveSpeed;
        }
        else
        {
            if (_allyRigidbody2D != null) _allyRigidbody2D.linearVelocity = Vector2.zero;
        }
    }

    void ValidateCurrentTarget()
    {
        if (targetToFollow != null)
        {
            // Verifica apenas se o GameObject do alvo está inativo
            // A verificação da tag é feita na busca, não precisamos revalidar a cada frame
            if (!targetToFollow.gameObject.activeInHierarchy)
            {
                Debug.LogWarning(gameObject.name + " perdeu o alvo: " + targetToFollow.name + ". Motivo: GameObject inativo.");
                targetToFollow = null;
                _moveDirection = Vector2.zero;
                if (_allyRigidbody2D != null) _allyRigidbody2D.linearVelocity = Vector2.zero;
            }
        }
    }

    void FindTargetToFollow()
    {
        if (targetToFollow != null) return; // Já tem um alvo válido

        Debug.Log(gameObject.name + ": Procurando por GameObject com tag '" + playerTag + "' ativo e mais próximo...");
        GameObject[] players = GameObject.FindGameObjectsWithTag(playerTag);
        Transform potentialTarget = null;
        float closestDistanceSqr = Mathf.Infinity;

        foreach (GameObject playerObject in players)
        {
            if (playerObject == this.gameObject) continue; // Ignora a si mesmo

            // Verifica apenas se o GameObject está ativo (a tag já foi filtrada por FindGameObjectsWithTag)
            if (playerObject.activeInHierarchy)
            {
                float distanceSqr = (playerObject.transform.position - transform.position).sqrMagnitude;
                if (distanceSqr < closestDistanceSqr)
                {
                    closestDistanceSqr = distanceSqr;
                    potentialTarget = playerObject.transform;
                }
            }
            else
            {
                // Log opcional para saber quem foi ignorado (embora FindGameObjectsWithTag geralmente só retorne ativos)
                // Debug.Log("Ignorando GameObject inativo com tag '" + playerTag + "': " + playerObject.name);
            }
        }

        if (potentialTarget != null)
        {
            targetToFollow = potentialTarget;
            Debug.Log(gameObject.name + " encontrou e definiu novo alvo (mais próximo com tag '" + playerTag + "'): " + targetToFollow.name);
        }
        else
        {
            Debug.LogWarning(gameObject.name + ": Não encontrou nenhum GameObject ativo com a tag '" + playerTag + "' para seguir.");
        }
    }

    void CalculateMovement()
    {
        if (targetToFollow == null) // Checagem extra de segurança
        {
            _moveDirection = Vector2.zero;
            return;
        }

        float distance = Vector2.Distance(transform.position, targetToFollow.position);
        if (distance > stopDistance)
        {
            _moveDirection = (targetToFollow.position - transform.position).normalized;
            _lastMoveDirection = _moveDirection;
        }
        else
        {
            _moveDirection = Vector2.zero;
        }
    }

    // --- Funções HandleShooting, FindNearestEnemy, ShootAt, UpdateAnimation, FlipBasedOnMovement --- 
    // (Mantidas como estavam, pois a lógica de tiro é baseada na tag "Enemy", não no alvo seguido)

    void HandleShooting()
    {
        if (projectilePrefab == null) { if (!loggedNoPrefab) { /* Debug.Log(gameObject.name + ": Prefab de projétil não definido."); */ loggedNoPrefab = true; } return; }
        loggedNoPrefab = false;

        GameObject nearestEnemy = FindNearestEnemy();
        if (nearestEnemy == null) { if (!loggedNoEnemyFound) { /* Debug.Log(gameObject.name + ": Nenhum inimigo encontrado no alcance."); */ loggedNoEnemyFound = true; } return; }
        loggedNoEnemyFound = false;

        float enemyDistanceSqr = (nearestEnemy.transform.position - transform.position).sqrMagnitude;
        if (enemyDistanceSqr > shootingRange * shootingRange)
        {
            if (!loggedEnemyOutOfRange)
            {
                // Debug.Log(gameObject.name + ": Inimigo " + nearestEnemy.name + " fora de alcance.");
                loggedEnemyOutOfRange = true;
            }
            return; // Inimigo fora do alcance
        }
        loggedEnemyOutOfRange = false;

        if (Time.time < nextFireTime)
        {
            if (!loggedCooldown)
            {
                // Debug.Log(gameObject.name + ": Aguardando cooldown para atirar.");
                loggedCooldown = true;
            }
            return; // Em cooldown
        }
        loggedCooldown = false;

        // Debug.Log(gameObject.name + ": ATIRANDO em " + nearestEnemy.name + "!");
        ShootAt(nearestEnemy.transform.position);
        nextFireTime = Time.time + 1f / fireRate;

        loggedNoEnemyFound = false;
        loggedEnemyOutOfRange = false;
        loggedCooldown = false;
    }

    GameObject FindNearestEnemy()
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        GameObject nearest = null;
        float minDistanceSqr = shootingRange * shootingRange;
        Vector2 currentPosition = transform.position;

        foreach (GameObject enemy in enemies)
        {
            if (enemy == null || !enemy.activeInHierarchy) continue;
            float distanceSqr = (currentPosition - (Vector2)enemy.transform.position).sqrMagnitude;
            if (distanceSqr < minDistanceSqr)
            {
                minDistanceSqr = distanceSqr;
                nearest = enemy;
            }
        }
        return nearest;
    }

    void ShootAt(Vector2 targetPosition)
    {
        Vector2 direction = (targetPosition - (Vector2)transform.position).normalized;
        if (direction.sqrMagnitude < 0.01f) return;

        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        Instantiate(projectilePrefab, transform.position, Quaternion.Euler(0, 0, angle));
    }

    void UpdateAnimation()
    {
        if (_allyAnimator == null) return;
        if (_moveDirection.sqrMagnitude > 0.01f)
        {
            bool currentMoveIsHorizontal = Mathf.Abs(_moveDirection.x) >= Mathf.Abs(_moveDirection.y);
            _allyAnimator.SetInteger("Movimento", currentMoveIsHorizontal ? 1 : (_moveDirection.y > 0 ? 4 : 5));
        }
        else
        {
            _allyAnimator.SetInteger("Movimento", 0); // Idle
        }
    }

    void FlipBasedOnMovement()
    {
        Vector2 directionToFlip = _moveDirection.sqrMagnitude > 0.01f ? _moveDirection : _lastMoveDirection;
        if (Mathf.Abs(directionToFlip.x) > 0.01f)
        {
            if (Mathf.Abs(directionToFlip.x) >= Mathf.Abs(directionToFlip.y) * 0.5f)
            {
                transform.eulerAngles = new Vector2(0f, directionToFlip.x > 0 ? 0f : 180f);
            }
        }
    }
}

