using Sandbox;
using System.Collections.Generic;

namespace PlatformWars
{
	partial class Player
	{
		[Net]
		public List<int> Ammo { get; set; } = new();
	}
}
