using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(Animator))]
public class AllyController : MonoBehaviour
{
    private Rigidbody2D _allyRigidbody2D;
    private Animator _allyAnimator;

    [Header("Configurações de IA")]
    public string playerTag = "Player"; 
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
    private const float searchInterval = 1.0f; 

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
        targetToFollow = null;
        FindTargetToFollow();
        _moveDirection = Vector2.zero;
        if (_allyRigidbody2D != null) _allyRigidbody2D.linearVelocity = Vector2.zero;
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
            HandleShooting();
            UpdateAnimation();
            FlipBasedOnMovement();
        }
        else
        {
          
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
        if (targetToFollow != null) return; 

        Debug.Log(gameObject.name + ": Procurando por GameObject com tag '" + playerTag + "' ativo e mais próximo...");
        GameObject[] players = GameObject.FindGameObjectsWithTag(playerTag);
        Transform potentialTarget = null;
        float closestDistanceSqr = Mathf.Infinity;

        foreach (GameObject playerObject in players)
        {
            if (playerObject == this.gameObject) continue; 

         
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
        if (targetToFollow == null) 
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

    
    void HandleShooting()
    {
        if (projectilePrefab == null) { if (!loggedNoPrefab) {  loggedNoPrefab = true; } return; }
        loggedNoPrefab = false;

        GameObject nearestEnemy = FindNearestEnemy();
        if (nearestEnemy == null) { if (!loggedNoEnemyFound) { loggedNoEnemyFound = true; } return; }
        loggedNoEnemyFound = false;

        float enemyDistanceSqr = (nearestEnemy.transform.position - transform.position).sqrMagnitude;
        if (enemyDistanceSqr > shootingRange * shootingRange)
        {
            if (!loggedEnemyOutOfRange)
            {

                loggedEnemyOutOfRange = true;
            }
            return; 
        }
        loggedEnemyOutOfRange = false;

        if (Time.time < nextFireTime)
        {
            if (!loggedCooldown)
            {
              
                loggedCooldown = true;
            }
            return; 
        }
        loggedCooldown = false;

      
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
            _allyAnimator.SetInteger("Movimento", 0); 
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

