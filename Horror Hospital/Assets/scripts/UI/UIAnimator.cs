using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class UIAnimator : MonoBehaviour
{
    public CanvasGroup questCanvasGroup; // CanvasGroup สำหรับควบคุม Fade
    public Image blackBackground; // พื้นหลังสีดำ
    public RectTransform questTransform; // UI Panel ที่ใช้แสดงข้อความ
    public float fadeDuration = 1.5f; // ระยะเวลา Fade In

    void Start()
    {
        // ซ่อน UI ตอนเริ่มเกม
        questCanvasGroup.alpha = 0f;
        blackBackground.color = new Color(0, 0, 0, 0);
        questTransform.localScale = Vector3.zero;
    }

    public void ShowUI()
    {
        StartCoroutine(FadeUI(true));
    }

    private IEnumerator FadeUI(bool isFadingIn)
    {
        float startAlpha = isFadingIn ? 0f : 1f;
        float endAlpha = isFadingIn ? 1f : 0f;

        float startScale = isFadingIn ? 0f : 1f;
        float endScale = isFadingIn ? 1f : 0f;

        float startBlackAlpha = isFadingIn ? 0f : 0.75f;
        float endBlackAlpha = isFadingIn ? 0.75f : 0f;

        float elapsedTime = 0f;

        while (elapsedTime < fadeDuration)
        {
            float progress = elapsedTime / fadeDuration;
            questCanvasGroup.alpha = Mathf.Lerp(startAlpha, endAlpha, progress);
            blackBackground.color = new Color(0, 0, 0, Mathf.Lerp(startBlackAlpha, endBlackAlpha, progress));
            questTransform.localScale = Vector3.Lerp(Vector3.one * startScale, Vector3.one * endScale, progress);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        questCanvasGroup.alpha = endAlpha;
        blackBackground.color = new Color(0, 0, 0, endBlackAlpha);
        questTransform.localScale = Vector3.one * endScale;

        if (isFadingIn)
        {
            yield return new WaitForSeconds(3f); // แสดง UI ค้างไว้ 3 วินาที
            StartCoroutine(FadeUI(false)); // Fade Out อัตโนมัติ
        }
    }
}