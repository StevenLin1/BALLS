using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

namespace KitchenGame
{
    public class KitchenRuntimeResetHotkey : MonoBehaviour
    {
        [SerializeField]
        private KeyCode resetKey = KeyCode.R;

        [SerializeField]
        private bool requirePlayMode = true;

        [SerializeField]
        private string hintText = "R Reset";

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Bootstrap()
        {
            var host = new GameObject("KitchenRuntimeResetHotkey");
            DontDestroyOnLoad(host);
            host.AddComponent<KitchenRuntimeResetHotkey>();
        }

        private void Awake()
        {
            CreateHintUi();
        }

        private void Update()
        {
            if (requirePlayMode && !Application.isPlaying)
            {
                return;
            }

            if (!Input.GetKeyDown(resetKey))
            {
                return;
            }

            var activeScene = SceneManager.GetActiveScene();
            if (!activeScene.IsValid())
            {
                return;
            }

            SceneManager.LoadScene(activeScene.buildIndex);
        }

        private void CreateHintUi()
        {
            var canvas = FindOverlayCanvas();
            if (canvas == null)
            {
                var canvasObject = new GameObject("KitchenResetHintCanvas");
                canvas = canvasObject.AddComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                canvasObject.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                canvasObject.AddComponent<GraphicRaycaster>();
                DontDestroyOnLoad(canvasObject);
            }

            if (canvas.transform.Find("ResetHint") != null)
            {
                return;
            }

            var root = new GameObject("ResetHint");
            root.transform.SetParent(canvas.transform, false);

            var rect = root.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0f, 0f);
            rect.anchorMax = new Vector2(0f, 0f);
            rect.pivot = new Vector2(0f, 0f);
            rect.sizeDelta = new Vector2(90f, 24f);
            rect.anchoredPosition = new Vector2(10f, 10f);

            var background = root.AddComponent<Image>();
            background.color = new Color(0.08f, 0.1f, 0.14f, 0.72f);

            var labelObject = new GameObject("Label");
            labelObject.transform.SetParent(root.transform, false);

            var labelRect = labelObject.AddComponent<RectTransform>();
            labelRect.anchorMin = Vector2.zero;
            labelRect.anchorMax = Vector2.one;
            labelRect.offsetMin = new Vector2(6f, 2f);
            labelRect.offsetMax = new Vector2(-6f, -2f);

            var label = labelObject.AddComponent<TextMeshProUGUI>();
            label.text = hintText;
            label.fontSize = 12f;
            label.color = Color.white;
            label.alignment = TextAlignmentOptions.Center;
        }

        private static Canvas FindOverlayCanvas()
        {
            var canvases = FindObjectsByType<Canvas>(FindObjectsSortMode.None);
            foreach (var canvas in canvases)
            {
                if (canvas != null && canvas.renderMode == RenderMode.ScreenSpaceOverlay)
                {
                    return canvas;
                }
            }

            return null;
        }
    }
}
