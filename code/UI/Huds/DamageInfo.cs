using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;
using System.Collections.Generic;

namespace PlatformWars.UI.Huds
{
	class DamageDisplay : Panel
	{
		public Pawn Pawn { get; private set; }

		public float Damage { get; private set; }

		Label LabelDamage;
		Vector3 Position;

		float LifeTime;

		public DamageDisplay( Vector3 pos, Pawn pawn, float damage )
		{
			Pawn = Pawn;
			Damage = damage;
			Position = pos;
			LifeTime = 0;

			LabelDamage = Add.Label( $"{ damage}" );
			if ( damage < 0.0f )
				SetClass( "positive", true );
			else
				SetClass( "negative", true );
		}

		public bool Update()
		{
			float MaxLifeTime = 2.0f;
			float alpha = LifeTime / MaxLifeTime;
			float Speed = 20.0f + (alpha * 20.0f);

			Position += new Vector3( 0, 0, Speed * Time.Delta );

			var screenPos = Position.ToScreen();
			Style.Left = Length.Fraction( screenPos.x );
			Style.Top = Length.Fraction( screenPos.y );
			Style.Opacity = 1.0f - alpha;

			var transform = new PanelTransform();
			transform.AddScale( 1.0f );
			transform.AddTranslateY( Length.Fraction( -1.0f ) );
			transform.AddTranslateX( Length.Fraction( -0.5f ) );

			Style.Transform = transform;

			Style.Dirty();

			LifeTime += Time.Delta;
			return LifeTime < MaxLifeTime;
		}
	}

	class DamageInfo : Panel
	{
		List<DamageDisplay> Panels = new();

		public DamageInfo()
		{
			StyleSheet.Load( "/ui/huds/DamageInfo.scss" );
		}

		public override void Tick()
		{
			base.Tick();

			for ( int i = 0; i < Panels.Count; i++ )
			{
				var pnl = Panels[i];
				if ( !pnl.Update() )
				{
					pnl.Delete();
					Panels.RemoveAt( i );
					i--;
				}
			}
		}

		public void AddDamageDisplay( Pawn pawn, float damage )
		{
			var head = pawn.GetAttachment( "hat" ) ?? new Transform( pawn.EyePos );
			var pos = head.Position + head.Rotation.Up * 5;

			var infoPanel = new DamageDisplay( pos, pawn, damage );
			infoPanel.Parent = this;

			Panels.Add( infoPanel );
		}

	}
}
