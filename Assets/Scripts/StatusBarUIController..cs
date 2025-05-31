using UnityEngine;
using UnityEngine.UI;

// Controla a exibição de uma barra de status na UI (Lixo ou Reciclagem)
public class StatusBarUIController : MonoBehaviour
{
    [Header("Referências da UI")]
    [SerializeField] private Image statusBarImage; // A imagem da barra (tipo Filled)
    // [SerializeField] private Text statusText; // Opcional: Texto para mostrar valor (ex: 50/100)

    [Header("Tipo de Barra")]
    [SerializeField] private BarType barType = BarType.Recycling; // Define qual barra este script controla

    private enum BarType { Trash, Recycling }

    void Start()
    {
        if (statusBarImage == null)
        {
            Debug.LogError("StatusBarUIController: Imagem da barra não configurada!", this);
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
            Debug.LogError("StatusBarUIController: Instância do EnvironmentalManager não encontrada!", this);
            enabled = false;
        }
    }

    // Método chamado pelos eventos do EnvironmentalManager
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
                statusBarImage.fillAmount = 0; // Evita divisão por zero
            }

            // Atualiza o texto opcional
            // if (statusText != null)
            // {
            //     statusText.text = $"{currentValue:F0}/{maxValue:F0}"; // Formata sem casas decimais
            // }
        }
    }

    // Limpa a inscrição no evento ao destruir o objeto para evitar memory leaks
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

