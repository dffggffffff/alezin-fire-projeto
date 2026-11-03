using UnityEngine;

namespace Alezinfire
{
    [System.Serializable]
    public class WeaponData
    {
        public string name;
        public float damage, rpm, range, spread, reload, length;
        public int mag, pellets = 1;
        public bool auto;
        public Color color;
    }

    public static class Arsenal
    {
        public static readonly WeaponData[] All = {
            new WeaponData { name = "Pistola Faísca", damage = 24, rpm = 300, range = 70, spread = 1.2f, reload = 1.2f, mag = 12, color = new Color(.75f,.75f,.8f), length = .35f },
            new WeaponData { name = "SMG Rajada", damage = 13, rpm = 850, range = 55, spread = 3f, reload = 1.6f, mag = 30, auto = true, color = new Color(.2f,.2f,.25f), length = .5f },
            new WeaponData { name = "Fuzil Trovão", damage = 22, rpm = 600, range = 110, spread = 2f, reload = 2f, mag = 30, auto = true, color = new Color(.35f,.3f,.2f), length = .8f },
            new WeaponData { name = "Escopeta Estrondo", damage = 11, pellets = 8, rpm = 70, range = 28, spread = 5f, reload = 2.4f, mag = 6, color = new Color(.5f,.25f,.15f), length = .8f },
            new WeaponData { name = "Sniper Corvo", damage = 85, rpm = 45, range = 350, spread = .15f, reload = 2.8f, mag = 5, color = new Color(.1f,.15f,.1f), length = 1.1f },
        };
    }
}
