using UnityEngine;
using Unity.Cinemachine;

public class CharacterSwitcher : MonoBehaviour
{
    [Header("Personagens")]
    public GameObject[] characters;
    private int currentCharacterIndex = 0;

    [Header("C�mera")]
    public CinemachineCamera cineCamera;



    void Start()
    {
        // Verifica��o de seguran�a
        if (cineCamera == null)
        {
            cineCamera = FindObjectOfType<CinemachineCamera>();
            if (cineCamera == null)
            {
                Debug.LogError("Nenhuma CinemachineVirtualCamera encontrada na cena!");
                return;
            }
        }

        if (characters.Length == 0)
        {
            Debug.LogError("Nenhum personagem atribu�do ao CharacterSwitcher!");
            return;
        }

        // Inicializa��o
        SwitchToCharacter(0);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab) || Input.GetKeyDown(KeyCode.Q))
        {
            SwitchToNextCharacter();
        }
    }

    void SwitchToNextCharacter()
    {
        int nextIndex = (currentCharacterIndex + 1) % characters.Length;
        SwitchToCharacter(nextIndex);
    }

    void SwitchToCharacter(int newIndex)
    {
        if (newIndex < 0 || newIndex >= characters.Length) return;

        // Desativa personagem atual
        characters[currentCharacterIndex].GetComponent<PlayerController>().isActive = false;

        // Ativa novo personagem
        currentCharacterIndex = newIndex;
        GameObject currentChar = characters[currentCharacterIndex];
        currentChar.GetComponent<PlayerController>().isActive = true;

        // Atualiza c�mera
        UpdateCameraTarget(currentChar.transform);
    }

    void UpdateCameraTarget(Transform newTarget)
    {
        cineCamera.Follow = newTarget;
        cineCamera.LookAt = newTarget;
    }

}