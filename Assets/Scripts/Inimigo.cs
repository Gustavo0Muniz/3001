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

    [Header("Anima��o Direcional")]
    [SerializeField] private string animatorDirectionParamName = "MovementDirection"; 

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
            Debug.LogError("Refer�ncia para barHealth n�o configurada no Inimigo: " + gameObject.name);
        }
        if (player == null)
        {
            Debug.LogWarning("Inimigo n�o encontrou objeto com tag 'Player' no Start.");
        }
    }

    public void ReceberDano(float quantidadeDano)
    {
        vidaAtual -= quantidadeDano;
        vidaAtual = Mathf.Clamp(vidaAtual, 0, vidaMaxima);
        if (barHealth != null) editBarHealth(vidaAtual, vidaMaxima);
        Debug.Log(gameObject.name + " recebeu " + quantidadeDano + " de dano. Vida atual: " + vidaAtual);
        if (vidaAtual <= 0) Morrer();
    }

    public void editBarHealth(float vidaAtual, float vidaMaxima)
    {
        if (barHealth != null) barHealth.fillAmount = vidaAtual / vidaMaxima;
    }

    void Morrer()
    {
        Debug.Log(gameObject.name + " morreu.");
        OnDeath?.Invoke(this);
      
        Destroy(gameObject);
    }

    void Update()
    {
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
        Vector2 currentVelocity = Vector2.zero; 
        if (_detectionArea != null && _detectionArea.detectedObjs.Count > 0)
        {
            Transform target = _detectionArea.detectedObjs[0].transform;
            inimigoDirection = (target.position - transform.position).normalized;
            currentVelocity = inimigoDirection * velocidadeDoInimigo;

            inimigoRB2D.linearVelocity = currentVelocity; 
            if (inimigoDirection.x > 0.01f)
            {
                _spriteRenderer.flipX = false;
            }
            else if (inimigoDirection.x < -0.01f)
            {
                _spriteRenderer.flipX = true;
            }
        }
        else
        {
            inimigoRB2D.linearVelocity = Vector2.zero; 
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
            Debug.LogWarning("Tentativa de atirar sem prefab de bala ou posi��o definida em: " + gameObject.name);
        }
    }
}

