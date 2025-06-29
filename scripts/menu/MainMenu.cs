using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public void OnLoadGameClicked()
    {
        SaveManager.LoadOnNextScene = true;
        SceneManager.LoadScene(2);
    }

    public void OnNewGGameClicked()
    {
        SceneManager.LoadScene(2);
    }

    public void OnQuitClicked()
    {
        Application.Quit();
    }
}
