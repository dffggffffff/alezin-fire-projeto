using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Alezinfire
{
    /// HUD personalizado: minimapa, vida/colete, munição, zona, kill feed, mira e hitmarker.
    public class HUD
    {
        public TouchControls tc;
        Canvas canvas; MatchManager m; PlayerController p;
        Text aliveTxt, killTxt, zoneTxt, feedTxt, hpTxt, ammoTxt, promptTxt, hitMark;
        RectTransform hpFill, armorFill, arrow;
        Image dmgFlash;
        Camera miniCam; RenderTexture rt;
        float hitT;
        readonly List<string> feed = new List<string>();
        readonly List<float> feedT = new List<float>();

        public HUD(MatchManager match)
        {
            m = match; p = m.player;
            tc = new TouchControls(true);
            tc.onEmote = id => p.DoEmote(id);
            canvas = UIKit.MakeCanvas("HUD", 4);
            var r = canvas.transform;

            // minimapa
            rt = new RenderTexture(256, 256, 16);
            var cg = new GameObject("MiniCam"); miniCam = cg.AddComponent<Camera>();
            miniCam.orthographic = true; miniCam.orthographicSize = 100; miniCam.targetTexture = rt;
            miniCam.clearFlags = CameraClearFlags.SolidColor; miniCam.backgroundColor = new Color(.1f, .3f, .5f);
            miniCam.nearClipPlane = 1; miniCam.farClipPlane = 600; miniCam.depth = -10;
            cg.transform.rotation = Quaternion.Euler(90, 0, 0);
            var frame = UIKit.Pix(r, "MiniFrame", new Vector2(0, 1), new Vector2(20, -20), new Vector2(320, 320));
            frame.gameObject.AddComponent<Image>().color = new Color(1, 1, 1, .9f);
            var ri = UIKit.Rect(frame, "Mini", Vector2.zero, Vector2.one).gameObject.AddComponent<RawImage>();
            ri.rectTransform.offsetMin = new Vector2(5, 5); ri.rectTransform.offsetMax = new Vector2(-5, -5);
            ri.texture = rt; ri.raycastTarget = false;
            var ar = UIKit.Pix(frame, "Seta", new Vector2(.5f, .5f), Vector2.zero, new Vector2(26, 26), new Vector2(.5f, .5f));
            var ai = ar.gameObject.AddComponent<Image>(); ai.color = new Color(1f, .9f, 0); ai.raycastTarget = false;
            UIKit.Txt(ar, "^", 30, Color.black, Vector2.zero, Vector2.one);
            arrow = ar;

            // topo
            aliveTxt = UIKit.Txt(r, "", 38, Color.white, new Vector2(.4f, .94f), new Vector2(.5f, 1));
            killTxt = UIKit.Txt(r, "", 38, new Color(1f, .8f, .2f), new Vector2(.5f, .94f), new Vector2(.6f, 1));
            zoneTxt = UIKit.Txt(r, "", 32, new Color(.5f, .8f, 1f), new Vector2(.35f, .88f), new Vector2(.65f, .94f));
            feedTxt = UIKit.Txt(r, "", 26, Color.white, new Vector2(.7f, .75f), new Vector2(.99f, .99f), TextAnchor.UpperRight);

            // vida / colete
            UIKit.Img(r, "VidaBg", new Color(0, 0, 0, .6f), new Vector2(.32f, .035f), new Vector2(.58f, .075f));
            hpFill = UIKit.Img(r, "Vida", new Color(.2f, .85f, .3f), new Vector2(.32f, .035f), new Vector2(.58f, .075f)).rectTransform;
            UIKit.Img(r, "ColeteBg", new Color(0, 0, 0, .6f), new Vector2(.32f, .08f), new Vector2(.58f, .095f));
            armorFill = UIKit.Img(r, "Colete", new Color(.3f, .6f, 1f), new Vector2(.32f, .08f), new Vector2(.58f, .095f)).rectTransform;
            hpTxt = UIKit.Txt(r, "", 28, Color.white, new Vector2(.32f, .035f), new Vector2(.58f, .075f));
            ammoTxt = UIKit.Txt(r, "", 34, Color.white, new Vector2(.3f, .1f), new Vector2(.6f, .16f));
            promptTxt = UIKit.Txt(r, "", 40, new Color(1f, .95f, .5f), new Vector2(.25f, .6f), new Vector2(.75f, .7f));

            UIKit.Txt(r, "+", 50, new Color(1, 1, 1, .8f), new Vector2(.49f, .49f), new Vector2(.51f, .55f));
            hitMark = UIKit.Txt(r, "X", 60, Color.red, new Vector2(.48f, .47f), new Vector2(.52f, .57f));
            hitMark.enabled = false;
            dmgFlash = UIKit.Img(r, "Dano", new Color(1, 0, 0, 0), Vector2.zero, Vector2.one);
            dmgFlash.raycastTarget = false;

            WeaponSystem.HitMarker += OnHit;
            Health.KillFeed += OnKill;
            p.health.OnDamaged += OnDamaged;
        }

        void OnHit(bool head) { hitT = .18f; hitMark.color = head ? new Color(1f, .8f, 0) : Color.red; }
        void OnDamaged(float a) { var c = dmgFlash.color; c.a = .35f; dmgFlash.color = c; }
        void OnKill(string k, string v) { feed.Add(k + "  eliminou  " + v); feedT.Add(Time.time + 5f); if (feed.Count > 5) { feed.RemoveAt(0); feedT.RemoveAt(0); } }

        public void Tick()
        {
            var h = p.health;
            aliveTxt.text = "VIVOS " + m.Alive;
            killTxt.text = "ABATES " + h.kills;
            zoneTxt.text = m.zone != null ? m.zone.Status : "";
            hpFill.anchorMax = new Vector2(.32f + .26f * Mathf.Clamp01(h.hp / 100f), .075f);
            armorFill.anchorMax = new Vector2(.32f + .26f * Mathf.Clamp01(h.armor / 100f), .095f);
            hpTxt.text = Mathf.CeilToInt(h.hp) + " / 100";
            var w = p.weapons.Current;
            ammoTxt.text = w == null ? "SEM ARMA - pegue uma no chão" : w.name + "   " + p.weapons.ammo[p.weapons.cur] + "/" + w.mag + (p.weapons.Reloading ? "  RECARREGANDO" : "");

            string prompt = "";
            switch (p.mode)
            {
                case PMode.Plane: prompt = "Toque PULO para saltar do avião"; tc.jumpLabel.text = "PULAR"; break;
                case PMode.Freefall: prompt = "Toque ABRIR para abrir a prancha planadora"; tc.jumpLabel.text = "ABRIR"; break;
                case PMode.Glide: prompt = "Planando... mire onde quer pousar"; tc.jumpLabel.text = "PULO"; break;
                default:
                    tc.jumpLabel.text = "PULO";
                    if (p.NearPickup) prompt = "PEGAR: " + p.NearPickup.Label;
                    else if (m.zone != null && !m.zone.Inside(p.transform.position)) prompt = "FORA DA ZONA! Corra para o círculo!";
                    break;
            }
            promptTxt.text = prompt;

            miniCam.transform.position = p.transform.position + Vector3.up * 250f;
            arrow.localEulerAngles = new Vector3(0, 0, -p.cam.yaw);

            hitT -= Time.deltaTime; hitMark.enabled = hitT > 0;
            var c = dmgFlash.color; c.a = Mathf.MoveTowards(c.a, 0, Time.deltaTime); dmgFlash.color = c;
            for (int i = feed.Count - 1; i >= 0; i--) if (Time.time > feedT[i]) { feed.RemoveAt(i); feedT.RemoveAt(i); }
            feedTxt.text = string.Join("\n", feed.ToArray());
        }

        public void Destroy()
        {
            WeaponSystem.HitMarker -= OnHit; Health.KillFeed -= OnKill;
            tc.Destroy();
            if (canvas) Object.Destroy(canvas.gameObject);
            if (miniCam) Object.Destroy(miniCam.gameObject);
            if (rt) rt.Release();
        }
    }
}
