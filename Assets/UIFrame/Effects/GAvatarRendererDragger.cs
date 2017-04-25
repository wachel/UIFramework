using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class GAvatarRendererDragger : MonoBehaviour, IPointerDownHandler, IDragHandler
{
    public GAvatarRenderer target;

    Vector2 mouseDownPos;
    float startYaw;

    public void OnPointerDown(PointerEventData eventData)
    {
        mouseDownPos = GetLocalPos(eventData.position);
        startYaw = target.yaw;
    }

    public void OnDrag(PointerEventData eventData)
    {
        Vector2 localPos = GetLocalPos(eventData.position);
        target.yaw = startYaw - (localPos.x - mouseDownPos.x);
    }

    private Vector2 GetLocalPos(Vector2 eventPos)
    {
        Vector2 pos;
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(transform as RectTransform, eventPos, target.uiWorldSpaceCamera, out pos)) {
            return pos;
        }
        return Vector2.zero;
    }

}
