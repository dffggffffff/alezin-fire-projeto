using UnityEngine;
using UnityEngine.UI;

namespace Alezinfire
{
    /// Joystick virtual, área de câmera, botões de ação e roda de emotes.
    public class TouchControls
    {
        public Canvas canvas;
        public Text jumpLabel;
        public System.Action<int> onEmote;
        GameObject wheel;

        public TouchControls(bool combat)
        {
            canvas = UIKit.MakeCanvas("Controles", 5);
            var r = canvas.transform;
            var pad = UIKit.Img(r, "LookPad", new Color(0, 0, 0, 0), new Vector2(.4f, 0), Vector2.one);
            pad.gameObject.AddComponent<LookPad>();

            var jr = UIKit.Pix(r, "Joystick", Vector2.zero, new Vector2(230, 230), new Vector2(300, 300), new Vector2(.5f, .5f));
            var jbg = jr.gameObject.AddComponent<Image>(); jbg.color = new Color(1, 1, 1, .15f);
            var kr = UIKit.Pix(jr, "Knob", new Vector2(.5f, .5f), Vector2.zero, new Vector2(120, 120), new Vector2(.5f, .5f));
            var ki = kr.gameObject.AddComponent<Image>(); ki.color = new Color(1, 1, 1, .5f); ki.raycastTarget = false;
            var joy = jr.gameObject.AddComponent<VirtualJoystick>(); joy.knob = kr;

            var BR = new Vector2(1, 0);
            var jb = UIKit.PixBtn(r, "PULO", 30, new Color(.15f, .5f, .9f, .8f), BR, new Vector2(-300, 90), new Vector2(130, 130), () => InputHub.UiJump = true);
            jumpLabel = jb.GetComponentInChildren<Text>();
            UIKit.PixBtn(r, "EMOTE", 26, new Color(.8f, .4f, .9f, .8f), Vector2.zero, new Vector2(60, 480), new Vector2(120, 100), ToggleWheel);

            if (combat)
            {
                var fr = UIKit.Pix(r, "Fogo", BR, new Vector2(-60, 60), new Vector2(200, 200));
                var fi = fr.gameObject.AddComponent<Image>(); fi.color = new Color(.95f, .2f, .15f, .75f);
                fr.gameObject.AddComponent<HoldButton>();
                UIKit.Txt(fr, "FOGO", 40, Color.white, Vector2.zero, Vector2.one);
                UIKit.PixBtn(r, "AGACHAR", 24, new Color(.2f, .2f, .25f, .8f), BR, new Vector2(-100, 290), new Vector2(110, 100), () => InputHub.UiCrouch = true);
                UIKit.PixBtn(r, "RECARREGAR", 20, new Color(.2f, .2f, .25f, .8f), BR, new Vector2(-230, 290), new Vector2(120, 100), () => InputHub.UiReload = true);
                UIKit.PixBtn(r, "PEGAR", 26, new Color(.2f, .6f, .3f, .85f), BR, new Vector2(-370, 250), new Vector2(120, 100), () => InputHub.UiPickup = true);
                UIKit.PixBtn(r, "TROCAR", 24, new Color(.2f, .2f, .25f, .8f), BR, new Vector2(-460, 90), new Vector2(120, 100), () => InputHub.UiSwap = true);
            }

            wheel = new GameObject("RodaEmotes", typeof(RectTransform));
            wheel.transform.SetParent(r, false);
            for (int i = 0; i < Config.EmoteNames.Length; i++)
            {
                int id = i;
                UIKit.PixBtn(wheel.transform, Config.EmoteNames[i], 26, new Color(.5f, .2f, .7f, .9f), new Vector2(.5f, .5f), new Vector2((i - 2) * 190, 0), new Vector2(175, 90), () =>
                {
                    Config.SelectedEmote = id; wheel.SetActive(false);
                    if (onEmote != null) onEmote(id);
                });
            }
            wheel.SetActive(false);
        }

        void ToggleWheel() { wheel.SetActive(!wheel.activeSelf); }
        public void Destroy() { if (canvas) Object.Destroy(canvas.gameObject); InputHub.UiMove = Vector2.zero; InputHub.UiFire = false; }
    }
}
