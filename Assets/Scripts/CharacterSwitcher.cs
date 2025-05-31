// CharacterSwitcher.cs (v2 - Handles Inactive Characters)
using UnityEngine;
using Unity.Cinemachine;

public class CharacterSwitcher : MonoBehaviour
{
    [Header("Personagens")]
    [Tooltip("Lista dos GameObjects dos personagens que podem ser controlados ou agir como aliados")]
    public GameObject[] characters;

    [Header("UI de Cora��es")]
    [Tooltip("Lista dos GameObjects das UIs de cora��es. A ordem DEVE corresponder � lista 'characters' acima!")]
    public GameObject[] characterUIs;

    private int currentCharacterIndex = 0;

    [Header("C�mera")]
    [Tooltip("C�mera Cinemachine que seguir� o personagem ativo")]
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
                Debug.LogWarning("CharacterSwitcher: Nenhuma CinemachineCamera atribu�da, usando a c�mera ativa: " + cineCamera.name, this);
            }
            else
            {
                Debug.LogError("CharacterSwitcher: Nenhuma CinemachineCamera encontrada ou atribu�da na cena!", this);
                this.enabled = false;
                return;
            }
        }

        if (characters == null || characters.Length == 0)
        {
            Debug.LogError("CharacterSwitcher: Nenhum personagem atribu�do � lista 'characters'!", this);
            this.enabled = false;
            return;
        }
        if (characterUIs == null || characterUIs.Length != characters.Length)
        {
            Debug.LogError("CharacterSwitcher: O array 'characterUIs' est� nulo ou tem tamanho diferente do array 'characters'! Verifique as refer�ncias no Inspector.", this);
            this.enabled = false;
            return;
        }

        // Inicializa��o: Define o primeiro personagem ativo e sua UI como ativos
        // Os outros s�o desativados ou configurados como aliados
        // <<< MODIFICADO: A ativa��o inicial agora respeita o estado inicial do GameObject >>>
        bool foundFirstActive = false;
        for (int i = 0; i < characters.Length; i++)
        {
            if (characters[i] != null)
            {
                bool shouldBeActive = characters[i].activeSelf && !foundFirstActive; // O primeiro ativo na lista ser� o inicial

                MonoBehaviour playerControlScript = GetPlayerControlScript(characters[i]);
                AllyController ac = characters[i].GetComponent<AllyController>();

                SetPlayerControlActive(playerControlScript, shouldBeActive);
                if (ac != null) ac.enabled = !shouldBeActive && characters[i].activeSelf; // IA s� ativa se o GO estiver ativo mas n�o for o player

                if (characterUIs[i] != null)
                {
                    characterUIs[i].SetActive(shouldBeActive);
                }
                else
                {
                    Debug.LogWarning("CharacterSwitcher: UI n�o configurada para o personagem no �ndice " + i + " (" + characters[i].name + ") no array 'characterUIs'.", this);
                }

                if (shouldBeActive)
                {
                    currentCharacterIndex = i;
                    UpdateCameraTarget(characters[i].transform);
                    foundFirstActive = true;
                    Debug.Log("CharacterSwitcher: Personagem inicial definido como: " + characters[i].name);
                }
                // Garante que personagens inativos no in�cio tenham seus controles desabilitados
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
            Debug.LogError("CharacterSwitcher: Nenhum personagem inicial ATIVO encontrado na lista 'characters'! Verifique se pelo menos um est� ativo na cena ao iniciar.", this);
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
        if (characters.Length <= 1) return; // N�o faz sentido trocar se s� tem um

        int nextIndex = currentCharacterIndex;
        int attempts = 0;
        do
        {
            nextIndex = (nextIndex + 1) % characters.Length;
            attempts++;
            // <<< MODIFICADO: Pula personagens nulos OU INATIVOS >>>
        } while ((characters[nextIndex] == null || !characters[nextIndex].activeSelf) && attempts < characters.Length);

        // S� troca se encontrou um personagem v�lido e ativo que n�o seja o atual
        if (characters[nextIndex] != null && characters[nextIndex].activeSelf && nextIndex != currentCharacterIndex)
        {
            SwitchToCharacter(nextIndex);
        }
        else if (attempts >= characters.Length)
        {
            Debug.LogWarning("CharacterSwitcher: N�o foi poss�vel encontrar outro personagem ATIVO para trocar.", this);
        }
    }

    public void SwitchToCharacter(int newIndex)
    {
        if (newIndex < 0 || newIndex >= characters.Length || newIndex == currentCharacterIndex)
        {
            return;
        }
        // <<< MODIFICADO: Verifica se o personagem alvo existe E EST� ATIVO >>>
        if (characters[newIndex] == null || !characters[newIndex].activeSelf)
        {
            Debug.LogWarning("CharacterSwitcher: Tentando trocar para um personagem nulo ou inativo no �ndice " + newIndex, this);
            return;
        }

        // --- Desativa o Personagem e UI Atual --- 
        if (characters[currentCharacterIndex] != null && characters[currentCharacterIndex].activeSelf) // S� desativa se o atual for v�lido e ativo
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
        GameObject newChar = characters[currentCharacterIndex]; // J� sabemos que � v�lido e ativo

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

    // Fun��o auxiliar para encontrar o script de controle do jogador
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

        // Adicione outros scripts de controle aqui se necess�rio

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
            // <<< MODIFICADO: Verifica se o personagem existe, tem o nome E EST� ATIVO >>>
            if (characters[i] != null && characters[i].name == characterName && characters[i].activeSelf)
            {
                SwitchToCharacter(i);
                return;
            }
        }
        Debug.LogWarning("CharacterSwitcher: Personagem ATIVO com nome '" + characterName + "' n�o encontrado.", this);
    }
}

