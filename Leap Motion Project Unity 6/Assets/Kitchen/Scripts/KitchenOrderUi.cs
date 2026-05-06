using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace KitchenGame
{
    public class KitchenOrderUi : MonoBehaviour
    {
        [SerializeField]
        private KitchenServingAssembly servingAssembly;

        [SerializeField]
        private Vector2 panelSize = new Vector2(520f, 88f);

        [SerializeField]
        private Vector2 panelOffset = new Vector2(0f, -10f);

        private RectTransform rootPanel;
        private OrderRowUi[] rows;
        private float nextRefreshTime;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Bootstrap()
        {
            var host = new GameObject("KitchenOrderUi");
            DontDestroyOnLoad(host);
            host.AddComponent<KitchenOrderUi>();
        }

        private void Awake()
        {
            if (servingAssembly == null)
            {
                servingAssembly = FindFirstObjectByType<KitchenServingAssembly>();
            }

            CreateUi();
            RefreshNow();
        }

        private void Update()
        {
            if (Time.unscaledTime < nextRefreshTime)
            {
                return;
            }

            RefreshNow();
        }

        private void CreateUi()
        {
            var existingCanvas = FindFirstObjectByType<Canvas>();
            Canvas canvas;

            if (existingCanvas != null && existingCanvas.renderMode == RenderMode.ScreenSpaceOverlay)
            {
                canvas = existingCanvas;
            }
            else
            {
                var canvasObject = new GameObject("KitchenRuntimeCanvas");
                canvas = canvasObject.AddComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                canvasObject.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                canvasObject.AddComponent<GraphicRaycaster>();
            }

            var panelObject = new GameObject("OrderPanel");
            panelObject.transform.SetParent(canvas.transform, false);
            rootPanel = panelObject.AddComponent<RectTransform>();
            var background = panelObject.AddComponent<Image>();
            background.color = new Color(0.08f, 0.1f, 0.14f, 0.82f);

            rootPanel.anchorMin = new Vector2(0.5f, 1f);
            rootPanel.anchorMax = new Vector2(0.5f, 1f);
            rootPanel.pivot = new Vector2(0.5f, 1f);
            rootPanel.sizeDelta = panelSize;
            rootPanel.anchoredPosition = panelOffset;

            var layout = panelObject.AddComponent<HorizontalLayoutGroup>();
            layout.padding = new RectOffset(10, 10, 8, 8);
            layout.spacing = 8;
            layout.childControlHeight = false;
            layout.childControlWidth = true;
            layout.childForceExpandHeight = true;
            layout.childForceExpandWidth = true;

            var slotCount = servingAssembly != null ? Mathf.Max(3, servingAssembly.OrderSlotCount) : 3;
            rows = new OrderRowUi[slotCount];
            for (var i = 0; i < rows.Length; i++)
            {
                rows[i] = CreateOrderRow(i + 1, rootPanel);
            }
        }

        private void RefreshNow()
        {
            nextRefreshTime = Time.unscaledTime + 0.15f;

            if (rows == null || rows.Length == 0)
            {
                return;
            }

            for (var i = 0; i < rows.Length; i++)
            {
                if (servingAssembly == null || i >= servingAssembly.OrderSlotCount)
                {
                    rows[i].Header.SetText($"Order {i + 1}");
                    rows[i].Body.SetText("No slot");
                    rows[i].Background.color = new Color(0.18f, 0.2f, 0.24f, 0.88f);
                    continue;
                }

                var snapshot = servingAssembly.GetOrderSnapshot(i);
                rows[i].Header.SetText($"Order {i + 1}");
                rows[i].Body.SetText(
                    $"N {snapshot.RequiredMeatCount}/{snapshot.RequiredCheeseCount}/{snapshot.RequiredVegetableCount}\n" +
                    $"M {snapshot.MaxMeatCount}/{snapshot.MaxCheeseCount}/{snapshot.MaxVegetableCount}\n" +
                    $"C {snapshot.AcceptedMeatCount}/{snapshot.AcceptedCheeseCount}/{snapshot.AcceptedVegetableCount}" +
                    $"{(snapshot.AcceptedBurntMeatCount > 0 ? $"  Burnt {snapshot.AcceptedBurntMeatCount}" : string.Empty)}" +
                    $"{(snapshot.HasTopBun ? "  Top" : string.Empty)}");

                rows[i].Background.color = snapshot.IsCompleted
                    ? new Color(0.18f, 0.42f, 0.24f, 0.92f)
                    : snapshot.IsCurrent
                        ? new Color(0.42f, 0.28f, 0.12f, 0.92f)
                        : new Color(0.18f, 0.2f, 0.24f, 0.88f);
            }
        }

        private static TMP_Text CreateLabel(string text, float size, FontStyles style, Transform parent)
        {
            var textObject = new GameObject(text);
            textObject.transform.SetParent(parent, false);
            var label = textObject.AddComponent<TextMeshProUGUI>();
            label.fontSize = size;
            label.fontStyle = style;
            label.color = Color.white;
            label.text = text;
            return label;
        }

        private static OrderRowUi CreateOrderRow(int orderNumber, Transform parent)
        {
            var rowObject = new GameObject($"OrderRow{orderNumber}");
            rowObject.transform.SetParent(parent, false);

            var background = rowObject.AddComponent<Image>();
            background.color = new Color(0.18f, 0.2f, 0.24f, 0.88f);

            var layoutElement = rowObject.AddComponent<LayoutElement>();
            layoutElement.minWidth = 160f;
            layoutElement.preferredWidth = 160f;
            layoutElement.minHeight = 68f;

            var layout = rowObject.AddComponent<VerticalLayoutGroup>();
            layout.padding = new RectOffset(8, 8, 6, 6);
            layout.spacing = 1;
            layout.childControlHeight = true;
            layout.childControlWidth = true;
            layout.childForceExpandHeight = false;
            layout.childForceExpandWidth = true;

            var header = CreateLabel($"Order {orderNumber}", 15, FontStyles.Bold, rowObject.transform);
            var body = CreateLabel(string.Empty, 12, FontStyles.Normal, rowObject.transform);
            body.enableWordWrapping = false;

            return new OrderRowUi
            {
                Background = background,
                Header = header,
                Body = body
            };
        }

        private struct OrderRowUi
        {
            public Image Background;
            public TMP_Text Header;
            public TMP_Text Body;
        }
    }
}
