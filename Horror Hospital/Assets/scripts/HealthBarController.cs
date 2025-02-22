using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;

public class HealthBarController : NetworkBehaviour
{
    public Slider healthBarSlider; // ตัว Slider
    public float maxHealth = 100f; // เลือดสูงสุด
    private NetworkVariable<float> currentHealth = new NetworkVariable<float>(100f);   // เลือดปัจจุบัน
    
    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            currentHealth.Value = maxHealth;
        }
        currentHealth.OnValueChanged += OnHealthChanged;
        UpdateHealthBar();
    }

    private void OnHealthChanged(float oldHealth, float newHealth)
    {
        Debug.Log($"[CLIENT] Health Changed: {oldHealth} -> {newHealth}");
        UpdateHealthBar();
    }

    [ServerRpc(RequireOwnership = false)]
    public void TakeDamageServerRpc(float damage)
    {
        if (!IsServer) return;
        currentHealth.Value = Mathf.Clamp(currentHealth.Value - damage, 0, maxHealth);
        UpdateHealthUIClientRpc(currentHealth.Value);
    }

    [ServerRpc(RequireOwnership = false)]
    public void HealServerRpc(float healAmount)
    {
        currentHealth.Value = Mathf.Clamp(currentHealth.Value + healAmount, 0, maxHealth);
        UpdateHealthUIClientRpc(currentHealth.Value);
    }

    public void UpdateHealthBar()
    {
        healthBarSlider.value = currentHealth.Value / maxHealth;
    }

    public void SetHealthBarColor(Color color)
    {
        if (healthBarSlider.fillRect != null)
        {
            healthBarSlider.fillRect.GetComponent<Image>().color = color;
        }
    }
    
    [ClientRpc]
    private void UpdateHealthUIClientRpc(float newHealth)
    {
        Debug.Log($"[CLIENT RPC] Received updated health: {newHealth}");
        healthBarSlider.value = newHealth / maxHealth;
    }
}