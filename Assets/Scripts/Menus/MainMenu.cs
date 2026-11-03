using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public GameObject mainMenu;
    public GameObject levelSelector;
    void Start()
    {
        levelSelector.SetActive(false);
    }
    public void StartButton()
    {
        StartCoroutine(OpenBook());
    }
    public void ExitButton()
    {
        Application.Quit();
    }
    public void LevelOneButton()
    {
        SceneManager.LoadScene("Pruebas");
    }
    public void BackButton()
    {
        mainMenu.SetActive(true);
        levelSelector.SetActive(false);
    }
    public IEnumerator OpenBook()
    {
        mainMenu.SetActive(false);
        yield return new WaitForSeconds(0f);
        levelSelector.SetActive(true);
    }
}
