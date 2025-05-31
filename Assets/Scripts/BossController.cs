// BossController.cs (v5 - No Internal Trigger)
using UnityEngine;
using System.Collections;
using UnityEngine.Events;

[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
public class BossController : MonoBehaviour
{
    [Header("Configurações Gerais")]
    [SerializeField] private float moveSpeed = 2f;

    [Header("Comportamento de Lixo")]
    [SerializeField] private GameObject trashPrefab;
    [SerializeField] private float trashSpawnInterval = 5f;
    [SerializeField] private float trashSpawnRadius = 3f;
    [SerializeField] private int maxTrashCount = 20;

    [Header("Movimento")]
    [SerializeField] private Transform[] waypoints;
    [SerializeField] private float waypointStopTime = 2f;
    [Tooltip("Se true, o boss para permanentemente ao chegar no último waypoint (além de parar quando a reciclagem encher).")]
    [SerializeField] private bool stopAtLastWaypoint = false;

    [Header("Animação")]
    [SerializeField] private Animator animator;
    [SerializeField] private SpriteRenderer spriteRenderer;
    [Tooltip("Nome do trigger a ser acionado no Animator quando o Boss parar (opcional).")]
    [SerializeField] private string stoppedAnimationTrigger = "";

    // <<< REMOVIDO: Trigger de interação interno não é mais necessário >>>
    // [Header("Interação Final")]
    // [Tooltip("Collider usado para detectar a aproximação do jogador APÓS o boss parar (reciclagem cheia). Deve ser 'Is Trigger'.")]
    // [SerializeField] private Collider2D interactionTrigger;

    [Header("Eventos")]
    public UnityEvent OnBossStopped; // <<< RENOMEADO: Evento genérico de parada

    // Variáveis privadas
    private int currentWaypointIndex = 0;
    private bool isMoving = true;
    private bool hasStoppedPermanently = false;
    private float lastTrashSpawnTime;
    private int currentTrashCount = 0;
    private Coroutine spawnTrashCoroutine;
    private Coroutine waitAtWaypointCoroutine;
    private Rigidbody2D rb2d;

    private void Awake()
    {
        rb2d = GetComponent<Rigidbody2D>();
        if (animator == null) animator = GetComponent<Animator>();
        if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();
        // <<< REMOVIDO: Verificações do interactionTrigger >>>
    }

    private void Start()
    {
        isMoving = true;
        hasStoppedPermanently = false;

        if (waypoints == null || waypoints.Length == 0) { Debug.LogError("BossController: Nenhum waypoint configurado!", this); isMoving = false; }
        if (trashPrefab == null) { Debug.LogError("BossController: Prefab de lixo não configurado!", this); }

        spawnTrashCoroutine = StartCoroutine(SpawnTrashRoutine());
    }

    private void Update()
    {
        if (hasStoppedPermanently) return;
        if (isMoving) { MoveTowardsWaypoint(); }
        UpdateAnimation();
    }

    private void MoveTowardsWaypoint()
    {
        if (waypoints.Length == 0 || !isMoving || hasStoppedPermanently) return;

        Transform targetWaypoint = waypoints[currentWaypointIndex];
        float distance = Vector2.Distance(transform.position, targetWaypoint.position);

        if (distance > 0.1f)
        {
            Vector2 newPos = Vector2.MoveTowards(rb2d.position, targetWaypoint.position, moveSpeed * Time.fixedDeltaTime);
            rb2d.MovePosition(newPos);
            if (spriteRenderer != null && Mathf.Abs(targetWaypoint.position.x - transform.position.x) > 0.01f)
            {
                spriteRenderer.flipX = (targetWaypoint.position.x - transform.position.x) < 0;
            }
        }
        else // Reached waypoint
        {
            bool isLastWaypoint = (currentWaypointIndex == waypoints.Length - 1);
            if (isLastWaypoint && stopAtLastWaypoint)
            {
                Debug.Log("Boss reached the final waypoint. Stopping permanently (option enabled).");
                StopMovingPermanently();
            }
            else
            {
                if (waitAtWaypointCoroutine == null) { waitAtWaypointCoroutine = StartCoroutine(WaitAtWaypoint()); }
            }
        }
    }

    private IEnumerator WaitAtWaypoint()
    {
        isMoving = false;
        rb2d.linearVelocity = Vector2.zero;
        if (animator != null) animator.SetBool("IsMoving", false);
        yield return new WaitForSeconds(waypointStopTime);
        if (!hasStoppedPermanently)
        {
            currentWaypointIndex = (currentWaypointIndex + 1) % waypoints.Length;
            isMoving = true;
        }
        waitAtWaypointCoroutine = null;
    }

    private void UpdateAnimation()
    {
        if (animator == null || hasStoppedPermanently) return;
        Vector2 moveDirection = Vector2.zero;
        bool shouldBeMoving = isMoving && !hasStoppedPermanently;
        if (shouldBeMoving && waypoints.Length > 0)
        {
            Transform targetWaypoint = waypoints[currentWaypointIndex];
            if (Vector2.Distance(transform.position, targetWaypoint.position) > 0.1f)
            {
                moveDirection = (targetWaypoint.position - transform.position).normalized;
            }
        }
        animator.SetFloat("MoveX", moveDirection.x);
        animator.SetFloat("MoveY", moveDirection.y);
        animator.SetBool("IsMoving", shouldBeMoving && moveDirection.sqrMagnitude > 0.01f);
    }

    private IEnumerator SpawnTrashRoutine()
    {
        while (!hasStoppedPermanently)
        {
            yield return new WaitForSeconds(trashSpawnInterval);
            if (isMoving && currentTrashCount < maxTrashCount && !hasStoppedPermanently)
            {
                SpawnTrash();
            }
        }
    }

    private void SpawnTrash()
    {
        if (trashPrefab == null || hasStoppedPermanently) return;
        Vector2 randomOffset = Random.insideUnitCircle * trashSpawnRadius;
        Vector3 spawnPosition = transform.position + new Vector3(randomOffset.x, randomOffset.y, 0);
        GameObject trash = Instantiate(trashPrefab, spawnPosition, Quaternion.identity);
        currentTrashCount++;
    }

    // <<< MODIFICADO: Função simplificada, não precisa mais do parâmetro bool >>>
    public void StopMovingPermanently()
    {
        if (hasStoppedPermanently) return;
        hasStoppedPermanently = true;
        isMoving = false;
        Debug.Log("Boss parando permanentemente.");

        rb2d.linearVelocity = Vector2.zero;
        rb2d.isKinematic = true;
        if (waitAtWaypointCoroutine != null) StopCoroutine(waitAtWaypointCoroutine);
        if (spawnTrashCoroutine != null) StopCoroutine(spawnTrashCoroutine);
        waitAtWaypointCoroutine = null;
        spawnTrashCoroutine = null;

        if (animator != null)
        {
            animator.SetBool("IsMoving", false);
            if (!string.IsNullOrEmpty(stoppedAnimationTrigger))
            {
                animator.SetTrigger(stoppedAnimationTrigger);
            }
        }
        OnBossStopped?.Invoke(); // Dispara evento genérico de parada
    }

    // <<< REMOVIDO: OnTriggerEnter2D não é mais necessário aqui >>>

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, trashSpawnRadius);
    }
}

