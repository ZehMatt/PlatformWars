using Sandbox;
using System;

namespace PlatformWars
{
    partial class Player
    {
        public void ClearAmmo()
        {
            Ammo.Clear();
        }

        public int AmmoCount(Weapons.AmmoType type)
        {
            if (Ammo == null)
                return 0;

            return Ammo.Get(type);
        }

        public bool GiveAmmo(Weapons.AmmoType type, int amount)
        {
            if (!Host.IsServer)
                return false;

            if (Ammo == null)
                return false;

            var currentAmmo = AmmoCount(type);
            return Ammo.Set(type, currentAmmo + amount);
        }

        public int TakeAmmo(Weapons.AmmoType type, int amount)
        {
            var available = Ammo.Get(type);
            amount = Math.Min(Ammo.Get(type), amount);

            Ammo.Set(type, available - amount);

            NetworkDirty("Ammo", NetVarGroup.Net);

            return amount;
        }
    }
}
