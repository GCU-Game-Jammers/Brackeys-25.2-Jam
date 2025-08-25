using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GachaMachine : MonoBehaviour
{
    [SerializeField] private List<UnlockData> AllUnlocks; // place all unlocks here, tracking all unlocks in game
    private List<UnlockData> StillToUnlock = new List<UnlockData>(); // used to track what is left to unlock, used for random selection

    private int Currency = 0;

    [SerializeField] private TextMeshProUGUI UnlockText;
    [SerializeField] private Image UnlockImage;

    [SerializeField] private Button GatchaButton;
    [SerializeField] private Button ConfirmButton;

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
        if (StillToUnlock.Count == 0)
        {
            return; // nothing left to unlock
        }
        GatchaButton.gameObject.SetActive(false);
        ConfirmButton.gameObject.SetActive(true);
        // Roll to unlock new item
        int i = UnityEngine.Random.Range(0, StillToUnlock.Count);
        StillToUnlock[i].Unlock();

        UnlockText.text = "Unlocked: " + StillToUnlock[i].UnlockName;
        UnlockText.enabled = true;
        UnlockImage.sprite = Sprite.Create(StillToUnlock[i].UnlockImage, new Rect(0.0f, 0.0f, StillToUnlock[i].UnlockImage.width, StillToUnlock[i].UnlockImage.height), new Vector2(0.5f, 0.5f), 100.0f);
        UnlockImage.enabled = true;
        StillToUnlock.RemoveAt(i);
    }

    public void OnConfirm() // after confirming unlock
    {
        UnlockText.enabled = false;
        UnlockImage.enabled = false;
        ConfirmButton.gameObject.SetActive(false);
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
}
