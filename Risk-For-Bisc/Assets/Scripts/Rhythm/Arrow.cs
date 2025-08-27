using UnityEngine;

public class Arrow : MonoBehaviour
{
    [HideInInspector] public Direction direction;
    [HideInInspector] public float hitTime;
    [HideInInspector] public float speed;
    [HideInInspector] public NoteManager noteManager;
    [HideInInspector] public bool hit = false;

    void Update()
    {
        transform.position += Vector3.down * speed * Time.deltaTime;

        if (transform.position.y < noteManager.despawnY || hit)
        {
            noteManager.UnregisterArrow(this);
            Destroy(gameObject);
        }
    }
}