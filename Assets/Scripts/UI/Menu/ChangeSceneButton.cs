using KBCore.Refs;
using Reflex.Attributes;
using UnityEngine;
using UnityEngine.UI;

public class ChangeSceneButton : ValidatedMonoBehaviour
{
    [Inject]
    private readonly BuildEditor _buildEditor;

    [SerializeField]
    private int _targetSceneBuildIndex = -1;

    [SerializeField]
    private int _sandboxSceneBuildIndex = 1;

    [SerializeField]
    private bool _requestSkipLinearIntroWhenLeavingSandbox;

    [SerializeField, Self]
    private Button _button;

    private void Awake()
    {
        _button.onClick.AddListener(LoadTargetScene);
    }

    private void OnDestroy()
    {
        _button.onClick.RemoveListener(LoadTargetScene);
    }

    private void LoadTargetScene()
    {
        SceneLoadUtility.LoadScene(
            _buildEditor,
            _targetSceneBuildIndex,
            _sandboxSceneBuildIndex,
            _requestSkipLinearIntroWhenLeavingSandbox);
    }
}
