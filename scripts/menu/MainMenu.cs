using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    [SerializeField] GameObject HelpMenu;
    [SerializeField] GameObject LoadMenu;
    [SerializeField] GameObject SettingsMenu;

    [SerializeField] GameObject BuildingHelp;
    [SerializeField] GameObject FactoringHelp;
    [SerializeField] GameObject DeliverenceHelp;

    public void OnToMenuClicked()
    {
        SaveManager.LoadOnNextScene = true;
        SceneManager.LoadScene(0);
    }

    public void OnPlayClicked()
    {
        LoadMenu.SetActive(!LoadMenu.activeSelf);
        SettingsMenu.SetActive(false);
        HelpMenu.SetActive(false);
    }

    public void OnQuitClicked()
    {
        Application.Quit();
    }

    public void OnHelpClicked()
    {
        HelpMenu.SetActive(!HelpMenu.activeSelf);
        SettingsMenu.SetActive(false);
        LoadMenu.SetActive(false);
    }

    public void OnSettingsClicked()
    {
        SettingsMenu.SetActive(!SettingsMenu.activeSelf);
        HelpMenu.SetActive(false);
        LoadMenu.SetActive(false);
    }

    public void OnBuildingHelpClicked()
    {
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
