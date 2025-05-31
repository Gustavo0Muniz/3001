// DracoController.cs (v2 - Com Tiro Especial)
// Combina movimento do PlayerController original com tiro do HenryController
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(Rigidbody2D), typeof(Animator), typeof(HeartSystem_Universal))]
public class DracoController : MonoBehaviour
{
    private Rigidbody2D _playerRigidbody2D;
    private Animator _playerAnimator;
    private HeartSystem_Universal _heartSystem;

    [Header("Movimento")]
    public float _playerSpeed;
    private float _playerInitialSpeed;
    public float _playerRunSpeed;

    [Header("Tiro Normal")]
    public GameObject projectilePrefab; // <<< Atribua o prefab do projétil normal de Draco
    public Transform firePoint;         // <<< Atribua o ponto de onde o tiro sai (usado por ambos os tiros)
    public float fireRate = 1.5f;         // <<< Cadência de tiro normal (ex: mais lenta)
    public float projectileSpeed = 12f; // <<< Velocidade do projétil normal (ex: mais rápido)
    private float _nextFireTime = 0f;

    // <<< NOVO: Habilidade Especial >>>
    [Header("Habilidade Especial (Tiro Forte)")]
    public GameObject specialProjectilePrefab; // <<< Atribua o prefab do projétil especial de Draco
    public float specialProjectileSpeed = 18f; // <<< Velocidade do projétil especial de Draco
    public float specialShotCooldown = 6f;    // <<< Tempo de recarga da habilidade especial de Draco
    private float _nextSpecialShotTime = 0f;
    // <<< FIM NOVO >>>

    [Header("Animação Morte")]
    public string deathAnimationStateName = "Player_die";
    private bool isDead = false;

    // Variáveis internas de estado
    private Vector2 _playerDirection;
    private Vector2 _lastMoveDirection = Vector2.right;
    private Vector2 _lastShootDirection = Vector2.right;
    private bool _isRunning = false;

    void Awake()
    {
        _playerRigidbody2D = GetComponent<Rigidbody2D>();
        _playerAnimator = GetComponent<Animator>();
        _heartSystem = GetComponent<HeartSystem_Universal>();

        _playerInitialSpeed = _playerSpeed;
        _lastMoveDirection = Vector2.right;
        _lastShootDirection = Vector2.right;
        isDead = false;

        if (_heartSystem == null) Debug.LogError("HeartSystem_Universal não encontrado no GameObject de Draco!", this);

        // Verificações para o sistema de tiro normal
        if (projectilePrefab == null) Debug.LogError("Projectile Prefab (Normal) de Draco não configurado!", this);
        if (firePoint == null)
        {
            firePoint = transform.Find("FirePoint");
            if (firePoint == null)
            {
                GameObject fp = new GameObject("FirePoint");
                fp.transform.SetParent(transform);
                fp.transform.localPosition = new Vector3(0.5f, 0, 0);
                firePoint = fp.transform;
                Debug.LogWarning("FirePoint de Draco não configurado, criando um padrão.", this);
            }
        }
        // <<< NOVO: Verificação para tiro especial >>>
        if (specialProjectilePrefab == null) Debug.LogError("Special Projectile Prefab de Draco não configurado!", this);
        // <<< FIM NOVO >>>
    }

    void OnEnable()
    {
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
        UpdateFirePointPosition();
    }

    void FixedUpdate()
    {
        if (isDead || !enabled) return;
        ApplyMovement();
    }

    void HandleInput()
    {
        _playerDirection = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
        if (_playerDirection.sqrMagnitude > 0.01f) _lastMoveDirection = _playerDirection.normalized;

        HandleRunInput();
        HandleShootInput();         // Input do tiro normal
        HandleSpecialShootInput(); // <<< NOVO: Input do tiro especial
    }

    void HandleRunInput()
    {
        if (Input.GetKeyDown(KeyCode.LeftShift)) { _playerSpeed = _playerRunSpeed; _isRunning = true; }
        if (Input.GetKeyUp(KeyCode.LeftShift)) { _playerSpeed = _playerInitialSpeed; _isRunning = false; }
        if (!Input.GetKey(KeyCode.LeftShift) && _isRunning) { _playerSpeed = _playerInitialSpeed; _isRunning = false; }
    }

    void HandleShootInput()
    {
        // Tiro normal com Botão Direito (Mouse 1) - Como definido anteriormente para Draco
        if (Input.GetKeyDown(KeyCode.P) && Time.time >= _nextFireTime)

        {
            Shoot(false); // Chama Shoot indicando que NÃO é especial
            _nextFireTime = Time.time + 1f / fireRate;
        }
    }

    // <<< NOVO: Input para Habilidade Especial >>>
    void HandleSpecialShootInput()
    {
        // Tiro especial com a tecla E (ou outra de sua escolha)
        if (Input.GetKeyDown(KeyCode.I) && Time.time >= _nextSpecialShotTime)
        {
            Shoot(true); // Chama Shoot indicando que É especial
            _nextSpecialShotTime = Time.time + specialShotCooldown;
        }
    }
    // <<< FIM NOVO >>>

    void ApplyMovement()
    {
        _playerRigidbody2D.linearVelocity = _playerDirection.normalized * _playerSpeed;
    }

    void UpdateAnimation()
    {
        if (isDead) return;
        if (_playerDirection.sqrMagnitude > 0.01f)
        {
            bool currentMoveIsHorizontal = Mathf.Abs(_playerDirection.x) >= Mathf.Abs(_playerDirection.y);
            if (_isRunning)
                _playerAnimator.SetInteger("Movimento", currentMoveIsHorizontal ? 3 : (_playerDirection.y > 0 ? 6 : 7));
            else
                _playerAnimator.SetInteger("Movimento", currentMoveIsHorizontal ? 1 : (_playerDirection.y > 0 ? 4 : 5));
        }
        else
        {
            _playerAnimator.SetInteger("Movimento", 0);
        }
    }

    void FlipBasedOnLastMoveDirection()
    {
        if (isDead) return;
        if (Mathf.Abs(_lastMoveDirection.x) > 0.01f)
        {
            if (Mathf.Abs(_lastMoveDirection.x) >= Mathf.Abs(_lastMoveDirection.y))
            {
                transform.eulerAngles = new Vector2(0f, _lastMoveDirection.x > 0 ? 0f : 180f);
            }
        }
    }

    // --- Lógica de Tiro Unificada --- <<< MODIFICADO >>>
    void Shoot(bool isSpecial)
    {
        GameObject prefabToShoot = isSpecial ? specialProjectilePrefab : projectilePrefab;
        float speed = isSpecial ? specialProjectileSpeed : projectileSpeed;

        if (prefabToShoot == null || firePoint == null)
        {
            Debug.LogError($"Prefab ou FirePoint nulo para tiro {(isSpecial ? "especial" : "normal")} de Draco!");
            return;
        }

        Vector2 shootDirection = DetermineShootDirection();
        _lastShootDirection = shootDirection;
        Quaternion projectileRotation = CalculateProjectileRotation(shootDirection);

        GameObject projectile = Instantiate(prefabToShoot, firePoint.position, projectileRotation);
        Rigidbody2D rb = projectile.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.linearVelocity = shootDirection * speed;
        }
        else
        {
            Debug.LogWarning($"Prefab do Projétil {(isSpecial ? "especial" : "normal")} de Draco não tem Rigidbody2D!", projectile);
        }
    }
    // <<< FIM MODIFICADO >>>

    Vector2 DetermineShootDirection()
    {
        Vector2 direction;
        if (_playerDirection.sqrMagnitude > 0.01f)
        {
            if (Mathf.Abs(_playerDirection.x) > Mathf.Abs(_playerDirection.y))
                direction = new Vector2(Mathf.Sign(_playerDirection.x), 0);
            else
                direction = new Vector2(0, Mathf.Sign(_playerDirection.y));
        }
        else
        {
            if (Mathf.Abs(_lastMoveDirection.x) > Mathf.Abs(_lastMoveDirection.y))
                direction = new Vector2(Mathf.Sign(_lastMoveDirection.x), 0);
            else
                direction = new Vector2(0, Mathf.Sign(_lastMoveDirection.y));
        }
        if (direction == Vector2.zero) direction = (transform.eulerAngles.y == 0f) ? Vector2.right : Vector2.left;
        return direction.normalized;
    }

    Quaternion CalculateProjectileRotation(Vector2 shootDirection)
    {
        if (Mathf.Abs(shootDirection.y) > Mathf.Abs(shootDirection.x))
        {
            return shootDirection.y > 0 ? Quaternion.Euler(0, 0, 90) : Quaternion.Euler(0, 0, -90);
        }
        else
        {
            return shootDirection.x > 0 ? Quaternion.Euler(0, 0, 0) : Quaternion.Euler(0, 0, 180);
        }
    }

    void UpdateFirePointPosition()
    {
        if (firePoint == null) return;
        Vector2 primaryDirection = DetermineShootDirection();
        float offsetX = 0.5f;
        float offsetY = 0.6f;
        if (Mathf.Abs(primaryDirection.x) > 0.5f)
        {
            firePoint.localPosition = new Vector3(primaryDirection.x > 0 ? offsetX : -offsetX, 0.1f, 0f);
        }
        else if (Mathf.Abs(primaryDirection.y) > 0.5f)
        {
            firePoint.localPosition = new Vector3(0f, primaryDirection.y > 0 ? offsetY : -offsetY, 0f);
        }
        firePoint.localEulerAngles = Vector3.zero;
    }

    // --- Lógica de Morte (sem alterações) ---
    void TriggerDeathSequence()
    {
        if (isDead) return;
        isDead = true;
        Debug.Log(gameObject.name + " iniciando sequência de morte!");
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

