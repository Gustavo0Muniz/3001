// HenryController.cs (v8 - Adaptado para HeartSystem_Universal)
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

// <<< MODIFICADO: Requer HeartSystem_Universal >>>
[RequireComponent(typeof(Rigidbody2D), typeof(Animator), typeof(HeartSystem_Universal))]
public class HenryController : MonoBehaviour
{
    private Rigidbody2D _playerRigidbody2D;
    private Animator _playerAnimator;
    // <<< MODIFICADO: Referência para HeartSystem_Universal >>>
    private HeartSystem_Universal _heartSystem;

    [Header("Movimento")]
    public float _playerSpeed;
    private float _playerInitialSpeed;
    public float _playerRunSpeed;

    [Header("Tiro")]
    public GameObject projectilePrefab;
    public Transform firePoint;
    public float fireRate = 2f;
    public float projectileSpeed = 10f;
    private float _nextFireTime = 0f;
    private Vector2 _lastShootDirection = Vector2.right;

    [Header("Habilidade Especial (Onda Sonora)")]
    public GameObject soundWavePrefab;
    public Transform specialAbilityOrigin;
    public float soundWaveCooldown = 5f;
    public float soundWaveSpeed = 5f;
    public float soundWaveGrowthRate = 1.5f;
    public float soundWaveDamage = 50f;
    public float soundWaveMaxLifetime = 4f;
    private float _nextSoundWaveTime = 0f;

    [Header("Animação Morte")]
    public string deathAnimationStateName = "Player_die";
    private bool isDead = false; // Estado local para controlar ações

    private Vector2 _playerDirection;
    private Vector2 _lastMoveDirection = Vector2.right;
    private bool _isRunning = false;

    void Awake()
    {
        _playerRigidbody2D = GetComponent<Rigidbody2D>();
        _playerAnimator = GetComponent<Animator>();
        // <<< MODIFICADO: Pega HeartSystem_Universal >>>
        _heartSystem = GetComponent<HeartSystem_Universal>();

        _playerInitialSpeed = _playerSpeed;
        _lastMoveDirection = Vector2.right;
        _lastShootDirection = Vector2.right;

        isDead = false;

        // <<< MODIFICADO: Mensagem de erro para HeartSystem_Universal >>>
        if (_heartSystem == null) Debug.LogError("HeartSystem_Universal não encontrado no GameObject de Henry!", this);
        // ... (outras verificações de Awake) ...
        if (projectilePrefab == null) Debug.LogError("Projectile Prefab não configurado!", this);
        if (firePoint == null)
        {
            firePoint = transform.Find("FirePoint");
            if (firePoint == null)
            {
                GameObject fp = new GameObject("FirePoint");
                fp.transform.SetParent(transform);
                fp.transform.localPosition = new Vector3(0.5f, 0, 0);
                firePoint = fp.transform;
                Debug.LogWarning("FirePoint não configurado, criando um padrão.", this);
            }
        }
        if (soundWavePrefab == null) Debug.LogError("Sound Wave Prefab não configurado!", this);
        if (specialAbilityOrigin == null) { specialAbilityOrigin = transform; }
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
        // Vida é resetada no OnEnable do HeartSystem_Universal se necessário
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
        // Verifica se morreu NESTE frame
        if (!isDead && _heartSystem != null && _heartSystem.IsDead)
        {
            TriggerDeathSequence();
        }

        // Só processa input e movimento se não estiver morto
        if (isDead || !enabled) return;

        HandleInput();
        UpdateAnimation();
        FlipBasedOnLastMoveDirection();
        UpdateFirePointPosition();
    }

    // ... (FixedUpdate, HandleInput, HandleRunInput, HandleShootInput, HandleSpecialAbilityInput, ApplyMovement, UpdateAnimation, FlipBasedOnLastMoveDirection, Shoot, UseSoundWave, DetermineShootDirection, CalculateProjectileRotation, UpdateFirePointPosition - SEM ALTERAÇÕES) ...

    void FixedUpdate()
    {
        if (isDead || !enabled) return;
        ApplyMovement();
    }

    void HandleInput()
    {
        _playerDirection = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
        if (_playerDirection.sqrMagnitude > 0.01f)
        {
            _lastMoveDirection = _playerDirection.normalized;
        }
        HandleRunInput();
        HandleShootInput();
        HandleSpecialAbilityInput();
    }

    void HandleRunInput()
    {
        if (Input.GetKeyDown(KeyCode.LeftShift)) { _playerSpeed = _playerRunSpeed; _isRunning = true; }
        if (Input.GetKeyUp(KeyCode.LeftShift)) { _playerSpeed = _playerInitialSpeed; _isRunning = false; }
        if (!Input.GetKey(KeyCode.LeftShift) && _isRunning) { _playerSpeed = _playerInitialSpeed; _isRunning = false; }
    }

    void HandleShootInput()
    {
        if (Input.GetMouseButtonDown(0) && Time.time >= _nextFireTime)
        {
            Shoot();
            _nextFireTime = Time.time + 1f / fireRate;
        }
    }

    void HandleSpecialAbilityInput()
    {
        if (Input.GetMouseButtonDown(1) && Time.time >= _nextSoundWaveTime)
        {
            UseSoundWave();
            _nextSoundWaveTime = Time.time + soundWaveCooldown;
        }
    }

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
            int animState = currentMoveIsHorizontal ? (_isRunning ? 3 : 1) : (_playerDirection.y > 0 ? (_isRunning ? 6 : 4) : (_isRunning ? 7 : 5));
            _playerAnimator.SetInteger("Movimento", animState);
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

    void Shoot()
    {
        if (projectilePrefab == null || firePoint == null) return;
        Vector2 shootDirection = DetermineShootDirection();
        _lastShootDirection = shootDirection;
        Quaternion projectileRotation = CalculateProjectileRotation(shootDirection);
        GameObject projectile = Instantiate(projectilePrefab, firePoint.position, projectileRotation);
        Rigidbody2D rb = projectile.GetComponent<Rigidbody2D>();
        if (rb != null) rb.linearVelocity = shootDirection * projectileSpeed;
        else Debug.LogWarning("Prefab do Projétil não tem Rigidbody2D!", projectile);
    }

    void UseSoundWave()
    {
        if (soundWavePrefab == null || specialAbilityOrigin == null) return;
        Debug.Log("Henry usando Onda Sonora!");
        Vector2 waveDirection = DetermineShootDirection();
        GameObject soundWaveObject = Instantiate(soundWavePrefab, specialAbilityOrigin.position, Quaternion.identity);
        SoundWave waveScript = soundWaveObject.GetComponent<SoundWave>();
        if (waveScript != null)
        {
            waveScript.Initialize(waveDirection, soundWaveSpeed, soundWaveGrowthRate, soundWaveDamage, soundWaveMaxLifetime);
        }
        else
        {
            Debug.LogError("Prefab da Onda Sonora não tem o script 'SoundWave.cs'!", soundWaveObject);
            Destroy(soundWaveObject);
            _nextSoundWaveTime = Time.time;
            return;
        }
    }

    Vector2 DetermineShootDirection()
    {
        Vector2 direction;
        if (_playerDirection.sqrMagnitude > 0.01f)
        {
            if (Mathf.Abs(_playerDirection.x) > Mathf.Abs(_playerDirection.y)) direction = new Vector2(Mathf.Sign(_playerDirection.x), 0);
            else if (Mathf.Abs(_playerDirection.y) > Mathf.Abs(_playerDirection.x)) direction = new Vector2(0, Mathf.Sign(_playerDirection.y));
            else direction = transform.right.normalized;
        }
        else
        {
            if (Mathf.Abs(_lastMoveDirection.x) > Mathf.Abs(_lastMoveDirection.y)) direction = new Vector2(Mathf.Sign(_lastMoveDirection.x), 0);
            else if (Mathf.Abs(_lastMoveDirection.y) > Mathf.Abs(_lastMoveDirection.x)) direction = new Vector2(0, Mathf.Sign(_lastMoveDirection.y));
            else direction = transform.right.normalized;
        }
        return direction;
    }

    Quaternion CalculateProjectileRotation(Vector2 shootDirection)
    {
        if (Mathf.Abs(shootDirection.y) >= Mathf.Abs(shootDirection.x))
        {
            return shootDirection.y > 0 ? Quaternion.Euler(0, 0, 0) : Quaternion.Euler(0, 0, 180);
        }
        else
        {
            return shootDirection.x > 0 ? Quaternion.Euler(0, 0, -90) : Quaternion.Euler(0, 0, 90);
        }
    }

    void UpdateFirePointPosition()
    {
        if (firePoint == null) return;
        Vector2 primaryDirection = DetermineShootDirection();
        if (primaryDirection.x > 0.5f) firePoint.localPosition = new Vector3(0.5f, 0f, 0f);
        else if (primaryDirection.x < -0.5f) firePoint.localPosition = new Vector3(-0.5f, 0f, 0f);
        else if (primaryDirection.y > 0.5f) firePoint.localPosition = new Vector3(0f, 0.6f, 0f);
        else if (primaryDirection.y < -0.5f) firePoint.localPosition = new Vector3(0f, -0.6f, 0f);
        firePoint.localEulerAngles = Vector3.zero;
    }


    // --- MODIFICADO: Lógica de Dano e Morte --- 

    // REMOVIDO: ApplyDamage - Dano é aplicado diretamente ao HeartSystem_Universal
    // public void ApplyDamage(int damageAmount) { ... }

    // Método chamado internamente quando HeartSystem_Universal.IsDead se torna true
    void TriggerDeathSequence()
    {
        if (isDead) return; // Já está morrendo

        isDead = true;
        Debug.Log(gameObject.name + " iniciando sequência de morte (detectado por HenryController)!");

        // 1. Para movimento e física
        _playerRigidbody2D.linearVelocity = Vector2.zero;
        _playerRigidbody2D.simulated = false;

        // 2. Desabilita controle e colisores
        this.enabled = false; // Desabilita este script para parar inputs e updates
        Collider2D playerCollider = GetComponent<Collider2D>();
        if (playerCollider != null) playerCollider.enabled = false;

        // 3. Toca animação de morte
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

    // Coroutine para destruir após animação (sem mudanças)
    IEnumerator DestroyAfterAnimation()
    {
        yield return new WaitForSeconds(0.1f);
        float waitTime = 1.5f;
        try
        {
            if (_playerAnimator != null && _playerAnimator.GetCurrentAnimatorStateInfo(0).IsName(deathAnimationStateName))
            {
                waitTime = _playerAnimator.GetCurrentAnimatorStateInfo(0).length;
                Debug.Log("Esperando " + waitTime + " segundos pela animação de morte ('" + deathAnimationStateName + "').");
            }
            else
            {
                Debug.LogWarning("Estado de animação de morte ('" + deathAnimationStateName + "') não encontrado ou não está tocando. Usando tempo fixo: " + waitTime + "s.");
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

    // REMOVIDO: OnCollisionEnter2D e OnTriggerEnter2D - Dano é aplicado por quem colide
    // void OnCollisionEnter2D(Collision2D collision) { ... }
    // void OnTriggerEnter2D(Collider2D other) { ... }

    // --- FIM DA MODIFICAÇÃO ---
}

