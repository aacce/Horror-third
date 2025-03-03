using System;
using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;

public class PlayerMoment : NetworkBehaviour
{
    [Header("Movement")] 
    public float moveSpeed;
    public float runSpeed; 
    public float walkSpeed;
    public float crouchSpeed;
    
    public float groundDrag;
    
    [Header("Stamina System")]
    public StaminaSystem staminaSystem;
    
    [Header("UI")]
    public Slider staminaBar; // Slider สำหรับแสดงค่า Stamina
    
    [Header("Ground Check")] 
    public float playerHeight;
    public LayerMask whatIsGround;
    private bool Grounded;
    
    public Transform orientation;
    public float gravity = 0f;
    private float horizontalInput;
    private float verticalInput;
    private Vector3 moveDirection;
    private bool isCrouching = false;
    private CharacterController controller;
    public GameObject OtherplayerUI;

    [Header("Stair Check")]
    public LayerMask stairLayer; // เพิ่ม LayerMask สำหรับตรวจสอบเลเยอร์ "Stair"

    private void Start()
    {
        if (!IsOwner)
        {
            // ถ้าไม่ใช่เจ้าของ (เป็น client) ให้ปิด UI
            DisableUI();
        }
        if (!IsOwner) return; // ปิดการควบคุมถ้าไม่ใช่ผู้เล่นตัวเอง
        
        controller = GetComponent<CharacterController>();
        staminaSystem = GetComponent<StaminaSystem>();
        staminaSystem.Initialize(UpdateStaminaBar); // ส่ง Callback เพื่ออัปเดต UI
    
        if (staminaBar != null)
        {
            staminaBar.maxValue = staminaSystem.GetMaxStamina();
        }
    }

    private void Update()
    {
        if (!IsOwner) return;

        // ground check
        Grounded = Physics.Raycast(transform.position, Vector3.down, playerHeight * 0.5f + 0.2f, whatIsGround);
        
        MyInput();
        HandleCrouch();
       
        // ตรวจสอบการกด Shift และการเคลื่อนไหวเพื่อให้ลด Stamina
        if (Input.GetKey(KeyCode.LeftShift) && (horizontalInput != 0 || verticalInput != 0) && staminaSystem.GetCurrentStamina() > 0 && !isCrouching)
        {
            moveSpeed = runSpeed; // เพิ่มความเร็ว
            staminaSystem.DrainStamina(); // ลด Stamina
        }
        else if (!isCrouching)
        {
            moveSpeed = walkSpeed; // กลับไปเดินปกติ
            staminaSystem.RecoverStamina(); // ฟื้นฟู Stamina เมื่อไม่ได้วิ่ง
        }
        
        // เพิ่มแรงโน้มถ่วง
        moveDirection.y += gravity * Time.deltaTime;
        controller.Move(moveDirection * Time.deltaTime);
    }

    private void FixedUpdate()
    {
        if (IsOwner)
            MovePlayer();
    }

    private void MyInput()
    {
        horizontalInput = Input.GetAxisRaw("Horizontal");
        verticalInput = Input.GetAxisRaw("Vertical");
    }

    private void MovePlayer()
    {
        moveDirection = orientation.forward * verticalInput + orientation.right * horizontalInput;
        if (moveDirection.magnitude >= 0.1f)
        {
            controller.Move(moveDirection * moveSpeed * Time.deltaTime);
        }
    }
    
    private void UpdateStaminaBar(float newStamina)
    {
        if (staminaBar != null)
        {
            staminaBar.value = newStamina;
        }
    }
    
    private void HandleCrouch()
    {
        if (Input.GetKey(KeyCode.LeftControl))
        {
            isCrouching = true;
            moveSpeed = crouchSpeed;
        }
        else
        {
            isCrouching = false;
            moveSpeed = walkSpeed;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // เช็คเมื่อชนกับเลเยอร์ "Stair"
        if (((1 << other.gameObject.layer) & stairLayer) != 0)
        {
            gravity = -300f; // เพิ่มค่า gravity เมื่ออยู่บนบันได
        }
    }

    private void OnTriggerExit(Collider other)
    {
        // เช็คเมื่อออกจากเลเยอร์ "Stair"
        if (((1 << other.gameObject.layer) & stairLayer) != 0)
        {
            if (((1 << other.gameObject.layer) & whatIsGround) != 0)
            {
                gravity = 0f; // กลับค่า gravity เป็น 0 เมื่อไม่อยู่บนบันได
            }
        }
    }
    
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        if (!IsOwner)
        {
            // ถ้าไม่ใช่เจ้าของ ให้ปิด UI
            DisableUI();
        }
    }

    void DisableUI()
    {
        // ปิด UI ของ health bar หรือ UI อื่นๆ ที่ไม่ใช่เจ้าของ
        if (OtherplayerUI != null)
        {
            OtherplayerUI.SetActive(false); // ปิด UI ของ health bar
        }
    }
    
    // ฟังก์ชันสำหรับให้ CharacterMovement อ่านค่าความเร็วปัจจุบัน
    public float GetCurrentSpeed()
    {
        return moveSpeed;
    }
}
