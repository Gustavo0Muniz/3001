using UnityEngine;
using UnityEngine.SceneManagement; // Você esqueceu de importar isso!

public class ChangeSceneOnTimer : MonoBehaviour
{
    public float changeTime;
    public string sceneName;

    void Update()
    {
        changeTime -= Time.deltaTime; // Corrigido aqui
        if (changeTime <= 0)
        {
            SceneManager.LoadScene(sceneName);
        }
    }
}
