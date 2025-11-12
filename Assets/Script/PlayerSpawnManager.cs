using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerSpawnManager : MonoBehaviour
{
    public Vector3 spawnPosition = new Vector3(5, 5, 0); // Vị trí spawn mặc định

    void Start()
    {
        // Chỉ chạy ở Scene 1 (Menu/Start Scene)
        if (SceneManager.GetActiveScene().buildIndex == 0)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                // Reset vị trí Player về spawn point
                player.transform.position = spawnPosition;

                // Reset velocity nếu có Rigidbody2D
                Rigidbody2D rb = player.GetComponent<Rigidbody2D>();
                if (rb != null)
                {
                    rb.velocity = Vector2.zero;
                }

                Debug.Log("Đã reset vị trí Player về: " + spawnPosition);
            }
        }
    }
}