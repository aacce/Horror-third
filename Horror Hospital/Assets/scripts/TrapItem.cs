using UnityEngine;
using System.Collections;

public class TrapItem : MonoBehaviour
{
    public float trapDuration = 5f; // เวลาติดกับดัก

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log(other.name + " ติดกับดัก!");

            // หา GameObject Parent (หรือใช้ตัวเองถ้าไม่มี Parent)
            Transform playerTransform = other.transform.parent != null ? other.transform.parent : other.transform;

            StartCoroutine(TrapEffect(playerTransform.gameObject));
        }
    }

    private IEnumerator TrapEffect(GameObject playerObj)
    {
        PlayerMoment playerScript = playerObj.GetComponent<PlayerMoment>();

        // ปิดการควบคุมผู้เล่น (หยุดเดิน)
        if (playerScript != null)
        {
            playerScript.enabled = false; // ปิดการควบคุม
        }

        Debug.Log("ผู้เล่นหยุดเดินแล้ว!");

        yield return new WaitForSeconds(trapDuration); // รอ 5 วิ

        // เปิดการควบคุมผู้เล่น (ให้เดินได้ตามปกติ)
        if (playerScript != null)
        {
            playerScript.enabled = true; // เปิดให้เดินได้ใหม่
        }

        Debug.Log("ผู้เล่นเดินต่อได้แล้ว!");

        // ทำลายกับดักหลังจากหมดเวลา
        Destroy(gameObject);
    }
}