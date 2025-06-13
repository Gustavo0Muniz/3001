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

    public bool debugMode = true;

    void Start()
    {
        if (dialoguePanel != null)
        {
            dialoguePanel.SetActive(false);
            if (debugMode) Debug.Log("Painel de diálogo desativado no Start");
        }
        else
        {
            if (debugMode) Debug.LogError("DialoguePanel não atribuído no Inspector!");
        }
    }

    void Update()
    {
        if (Input.GetButtonDown("Fire1") && startDialogue)
        {
            if (dialogueText.text == dialogueNpc[dialogueIndex])
            {
                if (debugMode) Debug.Log("Avançando para o próximo diálogo");
                NextDialogue();
            }
        }
    }

    void NextDialogue()
    {
        dialogueIndex++;
        if (dialogueIndex < dialogueNpc.Length)
        {
            if (debugMode) Debug.Log("Mostrando diálogo " + dialogueIndex);
            StartCoroutine(ShowDialogue());
        }
        else
        {
            if (debugMode) Debug.Log("Fim do diálogo");
            dialoguePanel.SetActive(false);
            startDialogue = false;
            dialogueIndex = 0;
        }
    }

    IEnumerator ShowDialogue()
    {
        dialogueText.text = "";
        if (debugMode) Debug.Log("Iniciando exibição de texto: " + dialogueNpc[dialogueIndex]);

        foreach (char letter in dialogueNpc[dialogueIndex])
        {
            dialogueText.text += letter;
            yield return new WaitForSeconds(0.05f); 
        }

        if (debugMode) Debug.Log("Texto completo exibido");
    }

    void StartDialogue()
    {
        if (dialogueNpc == null || dialogueNpc.Length == 0)
        {
            Debug.LogError("Array de diálogo está vazio ou não foi configurado!");
            return;
        }

        if (dialoguePanel == null)
        {
            Debug.LogError("DialoguePanel não atribuído no Inspector!");
            return;
        }

        if (nameNpc == null || imageNpc == null || dialogueText == null)
        {
            Debug.LogError("Componentes de UI não atribuídos no Inspector!");
            return;
        }

        nameNpc.text = "Aliado";

        if (spriteNpc != null)
        {
            imageNpc.sprite = spriteNpc;
        }
        else
        {
            Debug.LogWarning("Sprite do NPC não configurado!");
        }

        startDialogue = true;
        dialogueIndex = 0;

        dialoguePanel.SetActive(true);

        if (debugMode) Debug.Log("Diálogo iniciado - Painel ativado");

        StartCoroutine(ShowDialogue());
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (debugMode) Debug.Log("Colisão detectada com: " + collision.gameObject.name + " (Tag: " + collision.tag + ")");

        if (collision.CompareTag("Player"))
        {
            if (debugMode) Debug.Log("Player entrou na área de diálogo");
            readyToSpeak = true;

            if (!startDialogue)
            {
                if (debugMode) Debug.Log("Iniciando diálogo automaticamente");
                StartDialogue();
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            if (debugMode) Debug.Log("Player saiu da área de diálogo");
            readyToSpeak = false;

            if (startDialogue)
            {
                if (debugMode) Debug.Log("Encerrando diálogo porque o player saiu da área");
                dialoguePanel.SetActive(false);
                startDialogue = false;
                dialogueIndex = 0;
            }
        }
    }
}
