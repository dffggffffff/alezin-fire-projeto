using UnityEngine;

namespace Alezinfire
{
    /// Cenários isolados: palco do lobby e arena de espera do avião.
    public static class Stage
    {
        public static readonly Vector3 LobbyPos = new Vector3(5000, 0, 5000);
        public static readonly Vector3 ArenaPos = new Vector3(6000, 0, 6000);

        public static GameObject BuildLobby(out Transform previewHolder)
        {
            var root = new GameObject("LobbyStage");
            root.transform.position = LobbyPos;
            Mats.Prim(PrimitiveType.Cylinder, "Palco", root.transform, new Vector3(0, -.05f, 0), new Vector3(5f, .05f, 5f), new Color(.12f, .12f, .18f));
            var ring = Mats.Prim(PrimitiveType.Cylinder, "Anel", root.transform, new Vector3(0, .0f, 0), new Vector3(5.3f, .03f, 5.3f), Color.white, false);
            ring.GetComponent<Renderer>().sharedMaterial = Mats.Unlit(new Color(.1f, .9f, 1f));
            Mats.Prim(PrimitiveType.Cube, "Fundo", root.transform, new Vector3(0, 4f, 6f), new Vector3(16f, 9f, .4f), new Color(.25f, .1f, .4f), false);
            Mats.Prim(PrimitiveType.Cube, "Faixa", root.transform, new Vector3(0, 7.5f, 5.7f), new Vector3(10f, .4f, .2f), new Color(1f, .5f, .1f), false);
            var l1 = new GameObject("L1").AddComponent<Light>(); l1.type = LightType.Point; l1.color = new Color(.3f, .8f, 1f); l1.range = 14; l1.intensity = 2f;
            l1.transform.SetParent(root.transform, false); l1.transform.localPosition = new Vector3(-3, 3, -2);
            var l2 = new GameObject("L2").AddComponent<Light>(); l2.type = LightType.Point; l2.color = new Color(1f, .5f, .8f); l2.range = 14; l2.intensity = 2f;
            l2.transform.SetParent(root.transform, false); l2.transform.localPosition = new Vector3(3, 3, -2);
            var h = new GameObject("Preview"); h.transform.SetParent(root.transform, false);
            previewHolder = h.transform;
            return root;
        }

        public static GameObject BuildArena()
        {
            var root = new GameObject("WaitingArena");
            root.transform.position = ArenaPos;
            Color grass = new Color(.3f, .55f, .3f);
            Mats.Prim(PrimitiveType.Cube, "Chao", root.transform, new Vector3(0, -.5f, 0), new Vector3(80, 1, 80), grass);
            Color fence = new Color(.5f, .35f, .2f);
            Mats.Prim(PrimitiveType.Cube, "CercaN", root.transform, new Vector3(0, 1.5f, 40), new Vector3(80, 3, .5f), fence);
            Mats.Prim(PrimitiveType.Cube, "CercaS", root.transform, new Vector3(0, 1.5f, -40), new Vector3(80, 3, .5f), fence);
            Mats.Prim(PrimitiveType.Cube, "CercaL", root.transform, new Vector3(40, 1.5f, 0), new Vector3(.5f, 3, 80), fence);
            Mats.Prim(PrimitiveType.Cube, "CercaO", root.transform, new Vector3(-40, 1.5f, 0), new Vector3(.5f, 3, 80), fence);
            var plane = new GameObject("AviaoParado"); plane.transform.SetParent(root.transform, false);
            plane.transform.localPosition = new Vector3(0, 4f, 22f);
            plane.transform.localRotation = Quaternion.Euler(0, 90, 0);
            PlaneController.Model(plane.transform);
            for (int i = 0; i < 4; i++)
            {
                float a = i * 90f + 45f;
                var p = Quaternion.Euler(0, a, 0) * new Vector3(0, 0, 18f);
                Mats.Prim(PrimitiveType.Cube, "Tenda", root.transform, p + new Vector3(0, 1.25f, 0), new Vector3(5, 2.5f, 5), Color.HSVToRGB(i / 4f, .5f, .8f));
            }
            Mats.Prim(PrimitiveType.Cylinder, "Fogueira", root.transform, new Vector3(0, .2f, 0), new Vector3(1.2f, .2f, 1.2f), new Color(.9f, .4f, .1f), false);
            return root;
        }
    }
}
