using Sandbox;

namespace PlatformWars
{
    partial class Pawn : AnimEntity
    {
        [Net]
        Player PlayerOwner { get; set; }

        public Camera Camera = new FirstPersonCamera();

        public const int MaxHealth = 250;

        public Pawn()
        {
        }

        public override void Spawn()
        {
            LifeState = LifeState.Alive;
            Health = MaxHealth;
        }

        public void AssignPlayer(Player ply)
        {
            PlayerOwner = ply;
        }

        public Player GetPlayer()
        {
            return PlayerOwner;
        }

        public void Reset(Vector3 pos)
        {
            var ply = GetPlayer();
            var controller = ply.GetActiveController() as WalkController;
            var bodyGirth = controller.BodyGirth * 0.5f;

            SetModel("models/citizen/citizen.vmdl");
            //SetupPhysicsFromModel(PhysicsMotionType.Dynamic);
            SetupPhysicsFromOBB(PhysicsMotionType.Dynamic, new Vector3(-bodyGirth, -bodyGirth, 0), new Vector3(bodyGirth, bodyGirth, controller.BodyHeight));

            WorldPos = pos;
            EnableAllCollisions = true;
            EnableDrawing = true;
            EnableHideInFirstPerson = true;
            EnableShadowInFirstPerson = true;
            PhysicsEnabled = true;
            MoveType = MoveType.Physics;

            // THIS IS ALL SORTS OF WRONG.
            var plyPhysics = ply.PhysicsBody;
            if (PhysicsBody != null && plyPhysics != null)
            {
                PhysicsBody.Wake();
                PhysicsBody.MotionEnabled = true;
                PhysicsBody.EnableAutoSleeping = false;
                PhysicsBody.Mass = plyPhysics.Mass;
                PhysicsBody.LocalMassCenter = plyPhysics.LocalMassCenter;
                PhysicsBody.LinearDamping = plyPhysics.LinearDamping;
                PhysicsBody.LinearDrag = plyPhysics.LinearDrag;
                PhysicsBody.AngularDamping = plyPhysics.AngularDamping;
                PhysicsBody.AngularDrag = plyPhysics.AngularDrag;
                PhysicsBody.GravityScale = plyPhysics.GravityScale;
                PhysicsBody.SpeculativeContactEnabled = plyPhysics.SpeculativeContactEnabled;
                PhysicsBody.Pos = WorldPos;
            }

            Dress();
        }

        DamageInfo LastDamage;

        public override void TakeDamage(DamageInfo info)
        {
            base.TakeDamage(info);

            LastDamage = info;
        }

        public override void OnKilled()
        {
            if (LifeState == LifeState.Dead)
                return;

            LifeState = LifeState.Dead;

            Log.Info("Pawn got killed");

            var ragdoll = CreateRagdoll(LastDamage.Force, GetHitboxBone(LastDamage.HitboxIndex));

            var ply = GetPlayer();
            if (ply != null)
            {
                SetParent(null);

                // Hide player in case its the current.
                ply.EnableDrawing = false;
                ply.Corpse = ragdoll;
            }

            ClearCollisionLayers();
            EnableDrawing = false;

            // We no longer need this entity but keep it a bit
            // To not spazz out the cameras that actively view the pawn entity.
            DeleteAsync(10.0f);

            var roundMgr = RoundManager.Get();
            if (roundMgr != null)
            {
                roundMgr?.OnPawnKilled(this);
            }

            base.OnKilled();
        }

        [Event("physics.step")]
        void Tick()
        {
            if (!IsAuthority)
                return;

            if (PhysicsBody == null)
                return;

            // Keep up-right
            var rot = PhysicsBody.Rot;
            if (rot.x != 0 || rot.z != 0)
            {
                PhysicsBody.Rot = Rotation.From(0, rot.y, 0);
            }

            PhysicsBody.AngularVelocity = Vector3.Zero;

            var ply = GetPlayer();
            var controller = ply.GetActiveController() as WalkController;

            // FIXME: Try to mimic the player fall, the Pawns are falling way slower.
            //PhysicsBody.Velocity -= new Vector3(0, 0, controller.Gravity) * Time.Delta;

        }

    }
}
