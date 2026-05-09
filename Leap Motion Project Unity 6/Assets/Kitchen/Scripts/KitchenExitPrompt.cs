using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace KitchenGame
{
    public class KitchenExitPrompt : MonoBehaviour
    {
        private static KitchenExitPrompt instance;

        [SerializeField]
        private KeyCode promptKey = KeyCode.Escape;

        private RectTransform promptRoot;
        private CanvasGroup promptCanvasGroup;
        private bool isVisible;
        private float previousTimeScale = 1f;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Bootstrap()
        {
            if (instance != null)
            {
                return;
            }

            var host = new GameObject("KitchenExitPrompt");
            DontDestroyOnLoad(host);
            host.AddComponent<KitchenExitPrompt>();
        }

        private void Awake()
        {
            if (instance != null && instance != this)
            {
                Destroy(gameObject);
                return;
            }

            instance = this;
            SceneManager.sceneLoaded += HandleSceneLoaded;
            EnsurePromptUi();
            SetVisible(false, false);
        }

        private void OnDestroy()
        {
            if (instance == this)
            {
                instance = null;
            }

            SceneManager.sceneLoaded -= HandleSceneLoaded;
        }

        private void Update()
        {
            EnsurePromptUi();

            if (!Input.GetKeyDown(promptKey))
            {
                return;
            }

            if (!isVisible)
            {
                SetVisible(true, true);
                return;
            }

            SetVisible(false, true);
        }

        private void HandleSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            EnsurePromptUi(forceRecreate: true);
            SetVisible(false, false);
        }

        private void EnsurePromptUi(bool forceRecreate = false)
        {
            if (!forceRecreate && promptRoot != null && promptRoot.transform.parent != null)
            {
                return;
            }

            if (promptRoot != null)
            {
                Destroy(promptRoot.gameObject);
            }

            CreatePromptUi();
        }

        private void CreatePromptUi()
        {
            EnsureEventSystem();

            var canvas = FindOverlayCanvas();
            if (canvas == null)
            {
                var canvasObject = new GameObject("KitchenExitPromptCanvas");
                canvas = canvasObject.AddComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                canvasObject.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                canvasObject.AddComponent<GraphicRaycaster>();
                DontDestroyOnLoad(canvasObject);
            }
            else if (canvas.GetComponent<GraphicRaycaster>() == null)
            {
                canvas.gameObject.AddComponent<GraphicRaycaster>();
            }

            var rootObject = new GameObject("ExitPrompt");
            rootObject.transform.SetParent(canvas.transform, false);
            promptRoot = rootObject.AddComponent<RectTransform>();
            promptCanvasGroup = rootObject.AddComponent<CanvasGroup>();

            promptRoot.anchorMin = new Vector2(0.5f, 0.5f);
            promptRoot.anchorMax = new Vector2(0.5f, 0.5f);
            promptRoot.pivot = new Vector2(0.5f, 0.5f);
            promptRoot.sizeDelta = new Vector2(260f, 130f);

            var background = rootObject.AddComponent<Image>();
            background.color = new Color(0.08f, 0.1f, 0.14f, 0.92f);

            var title = CreateText("Title", "Exit Game?", 24f, FontStyles.Bold, TextAlignmentOptions.Center, rootObject.transform);
            title.rectTransform.anchorMin = new Vector2(0.5f, 1f);
            title.rectTransform.anchorMax = new Vector2(0.5f, 1f);
            title.rectTransform.pivot = new Vector2(0.5f, 1f);
            title.rectTransform.sizeDelta = new Vector2(220f, 32f);
            title.rectTransform.anchoredPosition = new Vector2(0f, -18f);

            CreateButton("YES", new Color(0.52f, 0.2f, 0.2f, 0.95f), rootObject.transform, new Vector2(-60f, -78f), ConfirmQuit);
            CreateButton("NO", new Color(0.18f, 0.32f, 0.22f, 0.95f), rootObject.transform, new Vector2(60f, -78f), CancelQuit);
        }

        private static TextMeshProUGUI CreateText(string name, string content, float size, FontStyles style, TextAlignmentOptions alignment, Transform parent)
        {
            var textObject = new GameObject(name);
            textObject.transform.SetParent(parent, false);
            var text = textObject.AddComponent<TextMeshProUGUI>();
            text.text = content;
            text.fontSize = size;
            text.fontStyle = style;
            text.color = Color.white;
            text.alignment = alignment;
            return text;
        }

        private static void CreateButton(string label, Color color, Transform parent, Vector2 anchoredPosition, UnityEngine.Events.UnityAction onClick)
        {
            var buttonObject = new GameObject(label);
            buttonObject.transform.SetParent(parent, false);

            var rect = buttonObject.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 1f);
            rect.anchorMax = new Vector2(0.5f, 1f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = new Vector2(100f, 38f);
            rect.anchoredPosition = anchoredPosition;

            var image = buttonObject.AddComponent<Image>();
            image.color = color;

            var button = buttonObject.AddComponent<Button>();
            button.targetGraphic = image;
            button.onClick.AddListener(onClick);
            button.transition = Selectable.Transition.ColorTint;
            var colors = button.colors;
            colors.normalColor = color;
            colors.highlightedColor = color * 1.1f;
            colors.pressedColor = color * 0.9f;
            colors.selectedColor = color;
            button.colors = colors;

            var text = CreateText("Label", label, 18f, FontStyles.Bold, TextAlignmentOptions.Center, buttonObject.transform);
            text.rectTransform.anchorMin = Vector2.zero;
            text.rectTransform.anchorMax = Vector2.one;
            text.rectTransform.offsetMin = Vector2.zero;
            text.rectTransform.offsetMax = Vector2.zero;
        }

        private void SetVisible(bool visible, bool manageTimeScale)
        {
            isVisible = visible;

            if (promptCanvasGroup != null)
            {
                promptCanvasGroup.alpha = visible ? 1f : 0f;
                promptCanvasGroup.interactable = visible;
                promptCanvasGroup.blocksRaycasts = visible;
            }

            if (!manageTimeScale)
            {
                return;
            }

            if (visible)
            {
                previousTimeScale = Time.timeScale;
                Time.timeScale = 0f;
            }
            else
            {
                Time.timeScale = previousTimeScale;
            }
        }

        private void ConfirmQuit()
        {
            SetVisible(false, true);
#if UNITY_EDITOR
            EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        private void CancelQuit()
        {
            SetVisible(false, true);
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

        private static void EnsureEventSystem()
        {
            var eventSystem = FindFirstObjectByType<EventSystem>();
            if (eventSystem != null)
            {
                if (eventSystem.GetComponent<StandaloneInputModule>() == null)
                {
                    eventSystem.gameObject.AddComponent<StandaloneInputModule>();
                }

                return;
            }

            var eventSystemObject = new GameObject("EventSystem");
            eventSystemObject.AddComponent<EventSystem>();
            eventSystemObject.AddComponent<StandaloneInputModule>();
            DontDestroyOnLoad(eventSystemObject);
        }
    }
}
