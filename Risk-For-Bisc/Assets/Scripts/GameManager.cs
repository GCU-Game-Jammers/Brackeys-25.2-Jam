using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;
    public RhythmManager rhythmManager;
    public NoteManager noteManager;


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
        StartCoroutine(PlayNote(beatEvent, dspTime/1000));
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
