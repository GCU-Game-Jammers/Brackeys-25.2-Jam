using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "UnlockData", menuName = "Data/UnlockData", order = 0)]
public class UnlockData : ScriptableObject
{
    public string UnlockName;

    public Texture2D UnlockImage;
    public Mesh UnlockMesh;
    [Range(0, 3)] public int Rarity = 1;
    [SerializeField] private bool bIsUnlockedDefault = false;
    private bool bIsCurrentlyUnlocked = false;

    public void SetCurrentlyUnlockedDefaults()
    {
        if (bIsUnlockedDefault) // bypass scriptable object saving stuff, 
        {
            bIsCurrentlyUnlocked = true; 
        }
        else
        {
            bIsCurrentlyUnlocked = false;
        }
    }

    public void Unlock()
    {
        bIsCurrentlyUnlocked = true;
    }
    public bool IsUnlocked()
    {
        return bIsCurrentlyUnlocked;
    }
}
