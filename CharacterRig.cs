using System.Collections.Generic;
using UnityEngine;

namespace Alezinfire
{
    /// Personagem procedural (sem assets externos) com animação por código e ragdoll físico ao morrer.
    public class CharacterRig : MonoBehaviour
    {
        public Transform body, torso, head, armL, armR, legL, legR, gun;
        public float speed;
        public bool aiming, crouching;
        public int emote = -1;
        float emoteTime, t;
        bool ragdolled;

        static Transform Pivot(Transform p, string n, Vector3 pos)
        {
            var g = new GameObject(n);
            g.transform.SetParent(p, false);
            g.transform.localPosition = pos;
            return g.transform;
        }

        public static CharacterRig Build(Transform parent, Config.OutfitData o)
        {
            var go = new GameObject("Rig");
            go.transform.SetParent(parent, false);
            var r = go.AddComponent<CharacterRig>();
            r.body = Pivot(go.transform, "Body", Vector3.zero);
            var b = r.body;
            r.torso = Mats.Prim(PrimitiveType.Cube, "Torso", b, new Vector3(0, 1.25f, 0), new Vector3(.5f, .6f, .28f), o.shirt, false).transform;
            r.head = Mats.Prim(PrimitiveType.Sphere, "Head", b, new Vector3(0, 1.72f, 0), new Vector3(.3f, .32f, .3f), o.skin, false).transform;
            Mats.Prim(PrimitiveType.Sphere, "Cap", r.head, new Vector3(0, .3f, 0), new Vector3(1.1f, .55f, 1.1f), o.pants, false);
            r.armL = Pivot(b, "ArmL", new Vector3(-.34f, 1.5f, 0));
            Mats.Prim(PrimitiveType.Capsule, "Arm", r.armL, new Vector3(0, -.28f, 0), new Vector3(.13f, .28f, .13f), o.skin, false);
            r.armR = Pivot(b, "ArmR", new Vector3(.34f, 1.5f, 0));
            Mats.Prim(PrimitiveType.Capsule, "Arm", r.armR, new Vector3(0, -.28f, 0), new Vector3(.13f, .28f, .13f), o.skin, false);
            r.legL = Pivot(b, "LegL", new Vector3(-.13f, .95f, 0));
            Mats.Prim(PrimitiveType.Capsule, "Leg", r.legL, new Vector3(0, -.45f, 0), new Vector3(.17f, .45f, .17f), o.pants, false);
            r.legR = Pivot(b, "LegR", new Vector3(.13f, .95f, 0));
            Mats.Prim(PrimitiveType.Capsule, "Leg", r.legR, new Vector3(0, -.45f, 0), new Vector3(.17f, .45f, .17f), o.pants, false);
            r.gun = Mats.Prim(PrimitiveType.Cube, "Gun", r.armR, new Vector3(0, -.6f, 0), new Vector3(.08f, .5f, .1f), Color.gray, false).transform;
            r.gun.gameObject.SetActive(false);
            return r;
        }

        public void SetWeapon(WeaponData w)
        {
            if (w == null) { gun.gameObject.SetActive(false); return; }
            gun.gameObject.SetActive(true);
            gun.localScale = new Vector3(.08f, w.length, .1f);
            gun.localPosition = new Vector3(0, -.45f - w.length * .3f, 0);
            gun.GetComponent<Renderer>().sharedMaterial = Mats.Lit(w.color);
        }

        public void PlayEmote(int i) { emote = i; emoteTime = 3f; }

        void Update()
        {
            float dt = Time.deltaTime;
            if (emote >= 0) { emoteTime -= dt; if (emoteTime <= 0) emote = -1; }
            float s = Mathf.Clamp01(speed / 6f);
            t += dt * (4f + speed * 1.4f);
            float sw = Mathf.Sin(t) * s * 50f;
            float targetY = crouching ? .7f : 1f;
            body.localScale = new Vector3(1, Mathf.Lerp(body.localScale.y, targetY, 12 * dt), 1);

            if (emote >= 0)
            {
                float e = Time.time * 6f;
                body.localPosition = Vector3.zero; body.localScale = Vector3.one;
                legL.localRotation = legR.localRotation = Quaternion.identity;
                armL.localRotation = armR.localRotation = Quaternion.identity;
                switch (emote)
                {
                    case 0: // dançar
                        armL.localRotation = Quaternion.Euler(0, 0, -(140 + Mathf.Sin(e) * 30));
                        armR.localRotation = Quaternion.Euler(0, 0, 140 + Mathf.Sin(e + 3) * 30);
                        legL.localRotation = Quaternion.Euler(Mathf.Sin(e) * 35, 0, 0);
                        legR.localRotation = Quaternion.Euler(-Mathf.Sin(e) * 35, 0, 0);
                        body.localPosition = new Vector3(0, Mathf.Abs(Mathf.Sin(e)) * .12f, 0);
                        body.localRotation = Quaternion.Euler(0, Mathf.Sin(e * .5f) * 25, 0);
                        break;
                    case 1: // acenar
                        armR.localRotation = Quaternion.Euler(0, 0, 150 + Mathf.Sin(e * 1.5f) * 20);
                        body.localRotation = Quaternion.identity;
                        break;
                    case 2: // músculo
                        armL.localRotation = Quaternion.Euler(0, 0, -95);
                        armR.localRotation = Quaternion.Euler(0, 0, 95);
                        body.localScale = Vector3.one * (1f + Mathf.Sin(e) * .05f);
                        body.localRotation = Quaternion.identity;
                        break;
                    case 3: // girar
                        armL.localRotation = Quaternion.Euler(0, 0, -80);
                        armR.localRotation = Quaternion.Euler(0, 0, 80);
                        body.Rotate(0, 420 * dt, 0);
                        break;
                    default: // comemorar
                        armL.localRotation = Quaternion.Euler(0, 0, -165 + Mathf.Sin(e * 2) * 10);
                        armR.localRotation = Quaternion.Euler(0, 0, 165 - Mathf.Sin(e * 2) * 10);
                        body.localPosition = new Vector3(0, Mathf.Abs(Mathf.Sin(e * .8f)) * .35f, 0);
                        body.localRotation = Quaternion.identity;
                        break;
                }
                return;
            }
            body.localPosition = Vector3.zero;
            body.localRotation = Quaternion.Slerp(body.localRotation, Quaternion.identity, 10 * dt);
            legL.localRotation = Quaternion.Euler(sw, 0, 0);
            legR.localRotation = Quaternion.Euler(-sw, 0, 0);
            armL.localRotation = Quaternion.Euler(-sw * .8f, 0, 0);
            armR.localRotation = (aiming && gun.gameObject.activeSelf) ? Quaternion.Euler(-85, 0, 0) : Quaternion.Euler(sw * .8f, 0, 0);
        }

        /// Ragdoll com física real: cada parte vira Rigidbody ligado ao tronco por CharacterJoint.
        public void Ragdoll(Vector3 force)
        {
            if (ragdolled) return;
            ragdolled = true; enabled = false; emote = -1;
            var cc = GetComponentInParent<CharacterController>();
            if (cc) cc.enabled = false;
            gun.gameObject.SetActive(false);
            body.localScale = Vector3.one;
            var parts = new List<Transform>();
            foreach (var mr in GetComponentsInChildren<MeshRenderer>())
                if (mr.transform != gun && mr.name != "Cap") parts.Add(mr.transform);
            Rigidbody torsoRb = null;
            foreach (var p in parts)
            {
                p.gameObject.AddComponent<BoxCollider>();
                var rb = p.gameObject.AddComponent<Rigidbody>();
                rb.mass = p == torso ? 4f : 1f;
                if (p == torso) torsoRb = rb;
            }
            foreach (var p in parts)
            {
                if (p == torso) continue;
                var j = p.gameObject.AddComponent<CharacterJoint>();
                j.connectedBody = torsoRb;
                j.enableProjection = true;
            }
            var holder = new GameObject("Ragdoll").transform;
            foreach (var p in parts) p.SetParent(holder, true);
            if (torsoRb) torsoRb.AddForce(force, ForceMode.Impulse);
            Destroy(holder.gameObject, 25f);
        }
    }
}
