public static class SceneTransitionState
{
    private static bool _skipLinearIntroOnce;
    private static bool _sceneTransitionInProgress;
    private static bool _suppressMenuMusicOnce;

    public static bool IsSceneTransitionInProgress => _sceneTransitionInProgress;

    public static void BeginSceneTransition()
    {
        _sceneTransitionInProgress = true;
    }

    public static void CompleteSceneTransition()
    {
        _sceneTransitionInProgress = false;
    }

    public static void RequestSkipLinearIntroOnce()
    {
        _skipLinearIntroOnce = true;
        _suppressMenuMusicOnce = true;
    }

    public static bool ConsumeSkipLinearIntroOnce()
    {
        if (!_skipLinearIntroOnce)
            return false;

        _skipLinearIntroOnce = false;
        return true;
    }

    public static bool ConsumeSuppressMenuMusicOnce()
    {
        if (!_suppressMenuMusicOnce)
            return false;

        _suppressMenuMusicOnce = false;
        return true;
    }
}
