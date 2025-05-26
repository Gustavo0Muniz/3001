using UnityEngine;
using UnityEngine.UI;
using Unity.Cinemachine; // Adicionado para acessar CinemachineBrain

public class ObjectiveArrow : MonoBehaviour
{
    [Header("Refer�ncias")]
    [Tooltip("Transform do objeto que a seta deve apontar (Ex: Pr�xima �rea, NPC)")]
    public Transform target;
    [Tooltip("Transform do jogador para refer�ncia de posi��o (opcional, usar� a c�mera se nulo)")]
    public Transform playerReference;
    [Tooltip("RectTransform da imagem da seta na UI")]
    public RectTransform arrowRectTransform;
    [Tooltip("C�mera usada para converter posi��es do mundo para a tela (ser� detectada automaticamente se deixado vazio)")]
    public Camera uiCamera;

    [Header("Configura��es Visuais")]
    [Tooltip("Dist�ncia da borda da tela onde a seta ser� posicionada")]
    public float borderMargin = 50f;
    [Tooltip("Esconder a seta quando o alvo estiver vis�vel na tela?")]
    public bool hideWhenTargetVisible = true;

    private bool isArrowActive = false;

    void Start()
    {
        // Tenta encontrar refer�ncias se n�o foram atribu�das
        if (arrowRectTransform == null)
        {
            arrowRectTransform = GetComponent<RectTransform>();
        }
        if (playerReference == null)
        {
            // Se n�o houver refer�ncia do jogador, usaremos a posi��o da c�mera como refer�ncia no Update
            Debug.LogWarning("PlayerReference n�o atribu�do em ObjectiveArrow. A posi��o da c�mera ser� usada para calcular a dire��o.", this);
        }

        // --- L�gica de Detec��o Autom�tica da C�mera --- 
        if (uiCamera == null) // Tenta detectar automaticamente apenas se n�o foi atribu�da no Inspector
        {
            uiCamera = Camera.main; // Tenta pegar a c�mera com a tag "MainCamera"

            if (uiCamera == null) // Se ainda n�o encontrou (sem tag MainCamera)
            {
                // Tenta pegar a c�mera do Cinemachine Brain ativo
                CinemachineBrain brain = FindObjectOfType<CinemachineBrain>();
                if (brain != null && brain.OutputCamera != null)
                {
                    uiCamera = brain.OutputCamera;
                    Debug.Log("ObjectiveArrow: C�mera principal n�o encontrada ou sem tag 'MainCamera'. Usando c�mera do Cinemachine Brain.", this);
                }
                else
                {
                    // �ltima tentativa: pegar qualquer c�mera na cena (pode n�o ser a ideal)
                    uiCamera = FindObjectOfType<Camera>();
                    if (uiCamera != null)
                    {
                        Debug.LogWarning("ObjectiveArrow: Nenhuma c�mera principal ou Cinemachine Brain encontrada. Usando uma c�mera aleat�ria encontrada na cena: " + uiCamera.name, this);
                    }
                    else
                    {
                        Debug.LogError("ObjectiveArrow: Nenhuma c�mera encontrada na cena! A seta n�o funcionar�.", this);
                    }
                }
            }
            else
            {
                Debug.Log("ObjectiveArrow: Usando Camera.main (c�mera com tag 'MainCamera').", this);
            }
        }
        else
        {
            Debug.Log("ObjectiveArrow: Usando c�mera atribu�da manualmente no Inspector: " + uiCamera.name, this);
        }
        // --- Fim da L�gica de Detec��o Autom�tica --- 

        // Come�a com a seta desativada
        SetArrowActive(false);
    }

    void Update()
    {
        if (!isArrowActive || target == null || arrowRectTransform == null || uiCamera == null)
        {
            if (arrowRectTransform != null && arrowRectTransform.gameObject.activeSelf)
            {
                arrowRectTransform.gameObject.SetActive(false);
            }
            return;
        }

        // Define a refer�ncia de posi��o para c�lculo de dire��o
        // Usa a c�mera se a refer�ncia do jogador n�o estiver definida
        Vector3 referencePosition = (playerReference != null) ? playerReference.position : uiCamera.transform.position;

        Vector3 targetScreenPosition = uiCamera.WorldToScreenPoint(target.position);

        bool isTargetVisible = targetScreenPosition.z > 0 &&
                               targetScreenPosition.x > borderMargin && targetScreenPosition.x < Screen.width - borderMargin &&
                               targetScreenPosition.y > borderMargin && targetScreenPosition.y < Screen.height - borderMargin;

        if (isTargetVisible && hideWhenTargetVisible)
        {
            if (arrowRectTransform.gameObject.activeSelf)
            {
                arrowRectTransform.gameObject.SetActive(false);
            }
            return;
        }
        else
        {
            if (!arrowRectTransform.gameObject.activeSelf)
            {
                arrowRectTransform.gameObject.SetActive(true);
            }
        }

        Vector3 cappedTargetScreenPosition = targetScreenPosition;
        if (cappedTargetScreenPosition.z < 0)
        {
            cappedTargetScreenPosition *= -1;
            // Ajusta para o centro da tela se estiver atr�s, para evitar que a seta aponte para o lado errado
            cappedTargetScreenPosition = (cappedTargetScreenPosition - new Vector3(Screen.width / 2, Screen.height / 2, 0)).normalized;
            cappedTargetScreenPosition = new Vector3(Screen.width / 2, Screen.height / 2, 0) + cappedTargetScreenPosition * Mathf.Max(Screen.width, Screen.height);
        }

        cappedTargetScreenPosition.x = Mathf.Clamp(cappedTargetScreenPosition.x, borderMargin, Screen.width - borderMargin);
        cappedTargetScreenPosition.y = Mathf.Clamp(cappedTargetScreenPosition.y, borderMargin, Screen.height - borderMargin);

        arrowRectTransform.position = cappedTargetScreenPosition;

        // Calcula a rota��o baseada na dire��o do mundo
        Vector3 directionToTargetWorld = (target.position - referencePosition);
        // Ignora a diferen�a de altura (Y) para a rota��o da seta 2D, se necess�rio
        // directionToTargetWorld.y = 0;
        directionToTargetWorld.Normalize();

        // Converte a dire��o do mundo para a tela
        // Uma forma simplificada � usar a dire��o do centro da tela para a posi��o da seta
        Vector3 centerScreen = new Vector3(Screen.width / 2, Screen.height / 2, 0);
        Vector3 directionOnScreen = (arrowRectTransform.position - centerScreen).normalized;
        if (targetScreenPosition.z < 0) directionOnScreen *= -1; // Inverte se estiver atr�s

        float angle = Mathf.Atan2(directionOnScreen.y, directionOnScreen.x) * Mathf.Rad2Deg;
        arrowRectTransform.localEulerAngles = new Vector3(0, 0, angle);
    }

    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
        // Debug.Log("Seta de objetivo apontando para: " + (newTarget != null ? newTarget.name : "Nenhum"));
    }

    public void SetArrowActive(bool active)
    {
        isArrowActive = active;
        if (arrowRectTransform != null)
        {
            arrowRectTransform.gameObject.SetActive(active);
        }
        // Debug.Log("Seta de objetivo " + (active ? "ativada" : "desativada"));
    }
}
