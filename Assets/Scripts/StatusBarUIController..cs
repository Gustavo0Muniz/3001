using UnityEngine;
using UnityEngine.UI;

public class StatusBarUIController : MonoBehaviour
{
    [Header("Referências da UI")]
    [SerializeField] private Image statusBarImage; 

    [Header("Tipo de Barra")]
    [SerializeField] private BarType barType = BarType.Recycling; 

    private enum BarType { Trash, Recycling }

    void Start()
    {
        if (statusBarImage == null)
        {
            Debug.LogError("StatusBarUIController: Imagem da barra não configurada!", this);
            enabled = false;
            return;
        }

        if (EnvironmentalManager.Instance != null)
        {
            if (barType == BarType.Trash)
            {
                EnvironmentalManager.Instance.OnTrashUpdated.AddListener(UpdateBar);
                UpdateBar(EnvironmentalManager.Instance.GetCurrentTrash(), EnvironmentalManager.Instance.GetMaxTrash());
            }
            else 
            {
                EnvironmentalManager.Instance.OnRecyclingUpdated.AddListener(UpdateBar);
                UpdateBar(EnvironmentalManager.Instance.GetCurrentRecycling(), EnvironmentalManager.Instance.GetMaxRecycling());
            }
        }
        else
        {
            Debug.LogError("StatusBarUIController: Instância do EnvironmentalManager não encontrada!", this);
            enabled = false;
        }
    }

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
                statusBarImage.fillAmount = 0; 
            }

            
        }
    }

    
    void OnDestroy()
    {
        if (EnvironmentalManager.Instance != null)
        {
            if (barType == BarType.Trash)
            {
                EnvironmentalManager.Instance.OnTrashUpdated.RemoveListener(UpdateBar);
            }
            else
            {
                EnvironmentalManager.Instance.OnRecyclingUpdated.RemoveListener(UpdateBar);
            }
        }
    }
}

