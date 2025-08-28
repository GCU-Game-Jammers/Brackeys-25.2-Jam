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

    public void ChangeCamera(int index)
    {
        if (index == 0) SwitchToMainCamera();
        else if (index == 1) StartCoroutine(DJCameraSequence());
        anim.SetInteger("Camera Index", index);
    }

    private IEnumerator DJCameraSequence()
    {
        SwitchToDjCamera();

        yield return new WaitForSeconds(5.0f);

        anim.SetInteger("Camera Index", 2);
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
