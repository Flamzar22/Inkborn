using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{
    public GameObject pausePanel;
    public GameObject optionsPanel;
    public GameObject returnPanel;
    void Start()
    {
        pausePanel.SetActive(false);
        optionsPanel.SetActive(false);
        returnPanel.SetActive(false);
    }
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape) && !pausePanel.activeSelf && !optionsPanel.activeSelf)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            pausePanel.SetActive(true);
            Time.timeScale = 0f;
        }
        else if (Input.GetKeyDown(KeyCode.Escape) && (pausePanel.activeSelf || optionsPanel.activeSelf))
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            pausePanel.SetActive(false);
            optionsPanel.SetActive(false);
            returnPanel.SetActive(false);
            Time.timeScale = 1f;
        }
    }
    public void ContinueButton()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        pausePanel.SetActive(false);
        Time.timeScale = 1f;
    }
    public void OptionsButton()
    {
        optionsPanel.SetActive(true);
        pausePanel.SetActive(false);
    }
    public void MenuButton()
    {
        returnPanel.SetActive(true);
    }
    public void ReturnToMenuYesButton()
    {
        SceneManager.LoadScene("Menu");
    }
    public void ReturnToMenuNoButton()
    {
        returnPanel.SetActive(false);
    }
    public void BackButton()
    {
        optionsPanel.SetActive(false);
        pausePanel.SetActive(true);
    }

}
