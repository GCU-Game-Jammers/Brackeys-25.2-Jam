using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class HitEffect : MonoBehaviour
{
    public RectTransform Rect => (RectTransform)transform;
    public Image iconImage;
    public CanvasGroup canvasGroup;
    public float lifeTime = 0.45f;

    Action onComplete;

    float popStartScale = 0.6f;
    float popPeakScale = 1.12f;
    float popTime = 0.09f;

    private Vector3 startPos;
    public void Setup(HitResult r, Sprite overrideSprite, Vector3 pos, Action onComplete)
    {
        this.onComplete = onComplete;
        Rect.position = pos;
        startPos = pos;
        if (overrideSprite != null) iconImage.sprite = overrideSprite;
        switch (r)
        {
            case HitResult.Perfect:
                iconImage.color = new Color(1f, 0.85f, 0.25f, 1f);
                transform.localScale = new Vector3(.9f, .9f, .9f);
                break;
            case HitResult.Great:
                iconImage.color = new Color(0.4f, 0.9f, 1f, 1f);
                transform.localScale = new Vector3(0.75f, 0.75f, 0.75f);
                break;
            case HitResult.Good:
                iconImage.color = new Color(0.6f, 0.6f, 0.6f, 0.6f);
                transform.localScale = new Vector3(1f, 1f, 1f);
                break;
            case HitResult.Miss:
                iconImage.color = new Color(1f, 0.35f, 0.35f, 1f);
                transform.localScale = new Vector3(1f, 1f, 1f);
                break;
        }

        Rect.anchoredPosition += UnityEngine.Random.insideUnitCircle * 6f;

        StopAllCoroutines();
        StartCoroutine(PlayAnim());
    }

    IEnumerator PlayAnim()
    {
        float t = 0f;
        canvasGroup.alpha = 1f;
        Rect.localScale = Vector3.one * popStartScale;

        Vector3 endPos = startPos + new Vector3(0f, 40f, 0.0f);
        
        while (t < popTime)
        {
            t += Time.unscaledDeltaTime;
            float p = Mathf.Clamp01(t / popTime);
            float eased = EaseOutBack(p);
            float s = Mathf.Lerp(popStartScale, popPeakScale, eased);
            Rect.localScale = Vector3.one * s;
            yield return null;
        }

        float fadeDur = Mathf.Max(0.01f, lifeTime - popTime);
        t = 0f;
        float startScale = Rect.localScale.x;
        float endScale = startScale * 1.06f;
        while (t < fadeDur)
        {
            t += Time.unscaledDeltaTime;
            float p = Mathf.Clamp01(t / fadeDur);
            canvasGroup.alpha = Mathf.Lerp(1f, 0f, p);
            float s = Mathf.Lerp(startScale, endScale, p);
            Rect.localScale = Vector3.one * s;
            Rect.position = Vector3.Lerp(startPos, endPos, p);
            yield return null;
        }

        End();
    }

    float EaseOutBack(float p) { return 1 + (--p) * p * (2.70158f * p + 1.70158f); }

    void End()
    {
        onComplete?.Invoke();
    }
}
