using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CRTScreen : MonoBehaviour
{
    public Vector2 resolution;
    public Material crtMaterial;
    public Camera camToRender;
    public Animator cameraAnimator;
    private RenderTexture rt;

    float timeToAnimate = 5.0f;
    float timer;
    void Start()
    {
        rt = new RenderTexture(256, 224, 16, RenderTextureFormat.ARGB32);
        rt.Create();

        //crtMaterial.mainTexture = rt;
        crtMaterial.SetTexture("_EmissionMap", rt);
        camToRender.targetTexture = rt;
        cameraAnimator.SetBool("Idle", true);
    }

    private void Update()
    {
        timer += Time.deltaTime;

        if (timer > timeToAnimate)
        {
            cameraAnimator.SetBool("Idle", true);
        }
        if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.A))
        {
            ResetTimer();
        }
        float x = Input.GetAxis("Mouse X");
        float y = Input.GetAxis("Mouse Y");
        if (x != 0 || y != 0)
        {
            ResetTimer();
        }
    }
    private void ResetTimer()
    {
        timer = 0;
        cameraAnimator.SetBool("Idle", false);
    }
    private void OnDestroy()
    {
        rt.Release();
    }
}
