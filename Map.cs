using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Alezinfire
{
    /// Ilha "Bermuda" procedural: terreno com física, cidades com prédios ocos, rampas, caixas físicas, árvores e água.
    public static class Map
    {
        public struct City { public string name; public Vector2 pos; public float radius; }

        public static readonly City[] Cities = {
            new City { name = "Pico", pos = new Vector2(0, 40), radius = 55 },
            new City { name = "Usina Marte", pos = new Vector2(-170, -120), radius = 50 },
            new City { name = "Torre do Relógio", pos = new Vector2(150, 160), radius = 50 },
            new City { name = "Moinho", pos = new Vector2(-210, 170), radius = 45 },
            new City { name = "Porto Cabo", pos = new Vector2(240, -110), radius = 50 },
            new City { name = "Vila Rim", pos = new Vector2(-60, -260), radius = 45 },
            new City { name = "Fábrica", pos = new Vector2(60, -170), radius = 45 },
            new City { name = "Aeroporto Velho", pos = new Vector2(-300, 0), radius = 50 },
        };

        public static Terrain Terrain;
        public static List<Vector3> LootSpots = new List<Vector3>();
        const float Size = 1000f, MaxH = 80f, YOff = -10f, CityH = 8f;

        static float Raw(float x, float z)
        {
            float d = Mathf.Sqrt(x * x + z * z) / 480f;
            float island = Mathf.SmoothStep(1f, 0f, Mathf.InverseLerp(.55f, 1f, d));
            float n = Mathf.PerlinNoise(x * .004f + 100f, z * .004f + 100f) * .7f + Mathf.PerlinNoise(x * .015f + 7f, z * .015f + 7f) * .3f;
            float h = -8f + island * (8f + n * 42f);
            var p = new Vector2(x, z);
            foreach (var c in Cities)
            {
                float m = 1f - Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(c.radius, c.radius + 45f, Vector2.Distance(p, c.pos)));
                h = Mathf.Lerp(h, CityH, m);
            }
            return h;
        }

        public static float Height(float x, float z)
        {
            if (!Terrain) return 0f;
            return Terrain.SampleHeight(new Vector3(x, 0, z)) + Terrain.GetPosition().y;
        }

        public static Vector3 RandomLand(float maxR = 330f)
        {
            for (int i = 0; i < 40; i++)
            {
                var p = Random.insideUnitCircle * maxR;
                float h = Height(p.x, p.y);
                if (h > 2f) return new Vector3(p.x, h, p.y);
            }
            return new Vector3(0, Height(0, 0), 0);
        }

        static Texture2D GrassTex()
        {
            var t = new Texture2D(64, 64);
            for (int y = 0; y < 64; y++)
                for (int x = 0; x < 64; x++)
                {
                    float n = Mathf.PerlinNoise(x * .2f, y * .2f);
                    t.SetPixel(x, y, Color.Lerp(new Color(.22f, .45f, .18f), new Color(.38f, .55f, .25f), n));
                }
            t.wrapMode = TextureWrapMode.Repeat; t.Apply();
            return t;
        }

        public static IEnumerator Build(System.Action<float, string> prog)
        {
            var root = new GameObject("MapBermuda").transform;
            prog(.15f, "Erguendo a ilha Bermuda...");
            yield return null;
            int res = 513;
            var hm = new float[res, res];
            for (int i = 0; i < res; i++)
                for (int j = 0; j < res; j++)
                {
                    float x = j / (float)(res - 1) * Size - Size / 2f, z = i / (float)(res - 1) * Size - Size / 2f;
                    hm[i, j] = Mathf.Clamp01((Raw(x, z) - YOff) / MaxH);
                }
            var td = new TerrainData { heightmapResolution = res, size = new Vector3(Size, MaxH, Size) };
            td.SetHeights(0, 0, hm);
            var layer = new TerrainLayer { diffuseTexture = GrassTex(), tileSize = new Vector2(8, 8) };
            td.terrainLayers = new[] { layer };
            var tg = UnityEngine.Terrain.CreateTerrainGameObject(td);
            tg.transform.SetParent(root, false);
            tg.transform.position = new Vector3(-Size / 2f, YOff, -Size / 2f);
            Terrain = tg.GetComponent<Terrain>();
            prog(.3f, "Enchendo o mar...");
            yield return null;

            var water = GameObject.CreatePrimitive(PrimitiveType.Plane);
            water.name = "Agua"; water.transform.SetParent(root, false);
            water.transform.position = new Vector3(0, Config.WaterY - .2f, 0);
            water.transform.localScale = new Vector3(300, 1, 300);
            Object.Destroy(water.GetComponent<Collider>());
            water.GetComponent<Renderer>().sharedMaterial = Mats.Unlit(new Color(.1f, .45f, .75f, .8f));

            LootSpots.Clear();
            var bHolder = new GameObject("Predios").transform; bHolder.SetParent(root, false);
            var dyn = new GameObject("Dinamicos").transform; dyn.SetParent(root, false);
            for (int ci = 0; ci < Cities.Length; ci++)
            {
                var c = Cities[ci];
                prog(.35f + .4f * ci / Cities.Length, "Construindo " + c.name + "...");
                BuildCity(c, bHolder, dyn);
                yield return null;
            }

            prog(.8f, "Plantando árvores...");
            var tHolder = new GameObject("Arvores").transform; tHolder.SetParent(root, false);
            int placed = 0;
            for (int i = 0; i < 900 && placed < 380; i++)
            {
                var p = Random.insideUnitCircle * 400f;
                float h = Height(p.x, p.y);
                if (h < 3f) continue;
                bool nearCity = false;
                foreach (var c in Cities) if (Vector2.Distance(p, c.pos) < c.radius + 20f) { nearCity = true; break; }
                if (nearCity) continue;
                float s = Random.Range(.8f, 1.5f);
                Mats.Prim(PrimitiveType.Cylinder, "Tronco", tHolder, new Vector3(p.x, h + 2f * s, p.y), new Vector3(.5f * s, 2f * s, .5f * s), new Color(.35f, .22f, .1f));
                Mats.Prim(PrimitiveType.Sphere, "Copa", tHolder, new Vector3(p.x, h + 5f * s, p.y), new Vector3(3.5f * s, 3.5f * s, 3.5f * s), new Color(.12f, .4f + Random.value * .15f, .15f), false);
                placed++;
            }
            StaticBatchingUtility.Combine(tHolder.gameObject);
            StaticBatchingUtility.Combine(bHolder.gameObject);
            prog(.92f, "Espalhando loot...");
            for (int i = 0; i < 60; i++) { var l = RandomLand(380f); LootSpots.Add(l); }
            yield return null;
            prog(.95f, "Quase pronto...");
        }

        static void Box(Transform p, Vector3 pos, Vector3 size, Color c) { Mats.Prim(PrimitiveType.Cube, "Parede", p, pos, size, c); }

        static void BuildCity(City c, Transform parent, Transform dyn)
        {
            var disk = Mats.Prim(PrimitiveType.Cylinder, "Chao_" + c.name, parent, new Vector3(c.pos.x, CityH + .03f, c.pos.y), new Vector3(c.radius * 2f, .02f, c.radius * 2f), new Color(.38f, .38f, .4f), false);
            var label = new GameObject("Label_" + c.name);
            label.transform.SetParent(dyn, false);
            label.transform.position = new Vector3(c.pos.x, CityH + 45f, c.pos.y);
            var tm = label.AddComponent<TextMesh>();
            tm.text = c.name; tm.fontSize = 80; tm.characterSize = .5f; tm.anchor = TextAnchor.MiddleCenter;
            tm.font = UIKit.Font; tm.color = new Color(1f, .95f, .6f);
            label.GetComponent<MeshRenderer>().sharedMaterial = tm.font.material;
            label.AddComponent<Billboard>();

            var spots = new List<Vector2>();
            int count = 8 + (int)(c.radius / 12f);
            for (int i = 0; i < count; i++)
            {
                for (int tries = 0; tries < 25; tries++)
                {
                    var p = c.pos + Random.insideUnitCircle * (c.radius * .85f);
                    bool ok = true;
                    foreach (var s in spots) if (Vector2.Distance(s, p) < 17f) { ok = false; break; }
                    if (!ok) continue;
                    spots.Add(p);
                    float w = Random.Range(7f, 11f), d = Random.Range(7f, 10f), h = Random.Range(3.5f, 6.5f);
                    var wall = Color.HSVToRGB(Random.value, .12f, Random.Range(.6f, .9f));
                    var root = Building(parent, new Vector3(p.x, CityH, p.y), w, d, h, wall, Random.value < .4f);
                    for (int k = 0; k < 2; k++)
                    {
                        var pos = root.TransformPoint(new Vector3(Random.Range(-w / 3f, w / 3f), .25f, Random.Range(-d / 3f, d / 3f)));
                        LootSpots.Add(pos - Vector3.up * .2f);
                    }
                    break;
                }
            }
            for (int i = 0; i < 8; i++)
            {
                var p = c.pos + Random.insideUnitCircle * (c.radius * .8f);
                var cr = Mats.Prim(PrimitiveType.Cube, "Caixa", dyn, new Vector3(p.x, CityH + 1f + (i % 3) * 1.1f, p.y), Vector3.one, new Color(.55f, .38f, .2f));
                cr.AddComponent<Rigidbody>().mass = 8f;
                // objetos com Rigidbody ficam fora do batching estático (são criados antes de Combine mas dinâmicos)
            }
        }

        static Transform Building(Transform parent, Vector3 center, float w, float d, float h, Color wall, bool ramp)
        {
            var root = new GameObject("Predio").transform;
            root.SetParent(parent, false);
            root.position = center;
            float t = .3f, dw = 1.8f, dh = 2.6f, seg = (w - dw) / 2f;
            Color roof = wall * .7f; roof.a = 1f;
            Box(root, new Vector3(0, .1f, 0), new Vector3(w, .2f, d), new Color(.3f, .3f, .32f));
            Box(root, new Vector3(0, h / 2f, d / 2f - t / 2f), new Vector3(w, h, t), wall);
            Box(root, new Vector3(-w / 2f + t / 2f, h / 2f, 0), new Vector3(t, h, d), wall);
            Box(root, new Vector3(w / 2f - t / 2f, h / 2f, 0), new Vector3(t, h, d), wall);
            Box(root, new Vector3(-(dw / 2f + seg / 2f), h / 2f, -d / 2f + t / 2f), new Vector3(seg, h, t), wall);
            Box(root, new Vector3((dw / 2f + seg / 2f), h / 2f, -d / 2f + t / 2f), new Vector3(seg, h, t), wall);
            Box(root, new Vector3(0, (h + dh) / 2f, -d / 2f + t / 2f), new Vector3(dw, h - dh, t), wall);
            Box(root, new Vector3(0, h + .1f, 0), new Vector3(w + .4f, .2f, d + .4f), roof);
            if (ramp)
            {
                float run = h / Mathf.Tan(30f * Mathf.Deg2Rad);
                var r = Mats.Prim(PrimitiveType.Cube, "Rampa", root, new Vector3(w / 2f + run / 2f, h / 2f, 0), new Vector3(h * 2f, .25f, 2.4f), new Color(.45f, .45f, .5f));
                r.transform.localRotation = Quaternion.Euler(0, 0, -30f);
                Box(root, new Vector3(0, h + .6f, d / 2f - .1f), new Vector3(w, 1f, .2f), roof); // parapeito para atirador
            }
            root.rotation = Quaternion.Euler(0, 90 * Random.Range(0, 4), 0);
            return root;
        }
    }
}
