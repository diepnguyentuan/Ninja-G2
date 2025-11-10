using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement; // thêm dòng này để dùng SceneManager

public class PauseMenu : MonoBehaviour
{
    [SerializeField] private GameObject pauseMenu;

    public void Pause()
    {
        pauseMenu.SetActive(true);
        Time.timeScale = 0f; // tạm dừng game
    }

    public void Resume()
    {
        pauseMenu.SetActive(false);
        Time.timeScale = 1f; // tiếp tục game
    }

    public void Restart()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        Time.timeScale = 1f; // reset tốc độ thời gian
    }

    public void Home()
    {
        pauseMenu.SetActive(false);
        SceneManager.LoadScene(1);
        Time.timeScale = 1f;
    }

    public void ReturnMenu()
    {
        Time.timeScale = 0f;
        SceneManager.LoadScene(0);
    }
}
