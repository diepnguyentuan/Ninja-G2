using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    [Header("Volume Icons")]
    public GameObject volumeOnIcon;
    public GameObject volumeOffIcon;
    private bool isMuted = false;

    public void PlayGame()
    {
        SceneManager.LoadSceneAsync(1);
        Time.timeScale = 1f; // tiếp tục game  

    }

    public void QuitGame()
    {
        Application.Quit();
        Debug.Log("Quit Game");
    }

    public void ToggleVolumeOff()
    {
        isMuted = true;
        AudioListener.volume = 0f;
        volumeOnIcon.SetActive(false);
        volumeOffIcon.SetActive(true);
    }

    public void ToggleVolumeOn()
    {
        isMuted = false;
        AudioListener.volume = 1f;
        volumeOnIcon.SetActive(true);
        volumeOffIcon.SetActive(false);
    }
}
