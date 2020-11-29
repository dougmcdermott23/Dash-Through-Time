using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public LevelLoader levelLoader;

    public void PlayGame()
    {
        StartCoroutine(levelLoader.LoadLevel(SceneManager.GetActiveScene().buildIndex + 1));
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
