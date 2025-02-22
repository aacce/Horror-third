using System;
using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;

public class StaminaSystem : NetworkBehaviour
{
    public float maxStamina = 100f; // ค่า Stamina สูงสุด
    private NetworkVariable<float> currentStamina = new NetworkVariable<float>(100f,
        NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);  // ค่า Stamina ปัจจุบัน
    public float staminaDrainRate = 20f; // อัตราการลด Stamina เมื่อวิ่ง (ต่อวินาที)
    public float staminaRecoveryRate = 10f; // อัตราการฟื้นฟู Stamina (ต่อวินาที)
    public float staminaCooldownTime = 2f; // เวลาหน่วงก่อนฟื้นฟู Stamina หลังวิ่ง

    private bool canRecoverStamina = true;
    private Action<float> onStaminaChange; // ใช้สำหรับ callback เมื่อ Stamina เปลี่ยนแปลง

    public void Initialize(Action<float> onStaminaChanged)
    {
        onStaminaChange = onStaminaChanged;
    }
    
    private void Start()
    {
        // อัปเดตค่า UI อัตโนมัติเมื่อค่า Stamina เปลี่ยน
        currentStamina.OnValueChanged += (oldValue, newValue) =>
        {
            onStaminaChange?.Invoke(newValue);
        };
    }

    public void DrainStamina()
    {
        if (IsOwner)
        {
            // เพิ่มเงื่อนไขว่า Drain Stamina จะทำเมื่อกด Shift เท่านั้น
            if (Input.GetKey(KeyCode.LeftShift)) 
            {
                DrainStaminaServerRpc();
            }
        }
    }

    [ServerRpc]
    public void DrainStaminaServerRpc()
    {
        if (currentStamina.Value > 0)
        {
            currentStamina.Value -= staminaDrainRate * Time.deltaTime;
            currentStamina.Value = Mathf.Clamp(currentStamina.Value, 0, maxStamina);
            canRecoverStamina = false;
            Invoke(nameof(EnableStaminaRecovery), staminaCooldownTime);
        }
    }
    
    public void RecoverStamina()
    {
        if (IsOwner)
        {
            RecoverStaminaServerRpc();
        }
    }

    [ServerRpc]
    private void RecoverStaminaServerRpc()
    {
        if (canRecoverStamina && currentStamina.Value < maxStamina)
        {
            currentStamina.Value += staminaRecoveryRate * Time.deltaTime;
            currentStamina.Value = Mathf.Clamp(currentStamina.Value, 0, maxStamina);
        }
    }

    private void EnableStaminaRecovery()
    {
        canRecoverStamina = true;
    }

    public float GetCurrentStamina() => currentStamina.Value;
    public float GetMaxStamina() => maxStamina;
}