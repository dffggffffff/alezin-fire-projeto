using System.Collections.Generic;
using UnityEngine;

namespace Alezinfire
{
    public enum PickupKind { Weapon, Medkit, Armor }

    public class WeaponPickup : MonoBehaviour
    {
        public static List<WeaponPickup> All = new List<WeaponPickup>();
        public PickupKind kind;
        public int weaponIndex;
        float baseY;

        public string Label
        {
            get
            {
                if (kind == PickupKind.Weapon) return Arsenal.All[weaponIndex].name;
                return kind == PickupKind.Medkit ? "Kit Médico" : "Colete";
            }
        }

        void OnEnable() { All.Add(this); }
        void OnDisable() { All.Remove(this); }

        public static WeaponPickup Spawn(Vector3 pos, PickupKind k, int w, Transform parent)
        {
            var g = new GameObject("Pickup_" + k);
            g.transform.SetParent(parent, false);
            g.transform.position = pos + Vector3.up * .7f;
            var p = g.AddComponent<WeaponPickup>();
            p.kind = k; p.weaponIndex = w; p.baseY = g.transform.position.y;
            Color c = k == PickupKind.Weapon ? Arsenal.All[w].color + new Color(.25f, .25f, .25f) : (k == PickupKind.Medkit ? new Color(.9f, .2f, .2f) : new Color(.2f, .5f, 1f));
            Mats.Prim(PrimitiveType.Cube, "Item", g.transform, Vector3.zero, new Vector3(.7f, .2f, .25f), c, false);
            var beam = Mats.Prim(PrimitiveType.Cylinder, "Beam", g.transform, new Vector3(0, 1.5f, 0), new Vector3(.06f, 1.5f, .06f), Color.white, false);
            beam.GetComponent<Renderer>().sharedMaterial = Mats.Unlit(new Color(1f, .95f, .4f, .6f));
            return p;
        }

        void Update()
        {
            transform.Rotate(0, 90 * Time.deltaTime, 0);
            var p = transform.position; p.y = baseY + Mathf.Sin(Time.time * 2f) * .1f; transform.position = p;
        }

        public void Give(Health h, WeaponSystem ws)
        {
            if (kind == PickupKind.Weapon) ws.Equip(Arsenal.All[weaponIndex]);
            else if (kind == PickupKind.Medkit) h.Heal(50f);
            else h.armor = Mathf.Min(100f, h.armor + 50f);
            Destroy(gameObject);
        }
    }
}
