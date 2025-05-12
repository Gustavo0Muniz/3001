using UnityEngine;
using UnityEngine.UI;
using System.Collections; // Adicionado para usar IEnumerator

public class HeartSystem : MonoBehaviour
{
    public PlayerController player;
    public int vida;
    public int vidaMaxima;
    public bool IsDead;
    public Image[] coracao;
    public Sprite cheio;
    public Sprite vazio;

    void Start()
    {
        player = GetComponent<PlayerController>(); // Obtemos o PlayerController
    }

    void Update()
    {
        HealthLogic();
        DeadState();
    }

    void HealthLogic()
    {
        for (int i = 0; i < coracao.Length; i++)
        {
            if (vida > vidaMaxima)
            {
                vida = vidaMaxima;
            }

            if (i < vida)
            {
                coracao[i].sprite = cheio;
            }
            else
            {
                coracao[i].sprite = vazio;
            }

            coracao[i].enabled = i < vidaMaxima;
        }
    }

    void DeadState()
    {
        if (vida <= 0 && !IsDead) // Verificamos se o jogador morreu e se a animação ainda não foi acionada
        {
            IsDead = true;

            // Ativa a animação de morte
            Animator anim = player.GetComponent<Animator>();
            if (anim != null)
            {
                anim.SetBool("Player_die", true); // Dispara a animação de morte
            }

            // Realiza o flip da sprite para a direção correta (direita ou esquerda)
            FlipOnDeath();

            // Desabilita o controle do jogador
            player.enabled = false;

            // Espera a animação de morte ser concluída antes de destruir o jogador
            StartCoroutine(DestroyPlayerAfterDeathAnimation(anim));
        }
    }

    // Corrotina para esperar a animação de morte ser concluída
    private IEnumerator DestroyPlayerAfterDeathAnimation(Animator anim)
    {
        // Aguarda a duração total da animação de morte (ajuste conforme a duração da animação)
        yield return new WaitForSeconds(anim.GetCurrentAnimatorStateInfo(0).length);

        // Agora, destrua o jogador
        Destroy(gameObject);
    }

    // Função que faz o flip da sprite dependendo da direção
    private void FlipOnDeath()
    {
        if (player.transform.localScale.x > 0) // Player está olhando para a direita
        {
            // Garante que a sprite vai ficar virada para a direita
            player.transform.localScale = new Vector3(1f, 1f, 1f); // Para a direita
        }
        else if (player.transform.localScale.x < 0) // Player está olhando para a esquerda
        {
            // Garante que a sprite vai ficar virada para a esquerda
            player.transform.localScale = new Vector3(-1f, 1f, 1f); // Para a esquerda
        }
    }
}
