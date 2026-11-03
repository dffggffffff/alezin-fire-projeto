using System.Collections.Generic;
using UnityEngine;

namespace Alezinfire
{
    public class MatchManager : MonoBehaviour
    {
        public static MatchManager I;
        public PlayerController player;
        public PlaneController plane;
        public SafeZone zone;
        public List<BotBrain> bots = new List<BotBrain>();
        public float time;
        public int Alive;
        public bool over, won;
        public int rank;
        public System.Action<bool, int> OnEnded;
        Transform holder;
        float endAt = -1f;

        static readonly string[] Names = { "Zeca", "Mira", "Tigrão", "Nina", "Bruxo", "Lobo", "Faísca", "Duda", "Rex", "Sombra", "Kiko", "Vega", "Otto", "Jade", "Pantera", "Neo", "Cacau", "Trovão", "Luna", "Rocha", "Ágata", "Flecha", "Mel", "Bóris", "Cobra", "Ravi", "Tati", "Zeus" };

        public void Begin(PlayerController p)
        {
            I = this; player = p;
            holder = new GameObject("MatchObjects").transform;
            float a = Random.value * Mathf.PI * 2f;
            Vector3 dir = new Vector3(Mathf.Cos(a), 0, Mathf.Sin(a));
            Vector3 lat = Vector3.Cross(Vector3.up, dir) * Random.Range(-150f, 150f);
            plane = PlaneController.Create(-dir * 640f + lat + Vector3.up * 260f, dir * 640f + lat + Vector3.up * 260f);
            plane.transform.SetParent(holder, true);

            int mode = (int)Config.Mode;
            for (int i = 0; i < mode - 1; i++)
            {
                string n = i < Config.Squad.Count ? Config.Squad[i] : "Aliado " + (i + 1);
                var b = BotBrain.Create(n, 1, true, 9999f, holder);
                bots.Add(b);
            }
            int enemies = Config.TotalPlayers - mode;
            for (int i = 0; i < enemies; i++)
            {
                var b = BotBrain.Create(Names[i % Names.Length] + (i >= Names.Length ? i.ToString() : ""), 2 + i / mode, false, Random.Range(4f, 20f), holder);
                bots.Add(b);
            }
            foreach (var b in bots) { var bb = b; bb.health.OnDied += (v, k) => Destroy(bb.gameObject, 25f); }

            zone = new GameObject("Zona").AddComponent<SafeZone>();
            zone.transform.SetParent(holder, false);

            // loot
            foreach (var s in Map.LootSpots)
            {
                float r = Random.value;
                if (r < .6f) WeaponPickup.Spawn(s, PickupKind.Weapon, Random.Range(0, Arsenal.All.Length), holder);
                else if (r < .85f) WeaponPickup.Spawn(s, PickupKind.Medkit, 0, holder);
                else WeaponPickup.Spawn(s, PickupKind.Armor, 0, holder);
            }
            player.EnterPlane();
        }

        void Update()
        {
            if (over || player == null) return;
            time += Time.deltaTime;
            foreach (var b in bots)
            {
                if (b == null || b.dropped) continue;
                if (b.ally)
                {
                    if (player.mode != PMode.Plane)
                    {
                        var o = Random.insideUnitCircle * 6f;
                        var land = player.transform.position + new Vector3(o.x, 0, o.y);
                        land.y = Map.Height(land.x, land.z);
                        b.Drop(player.transform.position + new Vector3(o.x, 2, o.y), land);
                    }
                }
                else if (time >= b.dropTime)
                    b.Drop(plane.transform.position + new Vector3(Random.Range(-8f, 8f), -3f, Random.Range(-8f, 8f)));
            }

            int enemies = 0, alive = player.health.dead ? 0 : 1;
            foreach (var b in bots)
            {
                if (b == null || b.health.dead) continue;
                alive++;
                if (b.health.team != 1) enemies++;
            }
            Alive = alive;

            if (endAt < 0f)
            {
                if (player.health.dead) { endAt = Time.time + 3f; rank = Alive + 1; won = false; }
                else if (enemies == 0 && time > 10f) { endAt = Time.time + 2f; rank = 1; won = true; }
            }
            else if (Time.time >= endAt)
            {
                over = true;
                if (OnEnded != null) OnEnded(won, rank);
            }
        }

        public void Cleanup()
        {
            if (holder) Destroy(holder.gameObject);
            I = null;
            Destroy(this);
        }
    }
}
