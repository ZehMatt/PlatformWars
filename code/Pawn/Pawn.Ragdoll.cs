﻿using Sandbox;

namespace PlatformWars
{
    partial class Pawn
    {
        static EntityLimit RagdollLimit = new EntityLimit { MaxTotal = 20 };

        Entity CreateRagdoll(Vector3 force, int forceBone)
        {
            var ent = new ModelEntity();
            ent.WorldPos = WorldPos;
            ent.WorldRot = WorldRot;
            ent.MoveType = MoveType.Physics;
            ent.UsePhysicsCollision = true;
            ent.RenderColor = RenderColor;
            ent.SetInteractsAs(CollisionLayer.Debris);
            ent.SetInteractsWith(CollisionLayer.WORLD_GEOMETRY);
            ent.SetInteractsExclude(CollisionLayer.Player | CollisionLayer.Debris);

            ent.SetModel(GetModelName());
            ent.CopyBonesFrom(this);
            ent.TakeDecalsFrom(this);
            ent.SetRagdollVelocityFrom(this);


            // Copy the clothes over
            foreach (var child in Children)
            {
                if (child is ModelEntity e)
                {
                    var model = e.GetModelName();
                    if (model != null && !model.Contains("clothes")) // Uck we 're better than this, entity tags, entity type or something?
                        continue;

                    var clothing = new ModelEntity();
                    clothing.SetModel(model);
                    clothing.SetParent(ent, true);
                }
            }

            ent.PhysicsGroup.AddVelocity(force);

            if (forceBone >= 0)
            {
                var body = ent.GetBonePhysicsBody(forceBone);
                if (body != null)
                {
                    body.ApplyForce(force * 1000);
                }
                else
                {
                    ent.PhysicsGroup.AddVelocity(force);
                }
            }

            RagdollLimit.Watch(ent);

            return ent;
        }
    }
}
