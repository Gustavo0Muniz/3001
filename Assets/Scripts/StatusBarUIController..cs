using UnityEngine;
using UnityEngine.UI;

// Controla a exibi��o de uma barra de status na UI (Lixo ou Reciclagem)
public class StatusBarUIController : MonoBehaviour
{
    [Header("Refer�ncias da UI")]
    [SerializeField] private Image statusBarImage; // A imagem da barra (tipo Filled)
    // [SerializeField] private Text statusText; // Opcional: Texto para mostrar valor (ex: 50/100)

    [Header("Tipo de Barra")]
    [SerializeField] private BarType barType = BarType.Recycling; // Define qual barra este script controla

    private enum BarType { Trash, Recycling }

    void Start()
    {
        if (statusBarImage == null)
        {
            Debug.LogError("StatusBarUIController: Imagem da barra n�o configurada!", this);
            enabled = false;
            return;
        }

        // Encontra o EnvironmentalManager e se inscreve nos eventos corretos
        if (EnvironmentalManager.Instance != null)
        {
            if (barType == BarType.Trash)
            {
                EnvironmentalManager.Instance.OnTrashUpdated.AddListener(UpdateBar);
                // Atualiza o valor inicial
                UpdateBar(EnvironmentalManager.Instance.GetCurrentTrash(), EnvironmentalManager.Instance.GetMaxTrash());
            }
            else // Recycling
            {
                EnvironmentalManager.Instance.OnRecyclingUpdated.AddListener(UpdateBar);
                // Atualiza o valor inicial
                UpdateBar(EnvironmentalManager.Instance.GetCurrentRecycling(), EnvironmentalManager.Instance.GetMaxRecycling());
            }
        }
        else
        {
            Debug.LogError("StatusBarUIController: Inst�ncia do EnvironmentalManager n�o encontrada!", this);
            enabled = false;
        }
    }

    // M�todo chamado pelos eventos do EnvironmentalManager
    public void UpdateBar(float currentValue, float maxValue)
    {
        if (statusBarImage != null)
        {
            if (maxValue > 0)
            {
                statusBarImage.fillAmount = currentValue / maxValue;
            }
            else
            {
                statusBarImage.fillAmount = 0; // Evita divis�o por zero
            }

            // Atualiza o texto opcional
            // if (statusText != null)
            // {
            //     statusText.text = $"{currentValue:F0}/{maxValue:F0}"; // Formata sem casas decimais
            // }
        }
    }

    // Limpa a inscri��o no evento ao destruir o objeto para evitar memory leaks
    void OnDestroy()
    {
        if (EnvironmentalManager.Instance != null)
        {
            if (barType == BarType.Trash)
            {
                EnvironmentalManager.Instance.OnTrashUpdated.RemoveListener(UpdateBar);
            }
            else // Recycling
            {
                EnvironmentalManager.Instance.OnRecyclingUpdated.RemoveListener(UpdateBar);
            }
        }
    }
}

