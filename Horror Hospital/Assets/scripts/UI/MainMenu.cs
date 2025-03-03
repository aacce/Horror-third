using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    [SerializeField] private TMP_InputField joinCodeField;
    [SerializeField] private Button killerButton;
    [SerializeField] private Button survivorButton;
    private bool isRoleSelectionVisible = false; 

    public void ToggleRoleSelection()
    {
        isRoleSelectionVisible = !isRoleSelectionVisible; // สลับสถานะ

        killerButton.gameObject.SetActive(isRoleSelectionVisible);
        survivorButton.gameObject.SetActive(isRoleSelectionVisible);
    }
    
    public void SelectRole(string role)
    {
        // ซ่อนปุ่มเลือกตัวละคร
        ToggleRoleSelection();

        // เริ่มเกมตามบทบาทที่เลือก
        StartHost();
    }
    
    public async void StartHost()
    {
        await HostSingleton.Instance.GameManager.StartHostAsync();
    }

    public async void StartClient()
    {
        await ClientSingleton.Instance.GameManager.StartClientAsync(joinCodeField.text);
    }
}