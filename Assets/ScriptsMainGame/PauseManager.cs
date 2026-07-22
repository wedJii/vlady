using System;
using Unity.VectorGraphics;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseManager : MonoBehaviour
{
    [SerializeField] GameObject pauseMenu;
    [SerializeField] GameObject GameUI;
    private bool OpenedPauseMenu = false;

    private void Start()
    {
        GameUI.SetActive(true);
        pauseMenu.SetActive(false);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (OpenedPauseMenu)
            {
                ClosePauseMenu();
            }
            else
            {
                OpenPauseMenu();
            }
        }
    }
    
    public void OpenPauseMenu()
    {
        pauseMenu.SetActive(true);
        GameUI.SetActive(false);
        OpenedPauseMenu = true;
        Time.timeScale = 0;
    }
    
    public void ClosePauseMenu()
    {
        pauseMenu.SetActive(false);
        GameUI.SetActive(true);
        OpenedPauseMenu = false;
        Time.timeScale = 1;
    }

    public void GoToMainMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
