using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;
using System;
using System.Collections.Generic;

namespace PlatformWars.UI.Huds
{
    class BaseNameTag : Panel
    {
        public Label NameLabel;
        public Image Avatar;
        public Label HealthText;
        public Panel HealthBar;
        private float CurrentHealth;

        Pawn pawn;

        public BaseNameTag( Pawn pawn )
        {
            this.pawn = pawn;
            var player = pawn.GetPlayer();

            NameLabel = Add.Label( player.Name );
            Avatar = Add.Image( $"avatar:{player.SteamId}" );
            HealthText = Add.Label( $"{pawn.Health}" );
            HealthBar = Add.Panel();
        }

        public virtual void UpdateData( Pawn pawn )
        {
            if ( this.pawn.Health == CurrentHealth )
                return;

            CurrentHealth = this.pawn.Health;
            HealthText.Text = $"{CurrentHealth}";

            var p = CurrentHealth / Pawn.MaxHealth;
            var r = (1.0f - p);
            var g = p;
            var b = 0;

            var style = HealthBar.Style;
            style.BackgroundColor = new Color( r, g, b );
        }
    }

    class PawnTags : Panel
    {
        Dictionary<Pawn, BaseNameTag> ActiveTags = new();

        public float MaxDrawDistance = 4000;
        public int MaxTagsToShow = 100;

        public PawnTags()
        {
            StyleSheet.Load( "/ui/huds/PawnTags.scss" );
        }

        public override void Tick()
        {
            base.Tick();

            var roundMgr = RoundManager.Get();
            if ( roundMgr == null )
                return;

            var deleteList = new List<Pawn>();
            deleteList.AddRange( ActiveTags.Keys );

            var activePlayers = roundMgr.GetActivePlayers();

            int count = 0;
            foreach ( var player in activePlayers )
            {
                var pawns = player.GetPawns();
                foreach ( var pawn in pawns )
                {
                    if ( UpdateNameTag( pawn ) )
                    {
                        deleteList.Remove( pawn );
                        count++;
                    }

                    if ( count >= MaxTagsToShow )
                        break;
                }
            }

            foreach ( var player in deleteList )
            {
                ActiveTags[player].Delete();
                ActiveTags.Remove( player );
            }
        }

        public virtual BaseNameTag CreateNameTag( Pawn pawn )
        {
            var tag = new BaseNameTag( pawn );
            tag.Parent = this;
            return tag;
        }

        public bool UpdateNameTag( Pawn pawn )
        {
            // Don't draw local player
            var player = pawn.GetPlayer();

            if ( pawn.LifeState != LifeState.Alive )
                return false;

            if ( Local.Pawn == pawn )
                return false;

            //
            // Where we putting the label, in world coords
            //
            var head = pawn.GetAttachment( "hat" ) ?? new Transform( pawn.EyePos );
            var labelPos = head.Position + head.Rotation.Up * 5;
            float dist = labelPos.Distance( CurrentView.Position );

            //
            // Are we looking in this direction?
            //
            var lookDir = (labelPos - CurrentView.Position).Normal;
            var dotProduct = CurrentView.Rotation.Forward.Dot( lookDir );
            var dotThreshold = 0.90f;
            if ( dotProduct < dotThreshold )
                return false;

            var tr = Trace.Ray( CurrentView.Position, labelPos ).WorldAndEntities().Ignore( Local.Pawn ).Run();
            if ( tr.Hit )
            {
                if ( tr.Entity != pawn )
                    return false;
            }

            // Max Draw Distance
            var alpha = ((dotProduct - dotThreshold) / (1.0f - dotThreshold)) * 0.75f;
            var maxScaleDistance = 2000.0f;
            var objectSize = (dist / maxScaleDistance); // distanceScale / dist / (2.0f * MathF.Tan( (CurrentView.FieldOfView / 2.0f).DegreeToRadian() )) * MaxDrawDistance;

            objectSize = objectSize.Clamp( 0.5f, 0.7f );

            if ( !ActiveTags.TryGetValue( pawn, out var tag ) )
            {
                tag = CreateNameTag( pawn );
                ActiveTags[pawn] = tag;
            }

            tag.UpdateData( pawn );

            var screenPos = labelPos.ToScreen();

            tag.Style.Left = Length.Fraction( screenPos.x );
            tag.Style.Top = Length.Fraction( screenPos.y );
            tag.Style.Opacity = alpha;

            var transform = new PanelTransform();
            transform.AddScale( objectSize );
            transform.AddTranslateY( Length.Fraction( -1.0f ) );
            transform.AddTranslateX( Length.Fraction( -0.5f ) );

            tag.Style.Transform = transform;
            tag.Style.Dirty();

            return true;
        }
    }
}
