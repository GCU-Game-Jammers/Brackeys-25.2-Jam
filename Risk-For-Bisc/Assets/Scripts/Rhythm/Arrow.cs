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

    private bool hait = false;

    void Update()
    {
        float t = 1 - ((hitTime - Time.time) / travelTime);

        transform.position = Vector3.Lerp(spawnPoint, hitPoint, t);

        if (t >= 1 || hit)
        {
            noteManager.UnregisterArrow(this);
            Destroy(gameObject);
        }

        if (Vector3.Distance(hitPoint, transform.position) < 10.0f && !hait)
        {
            hait = true;
            Debug.Log("Hit at time " + Time.time);
        }
    }
}