using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlatformWars
{
	partial class RoundManager
	{
		[ServerCmd( "roundmanager_advance", Help = "Skip current state" )]
		public static void RoundManagerNextState()
		{
			var roundMgr = RoundManager.Get();
			if ( roundMgr == null )
				return;

			roundMgr.AdvanceState();
		}
	}
}
