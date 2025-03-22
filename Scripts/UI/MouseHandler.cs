using UnityEngine;
using UnityEngine.EventSystems;

public class MouseHandler : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public delegate void MouseEvent(PointerEventData pointerEventData);

    public MouseEvent OnMouseEnterEvent;
    public MouseEvent OnMouseExitEvent;

    public void OnPointerEnter(PointerEventData pointerEventData)
    {
        OnMouseEnterEvent?.Invoke(pointerEventData);
    }

    public void OnPointerExit(PointerEventData pointerEventData)
    {
        OnMouseExitEvent?.Invoke(pointerEventData);
    }
}