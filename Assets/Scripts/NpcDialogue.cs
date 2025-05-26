using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class NpcDialogue : MonoBehaviour
{
    public string[] dialogueNpc;
    public int dialogueIndex;

    public GameObject dialoguePanel;
    public Text dialogueText;

    public Text nameNpc;
    public Image imageNpc;
    public Sprite spriteNpc;

    public bool readyToSpeak;
    public bool startDialogue;

    // Para depura��o
    public bool debugMode = true;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // Garantir que o painel de di�logo comece desativado
        if (dialoguePanel != null)
        {
            dialoguePanel.SetActive(false);
            if (debugMode) Debug.Log("Painel de di�logo desativado no Start");
        }
        else
        {
            if (debugMode) Debug.LogError("DialoguePanel n�o atribu�do no Inspector!");
        }
    }

    // Update is called once per frame
    void Update()
    {
        // Agora o bot�o Fire1 apenas avan�a para o pr�ximo di�logo
        if (Input.GetButtonDown("Fire1") && startDialogue)
        {
            if (dialogueText.text == dialogueNpc[dialogueIndex])
            {
                if (debugMode) Debug.Log("Avan�ando para o pr�ximo di�logo");
                NextDialogue();
            }
        }
    }

    void NextDialogue()
    {
        dialogueIndex++;
        if (dialogueIndex < dialogueNpc.Length)
        {
            if (debugMode) Debug.Log("Mostrando di�logo " + dialogueIndex);
            StartCoroutine(ShowDialogue());
        }
        else
        {
            if (debugMode) Debug.Log("Fim do di�logo");
            dialoguePanel.SetActive(false);
            startDialogue = false;
            dialogueIndex = 0;
        }
    }

    IEnumerator ShowDialogue()
    {
        dialogueText.text = "";
        if (debugMode) Debug.Log("Iniciando exibi��o de texto: " + dialogueNpc[dialogueIndex]);

        foreach (char letter in dialogueNpc[dialogueIndex])
        {
            dialogueText.text += letter;
            yield return new WaitForSeconds(0.05f); // Velocidade um pouco mais r�pida
        }

        if (debugMode) Debug.Log("Texto completo exibido");
    }

    void StartDialogue()
    {
        if (dialogueNpc == null || dialogueNpc.Length == 0)
        {
            Debug.LogError("Array de di�logo est� vazio ou n�o foi configurado!");
            return;
        }

        if (dialoguePanel == null)
        {
            Debug.LogError("DialoguePanel n�o atribu�do no Inspector!");
            return;
        }

        if (nameNpc == null || imageNpc == null || dialogueText == null)
        {
            Debug.LogError("Componentes de UI n�o atribu�dos no Inspector!");
            return;
        }

        nameNpc.text = "Aliado";

        if (spriteNpc != null)
        {
            imageNpc.sprite = spriteNpc;
        }
        else
        {
            Debug.LogWarning("Sprite do NPC n�o configurado!");
        }

        startDialogue = true;
        dialogueIndex = 0;

        // Ativar o painel de di�logo
        dialoguePanel.SetActive(true);

        if (debugMode) Debug.Log("Di�logo iniciado - Painel ativado");

        // Iniciar a exibi��o do primeiro di�logo
        StartCoroutine(ShowDialogue());
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (debugMode) Debug.Log("Colis�o detectada com: " + collision.gameObject.name + " (Tag: " + collision.tag + ")");

        if (collision.CompareTag("Player"))
        {
            if (debugMode) Debug.Log("Player entrou na �rea de di�logo");
            readyToSpeak = true;

            // Inicia o di�logo automaticamente quando o jogador entra na �rea
            if (!startDialogue)
            {
                if (debugMode) Debug.Log("Iniciando di�logo automaticamente");
                StartDialogue();
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            if (debugMode) Debug.Log("Player saiu da �rea de di�logo");
            readyToSpeak = false;

            // Opcional: Encerrar o di�logo quando o jogador sair da �rea
            // Comente as linhas abaixo se preferir que o di�logo continue mesmo quando o jogador sair da �rea
            if (startDialogue)
            {
                if (debugMode) Debug.Log("Encerrando di�logo porque o player saiu da �rea");
                dialoguePanel.SetActive(false);
                startDialogue = false;
                dialogueIndex = 0;
            }
        }
    }
}
