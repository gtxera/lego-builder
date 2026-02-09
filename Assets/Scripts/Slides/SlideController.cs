using System;
using System.Collections.Generic;
using KBCore.Refs;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class SlideController : ValidatedMonoBehaviour
{
    [SerializeField]
    private Slide[] _slides;

    [SerializeField, Scene]
    private SlideCamera _camera;
    
    private LinkedList<Slide> _slidesList;

    private LinkedListNode<Slide> _currentSlide;

    private void Awake()
    {
        _slidesList = new LinkedList<Slide>(_slides);
    }

    public void StartSlides()
    {
        _currentSlide = _slidesList.First;
        _currentSlide.Value.Enter(_camera);
    }

    private void Update()
    {
        if (Keyboard.current.rightArrowKey.wasReleasedThisFrame)
            ShowNext();
        else if (Keyboard.current.leftArrowKey.wasReleasedThisFrame)
            ShowPrevious();
        else if (Keyboard.current.f11Key.wasReleasedThisFrame)
            Screen.fullScreen = !Screen.fullScreen;
    }

    private void ShowNext()
    {
        _currentSlide.Value.ExitNext();
        
        if (_currentSlide.Next == null)
            return;
        
        _currentSlide = _currentSlide.Next;
        _currentSlide.Value.Enter(_camera);
    }

    private void ShowPrevious()
    {
        _currentSlide.Value.ExitPrevious();
        
        if (_currentSlide.Previous == null)
            return;

        _currentSlide = _currentSlide.Previous;
        _currentSlide.Value.Enter(_camera);
    }

    [Serializable]
    private class Slide
    {
        [SerializeField]
        private UnityEvent _enteredSlide;

        [SerializeField]
        private UnityEvent _exitedNextSlide;
        
        [SerializeField]
        private UnityEvent _exitedPreviousSlide;

        [SerializeField]
        private Vector3 _cameraPosition;

        [SerializeField]
        private Vector3 _cameraRotation;

        public void Enter(SlideCamera camera)
        {
            _enteredSlide.Invoke();
            camera.SetPositionAndRotation(_cameraPosition, _cameraRotation);
        }

        public void ExitNext()
        {
            _exitedNextSlide.Invoke();
        }

        public void ExitPrevious()
        {
            _exitedPreviousSlide.Invoke();
        }
    }
}
