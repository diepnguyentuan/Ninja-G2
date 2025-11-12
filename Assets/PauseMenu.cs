using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{
    [SerializeField] private GameObject pauseMenu;

    [Header("Volume Icons")]
    public GameObject volumeOnIcon;
    public GameObject volumeOffIcon;
    private bool isMuted = false;

    public void Pause()
    {
        pauseMenu.SetActive(true);
        Time.timeScale = 0f;
    }

    public void Resume()
    {
        pauseMenu.SetActive(false);
        Time.timeScale = 1f;
    }

    public void Restart()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        Time.timeScale = 1f;
    }

    public void Home()
    {
        pauseMenu.SetActive(false);
        Time.timeScale = 1f;

        // Reset vị trí Player về (5, 5, 0)
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            player.transform.position = new Vector3(5, 5, 0);

            // Reset velocity để không bay lung tung
            Rigidbody2D rb = player.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.velocity = Vector2.zero;
            }
        }

        SceneManager.LoadScene(1);
    }

    public void ReturnMenu()
    {
        Time.timeScale = 0f;
        SceneManager.LoadScene(0);
    }

    // === PHẦN ÂM THANH ===
    public void ToggleVolumeOff()
    {
        isMuted = true;
        AudioListener.volume = 0f;

        if (volumeOnIcon != null)
            volumeOnIcon.SetActive(false);

        if (volumeOffIcon != null)
            volumeOffIcon.SetActive(true);
    }

    public void ToggleVolumeOn()
    {
        isMuted = false;
        AudioListener.volume = 1f;

        if (volumeOnIcon != null)
            volumeOnIcon.SetActive(true);

        if (volumeOffIcon != null)
            volumeOffIcon.SetActive(false);
    }
}