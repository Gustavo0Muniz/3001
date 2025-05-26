using UnityEngine;

// Este script deve ser adicionado ao GameObject que possui o Collider2D 
// referenciado como 'buildingEntranceTrigger' no seu GameEventManager.

[RequireComponent(typeof(Collider2D))]
public class PhaseTrigger : MonoBehaviour
{
    // Refer�ncia para o seu GameEventManager na cena
    // Arraste o GameObject que cont�m o GameEventManager para este campo no Inspector
    public GameEventManager gameEventManager;

    // Tag do objeto que deve ativar o gatilho (geralmente o jogador)
    public string playerTag = "Player";

    private bool triggered = false; // Para garantir que o gatilho s� dispare uma vez

    void Start()
    {
        // Garante que o Collider2D est� configurado como Trigger
        Collider2D col = GetComponent<Collider2D>();
        if (!col.isTrigger)
        {
            Debug.LogWarning("O Collider2D neste GameObject (" + gameObject.name + ") n�o est� marcado como 'Is Trigger'. A detec��o pode n�o funcionar corretamente.", this.gameObject);
            // Opcional: For�ar a ser trigger via script, mas � melhor configurar no Inspector
            // col.isTrigger = true; 
        }

        // Verifica se a refer�ncia ao GameEventManager foi definida no Inspector
        if (gameEventManager == null)
        {
            Debug.LogError("A refer�ncia ao GameEventManager n�o foi definida no Inspector deste PhaseTrigger (" + gameObject.name + ")!", this.gameObject);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Verifica se j� foi ativado para evitar m�ltiplas chamadas
        if (triggered)
        {
            return;
        }

        // Verifica se o objeto que entrou tem a tag correta (ex: "Player")
        if (other.CompareTag(playerTag))
        {
            Debug.Log("GATILHO FASE 3 (" + gameObject.name + "): Jogador ('" + playerTag + "') entrou na �rea.");

            // Verifica se a refer�ncia ao GameEventManager � v�lida
            if (gameEventManager != null)
            {
                Debug.Log("GATILHO FASE 3: Chamando TriggerPhase3Start no GameEventManager...");
                // Chama a fun��o no GameEventManager para tentar iniciar a Fase 3
                gameEventManager.TriggerPhase3Start();
                triggered = true; // Marca como ativado para n�o chamar novamente

                // Opcional: Desativar este gatilho ap�s o uso, se n�o for mais necess�rio
                // gameObject.SetActive(false);
            }
            else
            {
                Debug.LogError("GATILHO FASE 3: N�o foi poss�vel chamar TriggerPhase3Start porque a refer�ncia ao GameEventManager � nula!", this.gameObject);
            }
        }
    }
}

