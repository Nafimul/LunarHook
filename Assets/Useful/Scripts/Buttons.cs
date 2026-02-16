using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class Buttons : MonoBehaviour
{
    public GameObject pauseScreen;
    public GameObject pauseButton;
    public EventSystem eventSystem;

    public void Pause()
    {
        pauseScreen.SetActive(true);
        pauseButton.SetActive(false);
        eventSystem.SetSelectedGameObject(eventSystem.firstSelectedGameObject);
        AudioListener.pause = true;
        Time.timeScale = 0;
    }

    public void Unpause()
    {
        pauseScreen.SetActive(false);
        pauseButton.SetActive(true);
        AudioListener.pause = false;
        Time.timeScale = 1;
    }

    public void Exit()
    {
        AudioListener.pause = false;
        Time.timeScale = 1;
        Application.Quit();
    }

    public void Menu()
    {
        AudioListener.pause = false;
        Time.timeScale = 1;
        SceneManager.LoadScene("Menu");
    }
}
