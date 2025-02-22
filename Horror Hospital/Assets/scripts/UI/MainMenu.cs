using UnityEngine;
using UnityEngine.SceneManagement;
using Unity.Netcode;

public class MainMenu : MonoBehaviour
{
    public void StartGame()
    {
        SceneManager.LoadScene("GameScene"); // โหลด GameScene ก่อน
        SceneManager.sceneLoaded += OnGameSceneLoaded; // รอให้โหลดเสร็จ
    }

    private void OnGameSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "GameScene")
        {
            if (NetworkManager.Singleton != null)
            {
                NetworkManager.Singleton.StartHost(); // เริ่ม Host หลังจากโหลด Scene
            }
            else
            {
                Debug.LogError("NetworkManager not found in GameScene!");
            }

            SceneManager.sceneLoaded -= OnGameSceneLoaded; // เอา event ออกเพื่อไม่ให้มันเรียกซ้ำ
        }
    }

    public void QuitGame()
    {
        Application.Quit();
        Debug.Log("Game Quit!"); // Debug ให้เช็กว่าออกเกม
    }
}