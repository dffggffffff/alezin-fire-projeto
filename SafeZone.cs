using System.Collections.Generic;
using UnityEngine;

namespace Alezinfire
{
    public class SafeZone : MonoBehaviour
    {
        static readonly float[] R = { 380, 250, 150, 70, 25, 12 };
        static readonly float[] W = { 50, 40, 30, 25, 20, 15 };
        static readonly float[] S = { 40, 35, 30, 25, 20, 15 };
        static readonly float[] D = { 1, 2, 4, 7, 10, 14 };

        public Vector2 center;
        public float radius = 480f;
        public string Status = "";
        int phase; float t, tick; bool shrinking;
        Vector2 fromC, toC; float fromR, toR;
        LineRenderer[] rings;

        void Start()
        {
            center = Random.insideUnitCircle * 40f;
            rings = new LineRenderer[8];
            var mat = Mats.Unlit(new Color(.3f, .65f, 1f, .9f));
            for (int i = 0; i < rings.Length; i++)
            {
                var g = new GameObject("Anel" + i); g.transform.SetParent(transform, false);
                var lr = g.AddComponent<LineRenderer>();
                lr.loop = true; lr.positionCount = 90; lr.useWorldSpace = true;
                lr.startWidth = lr.endWidth = 1.2f; lr.material = mat;
                rings[i] = lr;
            }
        }

        public bool Inside(Vector3 p, float margin = 0f)
        {
            return (new Vector2(p.x, p.z) - center).magnitude <= radius - margin;
        }

        static string Fmt(float s) { s = Mathf.Max(0, s); return ((int)s / 60) + ":" + ((int)s % 60).ToString("00"); }

        void Update()
        {
            float dt = Time.deltaTime; t += dt;
            if (phase < R.Length)
            {
                if (!shrinking)
                {
                    float left = W[phase] - t;
                    Status = "ZONA FECHA EM " + Fmt(left);
                    if (left <= 0)
                    {
                        shrinking = true; t = 0; fromC = center; fromR = radius; toR = R[phase];
                        toC = fromC + Random.insideUnitCircle * Mathf.Max(0, fromR - toR) * .8f;
                    }
                }
                else
                {
                    float k = Mathf.Clamp01(t / S[phase]);
                    radius = Mathf.Lerp(fromR, toR, k); center = Vector2.Lerp(fromC, toC, k);
                    Status = "ZONA ENCOLHENDO";
                    if (k >= 1f) { shrinking = false; t = 0; phase++; }
                }
            }
            else Status = "ZONA FINAL";

            tick += dt;
            if (tick >= 1f)
            {
                tick = 0;
                float dmg = D[Mathf.Min(phase, D.Length - 1)];
                foreach (var h in new List<Health>(Health.All))
                    if (!h.dead && !h.untargetable && !Inside(h.transform.position)) h.Damage(dmg, null);
            }
            for (int i = 0; i < rings.Length; i++)
            {
                float y = 5f + i * 18f;
                for (int k = 0; k < 90; k++)
                {
                    float a = k / 90f * Mathf.PI * 2f;
                    rings[i].SetPosition(k, new Vector3(center.x + Mathf.Cos(a) * radius, y, center.y + Mathf.Sin(a) * radius));
                }
            }
        }
    }
}
