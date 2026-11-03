using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Alezinfire
{
    public class LoadingScreen
    {
        Canvas canvas; RectTransform fill; Text pct, msg, tip; float tipT;
        static readonly string[] Tips = {
            "Abra a prancha planadora antes de chegar ao chão!",
            "Fique dentro da zona azul para não perder vida.",
            "Tiros na cabeça causam o dobro de dano.",
            "Caixas e barris têm física: empurre-os para se cobrir.",
            "Cada cidade da ilha Bermuda tem loot diferente.",
            "Use emotes para provocar... ou comemorar!" };

        public LoadingScreen()
        {
            canvas = UIKit.MakeCanvas("Carregando", 20);
            var r = canvas.transform;
            UIKit.Img(r, "Fundo", new Color(.06f, .05f, .12f), Vector2.zero, Vector2.one);
            UIKit.Img(r, "Faixa", new Color(1f, .45f, .1f, .25f), new Vector2(0, .55f), new Vector2(1, .75f));
            UIKit.Txt(r, "ALEZINFIRE", 150, new Color(1f, .6f, .15f), new Vector2(0, .5f), new Vector2(1, .8f));
            UIKit.Txt(r, "BATTLE ROYALE", 44, Color.white, new Vector2(0, .44f), new Vector2(1, .52f));
            UIKit.Img(r, "BarraFundo", new Color(1, 1, 1, .15f), new Vector2(.15f, .15f), new Vector2(.85f, .18f));
            var f = UIKit.Img(r, "BarraFill", new Color(1f, .6f, .1f), new Vector2(.15f, .15f), new Vector2(.15f, .18f));
            fill = f.rectTransform;
            pct = UIKit.Txt(r, "0%", 36, Color.white, new Vector2(.85f, .15f), new Vector2(.95f, .18f));
            msg = UIKit.Txt(r, "", 34, Color.white, new Vector2(.15f, .19f), new Vector2(.85f, .25f), TextAnchor.MiddleLeft);
            tip = UIKit.Txt(r, Tips[0], 32, new Color(1, 1, 1, .7f), new Vector2(.1f, .05f), new Vector2(.9f, .12f));
        }

        public void Set(float p, string m)
        {
            fill.anchorMax = new Vector2(.15f + .7f * Mathf.Clamp01(p), .18f);
            pct.text = Mathf.RoundToInt(p * 100) + "%";
            msg.text = m;
        }

        public void Tick()
        {
            tipT += Time.deltaTime;
            if (tipT > 2.5f) { tipT = 0; tip.text = Tips[Random.Range(0, Tips.Length)]; }
        }

        public void Destroy() { Object.Destroy(canvas.gameObject); }
    }

    public class LobbyScreen
    {
        Canvas canvas; GameManager gm;
        Text outfitTxt, sensTxt, roomTxt, membersTxt;
        Button[] modeBtns = new Button[3];
        GameObject friendsPanel, settingsPanel;

        public LobbyScreen(GameManager g)
        {
            gm = g;
            canvas = UIKit.MakeCanvas("Lobby", 3);
            var r = canvas.transform;
            UIKit.Img(r, "Topo", new Color(0, 0, 0, .55f), new Vector2(0, .92f), Vector2.one);

            var nameR = UIKit.Pix(r, "Nome", new Vector2(0, 1), new Vector2(30, -12), new Vector2(460, 70));
            var nameBg = nameR.gameObject.AddComponent<Image>(); nameBg.color = new Color(1, 1, 1, .15f);
            var nameTxt = UIKit.Txt(nameR, Config.PlayerName, 36, Color.white, new Vector2(.04f, 0), new Vector2(.96f, 1), TextAnchor.MiddleLeft);
            var field = nameR.gameObject.AddComponent<InputField>();
            field.targetGraphic = nameBg; field.textComponent = nameTxt; field.text = Config.PlayerName; field.characterLimit = 14;
            field.onEndEdit.AddListener(s => { if (!string.IsNullOrEmpty(s.Trim())) Config.PlayerName = s.Trim(); });

            UIKit.Txt(r, "Nv. 12   Diamantes: 1000   Moedas: 5400", 30, new Color(1f, .9f, .4f), new Vector2(.35f, .92f), new Vector2(.8f, 1));
            UIKit.PixBtn(r, "CONFIG", 30, new Color(.25f, .25f, .35f, .9f), Vector2.one, new Vector2(-30, -10), new Vector2(200, 70), () => settingsPanel.SetActive(true));

            string[] modes = { "SOLO", "DUPLA", "ESQUADRÃO" };
            for (int i = 0; i < 3; i++)
            {
                int id = i;
                modeBtns[i] = UIKit.PixBtn(r, modes[i], 36, new Color(.2f, .2f, .3f, .9f), new Vector2(0, .5f), new Vector2(40, 130 - i * 110), new Vector2(320, 95), () => SetMode(id));
            }
            UIKit.PixBtn(r, "AMIGOS", 36, new Color(.2f, .55f, .85f, .95f), new Vector2(0, .5f), new Vector2(40, -210), new Vector2(320, 95), OpenFriends);
            SetMode(Config.Mode == GameMode.Solo ? 0 : (Config.Mode == GameMode.Dupla ? 1 : 2));

            for (int i = 0; i < Config.EmoteNames.Length; i++)
            {
                int id = i;
                UIKit.PixBtn(r, Config.EmoteNames[i], 28, new Color(.5f, .2f, .7f, .9f), new Vector2(.5f, 0), new Vector2((i - 2) * 200, 30), new Vector2(185, 85), () => { Config.SelectedEmote = id; gm.preview.PlayEmote(id); }, new Vector2(.5f, 0));
            }

            outfitTxt = UIKit.Txt(UIKit.Pix(r, "VisualLbl", new Vector2(1, .5f), new Vector2(-250, 120), new Vector2(420, 70), new Vector2(.5f, .5f)), "", 34, Color.white, Vector2.zero, Vector2.one);
            UIKit.PixBtn(r, "<", 40, new Color(.2f, .2f, .3f, .9f), new Vector2(1, .5f), new Vector2(-440, 30), new Vector2(100, 80), () => ChangeOutfit(-1), new Vector2(.5f, .5f));
            UIKit.PixBtn(r, ">", 40, new Color(.2f, .2f, .3f, .9f), new Vector2(1, .5f), new Vector2(-60, 30), new Vector2(100, 80), () => ChangeOutfit(1), new Vector2(.5f, .5f));
            UIKit.PixBtn(r, "JOGAR", 70, new Color(.2f, .75f, .25f, .97f), new Vector2(1, 0), new Vector2(-40, 40), new Vector2(460, 150), Play);
            RefreshOutfit();
            BuildFriends(r);
            BuildSettings(r);
        }

        void SetMode(int i)
        {
            Config.Mode = i == 0 ? GameMode.Solo : (i == 1 ? GameMode.Dupla : GameMode.Esquadrao);
            for (int k = 0; k < 3; k++)
                modeBtns[k].targetGraphic.color = k == i ? new Color(1f, .55f, .1f, .95f) : new Color(.2f, .2f, .3f, .9f);
        }

        void ChangeOutfit(int d) { Config.Outfit += d; RefreshOutfit(); gm.RebuildPreview(); }
        void RefreshOutfit() { outfitTxt.text = "Visual: " + Config.Outfits[Config.Outfit].name; }

        void BuildFriends(Transform r)
        {
            var p = UIKit.Img(r, "PainelAmigos", new Color(.05f, .05f, .1f, .96f), new Vector2(.25f, .15f), new Vector2(.75f, .85f));
            friendsPanel = p.gameObject;
            UIKit.Txt(p.transform, "JOGAR COM AMIGOS", 52, new Color(1f, .6f, .15f), new Vector2(0, .85f), Vector2.one);
            roomTxt = UIKit.Txt(p.transform, "", 40, Color.white, new Vector2(0, .72f), new Vector2(1, .85f));
            membersTxt = UIKit.Txt(p.transform, "", 34, Color.white, new Vector2(.05f, .38f), new Vector2(.95f, .72f));
            UIKit.Txt(p.transform, "Modo local: amigos simulados. Para online real, implemente INetworkLobby (Netcode/Photon).", 22, new Color(1, 1, 1, .55f), new Vector2(.05f, .27f), new Vector2(.95f, .37f));
            UIKit.Btn(p.transform, "CONVIDAR AMIGO", 32, new Color(.2f, .55f, .85f), new Vector2(.05f, .13f), new Vector2(.48f, .26f), () => { Net.Lobby.Invite(null); RefreshFriends(); });
            UIKit.Btn(p.transform, "LIMPAR", 32, new Color(.6f, .2f, .2f), new Vector2(.52f, .13f), new Vector2(.95f, .26f), () => { Net.Lobby.Clear(); RefreshFriends(); });
            UIKit.Btn(p.transform, "FECHAR", 32, new Color(.3f, .3f, .4f), new Vector2(.3f, .01f), new Vector2(.7f, .12f), () => friendsPanel.SetActive(false));
            friendsPanel.SetActive(false);
        }

        void OpenFriends()
        {
            if (string.IsNullOrEmpty(Net.Lobby.RoomCode)) Net.Lobby.CreateRoom();
            RefreshFriends(); friendsPanel.SetActive(true);
        }

        void RefreshFriends()
        {
            roomTxt.text = "Código da sala: " + Net.Lobby.RoomCode;
            var s = "Você: " + Config.PlayerName + " (líder)\n";
            foreach (var m in Net.Lobby.Members) s += m + "\n";
            membersTxt.text = s;
        }

        void BuildSettings(Transform r)
        {
            var p = UIKit.Img(r, "PainelConfig", new Color(.05f, .05f, .1f, .96f), new Vector2(.3f, .3f), new Vector2(.7f, .7f));
            settingsPanel = p.gameObject;
            UIKit.Txt(p.transform, "CONFIGURAÇÕES", 48, new Color(1f, .6f, .15f), new Vector2(0, .75f), Vector2.one);
            sensTxt = UIKit.Txt(p.transform, "", 38, Color.white, new Vector2(.25f, .4f), new Vector2(.75f, .7f));
            UIKit.Btn(p.transform, "-", 50, new Color(.3f, .3f, .4f), new Vector2(.05f, .45f), new Vector2(.23f, .65f), () => { Config.Sensitivity -= .1f; RefreshSens(); });
            UIKit.Btn(p.transform, "+", 50, new Color(.3f, .3f, .4f), new Vector2(.77f, .45f), new Vector2(.95f, .65f), () => { Config.Sensitivity += .1f; RefreshSens(); });
            UIKit.Btn(p.transform, "FECHAR", 34, new Color(.3f, .3f, .4f), new Vector2(.3f, .05f), new Vector2(.7f, .25f), () => settingsPanel.SetActive(false));
            RefreshSens();
            settingsPanel.SetActive(false);
        }

        void RefreshSens() { sensTxt.text = "Sensibilidade\n" + Config.Sensitivity.ToString("0.0"); }

        void Play()
        {
            int need = (int)Config.Mode - 1;
            Config.Squad.Clear();
            for (int i = 0; i < need && i < Net.Lobby.Members.Count; i++) Config.Squad.Add(Net.Lobby.Members[i]);
            gm.GoWaiting();
        }

        public void Destroy() { Object.Destroy(canvas.gameObject); }
    }

    public class WaitingScreen
    {
        Canvas canvas; Text count, timer; public TouchControls tc;
        float t0, nextJoin, nextEmote; int joined = 1; Transform arena;
        List<CharacterRig> npcs = new List<CharacterRig>();
        public bool Done;
        const float Dur = 25f;

        public WaitingScreen(GameManager gm, Transform arenaRoot)
        {
            arena = arenaRoot; t0 = Time.time;
            canvas = UIKit.MakeCanvas("Espera", 4);
            var r = canvas.transform;
            UIKit.Txt(r, "SALA DE ESPERA - ILHA BERMUDA", 44, Color.white, new Vector2(.2f, .9f), new Vector2(.8f, 1));
            count = UIKit.Txt(r, "", 40, new Color(1f, .9f, .4f), new Vector2(.3f, .83f), new Vector2(.7f, .9f));
            timer = UIKit.Txt(r, "", 56, new Color(1f, .6f, .15f), new Vector2(.25f, .74f), new Vector2(.75f, .83f));
            UIKit.PixBtn(r, "SAIR", 30, new Color(.6f, .2f, .2f, .9f), new Vector2(0, 1), new Vector2(30, -20), new Vector2(160, 70), () => gm.GoLobby());
            tc = new TouchControls(false);
        }

        void SpawnNpc()
        {
            var rig = CharacterRig.Build(arena, Config.Outfits[Random.Range(0, Config.Outfits.Length)]);
            rig.transform.localPosition = new Vector3(Random.Range(-28f, 28f), 0, Random.Range(-28f, 12f));
            rig.transform.localRotation = Quaternion.Euler(0, Random.value * 360f, 0);
            npcs.Add(rig);
        }

        public void Tick()
        {
            float left = Dur - (Time.time - t0);
            timer.text = "A PARTIDA COMEÇA EM " + Mathf.Max(0, Mathf.CeilToInt(left));
            if (Time.time >= nextJoin && joined < Config.TotalPlayers) { joined++; nextJoin = Time.time + Random.Range(.2f, .6f); SpawnNpc(); }
            if (left < 3f && joined < Config.TotalPlayers) joined = Config.TotalPlayers;
            count.text = "JOGADORES " + joined + "/" + Config.TotalPlayers;
            if (Time.time >= nextEmote && npcs.Count > 0) { nextEmote = Time.time + 1.2f; npcs[Random.Range(0, npcs.Count)].PlayEmote(Random.Range(0, 5)); }
            if (left <= 0) Done = true;
        }

        public void Destroy() { tc.Destroy(); Object.Destroy(canvas.gameObject); }
    }

    public class EndScreen
    {
        Canvas canvas;
        public EndScreen(GameManager gm, bool won, int rank, int kills)
        {
            canvas = UIKit.MakeCanvas("Fim", 30);
            var r = canvas.transform;
            UIKit.Img(r, "Fundo", new Color(0, 0, 0, .75f), Vector2.zero, Vector2.one);
            UIKit.Txt(r, won ? "VITÓRIA!" : "ELIMINADO", 150, won ? new Color(1f, .8f, .1f) : new Color(1f, .3f, .3f), new Vector2(0, .55f), new Vector2(1, .8f));
            UIKit.Txt(r, "Posição #" + rank + "     Abates: " + kills, 60, Color.white, new Vector2(0, .4f), new Vector2(1, .55f));
            UIKit.Btn(r, "VOLTAR AO LOBBY", 50, new Color(.2f, .6f, .9f), new Vector2(.3f, .15f), new Vector2(.7f, .3f), () => gm.GoLobby());
        }
        public void Destroy() { Object.Destroy(canvas.gameObject); }
    }
}
