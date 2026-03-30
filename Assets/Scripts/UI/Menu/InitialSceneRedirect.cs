using UnityEngine.SceneManagement;

public static class InitialSceneRedirect
{
    private const int SandboxSceneBuildIndex = 0;
    private const int World0SceneBuildIndex = 1;

    private static bool _hasHandledInitialScene;

    [UnityEngine.RuntimeInitializeOnLoadMethod(UnityEngine.RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Register()
    {
        _hasHandledInitialScene = false;
        SceneManager.sceneLoaded -= HandleInitialSceneLoaded;
        SceneManager.sceneLoaded += HandleInitialSceneLoaded;
    }

    private static void HandleInitialSceneLoaded(Scene scene, LoadSceneMode loadSceneMode)
    {
        if (_hasHandledInitialScene)
            return;

        _hasHandledInitialScene = true;
        SceneManager.sceneLoaded -= HandleInitialSceneLoaded;

        if (scene.buildIndex != SandboxSceneBuildIndex)
            return;

        SceneTransitionState.BeginSceneTransition();
        SceneManager.LoadScene(World0SceneBuildIndex);
    }
}
