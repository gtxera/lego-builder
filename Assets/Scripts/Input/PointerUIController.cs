using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class PointerUIController
{
    private readonly PointerEventData _pointerEventData = new(EventSystem.current);
    private readonly List<RaycastResult> _raycastResults = new();

    public bool IsPointerOverUI(Vector2 screenPosition)
    {
        _pointerEventData.position = screenPosition;
        _raycastResults.Clear();
        EventSystem.current.RaycastAll(_pointerEventData, _raycastResults);
        return _raycastResults.Count > 0;
    }
}
