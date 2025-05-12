using UnityEngine;

public class Inimigo : MonoBehaviour
{

    public GameObject bullet;
    public Transform bulletPos;
    private float timer;
    public float velocidadeDoInimigo = 3.5f;
    private Vector2 inimigoDirection;
    private Rigidbody2D inimigoRB2D;

    public DetectionController _detectionArea;
    private SpriteRenderer _spriteRenderer;
    private GameObject player;
    void Start()
    {
       
        player = GameObject.FindGameObjectWithTag("Player");
        inimigoRB2D = GetComponent<Rigidbody2D>();
        _spriteRenderer = GetComponent<SpriteRenderer>();

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
