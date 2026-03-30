using UnityEngine.SceneManagement;

public static class SceneLoadUtility
{
    public static void LoadScene(
        BuildEditor buildEditor,
        int targetSceneBuildIndex,
        int sandboxSceneBuildIndex,
        bool requestSkipLinearIntroWhenLeavingSandbox)
    {
        if (targetSceneBuildIndex < 0)
            return;

        if (buildEditor?.Build != null)
            buildEditor.FinishEditing();

        var currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
        if (requestSkipLinearIntroWhenLeavingSandbox &&
            currentSceneIndex == sandboxSceneBuildIndex &&
            targetSceneBuildIndex != sandboxSceneBuildIndex)
        {
            SceneTransitionState.RequestSkipLinearIntroOnce();
        }

        SceneTransitionState.BeginSceneTransition();
        SceneManager.LoadScene(targetSceneBuildIndex);
    }
}
