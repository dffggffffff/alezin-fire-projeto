using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Alezinfire
{
    public static class UIKit
    {
        static Font font;
        public static Font Font
        {
            get
            {
                if (!font) font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
                if (!font) font = Font.CreateDynamicFontFromOSFont("Arial", 32);
                return font;
            }
        }

        public static Canvas MakeCanvas(string name, int order)
        {
            var go = new GameObject(name);
            var c = go.AddComponent<Canvas>();
            c.renderMode = RenderMode.ScreenSpaceOverlay;
            c.sortingOrder = order;
            var s = go.AddComponent<CanvasScaler>();
            s.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            s.referenceResolution = new Vector2(1920, 1080);
            s.matchWidthOrHeight = 0.5f;
            go.AddComponent<GraphicRaycaster>();
            if (Object.FindObjectOfType<EventSystem>() == null)
            {
                var e = new GameObject("EventSystem");
                e.AddComponent<EventSystem>();
                e.AddComponent<StandaloneInputModule>();
                Object.DontDestroyOnLoad(e);
            }
            return c;
        }

        public static RectTransform Rect(Transform p, string n, Vector2 aMin, Vector2 aMax)
        {
            var go = new GameObject(n, typeof(RectTransform));
            var r = (RectTransform)go.transform;
            r.SetParent(p, false);
            r.anchorMin = aMin; r.anchorMax = aMax;
            r.offsetMin = r.offsetMax = Vector2.zero;
            return r;
        }

        public static RectTransform Pix(Transform p, string n, Vector2 anchor, Vector2 pos, Vector2 size, Vector2? pivot = null)
        {
            var go = new GameObject(n, typeof(RectTransform));
            var r = (RectTransform)go.transform;
            r.SetParent(p, false);
            r.anchorMin = r.anchorMax = anchor;
            r.pivot = pivot ?? anchor;
            r.anchoredPosition = pos;
            r.sizeDelta = size;
            return r;
        }

        public static Image Img(Transform p, string n, Color c, Vector2 aMin, Vector2 aMax)
        {
            var r = Rect(p, n, aMin, aMax);
            var i = r.gameObject.AddComponent<Image>();
            i.color = c;
            return i;
        }

        public static Text Txt(Transform p, string s, int size, Color c, Vector2 aMin, Vector2 aMax, TextAnchor a = TextAnchor.MiddleCenter)
        {
            var r = Rect(p, "Text", aMin, aMax);
            var t = r.gameObject.AddComponent<Text>();
            t.font = Font; t.text = s; t.fontSize = size; t.color = c; t.alignment = a;
            t.horizontalOverflow = HorizontalWrapMode.Wrap;
            t.verticalOverflow = VerticalWrapMode.Overflow;
            t.raycastTarget = false;
            return t;
        }

        public static Button Btn(Transform p, string label, int fs, Color bg, Vector2 aMin, Vector2 aMax, UnityAction on)
        {
            var im = Img(p, "Btn", bg, aMin, aMax);
            var b = im.gameObject.AddComponent<Button>();
            b.targetGraphic = im;
            if (on != null) b.onClick.AddListener(on);
            var t = Txt(im.transform, label, fs, Color.white, Vector2.zero, Vector2.one);
            t.resizeTextForBestFit = true; t.resizeTextMinSize = 12; t.resizeTextMaxSize = fs;
            return b;
        }

        public static Button PixBtn(Transform p, string label, int fs, Color bg, Vector2 anchor, Vector2 pos, Vector2 size, UnityAction on, Vector2? pivot = null)
        {
            var r = Pix(p, "Btn", anchor, pos, size, pivot);
            var im = r.gameObject.AddComponent<Image>();
            im.color = bg;
            var b = r.gameObject.AddComponent<Button>();
            b.targetGraphic = im;
            if (on != null) b.onClick.AddListener(on);
            var t = Txt(r, label, fs, Color.white, Vector2.zero, Vector2.one);
            t.resizeTextForBestFit = true; t.resizeTextMinSize = 12; t.resizeTextMaxSize = fs;
            return b;
        }
    }
}
