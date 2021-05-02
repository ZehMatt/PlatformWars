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

        Pawn pawn;

        public BaseNameTag(Pawn pawn)
        {
            this.pawn = pawn;

            var player = pawn.GetPlayer();
            NameLabel = Add.Label(player.Name);
            Avatar = Add.Image($"avatar:{player.SteamId}");
        }

        public virtual void UpdateFromPlayer(Player player)
        {
            // Nothing to do unless we're showing health and shit
        }
    }

    class PawnTags : Panel
    {
        Dictionary<Pawn, BaseNameTag> ActiveTags = new();

        public float MaxDrawDistance = 4000;
        public int MaxTagsToShow = 100;

        public PawnTags()
        {
            StyleSheet.Load("/ui/huds/PawnTags.scss");
        }

        public override void Tick()
        {
            base.Tick();

            var roundMgr = RoundManager.Get();
            if (roundMgr == null)
                return;

            var deleteList = new List<Pawn>();
            deleteList.AddRange(ActiveTags.Keys);

            var activePlayers = roundMgr.GetActivePlayers();

            int count = 0;
            foreach (var player in activePlayers)
            {
                var pawns = player.GetPawns();
                foreach (var pawn in pawns)
                {
                    if (UpdateNameTag(pawn))
                    {
                        deleteList.Remove(pawn);
                        count++;
                    }

                    if (count >= MaxTagsToShow)
                        break;
                }
            }

            foreach (var player in deleteList)
            {
                ActiveTags[player].Delete();
                ActiveTags.Remove(player);
            }
        }

        public virtual BaseNameTag CreateNameTag(Pawn pawn)
        {
            var tag = new BaseNameTag(pawn);
            tag.Parent = this;
            return tag;
        }

        public bool UpdateNameTag(Pawn pawn)
        {
            // Don't draw local player
            var player = pawn.GetPlayer();
            if (player.IsLocalPlayer)
                return false;

            if (pawn.LifeState != LifeState.Alive)
                return false;

            //
            // Where we putting the label, in world coords
            //
            var head = pawn.GetAttachment("hat");
            if (head.Pos == Vector3.Zero)
            {
                // FIXME: Make this work on non-players.
                head.Pos = player.EyePos;
            }

            var labelPos = head.Pos + head.Rot.Up * 5;

            //
            // Are we too far away?
            //
            float dist = labelPos.Distance(Camera.LastPos);
            if (dist > MaxDrawDistance)
                return false;

            //
            // Are we looking in this direction?
            //
            var lookDir = (labelPos - Camera.LastPos).Normal;
            var dotProduct = Camera.LastRot.Forward.Dot(lookDir);
            if (dotProduct < 0.5)
                return false;

            // TODO - can we see them

            // Max Draw Distance
            var alpha = dist.LerpInverse(MaxDrawDistance, MaxDrawDistance * 0.1f, true) * (dotProduct - 0.5f);

            // If I understood this I'd make it proper function
            var distanceScale = 0.08f;
            var objectSize = distanceScale / dist / (2.0f * MathF.Tan((Camera.LastFieldOfView / 2.0f).DegreeToRadian())) * MaxDrawDistance;

            objectSize = objectSize.Clamp(0.05f, 10.0f);

            if (!ActiveTags.TryGetValue(pawn, out var tag))
            {
                tag = CreateNameTag(pawn);
                ActiveTags[pawn] = tag;
            }

            tag.UpdateFromPlayer(player);

            var screenPos = labelPos.ToScreen();

            tag.Style.Left = Length.Fraction(screenPos.x);
            tag.Style.Top = Length.Fraction(screenPos.y);
            tag.Style.Opacity = alpha;

            var transform = new PanelTransform();
            transform.AddScale(objectSize);
            transform.AddTranslateY(Length.Fraction(-1.0f));
            transform.AddTranslateX(Length.Fraction(-0.5f));

            tag.Style.Transform = transform;
            tag.Style.Dirty();

            return true;
        }
    }
}
