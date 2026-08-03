using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class JoystickScript : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
{
    public Vector2 moveDirection;
    public bool isMoving;
    public bool isJumpRequest;
    [SerializeField] Image dragImage;
    float maxDistance = 18f;
    Vector2 centerPosition;
    public int pointerId = -1;

    void Start()
    {
        centerPosition = dragImage.rectTransform.position;
        isMoving = false;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        pointerId = eventData.pointerId;
        isMoving = true;
        Vector2 direction = eventData.position - centerPosition;
        direction = Vector2.ClampMagnitude(direction, maxDistance);
        moveDirection = direction;
        dragImage.rectTransform.position = centerPosition + direction;
    }

    public void OnDrag(PointerEventData eventData)
    {
        Vector2 direction = eventData.position - centerPosition;
        direction = Vector2.ClampMagnitude(direction, maxDistance);
        moveDirection = direction;
        dragImage.rectTransform.position = centerPosition + direction;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        pointerId = -1;
        isMoving = false;
        dragImage.rectTransform.position = centerPosition;
        moveDirection = Vector2.zero;
    }

    public void JumpRequest()
    {
        isJumpRequest = true;
    }

    public void FinishJumpRequest()
    {
        isJumpRequest = false;
    }
}
