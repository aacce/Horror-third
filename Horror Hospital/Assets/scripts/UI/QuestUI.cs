using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class QuestUI : MonoBehaviour
{
    public Text questText; // UI Text ที่แสดงข้อความเควสต์
    public UIAnimator uiAnimator; // อ้างอิงไปยัง UIAnimator

    public static QuestUI Instance { get; private set; }

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void UpdateQuestText(int collected)
    {
        if (collected >= QuestManager.Instance.hillyGoal)
        {
            questText.text = "สำเร็จเควสแล้ว!";
            uiAnimator.ShowUI(); // เรียกใช้ UIAnimator ให้แสดง UI
        }
        else
        {
            questText.text = $"การรับยา Hilly {collected}/{QuestManager.Instance.hillyGoal}";
        }
    }
}