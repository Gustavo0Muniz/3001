using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic; // Necess�rio para List
using UnityEngine.Events;
// Estrutura para definir um personagem que pode falar
[System.Serializable]
public struct SpeakerProfile
{
    public string speakerName;
    public Sprite speakerSprite;
}

// Estrutura para definir uma linha de di�logo e quem a fala
[System.Serializable]
public struct DialogueLine
{
    public int speakerIndex; // �ndice na lista 'speakers' que define quem est� falando
    [TextArea(3, 10)] // Permite editar textos maiores no Inspector
    public string lineText;
}

public class DialogueTrigger : MonoBehaviour
{
    [Header("Configura��o dos Falantes")]
    [Tooltip("Lista de personagens que participam deste di�logo (Ex: Henry, Draco)")]
    public List<SpeakerProfile> speakers;

    [Header("Configura��o do Di�logo")]
    [Tooltip("Sequ�ncia de linhas de di�logo para esta intera��o")]
    public DialogueLine[] dialogueLines;

    [Header("Refer�ncias da UI")]
    [Tooltip("Painel principal que cont�m os elementos de di�logo")]
    public GameObject dialoguePanel;
    [Tooltip("Texto onde o nome do falante atual ser� exibido")]
    public Text speakerNameText;
    [Tooltip("Imagem onde o retrato do falante atual ser� exibido")]
    public Image speakerImage;
    [Tooltip("Texto onde a linha de di�logo atual ser� exibida")]
    public Text dialogueText;

    [Header("Controle de Fluxo")]
    [Tooltip("O di�logo deve come�ar automaticamente ao entrar no trigger?")]
    public bool startAutomatically = true;
    [Tooltip("O di�logo deve terminar automaticamente ao sair do trigger?")]
    public bool endOnExit = true;

    private int dialogueIndex;
    private bool readyToSpeak;
    private bool dialogueActive;
    private Coroutine typingCoroutine;

    // Para depura��o
    public bool debugMode = true;

    void Start()
    {
        // Garantir que o painel de di�logo comece desativado
        if (dialoguePanel != null)
        {
            dialoguePanel.SetActive(false);
        }
        else
        {
            if (debugMode) Debug.LogError("DialoguePanel n�o atribu�do no Inspector!", this);
        }
    }

    void Update()
    {
        // Avan�a para o pr�ximo di�logo ou termina se o texto j� foi completamente exibido
        if (dialogueActive && Input.GetButtonDown("Fire1")) // Usar Fire1 (geralmente clique esquerdo ou Ctrl)
        {
            // Se o texto ainda est� sendo "digitado", completa-o imediatamente
            if (typingCoroutine != null)
            {
                StopCoroutine(typingCoroutine);
                typingCoroutine = null;
                // Garante que o texto completo seja exibido
                if (dialogueIndex < dialogueLines.Length)
                {
                    dialogueText.text = dialogueLines[dialogueIndex].lineText;
                    if (debugMode) Debug.Log("Texto completado imediatamente.");
                }
            }
            // Se o texto j� est� completo, avan�a para a pr�xima linha
            else
            {
                if (debugMode) Debug.Log("Avan�ando para o pr�ximo di�logo");
                NextDialogueLine();
            }
        }
    }

    void NextDialogueLine()
    {
        dialogueIndex++;
        if (dialogueIndex < dialogueLines.Length)
        {
            if (debugMode) Debug.Log("Mostrando di�logo " + dialogueIndex);
            UpdateDialogueUI();
            typingCoroutine = StartCoroutine(ShowDialogueText());
        }
        else
        {
            EndDialogue();
        }
    }

    IEnumerator ShowDialogueText()
    {
        dialogueText.text = "";
        string currentLine = dialogueLines[dialogueIndex].lineText;
        if (debugMode) Debug.Log("Iniciando exibi��o de texto: " + currentLine);

        foreach (char letter in currentLine)
        {
            dialogueText.text += letter;
            yield return new WaitForSeconds(0.05f); // Velocidade da "digita��o"
        }

        if (debugMode) Debug.Log("Texto completo exibido");
        typingCoroutine = null; // Marca que a digita��o terminou
    }

    void StartDialogueSequence()
    {
        // Valida��es
        if (dialogueLines == null || dialogueLines.Length == 0)
        {
            Debug.LogError("Array de 'dialogueLines' est� vazio ou n�o foi configurado!", this);
            return;
        }
        if (speakers == null || speakers.Count == 0)
        {
            Debug.LogError("Lista de 'speakers' est� vazia ou n�o foi configurada!", this);
            return;
        }
        if (dialoguePanel == null || speakerNameText == null || speakerImage == null || dialogueText == null)
        {
            Debug.LogError("Um ou mais componentes de UI n�o foram atribu�dos no Inspector!", this);
            return;
        }

        dialogueActive = true;
        dialogueIndex = 0;
        dialoguePanel.SetActive(true);
        if (debugMode) Debug.Log("Di�logo iniciado - Painel ativado");

        UpdateDialogueUI();
        typingCoroutine = StartCoroutine(ShowDialogueText());
    }

    void UpdateDialogueUI()
    {
        if (dialogueIndex < dialogueLines.Length)
        {
            int speakerIdx = dialogueLines[dialogueIndex].speakerIndex;

            // Valida o �ndice do falante
            if (speakerIdx >= 0 && speakerIdx < speakers.Count)
            {
                SpeakerProfile currentSpeaker = speakers[speakerIdx];
                speakerNameText.text = currentSpeaker.speakerName;
                if (currentSpeaker.speakerSprite != null)
                {
                    speakerImage.sprite = currentSpeaker.speakerSprite;
                    speakerImage.enabled = true; // Garante que a imagem esteja vis�vel
                }
                else
                {
                    speakerImage.enabled = false; // Esconde a imagem se n�o houver sprite
                    if (debugMode) Debug.LogWarning("Sprite n�o configurado para o falante: " + currentSpeaker.speakerName);
                }
            }
            else
            {
                Debug.LogError("�ndice de falante inv�lido na linha " + dialogueIndex + ": " + speakerIdx, this);
                // Define um padr�o para evitar erros
                speakerNameText.text = "???";
                speakerImage.enabled = false;
            }
        }
    }

    void EndDialogue()
    {
        if (debugMode) Debug.Log("Fim do di�logo");
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
            typingCoroutine = null;
        }
        dialoguePanel.SetActive(false);
        OnDialogueEnd?.Invoke(this);
        dialogueActive = false;
        dialogueIndex = 0;
        // Adicionar l�gica para notificar outros sistemas que o di�logo terminou 
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (debugMode) Debug.Log("Colis�o detectada com: " + collision.gameObject.name + " (Tag: " + collision.tag + ")", this);

        if (collision.CompareTag("Player"))
        {
            if (debugMode) Debug.Log("Player entrou na �rea de di�logo", this);
            readyToSpeak = true;

            if (startAutomatically && !dialogueActive)
            {
                if (debugMode) Debug.Log("Iniciando di�logo automaticamente", this);
                StartDialogueSequence();
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            if (debugMode) Debug.Log("Player saiu da �rea de di�logo", this);
            readyToSpeak = false;

            if (endOnExit && dialogueActive)
            {
                if (debugMode) Debug.Log("Encerrando di�logo porque o player saiu da �rea", this);
                EndDialogue();
            }
        }
    }

    // M�todo p�blico para iniciar o di�logo externamente
    public void TriggerDialogue()
    {
        if (!dialogueActive)
        {
            if (debugMode) Debug.Log("Di�logo iniciado por gatilho externo", this);
            StartDialogueSequence();
        }
    }
    public UnityEvent<DialogueTrigger> OnDialogueEnd;
}
