using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameOverManager : MonoBehaviour
{
    public GameObject gameOverUI;
    public string villageSceneName = "Village";

    private bool isGameOver = false;
    public static GameOverManager instance;

    private void Start()
    {
         gameOverUI.SetActive(false);
    }
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            SceneManager.sceneLoaded += OnSceneLoaded; // 👈 Đăng ký sự kiện load scene
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Mỗi khi load scene mới, thử tìm lại Canvas_GameOver nếu bị mất
        if (gameOverUI == null)
        {
            GameObject found = GameObject.Find("Canvas_GameOver");
            if (found != null)
            {
                gameOverUI = found;
                gameOverUI.SetActive(false);
                Debug.Log($"[GameOverManager] Found new Canvas_GameOver in scene {scene.name}");
            }
        }
    }

    public void ShowGameOver()
    {
        
        isGameOver = true;

        Time.timeScale = 0f;
        var camFollow = Camera.main.GetComponent<CameraFollow>();
        if (camFollow != null)
            camFollow.enabled = false;

        if (gameOverUI != null)
        {
            gameOverUI.SetActive(true);
        }
        else
        {
            Debug.LogError("[GameOverManager] ❌ GameOverUI is NULL!");
        }

        Debug.Log(">>> GameOver UI Activated");
    }

    public void ReturnToVillage()
    {
        StartCoroutine(ReturnToVillageAfterDelay(0.5f));
    }

    IEnumerator ReturnToVillageAfterDelay(float delay)
    {
        yield return new WaitForSecondsRealtime(delay);
        Time.timeScale = 1f;
        SceneManager.LoadScene(villageSceneName);

        StartCoroutine(ResetPlayerHealthNextFrame());
        isGameOver = false;
        if (gameOverUI != null) gameOverUI.SetActive(false);
    }

    IEnumerator ResetPlayerHealthNextFrame()
    {
        yield return null;
        PlayerStats player = FindObjectOfType<PlayerStats>();
        if (player != null)
        {
            player.ResetHealth();
        }
    }
}
