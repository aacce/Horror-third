using UnityEngine;
using Unity.Netcode;

public class KillerMovement : NetworkBehaviour
{
    [Header("Movement")] 
    public float moveSpeed;

    [Header("Attack Settings")]
    public float attackDamage = 25f;
    public float attackCooldown = 1f;
    private bool canAttack = true;

    [Header("References")]
    private Transform playerTarget;
    private CharacterController controller;
    private KillerAnimatorController animatorController;
    
    [Header("Ground Check")] 
    public LayerMask whatIsGround;
    private bool Grounded;
    public float gravity = 0f;
    public float groundCheckDistance = 0.2f;
    
    public Transform orientation;
    private float horizontalInput;
    private float verticalInput;
    private Vector3 moveDirection;
    
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
        animatorController = GetComponent<KillerAnimatorController>(); // เชื่อมต่อกับ KillerAnimatorController
    }

    private void Update()
    {
        if (!IsOwner) return;
        
        // ตรวจสอบว่าติดพื้นหรือไม่
        Grounded = Physics.Raycast(transform.position, Vector3.down, groundCheckDistance, whatIsGround);
        if (Grounded && moveDirection.y < 0)
        {
            moveDirection.y = -2f; // ให้ตัวละครติดพื้น ไม่ลอย
        }
        
        MoveKiller();
        
        // เพิ่มแรงโน้มถ่วง
        moveDirection.y += gravity * Time.deltaTime;
        controller.Move(moveDirection * Time.deltaTime);
    }

    private void MoveKiller()
    {
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");
        moveDirection = orientation.forward * vertical + orientation.right * horizontal;
        
        // เริ่มแอนิเมชันการเคลื่อนไหว
        if (moveDirection.magnitude >= 0.1f)
        {
            controller.Move(moveDirection * moveSpeed * Time.deltaTime);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerTarget = other.transform;
        }
        
        // เช็คเมื่อชนกับเลเยอร์ "Stair"
        if (((1 << other.gameObject.layer) & stairLayer) != 0)
        {
            gravity = -300f; // เพิ่มค่า gravity เมื่ออยู่บนบันได
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerTarget = null;
        }
        
        // เช็คเมื่อออกจากเลเยอร์ "Stair"
        if (((1 << other.gameObject.layer) & stairLayer) != 0)
        {
            if (((1 << other.gameObject.layer) & whatIsGround) != 0)
            {
                gravity = 0f; // กลับค่า gravity เป็น 0 เมื่อไม่อยู่บนบันได
            }
        }
    }

    private void FixedUpdate()
    {
        if (!IsOwner) return;
        if (Input.GetKeyDown(KeyCode.Mouse0) && canAttack && playerTarget != null)
        {
            AttackPlayer();
        }
    }

    private void AttackPlayer()
    {
        canAttack = false;
        animatorController.TriggerAttack(); // เล่นแอนิเมชันการโจมตี
        Invoke(nameof(ResetAttack), attackCooldown);
        playerTarget.GetComponent<HealthBarController>().TakeDamageServerRpc(attackDamage);
    }

    private void ResetAttack()
    {
        canAttack = true;
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
