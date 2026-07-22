using System;
using UnityEngine;

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
            if (!OpenedPauseMenu)
            {
                OpenPauseMenu();
            }
        }
    }

    void OpenPauseMenu()
    {
        pauseMenu.SetActive(true);
        GameUI.SetActive(false);
        OpenedPauseMenu = true;
    }
    
    void ClosePauseMenu()
    {
        pauseMenu.SetActive(false);
        GameUI.SetActive(true);
        OpenedPauseMenu = false;
    }
}
