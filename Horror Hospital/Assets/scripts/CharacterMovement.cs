using UnityEngine;
using Unity.Netcode;

public class CharacterMovement : NetworkBehaviour
{
    private Animator animator;
    private PlayerMoment playerMoment;
    
    private Vector3 inputVec; // เก็บข้อมูลอินพุตจากผู้เล่น
    private bool isRunning; // ตรวจสอบสถานะการวิ่ง
    private bool isRunningBack; // ตรวจสอบสถานะการวิ่งถอยหลัง
    private bool isCrouching; // ตรวจสอบสถานะการย่อตัว
    
    void Start()    
    {
        if (!IsOwner) return;
        Time.timeScale = 1;
        animator = GetComponent<Animator>();
        playerMoment = GetComponentInParent<PlayerMoment>(); // ใช้ GetComponentInParent แทนเพื่อให้หา PlayerMoment ใน GameObject 'player'
    }

    void Update()
    {
        if (!IsOwner) return;
        // รับค่าการเคลื่อนไหวจากปุ่ม (WASD หรือ ลูกศร)
        float x = -(Input.GetAxisRaw("Vertical")); // แกนหน้า-หลัง
        float z = Input.GetAxisRaw("Horizontal"); // แกนซ้าย-ขวา
        inputVec = new Vector3(x, 0, z);

        // ตรวจสอบการวิ่ง
        isRunning = Input.GetKey(KeyCode.LeftShift) && x < 0 && !isCrouching;
        isRunningBack = Input.GetKey(KeyCode.LeftShift) && x > 0 && !isCrouching;

        // ส่งค่าทิศทางการเคลื่อนไหวไปยัง Animator
        animator.SetFloat("input X", z);
        animator.SetFloat("input Z", -(x));
        // อัปเดตสถานะการย่อตัว
        animator.SetBool("isCrouching", isCrouching);

        // เช็คสถานะการย่อตัว
        if (Input.GetKey(KeyCode.LeftControl))
        {
            isCrouching = true;
        }
        else
        {
            isCrouching = false;
        }
       

        // การอัปเดตอนิเมชันเมื่อเดินหรือวิ่ง
        if (x != 0 || z != 0) // ถ้าเคลื่อนที่
        {
            if (isCrouching) // กำลังย่อตัวเดิน
            {
                animator.SetBool("isCrouchWalking", true);
                animator.SetBool("isWalking", false);
                animator.SetBool("isRunning", false);
                animator.SetBool("isRunningBack", false);
                animator.SetBool("isMoonwalking", false);
            }
            else // เดินปกติ
            {   
                // เช็คว่ากด W หรือ S
                if (x < 0) // กด W (เดินหน้า)
                {
                    animator.SetBool("isWalking", true);
                    animator.SetBool("isMoonwalking", false);
                    animator.SetBool("isRunningBack", false); 
                    animator.SetBool("isCrouchWalking", false);
                }
                else if (x > 0) // กด S (เดินถอยหลัง)
                {
                    animator.SetBool("isMoonwalking", true);
                    animator.SetBool("isWalking", false);
                    animator.SetBool("isCrouchWalking", false);
                    animator.SetBool("isRunningBack", isRunningBack);
                }
                if (z != 0) // ถ้ากด A หรือ D (เดินซ้ายหรือขวา)
                {
                    animator.SetBool("isWalking", true);
                }
                animator.SetBool("isRunning", isRunning);
            }   
        }
        else // ไม่เคลื่อนที่
        {
            animator.SetBool("isWalking", false);
            animator.SetBool("isMoonwalking", false);
            animator.SetBool("isRunning", false);
            animator.SetBool("isRunningBack", false);
            animator.SetBool("isCrouchWalking", false);

            if (isCrouching)
            {
                animator.SetBool("isCrouchIdle", true);
            }
            else
            {
                animator.SetBool("isCrouchIdle", false);
            }
        }

        // คำนวณความเร็วการเคลื่อนไหว
        float currentSpeed = playerMoment.GetCurrentSpeed();
        Vector3 moveDirection = inputVec.normalized * currentSpeed;

        
    }
}
