using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class GameEventManager : MonoBehaviour
{
    [Header("Referências Principais")]
    public CharacterSwitcher characterSwitcher;
    public GameObject akunCharacter;
    public GameObject henryCharacter;
    public BossController bossController;
    public ObjectiveArrow objectiveArrow;
    public Transform periferiaTarget;
    public Transform bossTransform;
    public GameObject periferiaBarrier;

    [Header("Diálogos")]
    public DialogueTrigger initialDialogue;
    public DialogueTrigger postHorde1Dialogue;
    public DialogueTrigger akunDialogueTrigger;
    public DialogueTrigger finalAwarenessDialogue;

    [Header("Horda 1")]
    public GameObject robotPrefab;
    public Transform[] horde1SpawnPoints;
    public int horde1Size = 5;
    private List<GameObject> currentHorde1Enemies = new List<GameObject>();
    private int enemiesRemainingInHorde1 = 0;

    [Header("Combate Periferia")]
    public Transform[] periferiaRobotSpawnPoints;
    public int periferiaRobotCount = 3;
    public Collider2D periferiaTriggerArea;
    private List<GameObject> currentPeriferiaEnemies = new List<GameObject>();
    private int enemiesRemainingInPeriferia = 0;

    [Header("Spawn de Coleta")]
    public Transform[] generalRobotSpawnPoints;
    public int robotsPerCollection = 1;

    [Header("UI Mensagem Desbloqueio/Aviso")]
    public GameObject unlockMessagePanel;
    public Text unlockMessageText;
    private Coroutine hideMessageCoroutine;

    [Header("Configuração Final")]
    [Tooltip("Nome exato da cena de Menu para carregar ao completar o jogo.")]
    public string menuSceneName = "MainMenu";
    [Tooltip("O GameObject que contém o script FinalDialogueTrigger.cs. Ele deve começar desativado.")]
    public GameObject finalDialogueTriggerObject;

    [Header("Eventos (Opcional)")]
    public UnityEvent onGameStart;
    public UnityEvent onHorde1Start;
    public UnityEvent onHorde1Defeated;
    public UnityEvent onAkunPathUnlocked;
    public UnityEvent onPeriferiaCombatStart;
    public UnityEvent onPeriferiaCombatEnd;
    public UnityEvent onRecyclingFull;
    public UnityEvent onFinalDialogueStart;
    public UnityEvent onGameComplete;

    private bool akunPathUnlocked = false;
    private bool recyclingIsFull = false;
    private bool finalDialogueStarted = false;
    private bool periferiaAreaReached = false;
    private List<GameObject> activeCollectionRobots = new List<GameObject>();

    void Start()
    {
        InitializeGame();
    }

    void InitializeGame()
    {
        Debug.Log("GameEventManager: Iniciando Jogo.");

        if (characterSwitcher != null) characterSwitcher.SetSwitchingEnabled(false);
        else Debug.LogError("CharacterSwitcher não configurado!");

        if (akunCharacter != null) akunCharacter.SetActive(false);
        else Debug.LogError("Referência ao GameObject de Akun não definida!");
        if (henryCharacter != null) henryCharacter.SetActive(true);
        else Debug.LogError("Personagem inicial (Henry/Outro) não configurado!", this);

        if (bossController == null) Debug.LogError("BossController não configurado!");

        if (bossTransform == null) Debug.LogWarning("BossTransform não configurado para a seta!");
        if (periferiaTarget == null) Debug.LogError("PeriferiaTarget (Trigger da Periferia) não configurado para a seta!");
        if (objectiveArrow != null) objectiveArrow.SetArrowActive(false); else Debug.LogWarning("ObjectiveArrow não configurada!");
        if (unlockMessagePanel != null) unlockMessagePanel.SetActive(false); else Debug.LogWarning("Painel de Mensagem de Desbloqueio/Aviso não configurado!");
        if (periferiaBarrier != null) periferiaBarrier.SetActive(true); else Debug.LogError("Referência para PeriferiaBarrier não configurada no Inspector!");

        if (finalDialogueTriggerObject == null)
        {
            Debug.LogError("GameEventManager: Final Dialogue Trigger Object não foi atribuído no Inspector! O diálogo final não funcionará.", this);
        }
        else
        {
            finalDialogueTriggerObject.SetActive(false);
        }

        if (initialDialogue != null) initialDialogue.TriggerDialogue(); else Debug.LogError("InitialDialogue não configurado!");

        if (EnvironmentalManager.Instance != null)
        {
            EnvironmentalManager.Instance.OnTrashCollected.AddListener(HandleTrashCollected_SpawnRobots);
            EnvironmentalManager.Instance.OnRecyclingHalfFull.AddListener(TriggerAkunPathUnlock);
            EnvironmentalManager.Instance.OnRecyclingFull.AddListener(HandleRecyclingFull);
        }
        else { Debug.LogError("GameEventManager: EnvironmentalManager não encontrado!", this); }

        onGameStart?.Invoke();
    }

    public void HandleDialogueEnd(DialogueTrigger dialogue)
    {
        Debug.Log("GameEventManager: Diálogo terminado: " + (dialogue != null ? dialogue.gameObject.name : "NULO"));

        if (dialogue == initialDialogue) { StartHorde1(); }
        else if (dialogue == postHorde1Dialogue)
        {
            Debug.Log("Retornando para coleta geral.");
            DeactivateObjectiveArrow();
        }
        else if (dialogue == akunDialogueTrigger)
        {
            Debug.Log("Diálogo com Akun terminado. Coleta final para encher reciclagem.");
            DeactivateObjectiveArrow();
        }
        else if (dialogue == finalAwarenessDialogue)
        {
            CompleteGame();
        }
    }


    void StartHorde1()
    {
        Debug.Log("Iniciando Horda 1");

        SpawnHorde1();
        onHorde1Start?.Invoke();
    }

    void SpawnHorde1()
    {
        currentHorde1Enemies.Clear();
        enemiesRemainingInHorde1 = horde1Size;
        if (robotPrefab == null) { Debug.LogError("Prefab do robô não configurado para horda 1!"); enemiesRemainingInHorde1 = 0; CheckHorde1Defeated(); return; }
        if (horde1SpawnPoints == null || horde1SpawnPoints.Length == 0) { Debug.LogError("Pontos de spawn da horda 1 não configurados!"); enemiesRemainingInHorde1 = 0; CheckHorde1Defeated(); return; }
        Debug.Log("Spawning " + horde1Size + " inimigos para a Horda 1");
        for (int i = 0; i < horde1Size; i++)
        {
            Transform spawnPoint = horde1SpawnPoints[i % horde1SpawnPoints.Length];
            GameObject enemy = Instantiate(robotPrefab, spawnPoint.position, spawnPoint.rotation);
            currentHorde1Enemies.Add(enemy);
            Inimigo enemyScript = enemy.GetComponent<Inimigo>();
            if (enemyScript != null)
            {
               
                enemyScript.OnDeath.AddListener(HandleHorde1EnemyDeath);
            }
            else Debug.LogWarning("Inimigo da Horda 1 spawnado não possui script 'Inimigo' com evento OnDeath!", enemy);
        }
    }

  
    public void HandleHorde1EnemyDeath(Inimigo enemy)
    {
        if (enemy == null) return; 
        int enemyInstanceId = enemy.GetInstanceID();
        Debug.Log($"HandleHorde1EnemyDeath chamado para ID: {enemyInstanceId}. Contador ANTES: {enemiesRemainingInHorde1}");

       
        if (!currentHorde1Enemies.Contains(enemy.gameObject))
        {
            Debug.LogWarning($"HandleHorde1EnemyDeath: ID {enemyInstanceId} já removido/não encontrado na lista currentHorde1Enemies. Pulando decremento.");
      
            enemy.OnDeath.RemoveListener(HandleHorde1EnemyDeath);
            return;
        }

        
        enemy.OnDeath.RemoveListener(HandleHorde1EnemyDeath);
        currentHorde1Enemies.Remove(enemy.gameObject);
        enemiesRemainingInHorde1--;
        Debug.Log($"ID {enemyInstanceId} processado. Contador DEPOIS: {enemiesRemainingInHorde1}");

        if (enemiesRemainingInHorde1 <= 0)
        {
            enemiesRemainingInHorde1 = 0;
            Debug.Log($"Chamando CheckHorde1Defeated (Contador={enemiesRemainingInHorde1})");
            CheckHorde1Defeated();
        }
    }

    void CheckHorde1Defeated()
    {
        Debug.Log($"CheckHorde1Defeated chamado. Contador={enemiesRemainingInHorde1}, Lista.Count={currentHorde1Enemies.Count}");
        
        if (enemiesRemainingInHorde1 > 0) return;

        if (currentHorde1Enemies.Count > 0)
        {
            Debug.LogError($"CheckHorde1Defeated: Disparando diálogo, MAS a lista de inimigos NÃO está vazia (Count: {currentHorde1Enemies.Count})! Limpando a lista agora.");
            currentHorde1Enemies.Clear(); 
        }

        Debug.Log("Horda 1 derrotada! Disparando diálogo pós-Horda 1.");
        if (postHorde1Dialogue != null) postHorde1Dialogue.TriggerDialogue();
        else Debug.LogError("Referência para postHorde1Dialogue é NULA!");
        onHorde1Defeated?.Invoke();
    
        currentHorde1Enemies.Clear();
    }

    private void HandleTrashCollected_SpawnRobots()
    {
       
        if (enemiesRemainingInHorde1 > 0 || enemiesRemainingInPeriferia > 0 || finalDialogueStarted || recyclingIsFull)
        {
            return;
        }
        if (robotPrefab == null || generalRobotSpawnPoints == null || generalRobotSpawnPoints.Length == 0) { Debug.LogWarning("GameEventManager: Prefab/Spawns de robô geral não configurados!", this); return; }
        Debug.Log("Lixo coletado! Spawning " + robotsPerCollection + " robôs de coleta.");
        for (int i = 0; i < robotsPerCollection; i++)
        {
            Transform spawnPoint = generalRobotSpawnPoints[Random.Range(0, generalRobotSpawnPoints.Length)];
            if (spawnPoint != null) { GameObject robot = Instantiate(robotPrefab, spawnPoint.position, spawnPoint.rotation); activeCollectionRobots.Add(robot); }
        }
    }

    private void TriggerAkunPathUnlock()
    {
        if (akunPathUnlocked) return;
        akunPathUnlocked = true;
        Debug.Log("Atingiu 50% de reciclagem! Liberando caminho para Periferia...");

        if (periferiaBarrier != null) periferiaBarrier.SetActive(false);
        else Debug.LogWarning("Tentativa de desativar PeriferiaBarrier, mas a referência não está configurada!");

        ShowUnlockMessage("Caminho para a Periferia liberado!", 10f);

        if (characterSwitcher != null) characterSwitcher.SetSwitchingEnabled(true);

        Debug.Log("Ativando seta para Periferia.");
        ActivateObjectiveArrow(periferiaTarget);

        onAkunPathUnlocked?.Invoke();
    }

    public void TriggerPeriferiaCombat()
    {
        if (!akunPathUnlocked || periferiaAreaReached) return;
        periferiaAreaReached = true;
        Debug.Log("Jogador entrou na área da Periferia. Iniciando combate.");

        DeactivateObjectiveArrow();
        SpawnPeriferiaRobots();
        onPeriferiaCombatStart?.Invoke();
    }

    void SpawnPeriferiaRobots()
    {
        currentPeriferiaEnemies.Clear();
        enemiesRemainingInPeriferia = periferiaRobotCount;
        if (robotPrefab == null) { Debug.LogError("Prefab do robô não configurado para Periferia!"); enemiesRemainingInPeriferia = 0; CheckPeriferiaDefeated(); return; }
        if (periferiaRobotSpawnPoints == null || periferiaRobotSpawnPoints.Length == 0) { Debug.LogError("Pontos de spawn da Periferia não configurados!"); enemiesRemainingInPeriferia = 0; CheckPeriferiaDefeated(); return; }
        Debug.Log("Spawning " + periferiaRobotCount + " inimigos na Periferia");
        for (int i = 0; i < periferiaRobotCount; i++)
        {
            Transform spawnPoint = periferiaRobotSpawnPoints[i % periferiaRobotSpawnPoints.Length];
            GameObject enemy = Instantiate(robotPrefab, spawnPoint.position, spawnPoint.rotation);
            currentPeriferiaEnemies.Add(enemy);
            Inimigo enemyScript = enemy.GetComponent<Inimigo>();
            if (enemyScript != null)
            {
              
                enemyScript.OnDeath.AddListener(HandlePeriferiaEnemyDeath);
            }
            else Debug.LogWarning("Inimigo da Periferia spawnado não possui script 'Inimigo' com evento OnDeath!", enemy);
        }
    }

    
    public void HandlePeriferiaEnemyDeath(Inimigo enemy)
    {
        if (enemy == null) return;
       
        if (!currentPeriferiaEnemies.Contains(enemy.gameObject)) return; 

        enemy.OnDeath.RemoveListener(HandlePeriferiaEnemyDeath);
        currentPeriferiaEnemies.Remove(enemy.gameObject);
        enemiesRemainingInPeriferia--;
        Debug.Log("Inimigo da PERIFERIA derrotado. Restantes: " + enemiesRemainingInPeriferia);
        if (enemiesRemainingInPeriferia <= 0)
        {
            enemiesRemainingInPeriferia = 0;
            CheckPeriferiaDefeated();
        }
    }

    void CheckPeriferiaDefeated()
    {
        if (enemiesRemainingInPeriferia > 0) return;
        Debug.Log("Combate na Periferia terminado!");
        currentPeriferiaEnemies.Clear();

        if (akunCharacter != null)
        {
            akunCharacter.SetActive(true);
            Debug.Log("Akun ativado após combate na Periferia.");
        }
        else { Debug.LogError("Tentativa de ativar Akun após combate, mas a referência não está configurada!"); }

        Debug.Log("Disparando diálogo de Akun.");
        if (akunDialogueTrigger != null) akunDialogueTrigger.TriggerDialogue();
        else Debug.LogError("Referência para akunDialogueTrigger é NULA!");
        onPeriferiaCombatEnd?.Invoke();
    }

    private void HandleRecyclingFull()
    {
        if (recyclingIsFull) return;
        recyclingIsFull = true;
        Debug.Log("GameEventManager: Recebeu evento OnRecyclingFull. Parando o Boss e preparando diálogo final.");

        if (bossController != null) { bossController.StopMovingPermanently(); }
        else { Debug.LogError("BossController não encontrado para mandar parar!"); }

        ShowApproachBossMessage();
        ActivateObjectiveArrow(bossTransform);

        if (finalDialogueTriggerObject != null)
        {
            finalDialogueTriggerObject.SetActive(true);
            Debug.Log("GameObject do trigger final habilitado.");
        }
        else { Debug.LogError("GameObject do trigger final não configurado!"); }

        onRecyclingFull?.Invoke();
    }

    public void ShowApproachBossMessage()
    {
        ShowUnlockMessage("O Boss parou! Aproxime-se para conversar.", 15f);
    }

    public void StartFinalDialogueSequence()
    {
        if (finalDialogueStarted) return;
        finalDialogueStarted = true;

        Debug.Log("GameEventManager: StartFinalDialogueSequence chamada. Iniciando diálogo final.");
        DeactivateObjectiveArrow();

        if (finalAwarenessDialogue != null)
        {
            finalAwarenessDialogue.TriggerDialogue();
            onFinalDialogueStart?.Invoke();
        }
        else
        {
            Debug.LogError("Tentando iniciar diálogo final, mas finalAwarenessDialogue não está configurado!");
  
            CompleteGame();
        }
    }

    private void CompleteGame()
    {
        Debug.Log("Jogo completado! Carregando cena de menu: " + menuSceneName);
        onGameComplete?.Invoke();
        if (!string.IsNullOrEmpty(menuSceneName)) { SceneManager.LoadScene(menuSceneName); }
        else { Debug.LogError("Nome da cena de menu não configurado no GameEventManager!"); }
    }


    void ActivateObjectiveArrow(Transform target)
    {
        if (objectiveArrow != null && target != null)
        {
            objectiveArrow.target = target;
            objectiveArrow.SetArrowActive(true);
            Debug.Log("Seta de objetivo ativada para: " + target.name);
        }
        else
        {
            Debug.LogWarning("Não foi possível ativar a seta: ObjectiveArrow ou Target nulo.");
        }
    }

    void DeactivateObjectiveArrow()
    {
        if (objectiveArrow != null)
        {
            objectiveArrow.SetArrowActive(false);
            Debug.Log("Seta de objetivo desativada.");
        }
    }

    void ShowUnlockMessage(string message, float duration)
    {
        if (unlockMessagePanel != null && unlockMessageText != null)
        {
            unlockMessageText.text = message;
            unlockMessagePanel.SetActive(true);
          
            if (hideMessageCoroutine != null) StopCoroutine(hideMessageCoroutine);
    
            hideMessageCoroutine = StartCoroutine(HideMessageAfterDelay(duration));
        }
        else
        {
            Debug.LogWarning("Painel/Texto de Mensagem de Desbloqueio/Aviso não configurado!");
        }
    }

    
    IEnumerator HideMessageAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (unlockMessagePanel != null) unlockMessagePanel.SetActive(false);
    }

   
    void OnDestroy()
    {
        if (EnvironmentalManager.Instance != null)
        {
            EnvironmentalManager.Instance.OnTrashCollected.RemoveListener(HandleTrashCollected_SpawnRobots);
            EnvironmentalManager.Instance.OnRecyclingHalfFull.RemoveListener(TriggerAkunPathUnlock);
            EnvironmentalManager.Instance.OnRecyclingFull.RemoveListener(HandleRecyclingFull);
        }
        
        foreach (var enemyGO in currentHorde1Enemies)
        {
            if (enemyGO != null)
            {
                Inimigo enemyScript = enemyGO.GetComponent<Inimigo>();
                if (enemyScript != null) enemyScript.OnDeath.RemoveListener(HandleHorde1EnemyDeath);
            }
        }
        foreach (var enemyGO in currentPeriferiaEnemies)
        {
            if (enemyGO != null)
            {
                Inimigo enemyScript = enemyGO.GetComponent<Inimigo>();
                if (enemyScript != null) enemyScript.OnDeath.RemoveListener(HandlePeriferiaEnemyDeath);
            }
        }
    }
}

