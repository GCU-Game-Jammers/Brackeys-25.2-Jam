using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum CamereViewState {
    DJ,
    audience1
}
public class ScreenCameraView : MonoBehaviour
{
    private Animator animator;


    private void Awake()
    {
        animator = GetComponent<Animator>();
        if (animator == null)
            throw new System.Exception("Could not find Animator on Screen Camera View: Obj " + name);
    }

    private void ChangeState(CamereViewState state)
    {
        switch (state)
        {
            case CamereViewState.DJ:
                break;
            case CamereViewState.audience1:
                break;
            default:
                break;
        }
    }
}
