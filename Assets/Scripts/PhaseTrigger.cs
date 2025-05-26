using UnityEngine;

// Este script deve ser adicionado ao GameObject que possui o Collider2D 
// referenciado como 'buildingEntranceTrigger' no seu GameEventManager.

[RequireComponent(typeof(Collider2D))]
public class PhaseTrigger : MonoBehaviour
{
    // Referência para o seu GameEventManager na cena
    // Arraste o GameObject que contém o GameEventManager para este campo no Inspector
    public GameEventManager gameEventManager;

    // Tag do objeto que deve ativar o gatilho (geralmente o jogador)
    public string playerTag = "Player";

    private bool triggered = false; // Para garantir que o gatilho só dispare uma vez

    void Start()
    {
        // Garante que o Collider2D está configurado como Trigger
        Collider2D col = GetComponent<Collider2D>();
        if (!col.isTrigger)
        {
            Debug.LogWarning("O Collider2D neste GameObject (" + gameObject.name + ") não está marcado como 'Is Trigger'. A detecção pode não funcionar corretamente.", this.gameObject);
            // Opcional: Forçar a ser trigger via script, mas é melhor configurar no Inspector
            // col.isTrigger = true; 
        }

        // Verifica se a referência ao GameEventManager foi definida no Inspector
        if (gameEventManager == null)
        {
            Debug.LogError("A referência ao GameEventManager não foi definida no Inspector deste PhaseTrigger (" + gameObject.name + ")!", this.gameObject);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Verifica se já foi ativado para evitar múltiplas chamadas
        if (triggered)
        {
            return;
        }

        // Verifica se o objeto que entrou tem a tag correta (ex: "Player")
        if (other.CompareTag(playerTag))
        {
            Debug.Log("GATILHO FASE 3 (" + gameObject.name + "): Jogador ('" + playerTag + "') entrou na área.");

            // Verifica se a referência ao GameEventManager é válida
            if (gameEventManager != null)
            {
                Debug.Log("GATILHO FASE 3: Chamando TriggerPhase3Start no GameEventManager...");
                // Chama a função no GameEventManager para tentar iniciar a Fase 3
                gameEventManager.TriggerPhase3Start();
                triggered = true; // Marca como ativado para não chamar novamente

                // Opcional: Desativar este gatilho após o uso, se não for mais necessário
                // gameObject.SetActive(false);
            }
            else
            {
                Debug.LogError("GATILHO FASE 3: Não foi possível chamar TriggerPhase3Start porque a referência ao GameEventManager é nula!", this.gameObject);
            }
        }
    }
}

