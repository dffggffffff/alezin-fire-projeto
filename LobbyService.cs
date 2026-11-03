using System.Collections.Generic;
using UnityEngine;

namespace Alezinfire
{
    /// Interface para jogar com amigos. LocalLobby simula localmente (bots como amigos).
    /// Para online de verdade, implemente esta interface com Unity Netcode/Relay, Photon ou Mirror.
    public interface INetworkLobby
    {
        string RoomCode { get; }
        IList<string> Members { get; }
        string CreateRoom();
        bool JoinRoom(string code);
        void Invite(string friendName);
        void Clear();
    }

    public class LocalLobby : INetworkLobby
    {
        readonly List<string> members = new List<string>();
        static readonly string[] Friends = { "Zeca", "Mari", "Biel", "Duda", "Léo", "Pedro", "Ana" };
        public string RoomCode { get; private set; }
        public IList<string> Members { get { return members; } }

        public string CreateRoom()
        {
            const string chars = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";
            var s = "";
            for (int i = 0; i < 5; i++) s += chars[Random.Range(0, chars.Length)];
            RoomCode = s; members.Clear();
            return s;
        }
        public bool JoinRoom(string code) { RoomCode = code; return true; }
        public void Invite(string friendName)
        {
            if (members.Count >= 3) return;
            if (string.IsNullOrEmpty(friendName)) friendName = Friends[Random.Range(0, Friends.Length)] + Random.Range(1, 99);
            members.Add(friendName);
        }
        public void Clear() { members.Clear(); }
    }

    public static class Net
    {
        public static INetworkLobby Lobby = new LocalLobby();
    }
}
