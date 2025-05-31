// CharacterSwitcher.cs (v2 - Handles Inactive Characters)
using UnityEngine;
using Unity.Cinemachine;

public class CharacterSwitcher : MonoBehaviour
{
    [Header("Personagens")]
    [Tooltip("Lista dos GameObjects dos personagens que podem ser controlados ou agir como aliados")]
    public GameObject[] characters;

    [Header("UI de Corações")]
    [Tooltip("Lista dos GameObjects das UIs de corações. A ordem DEVE corresponder à lista 'characters' acima!")]
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

        if (characters == null || characters.Length == 0)
        {
            Debug.LogError("CharacterSwitcher: Nenhum personagem atribuído à lista 'characters'!", this);
            this.enabled = false;
            return;
        }
        if (characterUIs == null || characterUIs.Length != characters.Length)
        {
            Debug.LogError("CharacterSwitcher: O array 'characterUIs' está nulo ou tem tamanho diferente do array 'characters'! Verifique as referências no Inspector.", this);
            this.enabled = false;
            return;
        }

        // Inicialização: Define o primeiro personagem ativo e sua UI como ativos
        // Os outros são desativados ou configurados como aliados
        // <<< MODIFICADO: A ativação inicial agora respeita o estado inicial do GameObject >>>
        bool foundFirstActive = false;
        for (int i = 0; i < characters.Length; i++)
        {
            if (characters[i] != null)
            {
                bool shouldBeActive = characters[i].activeSelf && !foundFirstActive; // O primeiro ativo na lista será o inicial

                MonoBehaviour playerControlScript = GetPlayerControlScript(characters[i]);
                AllyController ac = characters[i].GetComponent<AllyController>();

                SetPlayerControlActive(playerControlScript, shouldBeActive);
                if (ac != null) ac.enabled = !shouldBeActive && characters[i].activeSelf; // IA só ativa se o GO estiver ativo mas não for o player

                if (characterUIs[i] != null)
                {
                    characterUIs[i].SetActive(shouldBeActive);
                }
                else
                {
                    Debug.LogWarning("CharacterSwitcher: UI não configurada para o personagem no índice " + i + " (" + characters[i].name + ") no array 'characterUIs'.", this);
                }

                if (shouldBeActive)
                {
                    currentCharacterIndex = i;
                    UpdateCameraTarget(characters[i].transform);
                    foundFirstActive = true;
                    Debug.Log("CharacterSwitcher: Personagem inicial definido como: " + characters[i].name);
                }
                // Garante que personagens inativos no início tenham seus controles desabilitados
                else if (!characters[i].activeSelf)
                {
                    SetPlayerControlActive(playerControlScript, false);
                    if (ac != null) ac.enabled = false;
                    if (characterUIs[i] != null) characterUIs[i].SetActive(false);
                }
            }
        }
        if (!foundFirstActive && characters.Length > 0)
        {
            Debug.LogError("CharacterSwitcher: Nenhum personagem inicial ATIVO encontrado na lista 'characters'! Verifique se pelo menos um está ativo na cena ao iniciar.", this);
            this.enabled = false;
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
        if (characters.Length <= 1) return; // Não faz sentido trocar se só tem um

        int nextIndex = currentCharacterIndex;
        int attempts = 0;
        do
        {
            nextIndex = (nextIndex + 1) % characters.Length;
            attempts++;
            // <<< MODIFICADO: Pula personagens nulos OU INATIVOS >>>
        } while ((characters[nextIndex] == null || !characters[nextIndex].activeSelf) && attempts < characters.Length);

        // Só troca se encontrou um personagem válido e ativo que não seja o atual
        if (characters[nextIndex] != null && characters[nextIndex].activeSelf && nextIndex != currentCharacterIndex)
        {
            SwitchToCharacter(nextIndex);
        }
        else if (attempts >= characters.Length)
        {
            Debug.LogWarning("CharacterSwitcher: Não foi possível encontrar outro personagem ATIVO para trocar.", this);
        }
    }

    public void SwitchToCharacter(int newIndex)
    {
        if (newIndex < 0 || newIndex >= characters.Length || newIndex == currentCharacterIndex)
        {
            return;
        }
        // <<< MODIFICADO: Verifica se o personagem alvo existe E ESTÁ ATIVO >>>
        if (characters[newIndex] == null || !characters[newIndex].activeSelf)
        {
            Debug.LogWarning("CharacterSwitcher: Tentando trocar para um personagem nulo ou inativo no índice " + newIndex, this);
            return;
        }

        // --- Desativa o Personagem e UI Atual --- 
        if (characters[currentCharacterIndex] != null && characters[currentCharacterIndex].activeSelf) // Só desativa se o atual for válido e ativo
        {
            MonoBehaviour currentPC = GetPlayerControlScript(characters[currentCharacterIndex]);
            AllyController currentAC = characters[currentCharacterIndex].GetComponent<AllyController>();

            SetPlayerControlActive(currentPC, false);
            if (currentAC != null) currentAC.enabled = true; // Ativa IA (se existir)

            if (characterUIs[currentCharacterIndex] != null)
            {
                characterUIs[currentCharacterIndex].SetActive(false);
            }
        }

        // --- Ativa o Novo Personagem e UI --- 
        currentCharacterIndex = newIndex;
        GameObject newChar = characters[currentCharacterIndex]; // Já sabemos que é válido e ativo

        MonoBehaviour newPC = GetPlayerControlScript(newChar);
        AllyController newAC = newChar.GetComponent<AllyController>();

        SetPlayerControlActive(newPC, true);   // Ativa controle do jogador
        if (newAC != null) newAC.enabled = false;  // Desativa controle de IA (se existir)

        if (characterUIs[currentCharacterIndex] != null)
        {
            characterUIs[currentCharacterIndex].SetActive(true);
        }

        UpdateCameraTarget(newChar.transform);

        Debug.Log("CharacterSwitcher: Trocado para " + newChar.name);
    }

    // Função auxiliar para encontrar o script de controle do jogador
    // <<< MODIFICADO: Adicionado AkunController e DracoController >>>
    MonoBehaviour GetPlayerControlScript(GameObject character)
    {
        if (character == null) return null;

        AkunController akun = character.GetComponent<AkunController>();
        if (akun != null) return akun;

        DracoController draco = character.GetComponent<DracoController>();
        if (draco != null) return draco;

        HenryController hc = character.GetComponent<HenryController>();
        if (hc != null) return hc;

        PlayerController pc = character.GetComponent<PlayerController>();
        if (pc != null) return pc;

        // Adicione outros scripts de controle aqui se necessário

        return null;
    }

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
            // <<< MODIFICADO: Verifica se o personagem existe, tem o nome E ESTÁ ATIVO >>>
            if (characters[i] != null && characters[i].name == characterName && characters[i].activeSelf)
            {
                SwitchToCharacter(i);
                return;
            }
        }
        Debug.LogWarning("CharacterSwitcher: Personagem ATIVO com nome '" + characterName + "' não encontrado.", this);
    }
}

