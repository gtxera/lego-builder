using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class PointerUIController
{
    private readonly List<RaycastResult> _raycastResults = new();

    public bool IsPointerOverUI(Vector2 screenPosition)
    {
        if (EventSystem.current == null)
            return false;

        var pointerEventData = new PointerEventData(EventSystem.current)
        {
            position = screenPosition
        };

        _raycastResults.Clear();
        EventSystem.current.RaycastAll(pointerEventData, _raycastResults);
        var result = _raycastResults.Count > 0;
        Debug.Log(result);
        return result;
    }
}
