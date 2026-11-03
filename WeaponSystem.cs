using UnityEngine;

namespace Alezinfire
{
    public class WeaponSystem : MonoBehaviour
    {
        public static System.Action<bool> HitMarker;
        public WeaponData[] slots = new WeaponData[2];
        public int[] ammo = new int[2];
        public int cur;
        public Health owner;
        public CharacterRig rig;
        float nextFire, reloadEnd;

        public WeaponData Current { get { return slots[cur]; } }
        public bool Reloading { get { return reloadEnd > 0f; } }
        public float ReloadProgress { get { return Current == null || !Reloading ? 0f : 1f - (reloadEnd - Time.time) / Current.reload; } }

        public void Equip(WeaponData w)
        {
            int s = slots[0] == null ? 0 : (slots[1] == null ? 1 : cur);
            slots[s] = w; ammo[s] = w.mag; cur = s; reloadEnd = 0;
            rig.SetWeapon(w);
        }

        public void Switch()
        {
            int n = 1 - cur;
            if (slots[n] == null) return;
            cur = n; reloadEnd = 0; rig.SetWeapon(slots[n]);
        }

        public void Reload()
        {
            if (Current == null || Reloading || ammo[cur] >= Current.mag) return;
            reloadEnd = Time.time + Current.reload;
        }

        void Update()
        {
            if (reloadEnd > 0f && Time.time >= reloadEnd) { ammo[cur] = Current.mag; reloadEnd = 0f; }
        }

        public bool TryFire(Vector3 origin, Vector3 dir, float spreadMul = 1f)
        {
            var w = Current;
            if (w == null || Reloading || Time.time < nextFire) return false;
            if (ammo[cur] <= 0) { Reload(); return false; }
            nextFire = Time.time + 60f / w.rpm;
            ammo[cur]--;
            Sfx.Shot(origin, owner.isPlayer ? 0.7f : 0.35f);
            for (int i = 0; i < w.pellets; i++)
            {
                Vector3 d = (dir + Random.insideUnitSphere * Mathf.Tan(w.spread * spreadMul * Mathf.Deg2Rad)).normalized;
                var hits = Physics.RaycastAll(origin, d, w.range, ~0, QueryTriggerInteraction.Ignore);
                System.Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));
                Vector3 end = origin + d * w.range;
                foreach (var h in hits)
                {
                    if (h.collider.transform.IsChildOf(owner.transform)) continue;
                    end = h.point;
                    var hp = h.collider.GetComponentInParent<Health>();
                    if (hp != null && !hp.dead)
                    {
                        bool head = h.point.y > hp.transform.position.y + 1.55f;
                        hp.Damage(w.damage * (head ? 2f : 1f), owner, head);
                        if (owner.isPlayer && HitMarker != null) HitMarker(head);
                    }
                    break;
                }
                Tracer(rig.gun.TransformPoint(0, -.5f, 0), end);
            }
            return true;
        }

        static void Tracer(Vector3 a, Vector3 b)
        {
            var go = new GameObject("Tracer");
            var lr = go.AddComponent<LineRenderer>();
            lr.positionCount = 2; lr.SetPosition(0, a); lr.SetPosition(1, b);
            lr.startWidth = lr.endWidth = .035f;
            lr.material = Mats.Unlit(new Color(1f, .9f, .4f, .9f));
            Destroy(go, .06f);
        }
    }
}
