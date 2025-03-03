using UnityEngine;
using Unity.Netcode;

public class KillerAnimatorController : NetworkBehaviour
{
    private Animator animator;
    private KillerMovement kPlayerMoment;

    private Vector3 inputVec;
    private bool isAttacking = false; // เพิ่มตัวแปรเช็คสถานะโจมตี

    void Start()
    {
        if (!IsOwner) return;

        animator = GetComponentInChildren<Animator>();
        if (animator == null)
        {
            Debug.LogError("Animator ไม่ถูกกำหนดใน KillerAnimatorController!");
            return;
        }

        kPlayerMoment = GetComponentInParent<KillerMovement>();
        if (kPlayerMoment == null)
        {
            Debug.LogError("ไม่พบ KillerMovement ใน Parent!");
            return;
        }
    }

    void Update()
    {
        if (!IsOwner || animator == null || kPlayerMoment == null) return;

        // รับค่าการเคลื่อนไหว
        float x = -(Input.GetAxisRaw("Vertical"));
        float z = Input.GetAxisRaw("Horizontal");
        inputVec = new Vector3(x, 0, z);

        // ส่งค่าทิศทางไปยัง Animator
        animator.SetFloat("input X", inputVec.x);
        animator.SetFloat("input Z", inputVec.z);

        // อัปเดตสถานะการเดิน
        bool isWalking = inputVec.magnitude > 0;
        animator.SetBool("isWalkForward", isWalking);
        animator.SetBool("isWalkBack", x > 0); // ถอยหลัง

        // โจมตีเมื่อคลิกเมาส์ซ้าย
        if (Input.GetMouseButtonDown(0) && !isAttacking) 
        {
            TriggerAttack();
        }
    }

    public void TriggerAttack()
    {
        if (animator != null)
        {
            isAttacking = true;
            animator.SetBool("isAttack", true); // ใช้ SetBool แทน SetTrigger
            Invoke(nameof(ResetAttack), 1f); // ตั้งเวลาให้รีเซ็ต isAttack หลัง 1 วินาที
        }
    }

    private void ResetAttack()
    {
        isAttacking = false;
        animator.SetBool("isAttack", false); // กลับไป Idle ได้
    }
    
    public void SetWalking(bool isWalking)
    {
        if (animator != null)
        {
            animator.SetBool("isWalkForward", isWalking);
        }
    }
}