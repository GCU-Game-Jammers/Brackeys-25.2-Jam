using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.UI;
using System.Collections;

public enum HitResult 
{ 
    TooFast, 
    Perfect, 
    TooSlow 
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

    [Header("Timing & Movement")]
    public float arrowSpeed = 5f;
    public float perfectWindow = 0.08f;
    public float maxWindow = 0.25f;
    public float despawnY = -5f;

    [Header("Input")]
    public KeyCode leftKey = KeyCode.LeftArrow;
    public KeyCode downKey = KeyCode.DownArrow;
    public KeyCode upKey = KeyCode.UpArrow;
    public KeyCode rightKey = KeyCode.RightArrow;

    private readonly List<Arrow> active = new List<Arrow>();

    public event Action<Arrow, HitResult, float> OnHit; // (arrow, result, deltaSeconds)

    void Update()
    {
        if (Input.GetKeyDown(leftKey)) TryHit(Direction.Left);
        if (Input.GetKeyDown(downKey)) TryHit(Direction.Down);
        if (Input.GetKeyDown(upKey)) TryHit(Direction.Up);
        if (Input.GetKeyDown(rightKey)) TryHit(Direction.Right);

        if (Input.GetKeyDown(KeyCode.F1))
            Spawn(Direction.Left);
        if (Input.GetKeyDown(KeyCode.F2))
            Spawn(Direction.Down);
        if (Input.GetKeyDown(KeyCode.F3))
            Spawn(Direction.Up);
        if (Input.GetKeyDown(KeyCode.F4))
            Spawn(Direction.Right);
    }

    public void SpawnLeftArrow(float dspTime = 0) => Spawn(Direction.Left, dspTime);
    public void SpawnDownArrow(float dspTime = 0) => Spawn(Direction.Down, dspTime);
    public void SpawnUpArrow(float dspTime = 0) => Spawn(Direction.Up, dspTime);
    public void SpawnRightArrow(float dspTime = 0) => Spawn(Direction.Right, dspTime);

    private void Spawn(Direction dir, float dspTime = 0)
    {
        var arrowPrefab = GetArrowPrefab(dir);
        var spawnPoint = GetArrowSpawnPoint(dir);
        var hitPoint = GetArrowHitPoint(dir);
        if (arrowPrefab == null || spawnPoint == null || hitPoint == null)
            throw new Exception("Fix your shit designers");

        GameObject go = Instantiate(arrowPrefab, spawnPoint.position, Quaternion.identity, container);
        Arrow a = go.GetComponent<Arrow>();
        if (a == null)
            throw new Exception("LOCK IN DESIGNERS. THIS IS NOOB BEHAVIOUR");

        // Setup
        a.direction = dir;
        a.speed = arrowSpeed;
        a.noteManager = this;

        // Get expected hit time
        float distance = spawnPoint.position.y - hitPoint.position.y;
        if (distance < 0f) distance = -distance; // Abs
        float travelTime = distance / arrowSpeed;
        a.hitTime = Time.time + travelTime;

        RegisterArrow(a);
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
        else if (d < 0) result = HitResult.TooFast;
        else result = HitResult.TooSlow;

        best.hit = true;
        OnHit?.Invoke(best, result, d);
    }
    private IEnumerator ClickButton(Direction dir)
    {
        var arrowImage = GetArrowHitPoint(dir).GetComponent<RawImage>();
        
        var colour = arrowImage.color;

        colour.a = 1.0f;
        arrowImage.color = colour;

        yield return new WaitForSeconds(0.05f);

        colour.a = 0.5f;
        arrowImage.color = colour;
    }
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
