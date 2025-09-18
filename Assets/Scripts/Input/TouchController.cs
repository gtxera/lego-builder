using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.EnhancedTouch;
using Touch = UnityEngine.InputSystem.EnhancedTouch.Touch;

public class TouchController : IDisposable
{
    private readonly Dictionary<int, (float time, Touch touch, Vector2 delta)> _touches = new();

    private Vector2 _lastFirstTouchPosition;
    private Vector2 _lastSecondTouchPosition;
    private Vector2 _lastTouchesVector;

    private int _firstTouchId;
    private int _secondTouchId;
    
    public TouchController()
    {
        
    }

    public event Action<Touch> TouchedDown = delegate { };
    public event Action<Touch, Vector2> TouchChanged = delegate { };
    public event Action<int> TouchLifted = delegate { };

    public event Action<Vector2> SingleTouchMoved = delegate { };
    public event Action<float> TouchesAngleChanged = delegate { };
    public event Action<float> TouchesMagnitudeChanged = delegate { };
    public event Action<float> TouchesHeightChanged = delegate { }; 
    
    private void OnFingerDown(Finger finger)
    {
        TouchedDown(finger.currentTouch);

        if (Touch.activeTouches.Count < 3)
        {
            var currentTouch = finger.currentTouch;
            _touches.Add(currentTouch.touchId, (Time.time, currentTouch, Vector2.zero));
            switch (finger.index)
            {
                case 0:
                    _firstTouchId = currentTouch.touchId;
                    _lastFirstTouchPosition = currentTouch.screenPosition;
                    break;
                case 1:
                    _secondTouchId = currentTouch.touchId;
                    _lastSecondTouchPosition = currentTouch.screenPosition;
                    _lastTouchesVector = _lastSecondTouchPosition - _lastFirstTouchPosition;
                    break;
            }
        }
    }

    private void OnFingerMove(Finger finger)
    {
        var movedTouchDelta = Touchscreen.current.touches[finger.index].delta.ReadValue();

        var currentTouch = finger.currentTouch;
        
        TouchChanged(currentTouch, movedTouchDelta);

        if (Touch.activeFingers.Count == 1)
        {
            SingleTouchMoved(movedTouchDelta);
            return;
        }

        var touchId = finger.currentTouch.touchId;
        
        if (!_touches.ContainsKey(touchId))
            return;

        var currentTime = Time.time;
        _touches[touchId] = new ValueTuple<float, Touch, Vector2>(currentTime, currentTouch, movedTouchDelta);

        if (!_touches.TryGetValue(_firstTouchId, out var first))
            return;
        if (!_touches.TryGetValue(_secondTouchId, out var second))
            return;
        
        if (!Mathf.Approximately(first.time, currentTime) || !Mathf.Approximately(second.time, currentTime))
            return;

        var firstPosition = first.touch.screenPosition;
        var secondPosition = second.touch.screenPosition;

        var firstDelta = first.delta;
        var secondDelta = second.delta;

        var touchesVector = secondPosition - firstPosition;
        Debug.Log($"Primeira : {firstDelta.normalized}");
        Debug.Log($"Segunda : {secondDelta.normalized}");
        Debug.Log($"Dot : {Vector2.Dot(firstDelta.normalized, secondDelta.normalized)}");

        var angleDifference = Vector2.SignedAngle(_lastTouchesVector, touchesVector);
        var magnitudeDifference = touchesVector.magnitude - _lastTouchesVector.magnitude;
        var heightDifference = ((firstPosition.y + secondPosition.y) / 2 -
                               (_lastFirstTouchPosition.y + _lastSecondTouchPosition.y) / 2) * Vector2.Dot(firstDelta.normalized, secondDelta.normalized);

        TouchesAngleChanged(angleDifference);
        TouchesMagnitudeChanged(magnitudeDifference);
        TouchesHeightChanged(heightDifference);
        
        var angleAbs = Mathf.Abs(angleDifference);
        var magnitudeAbs = Mathf.Abs(magnitudeDifference);
        var heightAbs = Mathf.Abs(heightDifference);
        
        Debug.Log($"Angulo : {angleDifference}");
        Debug.Log($"Magnitude : {magnitudeDifference}");
        Debug.Log($"Altura : {heightDifference}");

        _lastTouchesVector = touchesVector;
        _lastFirstTouchPosition = firstPosition;
        _lastSecondTouchPosition = secondPosition;
    }

    private void OnFingerUp(Finger finger)
    {
        TouchLifted(finger.index);

        if (_touches.ContainsKey(finger.currentTouch.touchId))
            _touches.Remove(finger.currentTouch.touchId);
    }
    
    public void Dispose()
    {
        Touch.onFingerDown -= OnFingerDown;
        Touch.onFingerMove -= OnFingerMove;
        Touch.onFingerUp -= OnFingerUp;
    }
}
