using System.Collections.Generic;
using UnityEngine;

namespace Alezinfire
{
    public class Health : MonoBehaviour
    {
        public static List<Health> All = new List<Health>();
        public static System.Action<string, string> KillFeed;

        public float hp = 100f, armor = 0f;
        public int team, kills;
        public string displayName = "Jogador";
        public bool isPlayer, dead, untargetable;
        public CharacterRig rig;
        public System.Action<Health, Health> OnDied;
        public System.Action<float> OnDamaged;

        void OnEnable() { if (!All.Contains(this)) All.Add(this); }
        void OnDisable() { All.Remove(this); }

        public void Damage(float amt, Health from, bool head = false)
        {
            if (dead) return;
            if (from != null && from != this && from.team == team) return; // sem fogo amigo
            if (armor > 0f) { float a = Mathf.Min(armor, amt * .5f); armor -= a; amt -= a; }
            hp -= amt;
            if (OnDamaged != null) OnDamaged(amt);
            if (hp <= 0f)
            {
                hp = 0f; dead = true;
                if (from != null && from != this) from.kills++;
                if (KillFeed != null) KillFeed(from != null ? from.displayName : "Zona/Água", displayName);
                Vector3 dir = from != null ? (transform.position - from.transform.position).normalized : Vector3.up;
                if (rig) rig.Ragdoll((dir + Vector3.up * .5f) * 6f);
                if (OnDied != null) OnDied(this, from);
            }
        }

        public void Heal(float a) { if (!dead) hp = Mathf.Min(100f, hp + a); }
    }
}
