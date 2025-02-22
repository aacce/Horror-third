using UnityEngine;
using UnityEngine.UI;

public class HealthBarController : MonoBehaviour
{
    public Slider healthBarSlider; // ตัว Slider
    public float maxHealth = 100f; // เลือดสูงสุด
    private float currentHealth;   // เลือดปัจจุบัน

    void Start()
    {
        currentHealth = maxHealth; // เริ่มต้นด้วยเลือดเต็ม
        UpdateHealthBar();
    }

    public void TakeDamage(float damage)
    {
        currentHealth -= damage; // ลดค่าความเสียหาย
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth); // จำกัดไม่ให้ต่ำกว่า 0
        UpdateHealthBar();
    }

    public void Heal(float healAmount)
    {
        currentHealth += healAmount; // เพิ่มค่าฟื้นฟู
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth); // จำกัดไม่ให้เกิน Max
        UpdateHealthBar();
    }

    private void UpdateHealthBar()
    {
        healthBarSlider.value = currentHealth / maxHealth; // อัปเดตค่า Slider
    }
}