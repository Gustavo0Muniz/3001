// GameEventManager.cs (v2 - Corrigido: StartBossFight agora é public)
using UnityEngine;
using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public enum GamePhase { Phase1_Start, Phase1_Horde1, Phase1_PostHorde1, Phase1_GoToArea, Phase2_StartHorde, Phase2_AkunDialogue, Phase2_PostHorde, Phase3_BuildingHorde, /*Phase3_PostHorde,*/ Phase3_BossDialogue, BossFight, Complete }

public class GameEventManager : MonoBehaviour
{
    [Header("Estado Atual")]
    public GamePhase currentPhase = GamePhase.Phase1_Start;

    [Header("Referências Principais")]
    public CharacterSwitcher characterSwitcher;
    public DialogueTrigger initialDialogue;
    public DialogueTrigger postHorde1Dialogue;
    public DialogueTrigger bossDialogue;
    public ObjectiveArrow objectiveArrow;

    [Header("Controle de Hordas")]
    public Transform[] horde1SpawnPoints;
    public Transform[] horde2SpawnPoints;
    public GameObject robotPrefab;
    public int horde1Size = 5;
    public int horde2Size = 8;
    private List<GameObject> currentHordeEnemies = new List<GameObject>();
    private int enemiesRemaining = 0;

    [Header("Gatilhos e Objetivos")]
    public Collider2D phase2TriggerArea;
    public Collider2D buildingEntranceTrigger;
    public Transform bossTarget;
    public string dracoCharacterName = "Draco";
    public string phase3SceneName = "Lab";

    [Header("Interação com Akun (Pós-Horda 2)")]
    public GameObject akunCharacter;
    public Transform akunSpawnPoint;
    public DialogueTrigger akunDialogueTrigger;
    public UnityEvent onAkunAppears;

    [Header("Eventos de Fase (Opcional - Para lógica extra)")]
    public UnityEvent onPhase1Start;
    public UnityEvent onHorde1Start;
    public UnityEvent onHorde1Defeated;
    public UnityEvent onSwitchToDracoTriggered;
    public UnityEvent onPhase2Start;
    public UnityEvent onHorde2Defeated;
    public UnityEvent onPhase3Start;
    public UnityEvent onBossDialogueStart;
    public UnityEvent onBossFightStart;

    private bool phase2AreaReached = false;

    void Start()
    {
        InitializePhase(currentPhase);
        if (currentPhase == GamePhase.Phase3_BuildingHorde)
        {
            Debug.Log("GameEventManager (Cena Lab): Iniciando Fase 3 - Pulando Horda e indo direto para diálogo do Boss.");
            StartBossDialogue();
        }
        if (akunCharacter != null && akunCharacter.scene.IsValid() && akunCharacter.activeSelf)
        {
            akunCharacter.SetActive(false);
        }
    }

    void InitializePhase(GamePhase phase)
    {
        currentPhase = phase;
        Debug.Log("GameEventManager: Iniciando Fase: " + phase);
        switch (phase)
        {
            case GamePhase.Phase1_Start:
                if (characterSwitcher != null) characterSwitcher.SetSwitchingEnabled(false);
                if (initialDialogue != null) initialDialogue.TriggerDialogue();
                onPhase1Start?.Invoke();
                break;
        }
    }

    public void HandleDialogueEnd(DialogueTrigger dialogue)
    {
        Debug.Log("GameEventManager: Diálogo terminado: " + (dialogue != null ? dialogue.gameObject.name : "NULO") + " na fase: " + currentPhase);
        if (dialogue == initialDialogue && currentPhase == GamePhase.Phase1_Start)
        {
            StartHorde1();
        }
        else if (dialogue == postHorde1Dialogue && currentPhase == GamePhase.Phase1_PostHorde1)
        {
            if (phase2TriggerArea != null) ActivateObjectiveArrow(phase2TriggerArea.transform);
            else Debug.LogError("Phase 2 Trigger Area não configurada!");
            if (characterSwitcher != null)
            {
                characterSwitcher.SwitchToCharacterByName(dracoCharacterName);
                characterSwitcher.SetSwitchingEnabled(true);
                onSwitchToDracoTriggered?.Invoke();
            }
            else Debug.LogError("CharacterSwitcher não configurado!");
            currentPhase = GamePhase.Phase1_GoToArea;
            Debug.Log("Fase alterada para " + currentPhase);
        }
        else if (dialogue == akunDialogueTrigger && currentPhase == GamePhase.Phase2_AkunDialogue)
        {
            currentPhase = GamePhase.Phase2_PostHorde;
            Debug.Log("Fase alterada para " + currentPhase + ". Ativando seta para entrada do prédio.");
            if (buildingEntranceTrigger != null) ActivateObjectiveArrow(buildingEntranceTrigger.transform);
            else Debug.LogWarning("Gatilho de entrada da Fase 3 não configurado para a seta.");
        }
        else if (dialogue == bossDialogue && currentPhase == GamePhase.Phase3_BossDialogue)
        {
            // <<< CHAMADA AINDA ACONTECE AQUI INTERNAMENTE, MAS A FUNÇÃO AGORA É PÚBLICA >>>
            StartBossFight();
        }
    }

    void StartHorde1()
    {
        Debug.Log("Iniciando Horda 1");
        currentPhase = GamePhase.Phase1_Horde1;
        SpawnHorde(horde1SpawnPoints, horde1Size);
        onHorde1Start?.Invoke();
    }

    void SpawnHorde(Transform[] spawnPoints, int count)
    {
        currentHordeEnemies.Clear();
        enemiesRemaining = count;
        if (robotPrefab == null) { Debug.LogError("Prefab do robô não configurado!"); enemiesRemaining = 0; CheckHordeDefeated(); return; }
        if (spawnPoints == null || spawnPoints.Length == 0) { Debug.LogError("Pontos de spawn não configurados!"); enemiesRemaining = 0; CheckHordeDefeated(); return; }
        Debug.Log("Spawning " + count + " inimigos para a fase " + currentPhase);
        for (int i = 0; i < count; i++)
        {
            Transform spawnPoint = spawnPoints[i % spawnPoints.Length];
            GameObject enemy = Instantiate(robotPrefab, spawnPoint.position, spawnPoint.rotation);
            currentHordeEnemies.Add(enemy);
            Inimigo enemyScript = enemy.GetComponent<Inimigo>();
            if (enemyScript != null) enemyScript.OnDeath.AddListener(HandleEnemyDeath);
            else Debug.LogWarning("Inimigo spawnado não possui script 'Inimigo'!", enemy);
        }
    }

    public void HandleEnemyDeath(Inimigo enemy)
    {
        if (enemy != null) enemy.OnDeath.RemoveListener(HandleEnemyDeath);
        enemiesRemaining--;
        Debug.Log("Inimigo derrotado. Restantes: " + enemiesRemaining);
        if (enemiesRemaining <= 0)
        {
            enemiesRemaining = 0;
            CheckHordeDefeated();
        }
    }

    void CheckHordeDefeated()
    {
        if (enemiesRemaining > 0) return;
        Debug.Log("Horda derrotada! Verificando fase atual: " + currentPhase);
        currentHordeEnemies.Clear();
        if (currentPhase == GamePhase.Phase1_Horde1)
        {
            currentPhase = GamePhase.Phase1_PostHorde1;
            Debug.Log("Fase alterada para " + currentPhase + ". Disparando diálogo pós-Horda 1.");
            if (postHorde1Dialogue != null) postHorde1Dialogue.TriggerDialogue();
            else Debug.LogError("Referência para postHorde1Dialogue é NULA!");
            onHorde1Defeated?.Invoke();
        }
        else if (currentPhase == GamePhase.Phase2_StartHorde)
        {
            currentPhase = GamePhase.Phase2_AkunDialogue;
            Debug.Log("Horda 2 derrotada. Fase alterada para " + currentPhase + ". Preparando para Akun aparecer.");
            onHorde2Defeated?.Invoke();
            if (akunCharacter != null)
            {
                GameObject akunInstance = null;
                if (!akunCharacter.scene.IsValid())
                {
                    akunInstance = Instantiate(akunCharacter, akunSpawnPoint.position, akunSpawnPoint.rotation);
                    if (akunDialogueTrigger == null) akunDialogueTrigger = akunInstance.GetComponentInChildren<DialogueTrigger>();
                }
                else
                {
                    akunInstance = akunCharacter;
                    if (akunSpawnPoint != null) { akunInstance.transform.position = akunSpawnPoint.position; akunInstance.transform.rotation = akunSpawnPoint.rotation; }
                    else Debug.LogWarning("AkunSpawnPoint não definido.");
                    akunInstance.SetActive(true);
                }
                onAkunAppears?.Invoke();
                if (akunDialogueTrigger != null) akunDialogueTrigger.TriggerDialogue();
                else Debug.LogError("Referência para akunDialogueTrigger é NULA!");
            }
            else Debug.LogError("Referência para akunCharacter é NULA!");
        }
    }

    void StartBossDialogue()
    {
        Debug.Log("Iniciando diálogo do Boss.");
        if (bossDialogue != null)
        {
            currentPhase = GamePhase.Phase3_BossDialogue;
            Debug.Log("Fase alterada para " + currentPhase);
            bossDialogue.TriggerDialogue();
            onBossDialogueStart?.Invoke();
        }
        else
        {
            Debug.LogError("Referência para bossDialogue é NULA! Não é possível iniciar diálogo do chefe.");
        }
    }

    void ActivateObjectiveArrow(Transform targetTransform)
    {
        if (objectiveArrow != null)
        {
            objectiveArrow.SetTarget(targetTransform);
            objectiveArrow.SetArrowActive(true);
            Debug.Log("Ativando seta para: " + (targetTransform != null ? targetTransform.name : "Nenhum"));
        }
        else Debug.LogError("Referência para ObjectiveArrow não configurada!");
    }

    void DeactivateObjectiveArrow()
    {
        if (objectiveArrow != null)
        {
            objectiveArrow.SetArrowActive(false);
            Debug.Log("Desativando seta de objetivo.");
        }
    }

    public void TriggerPhase2Start()
    {
        if (currentPhase == GamePhase.Phase1_GoToArea && !phase2AreaReached)
        {
            phase2AreaReached = true;
            Debug.Log("Iniciando Fase 2 - Horda");
            currentPhase = GamePhase.Phase2_StartHorde;
            DeactivateObjectiveArrow();
            SpawnHorde(horde2SpawnPoints, horde2Size);
            onPhase2Start?.Invoke();
        }
        else Debug.LogWarning("TriggerPhase2Start chamado, mas condições não atendidas (Fase: " + currentPhase + ", Area Reached: " + phase2AreaReached + ")");
    }

    public void TriggerPhase3Start()
    {
        if (currentPhase == GamePhase.Phase2_PostHorde)
        {
            Debug.Log("Jogador chegou à entrada do prédio (Fase 3). Iniciando transição para cena: " + phase3SceneName);
            DeactivateObjectiveArrow();
            onPhase3Start?.Invoke();
            if (!string.IsNullOrEmpty(phase3SceneName))
            {
                SceneManager.LoadScene(phase3SceneName);
            }
            else
            {
                Debug.LogError("Nome da cena da Fase 3 (phase3SceneName) não definido! Não é possível carregar a cena.");
            }
        }
        else
        {
            Debug.LogWarning("TriggerPhase3Start chamado, mas a fase atual não é Phase2_PostHorde (Fase: " + currentPhase + ")");
        }
    }

    // <<< MODIFICADO: Função agora é pública >>>
    public void StartBossFight()
    {
        Debug.Log("Iniciando Luta contra o Chefe!");
        // Só inicia a luta se estivermos vindo do diálogo
        if (currentPhase != GamePhase.Phase3_BossDialogue)
        {
            Debug.LogWarning("StartBossFight chamada fora da fase Phase3_BossDialogue! Ignorando.");
            return;
        }

        currentPhase = GamePhase.BossFight;
        Debug.Log("Fase alterada para " + currentPhase);
        DeactivateObjectiveArrow();
        onBossFightStart?.Invoke();

        if (bossTarget != null)
        {
            Debug.Log("Ativando GameObject do Boss: " + bossTarget.name);
            bossTarget.gameObject.SetActive(true);
            // Adicione aqui qualquer outra lógica de ativação do boss (scripts, etc)
        }
        else
        {
            Debug.LogError("Referência para bossTarget (GameObject do Boss?) não definida! Não é possível ativar o chefe.");
        }
    }
}

