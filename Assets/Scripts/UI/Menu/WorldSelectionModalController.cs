using System.Collections.Generic;
using System.Linq;
using KBCore.Refs;
using Reflex.Attributes;
using UnityEngine;
using UnityEngine.UI;

public class WorldSelectionModalController : ValidatedMonoBehaviour
{
    [Inject]
    private readonly BuildEditor _buildEditor;

    [Inject]
    private readonly ProgressManager _progressManager;

    [SerializeField, Self]
    private Button _button;

    [SerializeField]
    private GameObject _modalRoot;

    [SerializeField]
    private Button _closeButton;

    [SerializeField]
    private RectTransform _worldButtonsRoot;

    [SerializeField]
    private WorldSelectionEntry _worldButtonTemplate;

    [SerializeField]
    private GameObject _sandboxButtonRoot;

    [SerializeField]
    private int _sandboxSceneBuildIndex = 1;

    private void Awake()
    {
        _button.onClick.AddListener(OpenModal);
        _closeButton.onClick.AddListener(CloseModal);

        if (_worldButtonTemplate != null)
            _worldButtonTemplate.gameObject.SetActive(false);

        if (_modalRoot != null)
            _modalRoot.SetActive(false);
    }

    private void OnDestroy()
    {
        _button.onClick.RemoveListener(OpenModal);
        _closeButton.onClick.RemoveListener(CloseModal);
    }

    private void OpenModal()
    {
        RefreshWorldButtons();
        UpdateSandboxButtonVisibility();
        _modalRoot.SetActive(true);
    }

    private void CloseModal()
    {
        _modalRoot.SetActive(false);
    }

    private void RefreshWorldButtons()
    {
        if (_worldButtonsRoot == null || _worldButtonTemplate == null)
            return;

        ClearGeneratedButtons();

        var worlds = LoadAvailableWorlds();
        foreach (var world in worlds)
        {
            var entry = Instantiate(_worldButtonTemplate, _worldButtonsRoot);
            entry.gameObject.name = $"WorldButton_{world.Index}";
            entry.gameObject.SetActive(true);
            entry.Initialize(
                GetWorldLabel(world),
                _progressManager.IsUnlocked(world),
                () => LoadWorld(world));
        }
    }

    private void UpdateSandboxButtonVisibility()
    {
        if (_sandboxButtonRoot == null)
            return;

        var currentSceneIndex = UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex;
        _sandboxButtonRoot.SetActive(currentSceneIndex != _sandboxSceneBuildIndex);
    }

    private void LoadWorld(World world)
    {
        SceneLoadUtility.LoadScene(
            _buildEditor,
            world.Index,
            _sandboxSceneBuildIndex,
            requestSkipLinearIntroWhenLeavingSandbox: true);
    }

    private void ClearGeneratedButtons()
    {
        var transformsToDestroy = new List<Transform>();
        foreach (Transform child in _worldButtonsRoot)
        {
            if (child != _worldButtonTemplate.transform)
                transformsToDestroy.Add(child);
        }

        for (var i = 0; i < transformsToDestroy.Count; i++)
            Destroy(transformsToDestroy[i].gameObject);
    }

    private static IEnumerable<World> LoadAvailableWorlds()
    {
        return Resources.LoadAll<World>("Levels")
            .OrderBy(world => world.Index);
    }

    private static string GetWorldLabel(World world)
    {
        return string.IsNullOrWhiteSpace(world.Name)
            ? $"Mundo {world.Index + 1}"
            : world.Name;
    }
}
