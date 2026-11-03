using UnityEngine;

namespace Alezinfire
{
    public enum PMode { Ground, Plane, Freefall, Glide, Dead }

    [RequireComponent(typeof(CharacterController))]
    public class PlayerController : MonoBehaviour
    {
        public PMode mode = PMode.Ground;
        public Health health;
        public CharacterRig rig;
        public WeaponSystem weapons;
        public ThirdPersonCamera cam;
        public WeaponPickup NearPickup;
        public bool Crouching { get { return crouch; } }

        CharacterController cc;
        Vector3 hv;
        float vy;
        bool crouch;
        GameObject canopy, board;

        void Awake()
        {
            cc = GetComponent<CharacterController>();
            cc.height = 1.8f; cc.radius = .3f; cc.center = new Vector3(0, .9f, 0);
            cc.stepOffset = .4f; cc.slopeLimit = 50f; cc.skinWidth = .04f;
        }

        public void Init(Health h, CharacterRig r, WeaponSystem w, ThirdPersonCamera c)
        {
            health = h; rig = r; weapons = w; cam = c;
            cam.target = transform; cam.distance = 4.2f; cam.enabled = true;
            health.OnDied += (v, k) => { mode = PMode.Dead; cam.target = rig.torso; };
        }

        public void EnterPlane()
        {
            mode = PMode.Plane; cc.enabled = false; rig.gameObject.SetActive(false);
            health.untargetable = true; cam.distance = 16f; hv = Vector3.zero; vy = 0;
        }

        void JumpFromPlane()
        {
            var pl = MatchManager.I.plane;
            cc.enabled = true; rig.gameObject.SetActive(true);
            health.untargetable = false; cam.distance = 5f;
            hv = pl.transform.forward * 20f; vy = 0; mode = PMode.Freefall;
        }

        void OpenGlider()
        {
            mode = PMode.Glide;
            rig.transform.localRotation = Quaternion.identity; rig.transform.localPosition = Vector3.zero;
            canopy = Mats.Prim(PrimitiveType.Sphere, "Canopy", transform, new Vector3(0, 3.6f, 0), new Vector3(4.5f, 1.3f, 4.5f), new Color(1f, .45f, .1f), false);
            board = Mats.Prim(PrimitiveType.Cube, "Prancha", transform, new Vector3(0, -.05f, 0), new Vector3(1.4f, .08f, .55f), new Color(.1f, .9f, 1f), false);
            Mats.Prim(PrimitiveType.Cylinder, "Corda", canopy.transform, new Vector3(0, -2.1f, 0), new Vector3(.01f, 1.5f, .01f), Color.white, false);
            cam.distance = 6f;
        }

        void Land()
        {
            mode = PMode.Ground; vy = 0; hv *= .3f;
            if (canopy) Destroy(canopy);
            if (board) Destroy(board);
            rig.transform.localRotation = Quaternion.identity; rig.transform.localPosition = Vector3.zero;
            cam.distance = 4.2f;
        }

        static Vector3 Flat(Vector3 v) { v.y = 0; return v.normalized; }

        Vector3 InputDir()
        {
            Vector3 f = Flat(cam.transform.forward), r = Flat(cam.transform.right);
            return Vector3.ClampMagnitude(f * InputHub.Move.y + r * InputHub.Move.x, 1f);
        }

        void Face(float yaw, float speed)
        {
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.Euler(0, yaw, 0), speed * Time.deltaTime);
        }

        public void DoEmote(int i) { if (mode == PMode.Ground && cc.isGrounded) rig.PlayEmote(i); }

        void Update()
        {
            float dt = Time.deltaTime;
            if (health.dead) { mode = PMode.Dead; return; }
            switch (mode)
            {
                case PMode.Plane:
                    {
                        var pl = MatchManager.I.plane;
                        transform.position = pl.transform.position + Vector3.down * 3f;
                        if (InputHub.JumpDown || pl.Finished) JumpFromPlane();
                        break;
                    }
                case PMode.Freefall:
                    {
                        hv = Vector3.MoveTowards(hv, InputDir() * 28f, 25f * dt);
                        vy = Mathf.MoveTowards(vy, -50f, 25f * dt);
                        cc.Move((hv + Vector3.up * vy) * dt);
                        rig.transform.localRotation = Quaternion.Euler(80, 0, 0);
                        rig.transform.localPosition = new Vector3(0, .9f, -.9f);
                        Face(cam.yaw, 8f);
                        float h = transform.position.y - Map.Height(transform.position.x, transform.position.z);
                        if (InputHub.JumpDown || h < 120f) OpenGlider();
                        else if (cc.isGrounded) Land();
                        break;
                    }
                case PMode.Glide:
                    {
                        Vector3 d = InputDir();
                        if (d.sqrMagnitude < .01f) d = Flat(cam.transform.forward) * .5f;
                        hv = Vector3.MoveTowards(hv, d * 16f, 12f * dt);
                        vy = Mathf.MoveTowards(vy, -7f, 20f * dt);
                        cc.Move((hv + Vector3.up * vy) * dt);
                        Face(cam.yaw, 6f);
                        if (cc.isGrounded) Land();
                        break;
                    }
                case PMode.Ground:
                    GroundUpdate(dt);
                    break;
            }
        }

        void GroundUpdate(float dt)
        {
            if (InputHub.CrouchDown) { crouch = !crouch; cc.height = crouch ? 1.1f : 1.8f; cc.center = new Vector3(0, crouch ? .55f : .9f, 0); }
            Vector3 d = InputDir();
            float spd = crouch ? 2.8f : (InputHub.Move.magnitude > .9f ? 7.2f : 4.8f);
            hv = Vector3.MoveTowards(hv, d * spd, 40f * dt);
            if (cc.isGrounded)
            {
                if (vy < -20f) health.Damage((-vy - 20f) * 4f, null);
                vy = -2f;
                if (InputHub.JumpDown && !crouch) vy = 7.5f;
            }
            else vy -= 22f * dt;
            cc.Move((hv + Vector3.up * vy) * dt);

            bool firing = InputHub.Fire && weapons.Current != null;
            if (firing) Face(cam.yaw, 14f);
            else if (hv.sqrMagnitude > .5f) Face(Mathf.Atan2(hv.x, hv.z) * Mathf.Rad2Deg, 12f);
            rig.speed = new Vector3(hv.x, 0, hv.z).magnitude;
            rig.aiming = weapons.Current != null;
            rig.crouching = crouch;
            if (rig.emote >= 0 && (hv.sqrMagnitude > .5f || InputHub.Fire || InputHub.JumpDown)) rig.emote = -1;

            if (firing)
            {
                Vector3 o = cam.transform.position + cam.transform.forward * (cam.distance + .5f);
                if (weapons.TryFire(o, cam.transform.forward, crouch ? .6f : 1f)) cam.Kick(weapons.Current.damage > 50 ? 3f : .5f);
            }
            if (InputHub.ReloadDown) weapons.Reload();
            if (InputHub.SwapDown) weapons.Switch();

            NearPickup = null; float best = 2.4f;
            foreach (var p in WeaponPickup.All)
            {
                float dist = Vector3.Distance(p.transform.position, transform.position + Vector3.up);
                if (dist < best) { best = dist; NearPickup = p; }
            }
            if (InputHub.PickupDown && NearPickup) NearPickup.Give(health, weapons);

            if (transform.position.y < Config.WaterY - .6f) health.Damage(8f * dt, null); // afogando
        }

        void OnControllerColliderHit(ControllerColliderHit h)
        {
            var rb = h.collider.attachedRigidbody;
            if (rb && !rb.isKinematic && h.moveDirection.y > -.3f)
                rb.velocity = new Vector3(h.moveDirection.x, rb.velocity.y, h.moveDirection.z) * Mathf.Max(2f, cc.velocity.magnitude) * .7f;
        }
    }
}
