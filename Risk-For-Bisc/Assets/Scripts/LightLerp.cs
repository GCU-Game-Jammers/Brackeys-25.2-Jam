using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Light))]
public class LightLerp : MonoBehaviour
{
    public float duration = 2.0f;

    private Light lightComp;
    private Color[] colors = new Color[] { Color.red, Color.green, Color.blue };
    private int currentIndex = 0;

    void Start()
    {
        lightComp = GetComponent<Light>();
        StartCoroutine(CycleColors());
    }

    private IEnumerator CycleColors()
    {
        while (true)
        {
            Color startColor = colors[currentIndex];
            Color targetColor = colors[(currentIndex + 1) % colors.Length];
            float elapsed = 0f;

            while (elapsed < duration)
            {
                lightComp.color = Color.Lerp(startColor, targetColor, elapsed / duration);
                elapsed += Time.deltaTime;
                yield return null;
            }

            lightComp.color = targetColor;
            currentIndex = (currentIndex + 1) % colors.Length;
        }
    }
}