using Unity.Netcode;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using System.Collections;

public class ItemController : NetworkBehaviour
{
    public HealthBarController healthController; // ควบคุมเลือดตัวละคร
    public Volume postProcessingVolume; // Post Processing Volume
    
    private Vignette vignette; // เอฟเฟคขอบมืด (สำหรับ Damage)
    private ColorAdjustments colorAdjustments; // เอฟเฟคการปรับสี (สำหรับ Blackout)
    
    private bool isVulnerable = false; // สถานะเปราะบาง
    private float vulnerableDuration = 10f; // เวลาของสถานะเปราะบาง
    private float vulnerableEndTime = 0f; // เวลาที่สถานะเปราะบางสิ้นสุด

    private int healingPotionCount = 0; // นับจำนวนครั้งที่ใช้ฮีล
    private float healCooldownEndTime = 0; // เวลาสิ้นสุดของการแบนฮีล
    private float healBanDuration = 10f; // 2 นาที (120 วินาที)
    
    private float targetSmoothness = 0f; // ค่าปัจจุบันของ Smoothness
    private float smoothnessSpeed = 5f; // ความเร็วในการปรับ Smoothness
    
    private Color targetColor = new Color(125f / 255f, 125f / 255f, 125f / 255f); // สีที่จะเปลี่ยนไป
    private float colorChangeSpeed = 5f; // ความเร็วในการปรับสี
    
    private Color defaultHealthBarColor = Color.red;

    void Start()
    {
        // ตรวจสอบว่าดึง Vignette และ ColorAdjustments ได้หรือไม่
        if (postProcessingVolume.profile.TryGet(out vignette))
        {
            vignette.smoothness.value = 0; // ปิดขอบมืดตอนเริ่มเกม
        }
        
        if (postProcessingVolume.profile.TryGet(out colorAdjustments))
        {
            colorAdjustments.colorFilter.value = new Color(255f / 255f, 255f / 255f, 255f / 255f); // ตั้งค่าสี RGB (125, 125, 125)
        }
        
    }
    
    public void UseHilly()
    {
        if (isVulnerable) 
        {
            Debug.Log("Cannot use Hilly while vulnerable!");
            return; // ถ้าผู้เล่นอยู่ในสถานะเปราะบางแล้วไม่ให้เก็บยา
        }

        if (!IsOwner) 
        {
            Debug.LogError("Only the owner can use this item!");
            return; // แสดงข้อความผิดพลาดถ้าไม่ใช่เจ้าของ
        }

        isVulnerable = true; // ตั้งสถานะเป็นเปราะบาง
        vulnerableEndTime = Time.time + vulnerableDuration; // กำหนดเวลาหมดสถานะ
        Debug.Log("Player is vulnerable! Time left: " + vulnerableDuration);
        healthController.SetHealthBarColor(new Color(128f / 255f, 0f, 128f / 255f)); // เปลี่ยนสีเลือดเป็นสีม่วง

        // เรียกใช้ ServerRpc เมื่อผู้เล่นใช้ไอเท็ม
        UseHillyServerRpc();
    }

    void Update()
    {
        // ถ้าเวลาหมดสถานะเปราะบางแล้วให้ปิดสถานะ
        if (isVulnerable && Time.time >= vulnerableEndTime)
        {
            isVulnerable = false;
            healthController.SetHealthBarColor(defaultHealthBarColor);
        }
    }

    public void TakeDamage(float damage)
    {
        if (!IsOwner) return;

        // ถ้าผู้เล่นอยู่ในสถานะเปราะบาง ให้คูณดาเมจเป็น 2 เท่า
        if (isVulnerable)
        {
            damage *= 2f; // คูณดาเมจ
        }

        // ส่งความเสียหายไปยัง HealthController ของตัวผู้เล่น
        healthController.TakeDamageServerRpc(damage);
    }

    [ServerRpc]
    private void UseHillyServerRpc()
    {
        if (!IsOwner) 
        {
            return; // แสดงข้อความผิดพลาดถ้าไม่ใช่เจ้าของ
        }
        // แค่ผู้เล่นที่ใช้ไอเท็มเท่านั้นที่จะกระทบผลของมัน
        isVulnerable = true;
        vulnerableEndTime = Time.time + vulnerableDuration;
        Debug.Log("Player is vulnerable on the Server!");
        healthController.SetHealthBarColor(new Color(128f / 255f, 0f, 128f / 255f)); // เปลี่ยนสีเลือด
    }

    [ClientRpc]
    private void UpdateHealthUIClientRpc()
    {
        Debug.Log("[CLIENT RPC] Health update received on Client.");
        healthController.UpdateHealthBar();  // อัปเดต Health Bar
    }


    public void UseDamagePotion()
    {
        StartCoroutine(DamageOverTime(3, 10)); // ใช้ DamagePotion ที่ทำให้เสียหาย 3 ครั้ง, ครั้งละ 10 หน่วย
    }

    IEnumerator DamageOverTime(int times, float damage)
    {
        for (int i = 0; i < times; i++)
        {
            Debug.Log("Calling TakeDamage with damage: " + damage);
            TakeDamage(damage); // เรียกใช้ฟังก์ชัน TakeDamage
            StartCoroutine(ShowDamageEffect());
            yield return new WaitForSeconds(1f); // หน่วงเวลา 1 วินาทีระหว่างการเสียหายแต่ละครั้ง
        }
        StartCoroutine(RestoreSmoothness());
    }

    IEnumerator ShowDamageEffect()
    {
        if (vignette != null)
        {
            targetSmoothness = 0.8f; // กำหนดค่าที่ต้องการให้ smoothness
            // ค่อยๆ ปรับ smoothness ตามเวลาผ่านไป
            while (Mathf.Abs(vignette.smoothness.value - targetSmoothness) > 0.01f)
            {
                vignette.smoothness.value = Mathf.Lerp(vignette.smoothness.value, targetSmoothness, Time.deltaTime * smoothnessSpeed);
                yield return null;
            }
        }
    }

    IEnumerator RestoreSmoothness()
    {
        // ค่อยๆ ปรับ smoothness กลับไปที่ 0 หลังจากแสดงผลดาเมจ
        while (vignette.smoothness.value > 0.01f)
        {
            vignette.smoothness.value = Mathf.Lerp(vignette.smoothness.value, 0f, Time.deltaTime * smoothnessSpeed);
            yield return null;
        }
    }

    public void UseHealingPotion()
    {
        if (!IsOwner) return;
        if (Time.time < healCooldownEndTime)
        {
            Debug.Log("ไม่สามารถใช้ฮีลได้ในช่วงหน้ามืด!");
            return;
        }

        float healAmount = healthController.maxHealth * 0.25f;
        healthController.HealServerRpc(healAmount);
        healingPotionCount++;

        // เมื่อเก็บยาฮีลเกิน 4 ครั้ง ให้ปรับสี
        if (healingPotionCount >= 4)
        {
            StartCoroutine(ActivateBlackoutEffect()); // เริ่มเปิดเอฟเฟคสี
            healCooldownEndTime = Time.time + healBanDuration; // แบนฮีล 2 นาที
            healingPotionCount = 0; // รีเซ็ตจำนวนครั้ง
        }
    }

    IEnumerator ActivateBlackoutEffect()
    {
        if (colorAdjustments != null)
        {
            Color originalColor = colorAdjustments.colorFilter.value; // เก็บค่าสีปัจจุบัน
            Color targetColor = new Color(125f / 255f, 125f / 255f, 125f / 255f); // สีขาวดำ
            float t = 0f; // ตัวแปรในการคำนวณการปรับสี

            // เปลี่ยนสีอย่างค่อยๆ ไปยังสีใหม่
            while (t < 1f)
            {
                t += Time.deltaTime * colorChangeSpeed; // คำนวณค่าของ t
                colorAdjustments.colorFilter.value = Color.Lerp(originalColor, targetColor, t); // ปรับสีค่อยๆ
                yield return null;
            }

            yield return new WaitForSeconds(healBanDuration);

            // กลับมาที่สีเดิม
            t = 0f;
            while (t < 1f)
            {
                t += Time.deltaTime * colorChangeSpeed;
                colorAdjustments.colorFilter.value = Color.Lerp(targetColor, originalColor, t);
                yield return null;
            }
        }
    }

    private bool ColorsAreEqual(Color color1, Color color2)
    {
        return Mathf.Approximately(color1.r, color2.r) &&
               Mathf.Approximately(color1.g, color2.g) &&
               Mathf.Approximately(color1.b, color2.b);
    }

    public bool IsVulnerable()
    {
        return isVulnerable;
    }
}
