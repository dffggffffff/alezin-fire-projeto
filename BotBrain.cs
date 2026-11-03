using UnityEngine;

namespace Alezinfire
{
    [RequireComponent(typeof(CharacterController))]
    public class BotBrain : MonoBehaviour
    {
        public Health health;
        public CharacterRig rig;
        public WeaponSystem weapons;
        public bool ally, dropped;
        public float dropTime;
        CharacterController cc;
        bool falling;
        Vector3 landing, wp, lastPos;
        float vy, lootAt, nextWp, nextScan, seenAt, stuckT, seed;
        Health target;
        GameObject canopy;

        public static BotBrain Create(string name, int team, bool ally, float dropTime, Transform parent)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var cc = go.AddComponent<CharacterController>();
            cc.height = 1.8f; cc.radius = .3f; cc.center = new Vector3(0, .9f, 0); cc.stepOffset = .4f; cc.slopeLimit = 50f;
            var h = go.AddComponent<Health>();
            h.team = team; h.displayName = name;
            var rig = CharacterRig.Build(go.transform, Config.Outfits[Random.Range(0, Config.Outfits.Length)]);
            h.rig = rig;
            var ws = go.AddComponent<WeaponSystem>();
            ws.owner = h; ws.rig = rig;
            var b = go.AddComponent<BotBrain>();
            b.health = h; b.rig = rig; b.weapons = ws; b.ally = ally; b.dropTime = dropTime; b.cc = cc; b.seed = Random.value * 100f;
            go.SetActive(false);
            return b;
        }

        public void Drop(Vector3 from, Vector3? landAt = null)
        {
            transform.position = from;
            gameObject.SetActive(true);
            dropped = true; falling = true; cc.enabled = false; health.untargetable = true;
            if (landAt.HasValue) landing = landAt.Value;
            else
            {
                float best = float.MaxValue; landing = Map.RandomLand();
                for (int i = 0; i < 8; i++)
                {
                    var c = Map.RandomLand();
                    float d = (new Vector2(c.x - from.x, c.z - from.z)).magnitude;
                    if (d < best) { best = d; landing = c; }
                }
            }
            canopy = Mats.Prim(PrimitiveType.Sphere, "Canopy", transform, new Vector3(0, 3.4f, 0), new Vector3(4f, 1.2f, 4f), new Color(Random.value, Random.value, Random.value), false);
        }

        static Vector3 Flat(Vector3 v) { v.y = 0; return v; }

        void Update()
        {
            float dt = Time.deltaTime;
            if (health.dead) return;
            if (falling)
            {
                Vector3 p = transform.position;
                Vector3 to = Flat(landing - p);
                p += to.normalized * Mathf.Min(to.magnitude, 26f * dt);
                p.y -= 18f * dt;
                float gy = Map.Height(p.x, p.z);
                if (p.y <= gy + .2f)
                {
                    p.y = gy + .1f; falling = false; cc.enabled = true; health.untargetable = false;
                    if (canopy) Destroy(canopy);
                    lootAt = Time.time + Random.Range(3f, 8f);
                }
                transform.position = p;
                return;
            }
            if (weapons.Current == null && Time.time >= lootAt && lootAt > 0)
                weapons.Equip(Arsenal.All[Random.Range(0, Arsenal.All.Length)]);

            if (Time.time >= nextScan) { nextScan = Time.time + .4f; Scan(); }

            Vector3 pos = transform.position, move = Vector3.zero;
            float speed = 5f;
            rig.aiming = false;
            if (target != null && !target.dead)
            {
                Vector3 to = Flat(target.transform.position - pos);
                float dist = to.magnitude;
                Vector3 dir = to / Mathf.Max(dist, .01f);
                Face(dir, dt);
                Vector3 side = Vector3.Cross(Vector3.up, dir);
                if (dist > 22f) move = dir; else if (dist < 9f) move = -dir * .8f;
                move += side * Mathf.Sin(Time.time * .8f + seed) * .6f;
                TryShoot(dist);
            }
            else
            {
                if (Time.time >= nextWp || Flat(wp - pos).sqrMagnitude < 9f) PickWp();
                Vector3 to = Flat(wp - pos);
                if (to.magnitude > 1f) { move = to.normalized; Face(move, dt); }
            }

            if (cc.isGrounded) { vy = -2f; } else vy -= 22f * dt;
            stuckT += dt;
            if (stuckT > 2f)
            {
                if (move.sqrMagnitude > .1f && Flat(pos - lastPos).magnitude < .6f) { PickWp(); if (cc.isGrounded) vy = 6.5f; }
                lastPos = pos; stuckT = 0;
            }
            cc.Move((move.normalized * speed * Mathf.Min(1f, move.magnitude) + Vector3.up * vy) * dt);
            rig.speed = Flat(cc.velocity).magnitude;
        }

        void Face(Vector3 dir, float dt)
        {
            if (dir.sqrMagnitude < .001f) return;
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(dir), 10f * dt);
        }

        void PickWp()
        {
            nextWp = Time.time + Random.Range(6f, 12f);
            var m = MatchManager.I;
            Vector3 pos = transform.position;
            if (ally && m != null && m.player != null && !m.player.health.dead)
            {
                var o = Random.insideUnitCircle * 6f;
                wp = m.player.transform.position + new Vector3(o.x, 0, o.y);
                return;
            }
            if (m != null && m.zone != null && !m.zone.Inside(pos, 25f))
            {
                wp = new Vector3(m.zone.center.x, pos.y, m.zone.center.y);
                return;
            }
            var r = Random.insideUnitCircle * 60f;
            wp = pos + new Vector3(r.x, 0, r.y);
        }

        void Scan()
        {
            Health best = null; float bd = 3600f;
            foreach (var h in Health.All)
            {
                if (h.dead || h.untargetable || h == health || h.team == health.team) continue;
                float d = (h.transform.position - transform.position).sqrMagnitude;
                if (d < bd) { bd = d; best = h; }
            }
            if (best != target) { target = best; seenAt = Time.time; }
        }

        void TryShoot(float dist)
        {
            if (weapons.Current == null || Time.time - seenAt < .6f) return;
            Vector3 origin = transform.position + Vector3.up * 1.5f;
            Vector3 tp = target.transform.position + Vector3.up * 1.2f;
            RaycastHit hit;
            if (Physics.Linecast(origin, tp, out hit, ~0, QueryTriggerInteraction.Ignore) && hit.collider.GetComponentInParent<Health>() != target) return;
            Vector3 dir = (tp - origin).normalized;
            rig.aiming = true;
            rig.armR.localRotation = Quaternion.Euler(-85, 0, 0);
            weapons.TryFire(origin + dir * .5f, dir, 1.6f + dist / 35f);
        }
    }
}
