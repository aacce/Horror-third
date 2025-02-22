using System;
using UnityEngine;
using UnityEngine.UI;

public class PlayerMoment : MonoBehaviour
{
    [Header("Movement")] 
    public float moveSpeed;
    public float runSpeed; 
    public float walkSpeed;

    public float groundDrag;
    
    [Header("Stamina")]
    public float maxStamina = 100f; // ค่า Stamina สูงสุด
    private float currentStamina;   // ค่า Stamina ปัจจุบัน
    public float staminaDrainRate = 20f; // อัตราการลด Stamina เมื่อวิ่ง (ต่อวินาที)
    public float staminaRecoveryRate = 10f; // อัตราการฟื้นฟู Stamina (ต่อวินาที)
    public float staminaCooldownTime = 2f; // เวลาหน่วงก่อนฟื้นฟู Stamina หลังวิ่ง
    private bool canRecoverStamina = true;
    
    [Header("UI")]
    public Slider staminaBar; // Slider สำหรับแสดงค่า Stamina
    
    [Header("Ground Check")] 
    public float playerHeight;
    public LayerMask whatIsGround;
    private bool grounded;

    public Transform orientation;

    private float horizontalInput;
    private float vertiaclInput;

    private Vector3 moveDirection;

    private Rigidbody rb; 
    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
        
        // กำหนดค่า Stamina เริ่มต้นเป็น 100 (ค่า maxStamina)
        currentStamina = maxStamina;
        
        if (staminaBar != null)
        {
            staminaBar.maxValue = maxStamina; // ตั้งค่า Max Value ให้ Slider
            staminaBar.value = currentStamina; // ตั้งค่าเริ่มต้นของ Slider
        }
    }

    private void Update()
    {
        //ground check
        grounded = Physics.Raycast(transform.position, Vector3.down, playerHeight * 0.5f + 0.2f, whatIsGround);
        
        MyInput();

        if (grounded)
            rb.drag = groundDrag;
        else
            rb.drag = 0;
        
        // ตรวจจับการกด Shift และเปลี่ยน moveSpeed
        if (Input.GetKey(KeyCode.LeftShift) && currentStamina > 0)
        {
            moveSpeed = runSpeed; // เพิ่มความเร็ว
            DrainStamina();      // ลด Stamina
            canRecoverStamina = false; // หยุดการฟื้นฟู Stamina ขณะวิ่ง
        }
        else
        {
            moveSpeed = walkSpeed; // กลับไปเดินปกติ
            if (!canRecoverStamina)
            {
                // เริ่มฟื้นฟู Stamina หลังเวลาหน่วง
                Invoke(nameof(EnableStaminaRecovery), staminaCooldownTime);
            }
        }

        // ฟื้นฟู Stamina เมื่อไม่ได้วิ่ง
        if (canRecoverStamina && currentStamina < maxStamina)
        {
            RecoverStamina();
        }
        
        // อัปเดต Stamina Bar
        UpdateStaminaBar();
    }

    private void FixedUpdate()
    {
        MovePlayer();
    }

    private void MyInput()
    {
        horizontalInput = Input.GetAxisRaw("Horizontal");
        vertiaclInput = Input.GetAxisRaw("Vertical");
    }

    private void MovePlayer()
    {
        moveDirection = orientation.forward * vertiaclInput + orientation.right * horizontalInput;
        rb.AddForce(moveDirection.normalized * moveSpeed * 10f, ForceMode.Force);
    }
    
    private void DrainStamina()
    {
        currentStamina -= staminaDrainRate * Time.deltaTime; // ลด Stamina ตามอัตรา
        currentStamina = Mathf.Clamp(currentStamina, 0, maxStamina); // ไม่ให้ต่ำกว่า 0
    }

    private void RecoverStamina()
    {
        currentStamina += staminaRecoveryRate * Time.deltaTime; // ฟื้นฟู Stamina ตามอัตรา
        currentStamina = Mathf.Clamp(currentStamina, 0, maxStamina); // ไม่ให้เกินค่า Max
    }

    private void EnableStaminaRecovery()
    {
        canRecoverStamina = true; // อนุญาตให้ฟื้นฟู Stamina
    }

    private void UpdateStaminaBar()
    {
        if (staminaBar != null)
        {
            staminaBar.value = currentStamina; // อัปเดตค่าปัจจุบันของ Slider
        }
    }
}
