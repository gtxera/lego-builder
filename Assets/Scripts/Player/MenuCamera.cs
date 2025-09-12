using KBCore.Refs;
using PrimeTween;
using Reflex.Attributes;
using Unity.Cinemachine;
using UnityEngine;

public class MenuCamera : MonoBehaviour
{
    [Inject]
    private readonly CameraControlInputContext _cameraControlInputContext;
    
    [Inject]
    private readonly LevelSelectorInputContext _levelSelectorInputContext;
    
    [SerializeField, Scene]
    private CinemachineOrbitalFollow _orbitalFollow;

    [SerializeField]
    private float _menuRotationSpeed;
    
    [SerializeField]
    private int _animationRotations;

    private Tween _animationTween;

    private void Start()
    {
        _orbitalFollow.VerticalAxis.Value = _orbitalFollow.VerticalAxis.Range.y;
        _orbitalFollow.RadialAxis.Value = _orbitalFollow.RadialAxis.Range.y;
    }

    private void LateUpdate()
    {
        if (_animationTween.isAlive)
            return;
        
        _orbitalFollow.HorizontalAxis.Value += _menuRotationSpeed * Time.deltaTime;
    }

    public void PlayStartAnimation()
    {
        var rotation = _orbitalFollow.HorizontalAxis.Value;
        var targetAngle = 360f * _animationRotations;

        var vertical = _orbitalFollow.VerticalAxis.Value;
        var verticalTarget = _orbitalFollow.VerticalAxis.Center;

        var radial = _orbitalFollow.RadialAxis.Value;
        var radialTarget = _orbitalFollow.RadialAxis.Center;
            
        Sequence.Create(sequenceEase: Ease.InOutSine)
            .Group(Tween.Custom(rotation, targetAngle, 5f, value =>
                {
                    _orbitalFollow.HorizontalAxis.Value = value;
                })
                .Group(Tween.Custom(vertical, verticalTarget, 5f, value =>
                {
                    _orbitalFollow.VerticalAxis.Value = value;
                }))
                .Group(Tween.Custom(radial, radialTarget, 5f, value =>
                {
                    _orbitalFollow.RadialAxis.Value = value;
                }))
                .OnComplete(() =>
                {
                    _cameraControlInputContext.Enable();
                    _levelSelectorInputContext.Enable();
                    Destroy(this);
                }));
    }
}
