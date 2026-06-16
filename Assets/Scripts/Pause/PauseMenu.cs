using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{
    [SerializeField] GameObject pauseMenu;

    public string _mainMenu;

    public void Pause() { 
        pauseMenu.SetActive(true);
        Time.timeScale = 0;
    }

    public void Resume() {
        pauseMenu.SetActive(false);
        Time.timeScale = 1;
    }

    public void Home() {
        Time.timeScale = 1;
        SceneManager.LoadScene(_mainMenu);
    }

    public void Option() { 
    
    }

}
