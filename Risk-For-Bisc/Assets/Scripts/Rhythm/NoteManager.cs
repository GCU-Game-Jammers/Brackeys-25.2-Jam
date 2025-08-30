using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.UI;
using System.Collections;

public enum HitResult 
{ 
    Good,
    Great,
    Perfect, 
    Miss 
}
public enum Direction
{
    Left,
    Down,
    Up,
    Right
}
public class NoteManager : MonoBehaviour
{
    [Header("References")]
    public GameObject leftArrowPrefab;
    public GameObject downArrowPrefab;
    public GameObject upArrowPrefab;
    public GameObject rightArrowPrefab;

    public Transform leftArrowSpawn;
    public Transform downArrowSpawn;
    public Transform upArrowSpawn;
    public Transform rightArrowSpawn;

    public Transform leftArrowHitPoint;
    public Transform downArrowHitPoint;
    public Transform upArrowHitPoint;
    public Transform rightArrowHitPoint;

    public Transform container;

    public AudioSource tapSfx;
    public AudioSource hitNoteSfx;

    [Header("Timing & Movement")]
    public float travelTime = 1f;
    public float goodWindow = 0.2f;
    public float greatWindow = 0.15f;
    public float perfectWindow = 0.08f;
    public float maxWindow = 0.25f;
    public float despawnY = -5f;

    [Header("Input")]
    public KeyCode leftKey = KeyCode.LeftArrow;
    public KeyCode downKey = KeyCode.DownArrow;
    public KeyCode upKey = KeyCode.UpArrow;
    public KeyCode rightKey = KeyCode.RightArrow;

    public KeyCode altLeftKey = KeyCode.D;
    public KeyCode altDownKey = KeyCode.F;
    public KeyCode altUpKey = KeyCode.J;
    public KeyCode altRightKey = KeyCode.K;

    private readonly List<Arrow> active = new List<Arrow>();

    public event Action<Arrow, HitResult, float> OnHit; // (arrow, result, deltaSeconds)

    void Update()
    {
        if (Input.GetKeyDown(leftKey) || Input.GetKeyDown(altLeftKey)) TryHit(Direction.Left);
        if (Input.GetKeyDown(downKey) || Input.GetKeyDown(altDownKey)) TryHit(Direction.Down);
        if (Input.GetKeyDown(upKey) || Input.GetKeyDown(altUpKey)) TryHit(Direction.Up);
        if (Input.GetKeyDown(rightKey) || Input.GetKeyDown(altRightKey)) TryHit(Direction.Right);

        if (Input.GetKeyDown(KeyCode.F1))
            Spawn(Direction.Left);
        if (Input.GetKeyDown(KeyCode.F2))
            Spawn(Direction.Down);
        if (Input.GetKeyDown(KeyCode.F3))
            Spawn(Direction.Up);
        if (Input.GetKeyDown(KeyCode.F4))
            Spawn(Direction.Right);
    }

    public void SpawnLeftArrow(float timeTillShouldClick = 0) => Spawn(Direction.Left, timeTillShouldClick);
    public void SpawnDownArrow(float timeTillShouldClick = 0) => Spawn(Direction.Down, timeTillShouldClick);
    public void SpawnUpArrow(float timeTillShouldClick = 0) => Spawn(Direction.Up, timeTillShouldClick);
    public void SpawnRightArrow(float timeTillShouldClick = 0) => Spawn(Direction.Right, timeTillShouldClick);

    private void Spawn(Direction dir, float timeTillShouldClick = 0)
    {
        
        var arrowPrefab = GetArrowPrefab(dir);
        var spawnPoint = GetArrowSpawnPoint(dir);
        var hitPoint = GetArrowHitPoint(dir);
        if (arrowPrefab == null || spawnPoint == null || hitPoint == null)
            throw new Exception("Fix your shit designers");

        // compute travel time

        if (timeTillShouldClick <= 0f)
        {
            Debug.Log("Passed through arrow that should have already spawned " + timeTillShouldClick);
            GameObject go = Instantiate(arrowPrefab, spawnPoint.position, Quaternion.identity, container);
            Arrow a = go.GetComponent<Arrow>();
            if (a == null) throw new Exception("LOCK IN DESIGNERS. THIS IS NOOB BEHAVIOUR");

            a.direction = dir;
            a.noteManager = this;
            a.hitTime = Time.time + travelTime;
            a.hitPoint = hitPoint.transform.position;
            a.travelTime = travelTime;
            a.spawnPoint = spawnPoint.transform.position;


            RegisterArrow(a);
            return;
        }



        // Desired hit time in future (seconds from now)
        float desiredHitTime = Time.time + timeTillShouldClick;
        float spawnTime = desiredHitTime - travelTime;
        float delay = spawnTime - Time.time;

        Debug.Log("Delay is " + delay + " with time " + Time.time + " and travel time " + travelTime +" Time till should click " + timeTillShouldClick);


        // If spawn time is already passed or too soon, spawn immediately and make it hit as soon as possible
        if (delay <= 0f)
        {
            Debug.Log("Delay is " + delay);
            GameObject go = Instantiate(arrowPrefab, spawnPoint.position, Quaternion.identity, container);
            Arrow a = go.GetComponent<Arrow>();
            if (a == null) throw new Exception("LOCK IN DESIGNERS. THIS IS NOOB BEHAVIOUR");

            a.direction = dir;
            a.noteManager = this;
            a.hitTime = Time.time + travelTime; // hit as soon as possible
            a.hitPoint = hitPoint.transform.position;
            a.travelTime = travelTime;
            a.spawnPoint = spawnPoint.transform.position;

            RegisterArrow(a);
            return;
        }

        // Otherwise, delay instantiation so it arrives at the desired hit time
        StartCoroutine(SpawnAfterDelay(dir, delay, desiredHitTime));
    }

    private IEnumerator SpawnAfterDelay(Direction dir, float delaySeconds, float desiredHitTime)
    {
        yield return new WaitForSecondsRealtime(delaySeconds);
        Debug.Log("Waited " + delaySeconds);
        var arrowPrefab = GetArrowPrefab(dir);
        var spawnPoint = GetArrowSpawnPoint(dir);
        if (arrowPrefab == null || spawnPoint == null)
            yield break; // defensive, should not happen

        GameObject go = Instantiate(arrowPrefab, spawnPoint.position, Quaternion.identity, container);
        Arrow a = go.GetComponent<Arrow>();
        if (a == null)
        {
            Destroy(go);
            yield break;
        }

        a.direction = dir;
        a.noteManager = this;
        a.hitTime = desiredHitTime; // we delayed spawn so this will be correct
        a.hitPoint = GetArrowHitPoint(dir).transform.position;
        a.travelTime = travelTime;
        a.spawnPoint = spawnPoint.transform.position;

        RegisterArrow(a);

        //Debug.Log("Beat playing at " + Time.time);
    }

    internal void RegisterArrow(Arrow a)
    {
        active.Add(a);
    }

    internal void UnregisterArrow(Arrow a)
    {
        active.Remove(a);
    }

    public void TryHit(Direction dir)
    {
        StartCoroutine(ClickButton(dir));
        Arrow best = null;
        float bestAbsDelta = float.MaxValue;
        float now = Time.time;

        foreach (var a in active)
        {
            if (a.direction != dir) continue;
            float delta = now - a.hitTime; // Positive is Late, Negative is Early
            float absDelta = Mathf.Abs(delta);
            if (absDelta <= maxWindow && absDelta < bestAbsDelta)
            {
                bestAbsDelta = absDelta;
                best = a;
            }
        }

        // No Arrow Hit
        if (best == null) 
            return; 

        float d = now - best.hitTime;
        HitResult result;
        if (Mathf.Abs(d) <= perfectWindow) result = HitResult.Perfect;
        else if (Mathf.Abs(d) <= greatWindow) result = HitResult.Great;
        else if (Mathf.Abs(d) <= goodWindow) result = HitResult.Good;
        else result = HitResult.Miss;

        best.hit = true;
        OnHit?.Invoke(best, result, d);
    }

    public void ArrowMissed(Arrow arrow)
    {
        OnHit?.Invoke(arrow, HitResult.Miss, 999);
    }
    private IEnumerator ClickButton(Direction dir)
    {
        var arrowHit = GetArrowHitPoint(dir);
        if (arrowHit == null) yield break;
        var raw = arrowHit.GetComponent<RawImage>();
        if (raw == null) yield break;

        float durationPop = 0.06f;
        float durationRelease = 0.12f;
        float total = durationPop + durationRelease;

        float t = 0f;
        float startScale = 0.5f;
        float peakScale = 0.55f;
        float startAlpha = 0.5f;
        float peakAlpha = 1f;

        tapSfx.Play();

        while (t < total)
        {
            t += Time.unscaledDeltaTime;
            if (t <= durationPop)
            {
                float p = t / durationPop;
                float eased = EaseOutBack(p);
                float s = Mathf.Lerp(startScale, peakScale, eased);
                raw.transform.localScale = new Vector3(s, s, s);
                float a = Mathf.Lerp(startAlpha, peakAlpha, p);
                var c = raw.color;
                c.a = a;
                raw.color = c;
            }
            else
            {
                float p = (t - durationPop) / durationRelease;
                float eased = 1 - Mathf.Pow(1 - p, 2);
                float s = Mathf.Lerp(peakScale, startScale, eased);
                raw.transform.localScale = new Vector3(s, s, s);
                float a = Mathf.Lerp(peakAlpha, startAlpha, eased);
                var c = raw.color;
                c.a = a;
                raw.color = c;
            }
            yield return null;
        }

        var finalc = raw.color;
        finalc.a = startAlpha;
        raw.color = finalc;
        raw.transform.localScale = new Vector3(startScale, startScale, startScale);
    }

    float EaseOutBack(float p) { return 1 + (--p) * p * (2.70158f * p + 1.70158f); }
    public void ClearAll()
    {
        foreach (var a in active.ToArray())
        {
            if (a != null) Destroy(a.gameObject);
        }
        active.Clear();
    }

    private GameObject GetArrowPrefab(Direction dir)
    {
        switch (dir)
        {
            case Direction.Left:
                return leftArrowPrefab;
            case Direction.Down:
                return downArrowPrefab;
            case Direction.Up:
                return upArrowPrefab;
            case Direction.Right:
                return rightArrowPrefab;
        }
        return null;
    }

    private Transform GetArrowSpawnPoint(Direction dir)
    {
        switch (dir)
        {
            case Direction.Left:
                return leftArrowSpawn;
            case Direction.Down:
                return downArrowSpawn;
            case Direction.Up:
                return upArrowSpawn;
            case Direction.Right:
                return rightArrowSpawn;
        }
        return null;
    }

    private Transform GetArrowHitPoint(Direction dir)
    {
        switch (dir)
        {
            case Direction.Left:
                return leftArrowHitPoint;
            case Direction.Down:
                return downArrowHitPoint;
            case Direction.Up:
                return upArrowHitPoint;
            case Direction.Right:
                return rightArrowHitPoint;
        }
        return null;
    }

}
