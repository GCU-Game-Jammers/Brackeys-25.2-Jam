using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;
    public RhythmManager rhythmManager;
    public NoteManager noteManager;
    public CameraManager cameraManager;
    public HitEffectManager hitEffectManager;

    public Sprite perfectHit;
    public Sprite greatHit;
    public Sprite goodHit;
    public Sprite missHit;

    private void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(gameObject);

        rhythmManager.OnBeatEventScheduled += BeatEvent;
        noteManager.OnHit += HitNote;
    }

    private void HitNote(Arrow arrow, HitResult result, float arg3)
    {
        Sprite sprite = GetSprite(result);

        hitEffectManager.SpawnHitResultAtWorldPosition(arrow.gameObject.transform.position, result, sprite);

        switch (result)
        {
            case HitResult.Perfect:
                // +score, +combo
                break;
            case HitResult.Great:
                break;
            case HitResult.Good:
                break;
            case HitResult.Miss:
                // punish combo, screen-shake, red flash etc.
                break;
        }

        Destroy(arrow.gameObject);
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
                cameraManager.FlipCamera();
                break;
            default:
                Debug.Log("No event setup for index " + beatEvent.type);
                break;
        }
    }


    private Sprite GetSprite(HitResult hitResult)
    {
        switch (hitResult)
        {
            case HitResult.Perfect:
                return perfectHit;
            case HitResult.Great:
                return greatHit;
            case HitResult.Good:
                return goodHit;
            case HitResult.Miss:
                return missHit;
        }

        return null;
    }
}
