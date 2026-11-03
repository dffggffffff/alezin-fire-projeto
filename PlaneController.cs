using UnityEngine;

namespace Alezinfire
{
    public class PlaneController : MonoBehaviour
    {
        public Vector3 start, end;
        public float speed = 55f;
        public bool Finished;
        float prog;

        public static void Model(Transform p)
        {
            Color body = new Color(.95f, .55f, .1f), dark = new Color(.2f, .2f, .25f);
            Mats.Prim(PrimitiveType.Cube, "Fuselagem", p, Vector3.zero, new Vector3(3f, 3f, 14f), body, false);
            Mats.Prim(PrimitiveType.Cube, "Asa", p, new Vector3(0, -.3f, 0), new Vector3(22f, .3f, 3.5f), dark, false);
            Mats.Prim(PrimitiveType.Cube, "Cauda", p, new Vector3(0, 2f, -6.2f), new Vector3(.3f, 3.5f, 2.2f), dark, false);
            Mats.Prim(PrimitiveType.Cube, "CaudaH", p, new Vector3(0, 1.2f, -6.2f), new Vector3(7f, .25f, 1.6f), dark, false);
            Mats.Prim(PrimitiveType.Sphere, "Nariz", p, new Vector3(0, 0, 7f), new Vector3(3f, 3f, 3f), dark, false);
            Mats.Prim(PrimitiveType.Cube, "Cabine", p, new Vector3(0, 1.4f, 3f), new Vector3(2.2f, .8f, 2.5f), new Color(.5f, .8f, 1f), false);
        }

        public static PlaneController Create(Vector3 s, Vector3 e)
        {
            var go = new GameObject("Aviao");
            var pc = go.AddComponent<PlaneController>();
            pc.start = s; pc.end = e;
            Model(go.transform);
            go.transform.position = s;
            go.transform.rotation = Quaternion.LookRotation(e - s);
            return pc;
        }

        void Update()
        {
            prog += speed * Time.deltaTime;
            Vector3 d = end - start;
            transform.position = start + d.normalized * Mathf.Min(prog, d.magnitude);
            if (prog >= d.magnitude) Finished = true;
        }
    }
}
