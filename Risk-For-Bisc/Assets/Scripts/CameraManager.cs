using System.Collections;
using UnityEngine;

public class CameraManager : MonoBehaviour
{
    [SerializeField] private Camera mainCamera;
    [SerializeField] private Camera djCamera;

    private Animator anim;

    private void Awake()
    {
        anim = GetComponent<Animator>();
    }

    private void Start()
    {
        StartCoroutine(StartDJCamera());
    }

    private IEnumerator StartDJCamera()
    {
        yield return new WaitForSeconds(1.0f);

        SwitchToDjCamera();
        anim.SetInteger("Camera Index", 1);

        yield return new WaitForSeconds(5.0f);

        anim.SetInteger("Camera Index", 2);

        yield return new WaitForSeconds(6.0f);

        anim.SetInteger("Camera Index", 0);
        SwitchToMainCamera();
    }

    #region Camera switching
    private void SwitchToMainCamera()
    {
        mainCamera.enabled = true;
        djCamera.enabled = false;
    }

    private void SwitchToDjCamera()
    {
        mainCamera.enabled = false;
        djCamera.enabled = true;
    }
    #endregion
}
