using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using UnityEngine.VFX;

public class GachaMachine : MonoBehaviour, IInteractable
{
    [SerializeField] private List<UnlockData> AllUnlocks; // place all unlocks here, tracking all unlocks in the machine
    private List<UnlockData> StillToUnlock = new List<UnlockData>(); // used to track what is left to unlock, used for random selection
    // events
    public static Action OnBallOpen;
    public static Action OnUnlock;
    //

    private int CurrencyTaken = 0;
    private int CurrencyToTake = 1;
    [SerializeField] private GameObject winBallPrefab;
    [SerializeField] private Transform winBallSpawnLoc;

    [SerializeField] private TextMeshProUGUI UnlockText;
    [SerializeField] private Image UnlockImage;

    [SerializeField] private Button GatchaButton;
    [SerializeField] private Button ConfirmButton;

    [SerializeField] private Transform OpenGachaCameraPosition;
    private Vector3 PreviousCameraPosition;

    private List<Animation> BallMoveAnimations;

    [SerializeField] private VisualEffect RarityVFX;
    [SerializeField] private List<VisualEffectAsset> RarityVFXType;

    [Header("Sounds")]
    [SerializeField] private AudioSource AudioSource;
    [SerializeField] private AudioClip GatchaDropSound;
    [SerializeField] private AudioClip GatchaOpenSound;

    void Awake()
    {
        UnlockText.enabled = false;
        UnlockImage.enabled = false;
        ConfirmButton.gameObject.SetActive(false);

        GatchaButton.gameObject.SetActive(true);
        for (int i = 0; i < AllUnlocks.Count; i++)
        {
            if (AllUnlocks[i] != null)
            {
                AllUnlocks[i].SetCurrentlyUnlockedDefaults();
                if (!AllUnlocks[i].IsUnlocked()) // if the item is not unlocked add it to the list
                {
                    StillToUnlock.Add(AllUnlocks[i]);
                }
            }
        }
        
    }
    public void UnlockItem()
    {
        CurrencyTaken++;
        if (CurrencyToTake != CurrencyTaken) return;
        if (StillToUnlock.Count == 0)
        {
            return; // nothing left to unlock
        }
        OnUnlock?.Invoke();

        // Roll to unlock new item
        int i = UnityEngine.Random.Range(0, StillToUnlock.Count);
        RarityVFX.visualEffectAsset = RarityVFXType[StillToUnlock[i].Rarity];
        StartCoroutine(GachaOpenProcess());
        // UI change
        GatchaButton.gameObject.SetActive(false);

        if (StillToUnlock[i].IsUnlocked())
        {
            UnlockText.text = "Duplicate Item: " + StillToUnlock[i].UnlockName;
        }
        else
        {
            StillToUnlock[i].Unlock();
            UnlockText.text = "Unlocked: " + StillToUnlock[i].UnlockName;

        }
        UnlockImage.sprite = Sprite.Create(StillToUnlock[i].UnlockImage, new Rect(0.0f, 0.0f, StillToUnlock[i].UnlockImage.width, StillToUnlock[i].UnlockImage.height), new Vector2(0.5f, 0.5f), 100.0f);

        // wait until animations are done ShowUnlockItem before showing
        //ShowUnlockItem();
        // StillToUnlock.RemoveAt(i); // no duplicates in machine style

    }
    private void ShowUnlockItem()
    {
        UnlockText.enabled = true;
        UnlockImage.enabled = true;
        ConfirmButton.gameObject.SetActive(true);
    }

    IEnumerator GachaOpenProcess()
    {
        // lerp main camera to move infront of machine
        PreviousCameraPosition = Camera.main.transform.position;
        float timeElapsed = 0;
        float lerpDuration = 2;
        while (timeElapsed < lerpDuration)
        {
            Camera.main.transform.position = Vector3.Lerp(PreviousCameraPosition, OpenGachaCameraPosition.position, timeElapsed / lerpDuration);
            timeElapsed += Time.deltaTime;

            yield return null;
        }
        Camera.main.transform.position = OpenGachaCameraPosition.position;
        GameObject Ball = Instantiate(winBallPrefab, winBallSpawnLoc.position, Quaternion.identity, gameObject.transform);
        BallMoveAnimations = new List<Animation>(Ball.GetComponentsInChildren<Animation>());

        yield return new WaitForSeconds(1f);
        
        RarityVFX.Play(); // rare item buildup

        yield return new WaitForSeconds(2f);

        BallMoveAnimations[0].Play();
        BallMoveAnimations[1].Play();

        yield return new WaitForSeconds(1.5f);
        
        ShowUnlockItem(); // reveal

        StopCoroutine(GachaOpenProcess());
    }

    public void OnConfirm() // after confirming unlock
    {
        CurrencyTaken = 0;
        UnlockText.enabled = false;
        UnlockImage.enabled = false;
        ConfirmButton.gameObject.SetActive(false);
        Camera.main.transform.position = PreviousCameraPosition;

        if (StillToUnlock.Count > 0)
        {
            GatchaButton.gameObject.SetActive(true);
        }
        else
        {
            UnlockText.enabled = true;
            UnlockText.text = "All items unlocked!";
        }

    }

    // not used but required for interface defaults
    public void OnBeginDrag()
    {
       
    }
    public void OnDrag(Vector3 worldDelta)
    {
        
    }
    public void OnEndDrag()
    {
        
    }
    public void OnHoverEnter()
    {
        
    }
    public void OnHoverExit()
    {
       
    }
}
