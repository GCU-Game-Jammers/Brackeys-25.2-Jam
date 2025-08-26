using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;
    public RhythmManager rhythmManager;


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
        Debug.Log("BEAT HAPPENED AT " +  beatEvent.time);
    }

    private void Update()
    {
        
    }


}
