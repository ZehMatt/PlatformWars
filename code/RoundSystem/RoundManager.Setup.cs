using Sandbox;
using System;
using System.Collections.Generic;

namespace PlatformWars
{
    class PlatformTile
    {
        public bool Occupied;
        public bool SpawnPoint;
    }

    class PlatformTiles
    {
        int MaxX = 0;
        int MaxY = 0;
        int MaxZ = 0;
        List<PlatformTile> Tiles = new();

        public PlatformTiles(int maxX, int maxY, int maxZ)
        {
            MaxX = maxX;
            MaxY = maxY;
            MaxZ = maxZ;
            for (int z = 0; z < maxZ; z++)
            {
                for (int x = 0; x < maxX; x++)
                {
                    for (int y = 0; y < maxY; y++)
                    {
                        Tiles.Add(new PlatformTile()
                        {
                            Occupied = false
                        });
                    }
                }
            }
        }

        public PlatformTile Get(int x, int y, int z)
        {
            return Tiles[x + y * MaxX + z * MaxX * MaxY];
        }

        public void Occupy(int x, int y, int z, bool spawnPoint)
        {
            var tile = Get(x, y, z);
            tile.Occupied = true;
            tile.SpawnPoint = spawnPoint;
        }

        public void Remove(int x, int y, int z)
        {
            var tile = Get(x, y, z);
            tile.Occupied = false;
        }
    }

    partial class RoundManager
    {
        List<Entity> PlatformProps = new();
        List<Entity> SpawnProps = new();

        void ResetMap()
        {
            Host.AssertServer();

            foreach (var ent in Game.All)
            {
                if (ent is PlatformWars.Player)
                    continue;

                if (ent is not Sandbox.Prop)
                    continue;

                ent.Delete();
            }
        }

        void SetupTeams()
        {
            Host.AssertServer();

            ActivePlayers.Clear();

            // Setup the Teams.
            int playerIdx = 0;
            foreach (var p in Player.All)
            {
                var ply = p as Player;
                ply.SetTeam(Team.Red + playerIdx);

                playerIdx++;
                ActivePlayers.Add(ply);
            }
        }

        void SetupPawns()
        {
            for (int i = 0; i < ActivePlayers.Count; i++)
            {
                var ply = ActivePlayers.Get(i).Entity as Player;
                ply.SetupPawns(4);
            }
        }

        const int MaxTilesX = 64;
        const int MaxTilesY = 64;
        const int MaxTilesZ = 8;

        const int TileSizeX = Entities.Platform.BlockSize;
        const int TileSizeY = Entities.Platform.BlockSize;
        const int TileSizeZ = Entities.Platform.BlockSize;

        const int PlatformsOffsetZ = 3000;

        void CreatePlatform(float tileX, float tileY, float z, bool canSpawn = true)
        {
            float offsetX = (MaxTilesX * TileSizeX) / 2;
            float offsetY = (MaxTilesY * TileSizeY) / 2;

            var x = tileX * TileSizeX;
            var y = tileY * TileSizeY;

            var crate = Create<PlatformWars.Entities.Platform>();
            crate.WorldPos = new Vector3(x - offsetX, y - offsetY, PlatformsOffsetZ + (z * TileSizeZ));
            crate.Health = 100;
            crate.Spawn();

            var phys = crate.PhysicsBody;
            if (phys != null)
            {
                phys.MotionEnabled = false;
            }

            PlatformProps.Add(crate);
            if (canSpawn)
                SpawnProps.Add(crate);
        }

        // Public for command.
        public void GeneratePlatforms()
        {
            foreach (var ent in PlatformProps)
                ent.Delete();

            PlatformProps.Clear();
            SpawnProps.Clear();

            var tiles = new PlatformTiles(MaxTilesX, MaxTilesY, MaxTilesZ);

            // Generate Tiles
            {
                float offset1 = Rand.Float(-16000.0f, 16000.0f);
                float offset2 = Rand.Float(-16000.0f, 16000.0f);

                float z = 0.1f;
                for (int x = 0; x < MaxTilesX; x++)
                {
                    float x1 = (float)x / MaxTilesX;

                    for (int y = 0; y < MaxTilesY; y++)
                    {
                        float y1 = (float)y / MaxTilesY;

                        {
                            float scale = 8.0f;
                            float value = Noise.Perlin(offset1 + (x1 * scale), offset1 + (y1 * scale), z * scale);
                            var height = Math.Clamp(value * MaxTilesZ, -1, MaxTilesZ);
                            if (height > 0)
                            {
                                int ix = (int)x;
                                int iy = (int)y;
                                int iz = (int)MathF.Round(height);

                                tiles.Occupy(ix, iy, iz, true);
                                for (int z1 = 0; z1 < Math.Min(iz, 2); z1++)
                                    tiles.Occupy(ix, iy, z1, false);
                            }
                        }


                        {
                            float scale = 13.0f;
                            float value = Noise.Perlin(offset2 + (x1 * scale), offset2 + (y1 * scale), z * scale);
                            var height = Math.Clamp(value * MaxTilesZ, -1, MaxTilesZ);
                            if (height > 0)
                            {
                                int ix = (int)x;
                                int iy = (int)y;
                                int iz = (int)MathF.Round(height);

                                for (int z1 = 1; z1 < Math.Min(iz, 2); z1++)
                                    tiles.Remove(ix, iy, z1);
                            }
                        }

                    }
                }

            }

            // Convert to platform objects.
            for (int z = 0; z < MaxTilesZ; z++)
            {
                for (int x = 0; x < MaxTilesX; x++)
                {
                    for (int y = 0; y < MaxTilesY; y++)
                    {
                        var tile = tiles.Get(x, y, z);
                        if (!tile.Occupied)
                            continue;

                        CreatePlatform(x, y, z, tile.SpawnPoint);
                    }
                }
            }

        }

        void ReorganizePawns()
        {
            Host.AssertServer();

            List<Pawn> pawns = new List<Pawn>();
            foreach (var p in Player.All)
            {
                var ply = p as Player;
                pawns.AddRange(ply.GetPawns());
            }

            float total = pawns.Count;
            var platforms = SpawnProps.ToArray();
            for (float i = 0; i < total; i++)
            {
                var pawn = pawns[(int)i];

                var platform = Rand.FromArray(platforms);

                // Reset pawn at the platform position.
                pawn.Reset(platform.WorldPos + new Vector3(0, 0, TileSizeZ));
            }
        }

        void SetupPlayers()
        {
            for (int i = 0; i < ActivePlayers.Count; i++)
            {
                var ply = ActivePlayers.Get(i).Entity as Player;
                ply.Respawn();
            }
        }

        void HandleSetup()
        {
            if (IsServer)
            {
                ResetMap();
                SetupTeams();
                GeneratePlatforms();

                SetupPlayers();
                SetupPawns();
                ReorganizePawns();
            }

            SetState(RoundState.Starting);
        }

    }
}
