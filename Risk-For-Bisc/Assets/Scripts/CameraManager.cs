using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class CameraManager : MonoBehaviour
{
    [SerializeField] private Camera mainCamera;
    [SerializeField] private Camera djCamera;
    [SerializeField] private Volume postProcessing;

    private Camera activeCamera;
    private Animator anim;

    private PaniniProjection paniniProjection;

    private bool interludeCamEnabled = false;
    
    private void Awake()
    {
        activeCamera = mainCamera;
        anim = GetComponent<Animator>();
    }

    void Start()
    {
        if (postProcessing.profile.TryGet<PaniniProjection>(out paniniProjection));
        {
            paniniProjection.distance.value = 0.0f;
        }
    }
    public void FlipCamera()
    {
        interludeCamEnabled = !interludeCamEnabled;
        if (interludeCamEnabled)
        {
            StartCoroutine(DJCameraSequence());

            anim.SetInteger("Camera Index", 1);
        }
        else
        {
            SwitchToMainCamera();

            anim.SetInteger("Camera Index", 0);
        }
    }
    private IEnumerator DJCameraSequence()
    {
        SwitchToDjCamera();

        yield return new WaitForSeconds(4.0f);

        anim.SetInteger("Camera Index", 2);

        yield return new WaitForSeconds(2.5f);

        anim.SetInteger("Camera Index", 3);

        yield return new WaitForSeconds(2.0f);

        anim.SetInteger("Camera Index", 0);

    }

    #region Camera switching
    private void SwitchToMainCamera()
    {
        activeCamera = mainCamera;
        mainCamera.enabled = true;
        djCamera.enabled = false;
        paniniProjection.distance.value = 0.0f;
    }

    private void SwitchToDjCamera()
    {
        activeCamera = djCamera;
        mainCamera.enabled = false;
        djCamera.enabled = true;
        paniniProjection.distance.value = 1.0f;
    }
    #endregion
}
