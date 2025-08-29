using UnityEngine;

[CreateAssetMenu(menuName = "Audio/Beatmap")]
public class Beatmap : ScriptableObject
{
    public AudioClip clip;
    public float bpm;
    public float offset;
    public TextAsset beatsCSV;
}