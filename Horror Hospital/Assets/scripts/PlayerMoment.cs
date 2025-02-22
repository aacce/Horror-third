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
    bool grounded;
    
    [Header("Slope Handling")]
    public float maxSlopeAngle;
    private RaycastHit slopeHit;
    
    /*[Header("Stair Climbing")]
    public LayerMask stairLayer;
    public float stairClimbSpeed = 3f;
    private bool onStairs = false;*/

    public Transform orientation;

    private float horizontalInput;
    private float verticalInput;
    private Vector3 moveDirection;
    private Rigidbody rb; 
    private bool isCrouching = false;
    
    public GameObject OtherplayerUI;
                                    
    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
        
        if (!IsOwner)
        {
            // ถ้าไม่ใช่เจ้าของ (เป็น client) ให้ปิด UI
            DisableUI();
        }
        if (!IsOwner) return; // ปิดการควบคุมถ้าไม่ใช่ผู้เล่นตัวเอง
        
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
        grounded = Physics.Raycast(transform.position, Vector3.down, playerHeight * 0.5f + 0.2f, whatIsGround);

        MyInput();
        HandleCrouch();
        SpeedControl();

        if (grounded) rb.linearDamping = groundDrag;
        else rb.linearDamping = 0;

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
    }

    private void FixedUpdate()
    {
        if (IsOwner)
            MovePlayer();

        /*if (!grounded) // ถ้าตัวละครไม่อยู่บนพื้น ให้เพิ่มแรงกดลง
        {
            rb.AddForce(Vector3.down * 150f, ForceMode.Acceleration);
        }*/
    }

    private void MyInput()
    {
        horizontalInput = Input.GetAxisRaw("Horizontal");
        verticalInput = Input.GetAxisRaw("Vertical");
    }

    private void MovePlayer()
    {
        moveDirection = orientation.forward * verticalInput + orientation.right * horizontalInput;

        // ถ้าอยู่บนบันได ให้ควบคุมการเคลื่อนที่ในแนว Y โดยเพิ่มความเร็วการปีนขึ้นบันได
        /*if (onStairs)
        {
            moveDirection.y = stairClimbSpeed * verticalInput; // ปีนขึ้นหรือลงบันได
        }*/
        
        // on slope
        if (OnSlope())
        {
            rb.AddForce(GetSlopeMoveDirection() * moveSpeed * 20f, ForceMode.Force);

            if (rb.linearVelocity .y > 0)
                rb.AddForce(Vector3.down * 80f, ForceMode.Force);
        }

        // เพิ่มแรงเคลื่อนที่ในแนวนอน (x, z)
        else if(grounded)
            rb.AddForce(moveDirection.normalized * moveSpeed * 10f, ForceMode.Force);
        
        // turn gravity off while on slope
        rb.useGravity = !OnSlope();
    }
    
    private void SpeedControl()
    {
        // limiting speed on slope
        if (OnSlope())
        {
            if (rb.linearVelocity   .magnitude > moveSpeed)
                rb.linearVelocity    = rb.linearVelocity   .normalized * moveSpeed;
        }
        // limiting speed on ground or in air
        else
        {
            Vector3 flatVel = new Vector3(rb.linearVelocity   .x, 0f, rb.linearVelocity.z);

            // limit linearVelocity  if needed
            if (flatVel.magnitude > moveSpeed)
            {
                Vector3 limitedVel = flatVel.normalized * moveSpeed;
                rb.linearVelocity  = new Vector3(limitedVel.x, rb.linearVelocity .y, limitedVel.z);
            }
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
    
    /*private void OnTriggerEnter(Collider other)
    {
        if (((1 << other.gameObject.layer) & stairLayer) != 0)
        {
            onStairs = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (((1 << other.gameObject.layer) & stairLayer) != 0)
        {
            onStairs = false;
        }
    }
    
    private void OnCollisionStay(Collision collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("Stairs"))
        {
            rb.linearVelocity  = new Vector3(rb.linearVelocity .x, -2f, rb.linearVelocity .z); // บังคับความเร็ว Y ลง
        }
    }*/


    
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

        // คุณสามารถเพิ่มการปิด UI อื่นๆ ที่ต้องการที่นี่
    }
    
    private bool OnSlope()
    {
        if(Physics.Raycast(transform.position, Vector3.down, out slopeHit, playerHeight * 0.5f + 0.3f))
        {
            float angle = Vector3.Angle(Vector3.up, slopeHit.normal);
            return angle < maxSlopeAngle && angle != 0;
        }

        return false;
    }

    private Vector3 GetSlopeMoveDirection()
    {
        return Vector3.ProjectOnPlane(moveDirection, slopeHit.normal).normalized;
    }
    
    // ฟังก์ชันสำหรับให้ CharacterMovement อ่านค่าความเร็วปัจจุบัน
    public float GetCurrentSpeed()
    {
        return moveSpeed;
    }
}
 