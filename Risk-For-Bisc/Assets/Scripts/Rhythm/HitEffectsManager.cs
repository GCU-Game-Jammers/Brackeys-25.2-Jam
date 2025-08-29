using System.Collections.Generic;
using UnityEngine;

public class HitEffectManager : MonoBehaviour
{
    public float baseSFXVolume = 0.1f;
    public float sfxPitchVariation = 0.1f;
    public float sfxVolumeVariation = 0.1f;
    
    [Header("Canvas")]
    public RectTransform uiCanvasRect;
    public Camera uiCamera;

    [Header("Prefab (RectTransform with HitEffect)")]
    public GameObject hitEffectPrefab;
    public int poolSize = 24;

    [Header("SFX (assign)")]
    public AudioClip sfxPerfect;
    public AudioClip sfxGreat;
    public AudioClip sfxGood;
    public AudioClip sfxMiss;
  

    Queue<HitEffect> pool;

    AudioSource[] audioPool;
    int audioIndex = 0;

    void Awake()
    {
        pool = new Queue<HitEffect>(poolSize);
        for (int i = 0; i < poolSize; i++)
        {
            var go = Instantiate(hitEffectPrefab, uiCanvasRect);
            go.SetActive(false);
            pool.Enqueue(go.GetComponent<HitEffect>());
        }

        int audioPoolSize = Mathf.Max(4, poolSize / 4);
        audioPool = new AudioSource[audioPoolSize];
        for (int i = 0; i < audioPoolSize; i++)
        {
            var go = new GameObject("SFXSrc");
            go.transform.SetParent(transform, false);
            var a = go.AddComponent<AudioSource>();
            a.playOnAwake = false;
            a.spatialBlend = 0f;
            a.loop = false;
            audioPool[i] = a;
        }
    }

    HitEffect GetFromPool()
    {
        if (pool.Count > 0)
        {
            var e = pool.Dequeue();
            e.gameObject.SetActive(true);
            return e;
        }
        var go = Instantiate(hitEffectPrefab, uiCanvasRect);
        return go.GetComponent<HitEffect>();
    }

    public void ReturnToPool(HitEffect e)
    {
        e.gameObject.SetActive(false);
        pool.Enqueue(e);
    }

    public void SpawnHitResultAtWorldPosition(Vector3 pos, HitResult result, Sprite spriteForResult = null)
    {
        var effect = GetFromPool();
        effect.Setup(result, spriteForResult, pos, () => ReturnToPool(effect));
        
        PlaySfxForResult(result);
    }

    void PlaySfxForResult(HitResult r)
    {
        AudioClip clip = null;
        float basePitch = 1f;
        float baseVol = baseSFXVolume;


        switch (r)
        {
            case HitResult.Perfect: clip = sfxPerfect; basePitch = 1.02f; baseVol = baseVol/2; break;
            case HitResult.Great: clip = sfxGreat; basePitch = 1.00f; baseVol = baseVol/4; break;
            case HitResult.Good: clip = sfxGood; basePitch = 0.98f; baseVol = baseVol; break;
            case HitResult.Miss: clip = sfxMiss; basePitch = 0.95f; baseVol = baseVol; break;
        }

        if (clip == null) return;

        var a = audioPool[audioIndex];
        audioIndex = (audioIndex + 1) % audioPool.Length;

        a.pitch = basePitch * (1f + Random.Range(-sfxPitchVariation, sfxPitchVariation));
        a.volume = Mathf.Clamp01(baseVol * (1f + Random.Range(-sfxVolumeVariation, sfxVolumeVariation)));
        
        a.PlayOneShot(clip);
    }
}
