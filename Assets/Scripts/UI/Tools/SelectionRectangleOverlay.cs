using UnityEngine;
using UnityEngine.UI;

public class SelectionRectangleOverlay
{
    private const string OverlayName = "Selection Rectangle Overlay";
    private readonly RectTransform _overlayRectTransform;

    public SelectionRectangleOverlay()
    {
        var parentTransform = GetOverlayParent();

        var overlayObject = new GameObject(OverlayName, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        overlayObject.transform.SetParent(parentTransform, false);

        _overlayRectTransform = overlayObject.GetComponent<RectTransform>();
        _overlayRectTransform.anchorMin = Vector2.zero;
        _overlayRectTransform.anchorMax = Vector2.zero;
        _overlayRectTransform.pivot = Vector2.zero;

        var image = overlayObject.GetComponent<Image>();
        image.color = new Color(0.4f, 0.7f, 1f, 0.2f);
        image.raycastTarget = false;

        overlayObject.SetActive(false);
    }

    public void Show(Vector2 startScreenPosition, Vector2 currentScreenPosition)
    {
        var min = Vector2.Min(startScreenPosition, currentScreenPosition);
        var max = Vector2.Max(startScreenPosition, currentScreenPosition);

        _overlayRectTransform.gameObject.SetActive(true);
        _overlayRectTransform.anchoredPosition = min;
        _overlayRectTransform.sizeDelta = max - min;
    }

    public void Hide()
    {
        _overlayRectTransform.gameObject.SetActive(false);
    }

    private static Transform GetOverlayParent()
    {
        var buildEditorUi = Object.FindFirstObjectByType<BuildEditorUI>();
        if (buildEditorUi != null)
        {
            var buildEditorCanvas = buildEditorUi.GetComponentInParent<Canvas>();
            if (buildEditorCanvas != null)
                return buildEditorCanvas.transform;
        }

        var canvas = Object.FindFirstObjectByType<Canvas>();
        if (canvas != null)
            return canvas.transform;

        var canvasObject = new GameObject("Selection Overlay Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        var overlayCanvas = canvasObject.GetComponent<Canvas>();
        overlayCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        return canvasObject.transform;
    }
}
