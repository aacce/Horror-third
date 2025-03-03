using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class CharacterSelection : MonoBehaviour
{
    public int selectedCharacter = 0; // ค่าเริ่มต้น
    public Button startButton;

    private void Start()
    {
        startButton.onClick.AddListener(StartGame);
    }

    public void SelectCharacter(int id)
    {
        selectedCharacter = id;
    }

    public void StartGame()
    {
        if (NetworkManager.Singleton != null)
        {
            byte[] payload = new byte[] { (byte)selectedCharacter };
            NetworkManager.Singleton.NetworkConfig.ConnectionData = payload;
            NetworkManager.Singleton.StartClient();
        }
    }
}