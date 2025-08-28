using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;
    public RhythmManager rhythmManager;
    public NoteManager noteManager;
    public CameraManager cameraManager;

    private void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(gameObject);

        rhythmManager.OnBeatEventScheduled += BeatEvent;
    }

    private void BeatEvent(BeatEvent beatEvent, double dspTime)
    {
        float waitTime = (float)dspTime;
        switch (beatEvent.type)
        {
            case 0:
                break;
            case 1:// left
                noteManager.SpawnLeftArrow(waitTime);
                break;
            case 2:// up
                noteManager.SpawnUpArrow(waitTime);
                break;
            case 3:// down
                noteManager.SpawnDownArrow(waitTime);
                break;
            case 4:// right
                noteManager.SpawnRightArrow(waitTime);
                break;
            case 5:// start camera
                cameraManager.ChangeCamera(1);
                break;
            case 6:// stop camera
                cameraManager.ChangeCamera(0);
                break;
            default:
                Debug.Log("No event setup for index " + beatEvent.type);
                break;
        }
        //StartCoroutine(PlayNote(beatEvent, dspTime));
    }

    IEnumerator PlayNote(BeatEvent beatEvent, double dspTime)
    {
        Debug.Log("Waitign for " + dspTime);
        yield return new WaitForSecondsRealtime((float)dspTime);

        switch (beatEvent.type)
        {
            case 0:
                break;
            case 1:// left
                noteManager.SpawnLeftArrow();
                break;
            case 2:// up
                noteManager.SpawnUpArrow();
                break;
            case 3:// down
                noteManager.SpawnDownArrow();
                break;
            case 4:// right
                noteManager.SpawnRightArrow();
                break;
            default:
                Debug.Log("No event setup for index " + beatEvent.type);
                break;
        }
    }

    private void Update()
    {
        
    }


}
