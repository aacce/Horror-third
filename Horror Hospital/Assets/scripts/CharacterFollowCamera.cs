using UnityEngine;
using Unity.Netcode;

public class CharacterFollowCamera : NetworkBehaviour
{
    public Transform player; // ตัวละครที่เราต้องการให้หมุน
    public Transform cameraTransform; // กล้องที่เราต้องการอ้างอิง
    
    private NetworkVariable<Quaternion> networkRotation = new NetworkVariable<Quaternion>(
        Quaternion.identity,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Owner
    );

    private void Start()
    {
        if (!IsOwner)
        {
            cameraTransform.gameObject.SetActive(false); // ปิดกล้องของคนอื่น
        }
    }
    
    void Update()
    {
        if (IsOwner)
        {
            // ใช้การหมุนของกล้องในแกน Y (แนวนอน) เพื่อปรับตัวละคร
            Vector3 cameraForward = cameraTransform.forward;
            cameraForward.y = 0; // ล็อกแกน Y เพื่อไม่ให้ตัวละครหมุนเอียงขึ้น/ลงตามกล้อง
            if (cameraForward != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(cameraForward);
                player.rotation = Quaternion.Slerp(player.rotation, targetRotation, Time.deltaTime * 10f); // หมุนตัวละครแบบ Smooth
                networkRotation.Value = player.rotation; // ส่งค่า Rotation ไปยัง Network
            }
        }
        else
        {
            player.rotation = networkRotation.Value; // ใช้ค่า Rotation จาก Network
        }
    }
}