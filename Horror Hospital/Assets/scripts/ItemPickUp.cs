using UnityEngine;
using Unity.Netcode;

public class ItemPickUp : NetworkBehaviour
{
    public enum ItemType { DamagePotion, HealingPotion, Hilly } // ประเภทไอเท็ม
    public ItemType itemType;

    private bool isPlayerInRange = false;
    private ItemController itemController;
    
    public float pickUpRange = 5f; // ระยะที่ผู้เล่นสามารถเก็บไอเท็มได้
    public float fieldOfView = 60f; // ฟิลด์ของการมองเห็น
    private Camera playerCamera; // กล้องของผู้เล่น (จะใช้ Main Camera)
    
    void Start()
    {
        playerCamera = Camera.main; // ค้นหากล้องหลัก
    }
    
    void Update()
    {
        if (isPlayerInRange && IsLookingAtItem() && Vector3.Distance(transform.position, playerCamera.transform.position) <= pickUpRange)
        {
            if (Input.GetKeyDown(KeyCode.F)) 
            {
                Debug.Log($"[Client] Player {NetworkManager.Singleton.LocalClientId} pressed F to pick up {itemType}");

                if (!IsSpawned)
                {
                    Debug.LogWarning("NetworkObject is not spawned yet!");
                    GetComponent<NetworkObject>().Spawn();
                }

                PickUpItemServerRpc(NetworkManager.Singleton.LocalClientId);
            }
        }
    }
    
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInRange = true;
            itemController = other.GetComponent<ItemController>();
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInRange = false;
            itemController = null;
        }
    }
    
    [ServerRpc(RequireOwnership = false)]
    void PickUpItemServerRpc(ulong playerId, ServerRpcParams rpcParams = default)
    {
        NetworkObject networkObject = GetComponent<NetworkObject>();
        if (!networkObject.IsSpawned)
        {
            networkObject.Spawn();
        }

        // เปลี่ยน Ownership ให้ playerId ที่เก็บของ
        networkObject.ChangeOwnership(playerId);

        if (itemController != null && itemType == ItemType.Hilly && !itemController.IsVulnerable())
        {
            QuestManager.Instance.CollectHillyServerRpc(playerId);
        }

        PickUpItemClientRpc(playerId);
    }


    
    [ClientRpc]
    void PickUpItemClientRpc(ulong playerId)
    {
        if (itemController != null)
        {
            // ตรวจสอบว่าอยู่ในสถานะ vulnerable หรือไม่ ถ้าใช่ไม่ให้เก็บยา Hilly
            if (itemType == ItemType.Hilly && itemController.IsVulnerable())
            {
                Debug.Log("Cannot pick up Hilly while vulnerable!");
                return; // ไม่ให้เก็บยา Hilly
            }

            if (itemType == ItemType.DamagePotion)
            {
                itemController.UseDamagePotion();
            }
            else if (itemType == ItemType.HealingPotion)
            {
                itemController.UseHealingPotion();
            }
            else if (itemType == ItemType.Hilly)
            {
                itemController.UseHilly();
            }
        }

        Destroy(gameObject); // ทำลายไอเท็มหลังจากเก็บ
    }
    
    // ฟังก์ชันตรวจสอบว่าไอเท็มอยู่ในมุมมองของผู้เล่นหรือไม่
    bool IsLookingAtItem()
    {
        Vector3 directionToItem = transform.position - playerCamera.transform.position;
        float angle = Vector3.Angle(playerCamera.transform.forward, directionToItem);
        return angle <= fieldOfView / 2;
    }
}
