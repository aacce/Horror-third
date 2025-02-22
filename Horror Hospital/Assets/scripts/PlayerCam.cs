using Unity.Mathematics.Geometry;
using UnityEngine;

public class PlayerCam : MonoBehaviour
{
    public float sensX;
    public float sensY;
    
    public Transform orientation;

    float xRotation;
    float yRotation;
    
    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void Update()
    {
        //อัปเดตการหมุนของกล้องตามการเคลื่อนไหวของเมาส์
        float mouseX = Input.GetAxisRaw("Mouse X") * Time.deltaTime * sensX;
        float mouseY = Input.GetAxisRaw("Mouse Y") * Time.deltaTime * sensY;

        yRotation += mouseX;
        xRotation -= mouseY;
        //จำกัดการหมุนของกล้องในแนว X ให้อยู่ระหว่าง -85 ถึง 85 องศา เพื่อป้องกันการมองเกินระยะ (ไม่ให้มอง "พลิกกลับหัว")
        xRotation = Mathf.Clamp(xRotation, -85f, 85f);

        transform.rotation = Quaternion.Euler(xRotation, yRotation, 0);
        //ใช้ปรับการหมุนของตัวละครในแนวแกน Y เท่านั้น (ซ้าย-ขวา) เพื่อให้ตัวละครหมุนตามกล้อง แต่ไม่เอียงขึ้น/ลง
        orientation.rotation = Quaternion.Euler(0, yRotation, 0);
    }
}