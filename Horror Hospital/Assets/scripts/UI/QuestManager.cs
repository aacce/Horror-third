using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class QuestManager : NetworkBehaviour
{
    public static QuestManager Instance { get; private set; }

    private Dictionary<ulong, int> playerHillyCount = new Dictionary<ulong, int>(); // นับจำนวน Hilly แต่ละ Player
    public int hillyGoal;
    // เพิ่ม NetworkVariable สำหรับเก็บจำนวน Hilly
    private NetworkVariable<int> hillyCount = new NetworkVariable<int>(0);

    void Start()
    {
        // เรียกอัพเดต UI ทันทีเมื่อเริ่มเกม
        QuestUI.Instance.UpdateQuestText(hillyCount.Value);  
    }
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void CollectHillyServerRpc(ulong playerId)
    {
        if (!playerHillyCount.ContainsKey(playerId))
        {
            playerHillyCount[playerId] = 0;
        }

        playerHillyCount[playerId]++;

        // ส่งค่ากลับไปให้ Client ที่เป็นเจ้าของเท่านั้น
        CollectHillyClientRpc(playerId, playerHillyCount[playerId]);
    }

    [ClientRpc]
    private void CollectHillyClientRpc(ulong playerId, int count)
    {
        if (NetworkManager.Singleton.LocalClientId == playerId)
        {
            Debug.Log($"[Client] Updating quest UI for Player {playerId}: {count}/{hillyGoal}");
            
            if (QuestUI.Instance != null) // ป้องกัน null reference
            {
                QuestUI.Instance.UpdateQuestText(count);
            }
        }
    }
}       