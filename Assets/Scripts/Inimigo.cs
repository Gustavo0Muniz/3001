using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

[RequireComponent(typeof(Rigidbody2D), typeof(SpriteRenderer), typeof(Animator))]
public class Inimigo : MonoBehaviour
{
    public float vidaMaxima = 100f;
    public float vidaAtual;

    public GameObject bullet;
    public Transform bulletPos;
    private float timer;
    public float velocidadeDoInimigo = 3.5f;
    private Vector2 inimigoDirection;
    private Rigidbody2D inimigoRB2D;
    [SerializeField] private Image barHealth;
    public DetectionController _detectionArea;
    public UnityEvent<Inimigo> OnDeath;
    private SpriteRenderer _spriteRenderer;
    private Animator _animator;
    private GameObject player;

    [Header("Animação Direcional")]
    [SerializeField] private string animatorDirectionParamName = "MovementDirection";

    private bool isDead = false; 

    void Awake()
    {
        inimigoRB2D = GetComponent<Rigidbody2D>();
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _animator = GetComponent<Animator>();
        vidaAtual = vidaMaxima;
        player = GameObject.FindGameObjectWithTag("Player");
    }

    void Start()
    {
        if (barHealth != null)
        {
            editBarHealth(vidaAtual, vidaMaxima);
        }
        else
        {
            Debug.LogError("Referência para barHealth não configurada no Inimigo: " + gameObject.name);
        }
        if (player == null)
        {
            Debug.LogWarning("Inimigo não encontrou objeto com tag 'Player' no Start/Awake.");
        }
    }

    public void ReceberDano(float quantidadeDano)
    {
        if (isDead) return;

        vidaAtual -= quantidadeDano;
        vidaAtual = Mathf.Clamp(vidaAtual, 0, vidaMaxima);
        if (barHealth != null) editBarHealth(vidaAtual, vidaMaxima);
        Debug.Log(gameObject.name + " recebeu " + quantidadeDano + " de dano. Vida atual: " + vidaAtual);

        if (vidaAtual <= 0)
        {
            Morrer();
        }
    }

    public void editBarHealth(float currentLife, float maxLife)
    {
        if (barHealth != null) barHealth.fillAmount = currentLife / maxLife;
    }

    void Morrer()
    {
      
        if (isDead) return;
        isDead = true;

        Debug.Log(gameObject.name + " morreu.");
       
        OnDeath?.Invoke(this);

       
        if (inimigoRB2D != null) inimigoRB2D.linearVelocity = Vector2.zero;
        
        Collider2D col = GetComponent<Collider2D>();
        if (col != null) col.enabled = false;
      
        this.enabled = false;
     
        Destroy(gameObject);
    }

    void Update()
    {
        if (isDead) return;

        if (player != null)
        {
            float distance = Vector2.Distance(transform.position, player.transform.position);
            if (distance < 4)
            {
                timer += Time.deltaTime;
                if (timer > 2)
                {
                    timer = 0;
                    shoot();
                }
            }
        }

        UpdateAnimationState();
    }

    private void FixedUpdate()
    {
        if (isDead)
        {
            if (inimigoRB2D != null) inimigoRB2D.linearVelocity = Vector2.zero;
            return;
        }

        Vector2 currentVelocity = Vector2.zero;
        if (_detectionArea != null && _detectionArea.detectedObjs.Count > 0)
        {
            Transform target = _detectionArea.detectedObjs[0].transform;
            inimigoDirection = (target.position - transform.position).normalized;
            currentVelocity = inimigoDirection * velocidadeDoInimigo;

            if (inimigoRB2D != null) inimigoRB2D.linearVelocity = currentVelocity;

            if (inimigoDirection.x > 0.01f)
            {
                if (_spriteRenderer != null) _spriteRenderer.flipX = false;
            }
            else if (inimigoDirection.x < -0.01f)
            {
                if (_spriteRenderer != null) _spriteRenderer.flipX = true;
            }
        }
        else
        {
            if (inimigoRB2D != null) inimigoRB2D.linearVelocity = Vector2.zero;
            inimigoDirection = Vector2.zero;
        }
    }

    void UpdateAnimationState()
    {
        if (_animator == null) return;

        int directionState = 0; 
       
        if (inimigoDirection.sqrMagnitude > 0.01f)
        {
           
            if (Mathf.Abs(inimigoDirection.x) > Mathf.Abs(inimigoDirection.y))
            {
                directionState = 1; 
            }
            else 
            {
                if (inimigoDirection.y > 0)
                {
                    directionState = 2; 
                }
                else
                {
                    directionState = 3; 
                }
            }
        }

        _animator.SetInteger(animatorDirectionParamName, directionState);
    }

    void shoot()
    {
        if (bullet != null && bulletPos != null)
        {
            Instantiate(bullet, bulletPos.position, Quaternion.identity);
        }
        else
        {
            Debug.LogWarning("Tentativa de atirar sem prefab de bala ou posição definida em: " + gameObject.name);
        }
    }
}

