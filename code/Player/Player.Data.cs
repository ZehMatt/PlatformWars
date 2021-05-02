using Sandbox;

namespace PlatformWars
{
    partial class Player
    {
        [Net]
        public NetList<int> Ammo { get; set; } = new();
    }
}
