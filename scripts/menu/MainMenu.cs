using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    [SerializeField] GameObject HelpManu;

    [SerializeField] GameObject BuildingHelp;
    [SerializeField] GameObject FactoringHelp;
    [SerializeField] GameObject DeliverenceHelp;
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

    public void OnHelpClicked()
    {
        HelpManu.SetActive(!HelpManu.activeSelf);
    }

    public void OnBuildingHelpClicked() {
        BuildingHelp.SetActive(true);
        FactoringHelp.SetActive(false);
        DeliverenceHelp.SetActive(false);
    }

    public void OnFactoringHelpClicked()
    {
        BuildingHelp.SetActive(false);
        FactoringHelp.SetActive(true);
        DeliverenceHelp.SetActive(false);
    }

    public void OnDeliverenceHelpClicked()
    {
        BuildingHelp.SetActive(false);
        FactoringHelp.SetActive(false);
        DeliverenceHelp.SetActive(true);
    }
}
