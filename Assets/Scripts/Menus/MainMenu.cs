using System.Collections;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public GameObject mainMenu;
    public GameObject levelSelector;
    public Animator book;
    public Animator locks;
    public CinemachineCamera tableCam;
    public CinemachineCamera bookCam;
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
        StartCoroutine(CloseBook()); 
    }
    public IEnumerator OpenBook()
    {
        mainMenu.SetActive(false);
        locks.SetTrigger("Abrir");
        yield return new WaitForSeconds(1f);
        bookCam.gameObject.SetActive(true);
        tableCam.gameObject.SetActive(false);
        book.SetTrigger("Abrir");
        yield return new WaitForSeconds(1.5f);
        levelSelector.SetActive(true);
    }
    public IEnumerator CloseBook()
    {
        levelSelector.SetActive(false);
        book.SetTrigger("Cerrar");
        bookCam.gameObject.SetActive(false);
        tableCam.gameObject.SetActive(true);
        yield return new WaitForSeconds(1.5f);
        locks.SetTrigger("Cerrar");
        yield return new WaitForSeconds(1f);
        mainMenu.SetActive(true);
    }
}
