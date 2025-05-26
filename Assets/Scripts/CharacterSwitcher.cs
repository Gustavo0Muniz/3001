using UnityEngine;
using Unity.Cinemachine;

// Este script gerencia a troca entre personagens jogáveis,
// ativando/desativando os scripts de controle do jogador (PlayerController, HenryController, etc.)
// e o AllyController conforme necessário.
public class CharacterSwitcher : MonoBehaviour
{
    [Header("Personagens")]
    [Tooltip("Lista dos GameObjects dos personagens que podem ser controlados ou agir como aliados")]
    public GameObject[] characters;
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

        // Validação da Lista de Personagens
        if (characters == null || characters.Length == 0)
        {
            Debug.LogError("CharacterSwitcher: Nenhum personagem atribuído à lista 'characters'!", this);
            this.enabled = false;
            return;
        }

        // Inicialização: Define o primeiro personagem como ativo
        // e os outros como aliados (se tiverem AllyController)
        for (int i = 0; i < characters.Length; i++)
        {
            if (characters[i] != null)
            {
                // Tenta encontrar qualquer script de controle de jogador
                MonoBehaviour playerControlScript = GetPlayerControlScript(characters[i]);
                AllyController ac = characters[i].GetComponent<AllyController>();

                if (i == currentCharacterIndex) // Personagem inicial ativo
                {
                    SetPlayerControlActive(playerControlScript, true);
                    if (ac != null) ac.enabled = false;
                }
                else // Outros personagens começam como aliados
                {
                    SetPlayerControlActive(playerControlScript, false);
                    if (ac != null) ac.enabled = true;
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
        SwitchToCharacter(nextIndex);
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

        // --- Desativa o Personagem Atual --- 
        if (characters[currentCharacterIndex] != null)
        {
            MonoBehaviour currentPC = GetPlayerControlScript(characters[currentCharacterIndex]);
            AllyController currentAC = characters[currentCharacterIndex].GetComponent<AllyController>();

            SetPlayerControlActive(currentPC, false); // Desativa controle do jogador
            if (currentAC != null) currentAC.enabled = true;  // Ativa controle de IA (se existir)
        }

        // --- Ativa o Novo Personagem --- 
        currentCharacterIndex = newIndex;
        GameObject newChar = characters[currentCharacterIndex];

        MonoBehaviour newPC = GetPlayerControlScript(newChar);
        AllyController newAC = newChar.GetComponent<AllyController>();

        SetPlayerControlActive(newPC, true);   // Ativa controle do jogador
        if (newAC != null) newAC.enabled = false;  // Desativa controle de IA (se existir)

        UpdateCameraTarget(newChar.transform);

        Debug.Log("CharacterSwitcher: Trocado para " + newChar.name);
    }

    // Função auxiliar para encontrar o script de controle do jogador (PlayerController ou HenryController)
    MonoBehaviour GetPlayerControlScript(GameObject character)
    {
        if (character == null) return null;

        // Tenta encontrar HenryController primeiro
        HenryController hc = character.GetComponent<HenryController>();
        if (hc != null) return hc;

        // Se não encontrar, tenta encontrar PlayerController
        PlayerController pc = character.GetComponent<PlayerController>();
        if (pc != null) return pc;

        // Adicione aqui outras verificações se tiver mais scripts de controle
        // Ex: DracoController dc = character.GetComponent<DracoController>();
        // if (dc != null) return dc;

        // Se nenhum for encontrado
        // Debug.LogWarning("CharacterSwitcher: Nenhum script de controle de jogador (HenryController, PlayerController) encontrado em " + character.name, this);
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

