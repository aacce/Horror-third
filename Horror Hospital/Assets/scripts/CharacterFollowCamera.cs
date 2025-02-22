using UnityEngine;

public class CharacterFollowCamera : MonoBehaviour
{
    public Transform player; // ตัวละครที่เราต้องการให้หมุน
    public Transform cameraTransform; // กล้องที่เราต้องการอ้างอิง

    void Update()
    {
        // ใช้การหมุนของกล้องในแกน Y (แนวนอน) เพื่อปรับตัวละคร
        Vector3 cameraForward = cameraTransform.forward;
        cameraForward.y = 0; // ล็อกแกน Y เพื่อไม่ให้ตัวละครหมุนเอียงขึ้น/ลงตามกล้อง
        if (cameraForward != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(cameraForward);
            player.rotation = Quaternion.Slerp(player.rotation, targetRotation, Time.deltaTime * 10f); // หมุนตัวละครแบบ Smooth
        }
    }
}