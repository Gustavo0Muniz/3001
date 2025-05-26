using UnityEngine;
using Unity.Cinemachine;

// Este script gerencia a troca entre personagens jogáveis,
// ativando/desativando os scripts de controle do jogador (PlayerController, HenryController, etc.)
// e o AllyController conforme necessário.
// <<< MODIFICADO: Adicionado gerenciamento de UI de corações >>>
public class CharacterSwitcher : MonoBehaviour
{
    [Header("Personagens")]
    [Tooltip("Lista dos GameObjects dos personagens que podem ser controlados ou agir como aliados")]
    public GameObject[] characters;

    // <<< ADICIONADO: Array paralelo para as UIs de Corações >>>
    [Header("UI de Corações")]
    [Tooltip("Lista dos GameObjects das UIs de corações. A ordem DEVE corresponder à lista 'characters' acima! Ex: characters[0] usa characterUIs[0]")]
    public GameObject[] characterUIs;

    private int currentCharacterIndex = 0;

    [Header("Câmera")]
    [Tooltip("Câmera Cinemachine que seguirá o personagem ativo")]
    public CinemachineCamera cineCamera;

    [Header("Controle de Troca")]
    [Tooltip("Permite habilitar ou desabilitar a troca de personagens via input (Tab)")]
    public bool switchingEnabled = true;

    void Start()
    {
        // Configuração da Câmera
        if (cineCamera == null)
        {
            var brain = FindObjectOfType<CinemachineBrain>();
            if (brain != null && brain.ActiveVirtualCamera is CinemachineCamera vCam)
            {
                cineCamera = vCam;
                Debug.LogWarning("CharacterSwitcher: Nenhuma CinemachineCamera atribuída, usando a câmera ativa: " + cineCamera.name, this);
            }
            else
            {
                Debug.LogError("CharacterSwitcher: Nenhuma CinemachineCamera encontrada ou atribuída na cena!", this);
                this.enabled = false;
                return;
            }
        }

        // Validação da Lista de Personagens e UIs
        if (characters == null || characters.Length == 0)
        {
            Debug.LogError("CharacterSwitcher: Nenhum personagem atribuído à lista 'characters'!", this);
            this.enabled = false;
            return;
        }
        // <<< ADICIONADO: Validação do array de UIs >>>
        if (characterUIs == null || characterUIs.Length != characters.Length)
        {
            Debug.LogError("CharacterSwitcher: O array 'characterUIs' está nulo ou tem tamanho diferente do array 'characters'! Verifique as referências no Inspector.", this);
            this.enabled = false;
            return;
        }

        // Inicialização: Define o primeiro personagem e sua UI como ativos
        // e os outros como aliados (se tiverem AllyController) e suas UIs inativas
        for (int i = 0; i < characters.Length; i++)
        {
            if (characters[i] != null)
            {
                MonoBehaviour playerControlScript = GetPlayerControlScript(characters[i]);
                AllyController ac = characters[i].GetComponent<AllyController>();
                bool isActiveCharacter = (i == currentCharacterIndex);

                SetPlayerControlActive(playerControlScript, isActiveCharacter);
                if (ac != null) ac.enabled = !isActiveCharacter;

                // <<< MODIFICADO: Ativa/Desativa a UI correspondente >>>
                if (characterUIs[i] != null)
                {
                    characterUIs[i].SetActive(isActiveCharacter);
                }
                else
                {
                    Debug.LogWarning("CharacterSwitcher: UI não configurada para o personagem no índice " + i + " (" + characters[i].name + ") no array 'characterUIs'.", this);
                }
            }
        }

        // Define o alvo inicial da câmera
        if (characters[currentCharacterIndex] != null)
        {
            UpdateCameraTarget(characters[currentCharacterIndex].transform);
        }
    }

    void Update()
    {
        if (switchingEnabled && Input.GetKeyDown(KeyCode.Tab))
        {
            SwitchToNextCharacter();
        }
    }

    void SwitchToNextCharacter()
    {
        int nextIndex = (currentCharacterIndex + 1) % characters.Length;
        // Pula personagens nulos se houver
        int attempts = 0;
        while (characters[nextIndex] == null && attempts < characters.Length)
        {
            nextIndex = (nextIndex + 1) % characters.Length;
            attempts++;
        }
        if (characters[nextIndex] != null) // Só troca se encontrar um válido
        {
            SwitchToCharacter(nextIndex);
        }
        else
        {
            Debug.LogWarning("CharacterSwitcher: Não foi possível encontrar um próximo personagem válido para trocar.", this);
        }
    }

    public void SwitchToCharacter(int newIndex)
    {
        if (newIndex < 0 || newIndex >= characters.Length || newIndex == currentCharacterIndex)
        {
            return;
        }
        if (characters[newIndex] == null)
        {
            Debug.LogError("CharacterSwitcher: Tentando trocar para um personagem nulo no índice " + newIndex, this);
            return;
        }

        // --- Desativa o Personagem e UI Atual --- 
        if (characters[currentCharacterIndex] != null)
        {
            MonoBehaviour currentPC = GetPlayerControlScript(characters[currentCharacterIndex]);
            AllyController currentAC = characters[currentCharacterIndex].GetComponent<AllyController>();

            SetPlayerControlActive(currentPC, false); // Desativa controle do jogador
            if (currentAC != null) currentAC.enabled = true;  // Ativa controle de IA (se existir)

            // <<< ADICIONADO: Desativa a UI atual >>>
            if (characterUIs[currentCharacterIndex] != null)
            {
                characterUIs[currentCharacterIndex].SetActive(false);
            }
        }

        // --- Ativa o Novo Personagem e UI --- 
        currentCharacterIndex = newIndex;
        GameObject newChar = characters[currentCharacterIndex];

        MonoBehaviour newPC = GetPlayerControlScript(newChar);
        AllyController newAC = newChar.GetComponent<AllyController>();

        SetPlayerControlActive(newPC, true);   // Ativa controle do jogador
        if (newAC != null) newAC.enabled = false;  // Desativa controle de IA (se existir)

        // <<< ADICIONADO: Ativa a nova UI >>>
        if (characterUIs[currentCharacterIndex] != null)
        {
            characterUIs[currentCharacterIndex].SetActive(true);
        }

        UpdateCameraTarget(newChar.transform);

        Debug.Log("CharacterSwitcher: Trocado para " + newChar.name);
    }

    // Função auxiliar para encontrar o script de controle do jogador (PlayerController ou HenryController)
    MonoBehaviour GetPlayerControlScript(GameObject character)
    {
        if (character == null) return null;
        HenryController hc = character.GetComponent<HenryController>();
        if (hc != null) return hc;
        PlayerController pc = character.GetComponent<PlayerController>();
        if (pc != null) return pc;
        return null;
    }

    // Função auxiliar para ativar/desativar o script de controle encontrado
    void SetPlayerControlActive(MonoBehaviour playerControlScript, bool active)
    {
        if (playerControlScript != null)
        {
            playerControlScript.enabled = active;
        }
    }

    void UpdateCameraTarget(Transform newTarget)
    {
        if (cineCamera != null && newTarget != null)
        {
            cineCamera.Follow = newTarget;
            cineCamera.LookAt = newTarget;
        }
    }

    public void SetSwitchingEnabled(bool enabled)
    {
        switchingEnabled = enabled;
        Debug.Log("CharacterSwitcher: Troca via input " + (enabled ? "habilitada" : "desabilitada"));
    }

    public void SwitchToCharacterByName(string characterName)
    {
        for (int i = 0; i < characters.Length; i++)
        {
            if (characters[i] != null && characters[i].name == characterName)
            {
                SwitchToCharacter(i);
                return;
            }
        }
        Debug.LogWarning("CharacterSwitcher: Personagem com nome '" + characterName + "' não encontrado.", this);
    }
}

