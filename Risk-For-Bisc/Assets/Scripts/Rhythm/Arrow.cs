using UnityEngine;

public class Arrow : MonoBehaviour
{
    [HideInInspector] public Direction direction;
    [HideInInspector] public float hitTime;
    [HideInInspector] public float speed;
    [HideInInspector] public NoteManager noteManager;
    [HideInInspector] public bool hit = false;
    [HideInInspector] public Vector3 hitPoint;
    [HideInInspector] public Vector3 spawnPoint;
    [HideInInspector] public float travelTime;


    private bool debugHit = false;
    void Update()
    {
        float t = 1 - ((hitTime - Time.time) / travelTime);

        transform.position = Vector3.LerpUnclamped(spawnPoint, hitPoint, t);

        if (t >= 1)
        {
            debugHit = true;
            //Debug.Log("Hit at time " + Time.time);
        }

        if (t >= 1.2 || hit)
        {
            noteManager.ArrowMissed(this);
            noteManager.UnregisterArrow(this);
            Destroy(gameObject);
        }
    }
}