using System.Collections;
using UnityEngine;

namespace Alezinfire
{
    public enum GState { Loading, Lobby, Waiting, Match, End }

    [DefaultExecutionOrder(-100)]
    public class GameManager : MonoBehaviour
    {
        public static GameManager I;
        public GState state;
        public CharacterRig preview;
        Camera cam; ThirdPersonCamera tpc; Light sun;
        LoadingScreen loading; LobbyScreen lobby; WaitingScreen waiting; HUD hud; EndScreen end;
        GameObject lobbyStage, arena; Transform previewHolder;
        PlayerController player; MatchManager match;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Boot()
        {
            if (FindObjectOfType<GameManager>() != null) return;
            var go = new GameObject("Alezinfire");
            DontDestroyOnLoad(go);
            go.AddComponent<GameManager>();
        }

        void Awake()
        {
            I = this;
            Application.targetFrameRate = 60;
            Screen.sleepTimeout = SleepTimeout.NeverSleep;
            QualitySettings.shadowDistance = 60;
            cam = Camera.main;
            if (!cam)
            {
                var g = new GameObject("Main Camera"); g.tag = "MainCamera";
                cam = g.AddComponent<Camera>(); g.AddComponent<AudioListener>();
            }
            cam.nearClipPlane = .1f; cam.farClipPlane = 700f;
            tpc = cam.gameObject.AddComponent<ThirdPersonCamera>(); tpc.enabled = false;
            sun = FindObjectOfType<Light>();
            if (!sun || sun.type != LightType.Directional)
            {
                sun = new GameObject("Sol").AddComponent<Light>(); sun.type = LightType.Directional;
            }
            sun.transform.rotation = Quaternion.Euler(50, -30, 0);
            sun.shadows = LightShadows.Soft; sun.intensity = 1.1f;
            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
            RenderSettings.ambientLight = new Color(.55f, .58f, .62f);
            Physics.gravity = new Vector3(0, -22f, 0);
        }

        void Start() { StartCoroutine(LoadRoutine()); }

        void SetSky(bool lobbyLook)
        {
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = lobbyLook ? new Color(.08f, .06f, .16f) : new Color(.55f, .75f, .95f);
            RenderSettings.fog = !lobbyLook;
            RenderSettings.fogMode = FogMode.Linear;
            RenderSettings.fogColor = new Color(.65f, .8f, .95f);
            RenderSettings.fogStartDistance = 220; RenderSettings.fogEndDistance = 680;
        }

        IEnumerator LoadRoutine()
        {
            state = GState.Loading;
            SetSky(true);
            loading = new LoadingScreen();
            float t0 = Time.time;
            System.Action<float, string> prog = (p, s) => loading.Set(p, s);
            prog(.05f, "Montando o lobby...");
            lobbyStage = Stage.BuildLobby(out previewHolder);
            RebuildPreview();
            yield return null;
            yield return StartCoroutine(Map.Build(prog));
            while (Time.time - t0 < 3f) { loading.Tick(); prog(.99f, "Entrando no lobby..."); yield return null; }
            loading.Destroy(); loading = null;
            GoLobby();
        }

        public void RebuildPreview()
        {
            if (preview) Destroy(preview.gameObject);
            preview = CharacterRig.Build(previewHolder, Config.Outfits[Config.Outfit]);
            preview.SetWeapon(Arsenal.All[2]);
            preview.aiming = false;
        }

        void CleanupMatch()
        {
            if (hud != null) { hud.Destroy(); hud = null; }
            if (end != null) { end.Destroy(); end = null; }
            if (waiting != null) { waiting.Destroy(); waiting = null; }
            if (match) { match.Cleanup(); match = null; }
            if (arena) Destroy(arena);
            if (player) Destroy(player.gameObject);
            tpc.enabled = false;
            Health.All.Clear();
        }

        public void GoLobby()
        {
            CleanupMatch();
            state = GState.Lobby;
            SetSky(true);
            cam.transform.position = Stage.LobbyPos + new Vector3(0, 1.35f, -3.4f);
            cam.transform.rotation = Quaternion.LookRotation(Stage.LobbyPos + new Vector3(0, 1.05f, 0) - cam.transform.position);
            lobbyStage.SetActive(true);
            lobby = new LobbyScreen(this);
        }

        public void GoWaiting()
        {
            if (lobby != null) { lobby.Destroy(); lobby = null; }
            lobbyStage.SetActive(false);
            state = GState.Waiting;
            SetSky(false);
            arena = Stage.BuildArena();
            SpawnPlayer(Stage.ArenaPos + new Vector3(0, .1f, 0));
            waiting = new WaitingScreen(this, arena.transform);
            waiting.tc.onEmote = id => player.DoEmote(id);
        }

        void SpawnPlayer(Vector3 pos)
        {
            var go = new GameObject("Player");
            go.transform.position = pos;
            go.AddComponent<CharacterController>();
            var h = go.AddComponent<Health>();
            h.isPlayer = true; h.team = 1; h.displayName = Config.PlayerName;
            var rig = CharacterRig.Build(go.transform, Config.Outfits[Config.Outfit]);
            h.rig = rig;
            var ws = go.AddComponent<WeaponSystem>();
            ws.owner = h; ws.rig = rig;
            player = go.AddComponent<PlayerController>();
            player.Init(h, rig, ws, tpc);
            tpc.yaw = 0; tpc.pitch = 15;
        }

        void GoMatch()
        {
            waiting.Destroy(); waiting = null;
            if (arena) Destroy(arena);
            state = GState.Match;
            SetSky(false);
            match = gameObject.AddComponent<MatchManager>();
            match.Begin(player);
            match.OnEnded = (won, rank) =>
            {
                state = GState.End;
                if (hud != null) { hud.Destroy(); hud = null; }
                end = new EndScreen(this, won, rank, player.health.kills);
            };
            hud = new HUD(match);
        }

        void Update()
        {
            InputHub.Poll();
            switch (state)
            {
                case GState.Loading: if (loading != null) loading.Tick(); break;
                case GState.Lobby:
                    if (previewHolder) previewHolder.localRotation = Quaternion.Euler(0, 180 + Mathf.Sin(Time.time * .6f) * 30f, 0);
                    break;
                case GState.Waiting:
                    if (waiting != null) { waiting.Tick(); if (waiting.Done) GoMatch(); }
                    break;
                case GState.Match: if (hud != null) hud.Tick(); break;
            }
        }
    }
}
