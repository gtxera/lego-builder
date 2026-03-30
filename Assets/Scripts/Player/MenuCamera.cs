using System;
using KBCore.Refs;
using PrimeTween;
using Reflex.Attributes;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class MenuCamera : ValidatedMonoBehaviour
{
    [Inject]
    private readonly CameraControlInputContext _cameraControlInputContext;
    
    [Inject]
    private readonly LevelSelectorInputContext _levelSelectorInputContext;

    [Inject]
    private readonly BuildEditor _buildEditor;

    [Inject]
    private readonly LevelSelector _levelSelector;

    [SerializeField]
    private bool _slideshow;
    
    [SerializeField, Scene]
    private CinemachineOrbitalFollow _orbitalFollow;

    [SerializeField]
    private FMODUnity.StudioEventEmitter _gameplayMusicEmitter;

    [SerializeField]
    private FMODUnity.StudioEventEmitter _menuMusicEmitter;

    [SerializeField]
    private FMODUnity.StudioEventEmitter _cameraSwingEmitter;
    
    [SerializeField]
    private float _menuRotationSpeed;
    
    [SerializeField]
    private int _animationRotations;

    [SerializeField]
    private Button _menuButton;

    [SerializeField]
    private CanvasGroup _buttonGroup;
    
    [SerializeField]
    private TitleScreen _titleScreen;

    [SerializeField]
    private SlideController _slideController;

    private Tween _animationTween;

    private bool _playing;

    private void Awake()
    {
        _menuButton.onClick.AddListener(() =>
        {
            if (Application.platform == RuntimePlatform.Android)
            {
                PlayReturnAnimation();
                _levelSelector.Deselect();
            }
            else
            {
                Application.Quit();
            }
        });
        _buttonGroup.interactable = false;
        _buttonGroup.alpha = 0f;
        _playing = true;

        _buildEditor.StartedEditing += _ => Hide();
        _buildEditor.FinishedEditing += _ => Show();
    }

    private void Start()
    {
        _orbitalFollow.VerticalAxis.Value = _orbitalFollow.VerticalAxis.Range.y;
        _orbitalFollow.RadialAxis.Value = _orbitalFollow.RadialAxis.Range.y;

        if (!SceneTransitionState.IsSceneTransitionInProgress && !SceneTransitionState.ConsumeSuppressMenuMusicOnce())
            _menuMusicEmitter.Play();
    }

    private void LateUpdate()
    {
        if (!_playing)
            return;
        
        _orbitalFollow.HorizontalAxis.Value += _menuRotationSpeed * Time.deltaTime;
    }

    public void PlayStartAnimation()
    {
        _playing = false;

        _cameraSwingEmitter.Play();
        
        var rotation = _orbitalFollow.HorizontalAxis.Value;
        var targetAngle = 360f * _animationRotations;

        var vertical = _orbitalFollow.VerticalAxis.Value;
        var verticalTarget = _orbitalFollow.VerticalAxis.Center;

        var radial = _orbitalFollow.RadialAxis.Value;
        var radialTarget = _orbitalFollow.RadialAxis.Center;
        
        _menuMusicEmitter.Stop();
            
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
                    if (_slideshow)
                    {
                        _slideController.StartSlides();
                    }
                    else
                    {
                        EnterGameplayState();
                    }

                    if (!_gameplayMusicEmitter.IsPlaying())
                        _gameplayMusicEmitter.Play();
                }));
    }

    public void EnterGameplayWithoutIntro()
    {
        _playing = false;
        _menuMusicEmitter.Stop();

        _orbitalFollow.VerticalAxis.Value = _orbitalFollow.VerticalAxis.Center;
        _orbitalFollow.RadialAxis.Value = _orbitalFollow.RadialAxis.Center;

        EnterGameplayState();

        if (!_gameplayMusicEmitter.IsPlaying())
            _gameplayMusicEmitter.Play();
    }

    private void PlayReturnAnimation()
    {
        Hide();
        
        _cameraSwingEmitter.Play();
        
        _gameplayMusicEmitter.Stop();
        
        _cameraControlInputContext.Disable();
        _levelSelectorInputContext.Disable();
        
        var rotation = _orbitalFollow.HorizontalAxis.Value;
        var targetAngle = 360f * _animationRotations;

        var vertical = _orbitalFollow.VerticalAxis.Value;
        var verticalTarget = _orbitalFollow.VerticalAxis.Range.y;

        var radial = _orbitalFollow.RadialAxis.Value;
        var radialTarget = _orbitalFollow.RadialAxis.Range.y;
            
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
                    _playing = true;
                    _titleScreen.Show();

                    if (!SceneTransitionState.IsSceneTransitionInProgress)
                        _menuMusicEmitter.Play();
                }));
    }

    private void OnDestroy()
    {
        if (!SceneTransitionState.IsSceneTransitionInProgress)
            return;

        if (_menuMusicEmitter != null && _menuMusicEmitter.IsPlaying())
            _menuMusicEmitter.Stop();

        if (_gameplayMusicEmitter != null && _gameplayMusicEmitter.IsPlaying())
            _gameplayMusicEmitter.Stop();
    }

    private void EnterGameplayState()
    {
        _cameraControlInputContext.Disable();
        _levelSelectorInputContext.Disable();
        _cameraControlInputContext.ResetState();
        _cameraControlInputContext.EnableMoveControl();
        _cameraControlInputContext.Enable();
        _levelSelectorInputContext.Enable();
        Show();
        SceneTransitionState.CompleteSceneTransition();
    }

    private void Show()
    {
        Tween.Alpha(_buttonGroup, 1f, .5f)
            .OnComplete(() =>
            {
                _buttonGroup.interactable = true;
                _buttonGroup.blocksRaycasts = true;
            });
    }

    private void Hide()
    {
        _buttonGroup.interactable = false;
        _buttonGroup.blocksRaycasts = false;
        Tween.Alpha(_buttonGroup, 0f, .5f);
    }
}
