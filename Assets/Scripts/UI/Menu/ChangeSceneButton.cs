using KBCore.Refs;
using PrimeTween;
using TMPro;
using Reflex.Attributes;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ChangeSceneButton : MonoBehaviour
{
    [Inject]
    private readonly BuildEditor _buildEditor;

    [SerializeField]
    private int _sceneIndex = 1;

    [SerializeField]
    private int _linearSceneBuildIndex;

    [SerializeField]
    private int _sandboxSceneBuildIndex = 1;
    
    [SerializeField, Self]
    private Button _button;

    [SerializeField]
    private TMP_Text _label;

    private void Awake()
    {
        _button.onClick.AddListener(LoadTargetScene);
        UpdateLabel();
        Tween.StopAll();
    }

    private void OnDestroy()
    {
        _button.onClick.RemoveListener(LoadTargetScene);
    }

    private void LoadTargetScene()
    {
        if (_buildEditor?.Build != null)
            _buildEditor.FinishEditing();

        var currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
        var linearSceneBuildIndex = _linearSceneBuildIndex;
        var sandboxSceneBuildIndex = ResolveSandboxSceneBuildIndex();

        var targetSceneBuildIndex = ResolveTargetSceneBuildIndex(currentSceneIndex, linearSceneBuildIndex, sandboxSceneBuildIndex);
        if (targetSceneBuildIndex < 0)
            return;

        if (currentSceneIndex == sandboxSceneBuildIndex && targetSceneBuildIndex == linearSceneBuildIndex)
            SceneTransitionState.RequestSkipLinearIntroOnce();

        SceneTransitionState.BeginSceneTransition();
        SceneManager.LoadScene(targetSceneBuildIndex);
    }

    private int ResolveSandboxSceneBuildIndex()
    {
        return _sandboxSceneBuildIndex >= 0 ? _sandboxSceneBuildIndex : _sceneIndex;
    }

    private int ResolveTargetSceneBuildIndex(int currentSceneIndex, int linearSceneBuildIndex, int sandboxSceneBuildIndex)
    {
        if (currentSceneIndex == linearSceneBuildIndex)
            return sandboxSceneBuildIndex;

        if (currentSceneIndex == sandboxSceneBuildIndex)
            return linearSceneBuildIndex;

        return _sceneIndex;
    }

    private void UpdateLabel()
    {
        if (_label == null)
            return;

        var currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
        _label.SetText(currentSceneIndex == _sandboxSceneBuildIndex ? "Ir para Mundo 1" : "Ir para Sandbox");
    }
}
