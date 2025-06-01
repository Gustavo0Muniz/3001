using UnityEngine;
using UnityEngine.SceneManagement;

public class menuPrincipalMenager : MonoBehaviour
{
    [SerializeField] private GameObject menuMain;
    [SerializeField] private GameObject menuoptions;
    [SerializeField] private GameObject menuControl;
    [SerializeField] private GameObject MenuCredits;
    [SerializeField] private RectTransform creditosContent;
    [SerializeField] private float velocidadeCreditos = 50f;
    [SerializeField] private float posicaoInicialY = -400f;
    [SerializeField] private float posicaoFinalY = 600f;

    private bool creditosAtivos = false;

    void Update()
    {
        if (creditosAtivos)
        {
            creditosContent.anchoredPosition += Vector2.up * velocidadeCreditos * Time.deltaTime;

            if (creditosContent.anchoredPosition.y >= posicaoFinalY)
            {
                fecharCredits(); 
            }
        }
    }
    [SerializeField] private string nomeDoLevelDejogo;
    public void jogar()
    {
        SceneManager.LoadScene(nomeDoLevelDejogo);

    }
    public void abrirOptions()
    {
        menuMain.SetActive(false);
        menuoptions.SetActive(true);

    }
    public void fecharOptions()
    {
        menuoptions.SetActive(false);
        menuMain.SetActive(true);
        
    }
    public void abrirControl()
    {
        menuoptions.SetActive(false) ;
        menuControl.SetActive(true);

    }
    public void FecharControl()
    {
        menuControl.SetActive(false);
       menuoptions.SetActive(true);
    }
    public void abrirCredits()
    {
        menuMain.SetActive(false);
        MenuCredits.SetActive(true);
        creditosAtivos = true;

  
        Vector2 pos = creditosContent.anchoredPosition;
        pos.y = posicaoInicialY;
        creditosContent.anchoredPosition = pos;
    }
    public void fecharCredits()
    {
        MenuCredits.SetActive(false);
        menuMain.SetActive(true);
    }

}
