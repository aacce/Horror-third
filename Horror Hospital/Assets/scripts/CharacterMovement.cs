using UnityEngine;

public class CharacterMovement : MonoBehaviour
{
    private Animator animator;
    private CharacterController characterController;

    public float speed = 6.0f; // ความเร็วปกติ
    public float runSpeed = 12.0f; // ความเร็วเมื่อวิ่ง

    private Vector3 inputVec; // เก็บข้อมูลอินพุตจากผู้เล่น
    private bool isRunning; // ตรวจสอบสถานะการวิ่ง
    private bool isRunningBack; // ตรวจสอบสถานะการวิ่งถอยหลัง


    void Start()
    {
        Time.timeScale = 1;
        animator = GetComponent<Animator>();
        characterController = GetComponent<CharacterController>();
    }

    void Update()
    {
        // รับค่าการเคลื่อนไหวจากปุ่ม (WASD หรือ ลูกศร)
        float x = -(Input.GetAxisRaw("Vertical")); // แกนหน้า-หลัง
        float z = Input.GetAxisRaw("Horizontal"); // แกนซ้าย-ขวา
        inputVec = new Vector3(x, 0, z);

        // ตรวจสอบสถานะการวิ่งและวิ่งถอยหลัง
        isRunning = Input.GetKey(KeyCode.LeftShift) && x < 0; // วิ่งเมื่อกด Shift + W
        isRunningBack = Input.GetKey(KeyCode.LeftShift) && x > 0; // วิ่งถอยหลังเมื่อกด Shift + S

        // ส่งค่าทิศทางการเคลื่อนไหวไปยัง Animator
        animator.SetFloat("input X", z);
        animator.SetFloat("input Z", -(x));

        // เช็คการเคลื่อนไหว
        if (x != 0 || z != 0)
        {
            animator.SetBool("isWalking", true);

            // เช็คว่ากด W หรือ S
            if (x < 0) // กด W (เดินหน้า)
            {
                animator.SetBool("isWalking", true);
                animator.SetBool("isMoonwalking", false);
                animator.SetBool("isRunningBack", false); 
            }
            else if (x > 0) // กด S (เดินถอยหลัง)
            {
                animator.SetBool("isMoonwalking", true);
                animator.SetBool("isWalking", false);
                animator.SetBool("isRunningBack", isRunningBack);
            }

            // เพิ่มสถานะการวิ่ง
            animator.SetBool("isRunning", isRunning);
        }
        else
        {
            // ถ้าไม่ได้กดปุ่มใด ๆ ให้หยุดการเคลื่อนไหว
            animator.SetBool("isWalking", false);
            animator.SetBool("isMoonwalking", false);
            animator.SetBool("isRunning", false);
            animator.SetBool("isRunningBack", false);

        }

        // คำนวณความเร็วการเคลื่อนไหว
        float currentSpeed = isRunning || isRunningBack ? runSpeed : speed;
        Vector3 moveDirection = inputVec.normalized * currentSpeed;

        // เคลื่อนที่จริงในโลก 3D
        characterController.Move(moveDirection * Time.deltaTime);
    }
}
