using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events; 
using System.Collections;
using System.Collections.Generic; 

[System.Serializable]
public struct SpeakerProfile
{
    public string speakerName;
    public Sprite speakerSprite;
}

[System.Serializable]
public struct DialogueLine
{
    public int speakerIndex; 
    [TextArea(3, 10)] 
    public string lineText;
}

public class DialogueTrigger : MonoBehaviour
{
    [Header("Configuração dos Falantes")]
    [Tooltip("Lista de personagens que participam deste diálogo (Ex: Henry, Draco)")]
    public List<SpeakerProfile> speakers;

    [Header("Configuração do Diálogo")]
    [Tooltip("Sequência de linhas de diálogo para esta interação")]
    public DialogueLine[] dialogueLines;

    [Header("Referências da UI")]
    [Tooltip("Painel principal que contém os elementos de diálogo")]
    public GameObject dialoguePanel;
    [Tooltip("Texto onde o nome do falante atual será exibido")]
    public Text speakerNameText;
    [Tooltip("Imagem onde o retrato do falante atual será exibido")]
    public Image speakerImage;
    [Tooltip("Texto onde a linha de diálogo atual será exibida")]
    public Text dialogueText;

    [Header("Controle de Fluxo")]
    [Tooltip("O diálogo deve começar automaticamente ao entrar no trigger?")]
    public bool startAutomatically = true;
    [Tooltip("O diálogo deve terminar automaticamente ao sair do trigger?")]
    public bool endOnExit = true;

    [Header("Eventos")] 
    [Tooltip("Evento disparado quando o diálogo termina.")]
    public UnityEvent OnDialogueEnd; 

    private int dialogueIndex;
    private bool readyToSpeak;
    private bool dialogueActive;
    private Coroutine typingCoroutine;

   
    public bool debugMode = true;

    void Start()
    {

        if (dialoguePanel != null)
        {
            dialoguePanel.SetActive(false);
        }
        else
        {
            if (debugMode) Debug.LogError("DialoguePanel não atribuído no Inspector!", this);
        }

     
        if (OnDialogueEnd == null)
            OnDialogueEnd = new UnityEvent();
    }

    void Update()
    {

        if (dialogueActive && Input.GetButtonDown("Fire1")) 
        {
            
            if (typingCoroutine != null)
            {
                StopCoroutine(typingCoroutine);
                typingCoroutine = null;
          
                if (dialogueIndex < dialogueLines.Length)
                {
                    dialogueText.text = dialogueLines[dialogueIndex].lineText;
                    if (debugMode) Debug.Log("Texto completado imediatamente.");
                }
            }
          
            else
            {
                if (debugMode) Debug.Log("Avançando para o próximo diálogo");
                NextDialogueLine();
            }
        }
    }

    void NextDialogueLine()
    {
        dialogueIndex++;
        if (dialogueIndex < dialogueLines.Length)
        {
            if (debugMode) Debug.Log("Mostrando diálogo " + dialogueIndex);
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
        if (debugMode) Debug.Log("Iniciando exibição de texto: " + currentLine);

        foreach (char letter in currentLine)
        {
            dialogueText.text += letter;
            yield return new WaitForSeconds(0.05f); 
        }

        if (debugMode) Debug.Log("Texto completo exibido");
        typingCoroutine = null; 
    }

    void StartDialogueSequence()
    {
        if (dialogueLines == null || dialogueLines.Length == 0)
        {
            Debug.LogError("Array de 'dialogueLines' está vazio ou não foi configurado!", this);
            return;
        }
        if (speakers == null || speakers.Count == 0)
        {
            Debug.LogError("Lista de 'speakers' está vazia ou não foi configurada!", this);
            return;
        }
        if (dialoguePanel == null || speakerNameText == null || speakerImage == null || dialogueText == null)
        {
            Debug.LogError("Um ou mais componentes de UI não foram atribuídos no Inspector!", this);
            return;
        }

        dialogueActive = true;
        dialogueIndex = 0;
        dialoguePanel.SetActive(true);
        if (debugMode) Debug.Log("Diálogo iniciado - Painel ativado");

        UpdateDialogueUI();
        typingCoroutine = StartCoroutine(ShowDialogueText());
    }

    void UpdateDialogueUI()
    {
        if (dialogueIndex < dialogueLines.Length)
        {
            int speakerIdx = dialogueLines[dialogueIndex].speakerIndex;

            if (speakerIdx >= 0 && speakerIdx < speakers.Count)
            {
                SpeakerProfile currentSpeaker = speakers[speakerIdx];
                speakerNameText.text = currentSpeaker.speakerName;
                if (currentSpeaker.speakerSprite != null)
                {
                    speakerImage.sprite = currentSpeaker.speakerSprite;
                    speakerImage.enabled = true;
                }
                else
                {
                    speakerImage.enabled = false;
                    if (debugMode) Debug.LogWarning("Sprite não configurado para o falante: " + currentSpeaker.speakerName);
                }
            }
            else
            {
                Debug.LogError("Índice de falante inválido na linha " + dialogueIndex + ": " + speakerIdx, this);
              
                speakerNameText.text = "???";
                speakerImage.enabled = false;
            }
        }
    }

    void EndDialogue()
    {
        if (!dialogueActive) return; 

        if (debugMode) Debug.Log("Fim do diálogo");
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
            typingCoroutine = null;
        }
        dialoguePanel.SetActive(false);
        dialogueActive = false;
        dialogueIndex = 0;

  
        if (debugMode) Debug.Log("Disparando evento OnDialogueEnd");
        OnDialogueEnd?.Invoke(); 
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (debugMode) Debug.Log("Colisão detectada com: " + collision.gameObject.name + " (Tag: " + collision.tag + ")", this);

        if (collision.CompareTag("Player"))
        {
            if (debugMode) Debug.Log("Player entrou na área de diálogo", this);
            readyToSpeak = true;

            if (startAutomatically && !dialogueActive)
            {
                if (debugMode) Debug.Log("Iniciando diálogo automaticamente", this);
                StartDialogueSequence();
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            if (debugMode) Debug.Log("Player saiu da área de diálogo", this);
            readyToSpeak = false;

            if (endOnExit && dialogueActive)
            {
                if (debugMode) Debug.Log("Encerrando diálogo porque o player saiu da área", this);
                EndDialogue();
            }
        }
    }


    public void TriggerDialogue()
    {
        if (!dialogueActive)
        {
            if (debugMode) Debug.Log("Diálogo iniciado por gatilho externo", this);
            StartDialogueSequence();
        }
    }
}

