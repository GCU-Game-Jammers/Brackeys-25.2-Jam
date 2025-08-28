using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CurrencyInteractable : MonoBehaviour, IDragHandler, IBeginDragHandler,IEndDragHandler 
{

    private BoxCollider2D boxCollider;
    private Transform resetPoint;
    private void Awake()
    {
        resetPoint = transform;
        boxCollider = GetComponent<BoxCollider2D>();
    }
    private void Start()
    {
        // hide if no currency / default 
        //gameObject.SetActive(false);
    }
    public void ShowCurrency()
    {
        transform.position = resetPoint.position;
        gameObject.SetActive(true);
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        Debug.Log("Drag Currency");
        boxCollider.enabled = false;
    }
    public void OnDrag(PointerEventData eventData)
    {
        // Move the element with the mouse
        transform.position = Input.mousePosition;
    }
    public void OnEndDrag(PointerEventData eventData)
    {
        boxCollider.enabled = true;
        Debug.Log("Drag end");
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Gacha"))
        {
            gameObject.SetActive(false);
            other.gameObject.GetComponentInParent<GachaMachine>().UnlockItem();
        }
    }
    private void OnTriggerStay2D(Collider2D other)
    {
        if (other.CompareTag("Gacha"))
        {
            gameObject.SetActive(false);
        }
    }
}
