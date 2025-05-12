using UnityEngine;
using UnityEngine.UI;

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
    private SpriteRenderer _spriteRenderer;
    private GameObject player;
    void Start()
    {
        vidaAtual = vidaMaxima;
        if (barHealth != null)
        {
            editBarHealth(vidaAtual, vidaMaxima);
        }
        else
        {
            Debug.LogError("Referência para barHealth não configurada no Inimigo: " + gameObject.name);
        }
    

    player = GameObject.FindGameObjectWithTag("Player");
        inimigoRB2D = GetComponent<Rigidbody2D>();
        _spriteRenderer = GetComponent<SpriteRenderer>();


    }
public void ReceberDano(float quantidadeDano)
{
    vidaAtual -= quantidadeDano;
    vidaAtual = Mathf.Clamp(vidaAtual, 0, vidaMaxima); // Garante que a vida não seja menor que 0 ou maior que a máxima

    if (barHealth != null)
    {
        editBarHealth(vidaAtual, vidaMaxima);
    }

    Debug.Log(gameObject.name + " recebeu " + quantidadeDano + " de dano. Vida atual: " + vidaAtual);

    if (vidaAtual <= 0)
    {
        Morrer();
    }
}
public void editBarHealth(float vidaAtual, float vidaMaxima)
{
    if (barHealth != null)
    {
        barHealth.fillAmount = vidaAtual / vidaMaxima;
    }
}

void Morrer()
{
    // Adicione aqui a lógica de morte do inimigo
    // Ex: animação de morte, desabilitar o objeto, instanciar loot, etc.
    Debug.Log(gameObject.name + " morreu.");
    Destroy(gameObject); // Exemplo: Destruir o objeto do inimigo
}


void Update()
    {
        inimigoDirection = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
        

        float distance = Vector2.Distance(transform.position, player.transform.position);
        Debug.Log(distance);
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
    
    private void FixedUpdate()
    {
        if(_detectionArea.detectedObjs.Count > 0)
        {
            inimigoDirection = (_detectionArea.detectedObjs[0].transform.position - transform.position).normalized;

            inimigoRB2D.MovePosition(inimigoRB2D.position + inimigoDirection * velocidadeDoInimigo * Time.fixedDeltaTime);
        }
        if (inimigoDirection.x > 0)
        {
            _spriteRenderer.flipX = false;
        }
        else if (inimigoDirection.x < 0)
        {
            _spriteRenderer.flipX = true;
        }
    }
    void shoot()
    {
        Instantiate(bullet, bulletPos.position, Quaternion.identity);
    }
}
