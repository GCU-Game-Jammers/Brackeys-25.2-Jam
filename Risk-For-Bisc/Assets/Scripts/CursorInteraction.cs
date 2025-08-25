using UnityEngine;

[RequireComponent(typeof(Camera))]
public class CameraInteraction : MonoBehaviour
{
    [Header("Raycasts")]
    public LayerMask interactableMask;
    public float maxDistance = 100f;

    [Header("Cursors")]
    public Texture2D normalCursor;
    public Texture2D handCursor;
    public Texture2D closedHandCursor;
    public Vector2 cursorHotspot = new Vector2(16, 16);

    private Camera cam;
    IInteractable currentInteractable;
    Transform currentTransform;
    bool isDragging = false;

    Vector3 lastHitPoint;

    void Awake()
    {
        cam = Camera.main;
        if (cam == null) throw new System.Exception("NO CAMERA FOUND");
        if (normalCursor != null) Cursor.SetCursor(normalCursor, Vector2.zero, CursorMode.Auto);
    }

    void Update()
    {
        if (cam == null) return;

        Ray ray = cam.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (!isDragging)
        {
            if (Physics.Raycast(ray, out hit, maxDistance, interactableMask))
            {
                var interactable = hit.collider.GetComponent<IInteractable>();
                if (interactable != null)
                {
                    if (currentInteractable == null || currentInteractable != interactable)
                    {
                        ClearHover();
                        currentInteractable = interactable;
                        currentTransform = hit.collider.transform;
                        currentInteractable.OnHoverEnter();
                    }

                    if (handCursor != null) Cursor.SetCursor(handCursor, cursorHotspot, CursorMode.Auto);

                    if (Input.GetMouseButtonDown(0))
                    {
                        BeginDrag(hit.point);
                    }

                    return;
                }
            }

            ClearHover();
        }
        else
        {
            if (closedHandCursor != null) Cursor.SetCursor(closedHandCursor, cursorHotspot, CursorMode.Auto);
            // Track drag delta
            if (Physics.Raycast(ray, out hit, maxDistance, interactableMask, QueryTriggerInteraction.Collide))
            {
                Vector3 currentPoint = hit.point;
                Vector3 worldDelta = currentPoint - lastHitPoint;

                if (worldDelta.sqrMagnitude > 0f)
                {
                    currentInteractable?.OnDrag(worldDelta);
                    lastHitPoint = currentPoint;
                }
            }

            if (Input.GetMouseButtonUp(0))
            {
                EndDrag();
            }
        }

        if (!isDragging && currentInteractable == null)
            Cursor.SetCursor(normalCursor, Vector2.zero, CursorMode.Auto);
    }

    void BeginDrag(Vector3 hitPoint)
    {
        if (currentTransform == null || currentInteractable == null) return;

        isDragging = true;

        currentInteractable.OnBeginDrag();
    }

    void EndDrag()
    {
        if (!isDragging) return;

        isDragging = false;
        currentInteractable?.OnEndDrag();

        currentInteractable = null;
        currentTransform = null;

        Cursor.SetCursor(normalCursor, Vector2.zero, CursorMode.Auto);
    }

    void ClearHover()
    {
        if (currentInteractable != null)
        {
            currentInteractable.OnHoverExit();
            currentInteractable = null;
            currentTransform = null;
            Cursor.SetCursor(normalCursor, Vector2.zero, CursorMode.Auto);
        }
    }
}

public interface IInteractable
{
    void OnHoverEnter();
    void OnHoverExit();
    void OnBeginDrag();
    void OnDrag(Vector3 worldDelta);
    void OnEndDrag();
}
