using UnityEngine;
using Unity.Netcode;
using UnityEngine.UI;
using System.Collections;

public class DoorController : NetworkBehaviour
{
    public Transform leftDoor; // ประตูฝั่งซ้าย
    public Transform rightDoor; // ประตูฝั่งขวา
    public Slider doorProgressBar; // หลอดพลังงาน
    public float openSpeed = 2f; // ความเร็วเปิดประตู
    public float progressSpeed = 0.5f; // ความเร็วเติมหลอด

    private bool isOpening = false;
    private bool doorOpened = false;
    private float progress = 0f;

    private void Start()
    {
        if (doorProgressBar != null)
        {
            doorProgressBar.gameObject.SetActive(false); // ซ่อน UI ตอนเริ่ม
        }
    }

    private void Update()
    {
        if (!IsOwner || doorOpened) return;
        
        if (isOpening && Input.GetKey(KeyCode.F))
        {
            progress += Time.deltaTime * progressSpeed;
            doorProgressBar.value = progress;

            if (progress >= 1f)
            {
                OpenDoorServerRpc();
            }
        }
        else if (!Input.GetKey(KeyCode.F))
        {
            progress -= Time.deltaTime * progressSpeed;
            progress = Mathf.Clamp(progress, 0f, 1f);
            doorProgressBar.value = progress;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && QuestManager.Instance.IsQuestComplete())
        {
            isOpening = true;
            doorProgressBar.gameObject.SetActive(true); // แสดง UI เมื่อเข้าใกล้
            Debug.Log("ผู้เล่นเข้าใกล้ประตู!");
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isOpening = false;
            doorProgressBar.gameObject.SetActive(false); // ซ่อน UI เมื่อออกห่าง
            Debug.Log("ผู้เล่นออกจากบริเวณประตู");
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void OpenDoorServerRpc()
    {
        OpenDoorClientRpc();
    }

    [ClientRpc]
    private void OpenDoorClientRpc()
    {
        doorOpened = true;
        StartCoroutine(OpenDoorAnimation());
    }

    private IEnumerator OpenDoorAnimation()
    {
        float targetDistance = 3f; // ระยะที่ประตูจะเปิดออก
        float movedDistance = 0f;

        Vector3 leftStart = leftDoor.position;
        Vector3 rightStart = rightDoor.position;

        while (movedDistance < targetDistance)
        {
            float step = openSpeed * Time.deltaTime;
            leftDoor.position += Vector3.back * step;  // เปิดถอยหลัง
            rightDoor.position += Vector3.forward * step; // เปิดไปข้างหน้า
            movedDistance = Vector3.Distance(leftStart, leftDoor.position);
            yield return null;
        }
        // ซ่อน UI หลังจากเปิดประตู
        doorProgressBar.gameObject.SetActive(false);
    }
}