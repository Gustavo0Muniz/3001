using UnityEngine;
using UnityEngine.UI;
using Unity.Cinemachine; // Adicionado para acessar CinemachineBrain

public class ObjectiveArrow : MonoBehaviour
{
    [Header("Referências")]
    [Tooltip("Transform do objeto que a seta deve apontar (Ex: Próxima área, NPC)")]
    public Transform target;
    [Tooltip("Transform do jogador para referência de posição (opcional, usará a câmera se nulo)")]
    public Transform playerReference;
    [Tooltip("RectTransform da imagem da seta na UI")]
    public RectTransform arrowRectTransform;
    [Tooltip("Câmera usada para converter posições do mundo para a tela (será detectada automaticamente se deixado vazio)")]
    public Camera uiCamera;

    [Header("Configurações Visuais")]
    [Tooltip("Distância da borda da tela onde a seta será posicionada")]
    public float borderMargin = 50f;
    [Tooltip("Esconder a seta quando o alvo estiver visível na tela?")]
    public bool hideWhenTargetVisible = true;

    private bool isArrowActive = false;

    void Start()
    {
        // Tenta encontrar referências se não foram atribuídas
        if (arrowRectTransform == null)
        {
            arrowRectTransform = GetComponent<RectTransform>();
        }
        if (playerReference == null)
        {
            // Se não houver referência do jogador, usaremos a posição da câmera como referência no Update
            Debug.LogWarning("PlayerReference não atribuído em ObjectiveArrow. A posição da câmera será usada para calcular a direção.", this);
        }

        // --- Lógica de Detecção Automática da Câmera --- 
        if (uiCamera == null) // Tenta detectar automaticamente apenas se não foi atribuída no Inspector
        {
            uiCamera = Camera.main; // Tenta pegar a câmera com a tag "MainCamera"

            if (uiCamera == null) // Se ainda não encontrou (sem tag MainCamera)
            {
                // Tenta pegar a câmera do Cinemachine Brain ativo
                CinemachineBrain brain = FindObjectOfType<CinemachineBrain>();
                if (brain != null && brain.OutputCamera != null)
                {
                    uiCamera = brain.OutputCamera;
                    Debug.Log("ObjectiveArrow: Câmera principal não encontrada ou sem tag 'MainCamera'. Usando câmera do Cinemachine Brain.", this);
                }
                else
                {
                    // Última tentativa: pegar qualquer câmera na cena (pode não ser a ideal)
                    uiCamera = FindObjectOfType<Camera>();
                    if (uiCamera != null)
                    {
                        Debug.LogWarning("ObjectiveArrow: Nenhuma câmera principal ou Cinemachine Brain encontrada. Usando uma câmera aleatória encontrada na cena: " + uiCamera.name, this);
                    }
                    else
                    {
                        Debug.LogError("ObjectiveArrow: Nenhuma câmera encontrada na cena! A seta não funcionará.", this);
                    }
                }
            }
            else
            {
                Debug.Log("ObjectiveArrow: Usando Camera.main (câmera com tag 'MainCamera').", this);
            }
        }
        else
        {
            Debug.Log("ObjectiveArrow: Usando câmera atribuída manualmente no Inspector: " + uiCamera.name, this);
        }
        // --- Fim da Lógica de Detecção Automática --- 

        // Começa com a seta desativada
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

        // Define a referência de posição para cálculo de direção
        // Usa a câmera se a referência do jogador não estiver definida
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
            // Ajusta para o centro da tela se estiver atrás, para evitar que a seta aponte para o lado errado
            cappedTargetScreenPosition = (cappedTargetScreenPosition - new Vector3(Screen.width / 2, Screen.height / 2, 0)).normalized;
            cappedTargetScreenPosition = new Vector3(Screen.width / 2, Screen.height / 2, 0) + cappedTargetScreenPosition * Mathf.Max(Screen.width, Screen.height);
        }

        cappedTargetScreenPosition.x = Mathf.Clamp(cappedTargetScreenPosition.x, borderMargin, Screen.width - borderMargin);
        cappedTargetScreenPosition.y = Mathf.Clamp(cappedTargetScreenPosition.y, borderMargin, Screen.height - borderMargin);

        arrowRectTransform.position = cappedTargetScreenPosition;

        // Calcula a rotação baseada na direção do mundo
        Vector3 directionToTargetWorld = (target.position - referencePosition);
        // Ignora a diferença de altura (Y) para a rotação da seta 2D, se necessário
        // directionToTargetWorld.y = 0;
        directionToTargetWorld.Normalize();

        // Converte a direção do mundo para a tela
        // Uma forma simplificada é usar a direção do centro da tela para a posição da seta
        Vector3 centerScreen = new Vector3(Screen.width / 2, Screen.height / 2, 0);
        Vector3 directionOnScreen = (arrowRectTransform.position - centerScreen).normalized;
        if (targetScreenPosition.z < 0) directionOnScreen *= -1; // Inverte se estiver atrás

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
