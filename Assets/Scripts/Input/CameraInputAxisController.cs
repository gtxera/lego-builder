using System;
using Reflex.Attributes;
using Unity.Cinemachine;
using UnityEngine;
using Object = UnityEngine.Object;

public class CameraInputAxisController : InputAxisControllerBase<CameraInputAxisController.Reader>
{
    [Inject]
    private readonly CameraControlInputContext _cameraControlInputContext;

    [Inject]
    private readonly SensitivitySettings _sensitivitySettings;

    private void Awake()
    {
        if (!Application.isPlaying)
            return;
        
        foreach (var controller in Controllers)
            controller.Input.Initialize(_cameraControlInputContext, _sensitivitySettings);
    }

    private void Update()
    {
        if (Application.isPlaying)
            UpdateControllers();
    }

    [Serializable]
    public sealed class Reader : IInputAxisReader
    {
        [SerializeField]
        private int _eventIndex;

        private float _value;
        private Func<float> _sensitivityGetter;
        
        public void Initialize(CameraControlInputContext cameraControlInputContext, SensitivitySettings sensitivitySettings)
        {
            switch (_eventIndex)
            {
                case 0:
                    cameraControlInputContext.CameraLookOrbitXRequested += GetValue;
                    _sensitivityGetter = () => sensitivitySettings.LookXSensitivity;
                    break;
                case 1:
                    cameraControlInputContext.CameraLookOrbitYRequested += GetValue;
                    _sensitivityGetter = () => sensitivitySettings.LookYSensitivity;
                    break;
                case 2:
                    cameraControlInputContext.CameraZoomRequested += GetValue;
                    _sensitivityGetter = () => sensitivitySettings.ZoomSensitivity;
                    break;
            }
        }

        private void GetValue(float value) => _value = value;
        
        public float GetValue(Object context, IInputAxisOwner.AxisDescriptor.Hints hint)
        {
            if (_value == 0) 
                return _value;
            
            var val = _value;
            _value = 0;
            return val * _sensitivityGetter() * Time.deltaTime;
        }
    }
}
