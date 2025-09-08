using UnityEngine;

public interface ITool
{
    void Press(Vector2 pointerScreenPosition);

    void Release(Vector2 pointerScreenPosition);
    
    void Drag(Vector2 pointerScreenPosition);

    void Tap();
}
