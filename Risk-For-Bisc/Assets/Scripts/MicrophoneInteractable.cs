using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MicrophoneInteractable : MonoBehaviour, IInteractable
{
    public void OnHoverEnter()
    {
        Debug.Log("Hover Microphone");
    }

    public void OnHoverExit()
    {
        Debug.Log("Exit Hover Microphone");
    }

    public void OnBeginDrag()
    {
        Debug.Log("Drag Microhpone");
    }

    public void OnDrag(Vector3 worldDelta)
    {
        Debug.Log("Drag Delta " + worldDelta);
    }

    public void OnEndDrag()
    {
        Debug.Log("Drag Microhpone end");
    }
}
