using System.Collections.Generic;
using UnityEngine;

namespace Alezinfire
{
    public enum GameMode { Solo = 1, Dupla = 2, Esquadrao = 4 }

    public static class Config
    {
        public const int TotalPlayers = 30;
        public const float WaterY = 0f;
        public static GameMode Mode = GameMode.Solo;
        public static List<string> Squad = new List<string>();
        public static int SelectedEmote = 0;

        public static float Sensitivity
        {
            get { return PlayerPrefs.GetFloat("az_sens", 1f); }
            set { PlayerPrefs.SetFloat("az_sens", Mathf.Clamp(value, 0.3f, 3f)); }
        }
        public static string PlayerName
        {
            get { return PlayerPrefs.GetString("az_name", "Jogador"); }
            set { PlayerPrefs.SetString("az_name", value); }
        }
        public static int Outfit
        {
            get { return Mathf.Clamp(PlayerPrefs.GetInt("az_outfit", 0), 0, Outfits.Length - 1); }
            set { PlayerPrefs.SetInt("az_outfit", ((value % Outfits.Length) + Outfits.Length) % Outfits.Length); }
        }

        public struct OutfitData { public string name; public Color shirt, pants, skin; }
        static OutfitData O(string n, Color s, Color p, Color k) { return new OutfitData { name = n, shirt = s, pants = p, skin = k }; }
        static Color C(float r, float g, float b) { return new Color(r / 255f, g / 255f, b / 255f); }

        public static readonly OutfitData[] Outfits = {
            O("Soldado Musgo", C(70,110,60), C(60,60,50), C(230,185,150)),
            O("Neon Rosa", C(255,60,150), C(40,40,70), C(200,150,110)),
            O("Ninja Sombra", C(30,30,35), C(20,20,25), C(240,200,165)),
            O("Fogo Vivo", C(235,90,20), C(70,30,20), C(150,100,70)),
            O("Gelo Polar", C(120,200,255), C(60,90,130), C(250,215,190)),
            O("Rei Dourado", C(240,200,40), C(90,60,20), C(110,75,50)),
        };
        public static readonly string[] EmoteNames = { "Dançar", "Acenar", "Músculo", "Girar", "Comemorar" };
    }

    public static class Mats
    {
        static Shader shader;
        static readonly Dictionary<int, Material> cache = new Dictionary<int, Material>();

        static Shader Sh()
        {
            if (shader) return shader;
            shader = Shader.Find("Standard");
            if (!shader) shader = Shader.Find("Universal Render Pipeline/Lit");
            if (!shader) shader = Shader.Find("Sprites/Default");
            return shader;
        }

        public static Material Lit(Color c)
        {
            Color32 q = c;
            int k = q.r | (q.g << 8) | (q.b << 16) | (q.a << 24);
            Material m;
            if (cache.TryGetValue(k, out m) && m) return m;
            m = new Material(Sh());
            m.color = c;
            if (m.HasProperty("_BaseColor")) m.SetColor("_BaseColor", c);
            if (m.HasProperty("_Glossiness")) m.SetFloat("_Glossiness", 0.1f);
            cache[k] = m;
            return m;
        }

        public static Material Unlit(Color c)
        {
            var m = new Material(Shader.Find("Sprites/Default"));
            m.color = c;
            return m;
        }

        public static GameObject Prim(PrimitiveType t, string n, Transform p, Vector3 pos, Vector3 scale, Color c, bool collider = true)
        {
            var g = GameObject.CreatePrimitive(t);
            g.name = n;
            g.transform.SetParent(p, false);
            g.transform.localPosition = pos;
            g.transform.localScale = scale;
            g.GetComponent<Renderer>().sharedMaterial = Lit(c);
            if (!collider) Object.Destroy(g.GetComponent<Collider>());
            return g;
        }
    }

    public static class Sfx
    {
        static AudioClip shot;
        public static void Shot(Vector3 pos, float vol)
        {
            if (!shot)
            {
                int n = 5000; var d = new float[n];
                for (int i = 0; i < n; i++) d[i] = (Random.value * 2f - 1f) * Mathf.Exp(-i / 700f) * 0.6f;
                shot = AudioClip.Create("shot", n, 1, 44100, false);
                shot.SetData(d, 0);
            }
            AudioSource.PlayClipAtPoint(shot, pos, vol);
        }
    }
}
