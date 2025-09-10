// using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Config;
using Vintagestory.API.Server;
using Vintagestory.GameContent;


namespace SimpleBedSpawn
{
    // Config Object
    public class BedRespawnConfig
    {
        public bool Enabled { get; set; } = true;
        public bool ClearOnBedBreak { get; set; } = true;
    }

    // Mod Setup
    public class SimpleBedSpawnModSystem : ModSystem
    {
        private ICoreServerAPI? sapi;
        private BedRespawnConfig? cfg;

        // public override bool ShouldLoad(EnumAppSide side) => side == EnumAppSide.Server;

        // Called on server and client
        // Useful for registering block/entity classes on both sides
        /*public override void Start(ICoreAPI api)
        {
            Mod.Logger.Notification("Hello from template mod: " + api.Side);
        }*/

        public override void StartServerSide(ICoreServerAPI api)
        {
            sapi = api;
            cfg = new BedRespawnConfig();

            if (!cfg.Enabled)
            {
                Mod.Logger.Notification("[BedRespawn] Disabled via config.");
                return;
            }

            // Fires whenever a player uses (right-clicks) a block
            // Signature: (IServerPlayer byPlayer, BlockSelection blockSel)
            // Docs: BlockUsedDelegate
            sapi.Event.DidUseBlock += (IServerPlayer p, BlockSelection blockSel) =>
            {
                if (p == null || blockSel == null || sapi == null) return;

                var block = sapi.World.BlockAccessor.GetBlock(blockSel.Position);
                if (block is BlockBed)
                {
                    int X = blockSel.Position.X;
                    int Y = blockSel.Position.Y;
                    int Z = blockSel.Position.Z;
                    p.SetSpawnPosition(new PlayerSpawnPos(X, Y, Z));
                    p.SendMessage(GlobalConstants.GeneralChatGroup, "Your spawnpoint has been updated! ", EnumChatType.Notification);
                }
            };

            sapi.Event.OnEntityDeath += (entity, damage) =>
            {
                sapi.Logger.Notification("Player was killed.");
            };

            sapi.Event.DidBreakBlock += (IServerPlayer p, int blockId, BlockSelection blockSel) =>
            {
                // If the block is a bed && config's clear on bed break is set to `true`
                if (sapi.World.BlockAccessor.GetBlock(blockId) is BlockBed && cfg.ClearOnBedBreak)
                {
                    var bedPos = blockSel?.Position;
                    if (bedPos == null) return;

                    foreach (var item in sapi.Server.Players)
                    {
                        var pos = item.GetSpawnPosition(false);
                        if (pos.X - 0.5 == bedPos.X && pos.Y == bedPos.Y && pos.Z - 0.5 == bedPos.Z)
                        {
                            item.ClearSpawnPosition();
                            p.SendMessage(GlobalConstants.GeneralChatGroup, "Your spawnpoint has been reset!", EnumChatType.Notification);
                        }
                    }
                }
            };
        }

        // TODO: We'll set this up later...

        /* private void OnUseBlock(IServerPlayer byPlayer, BlockSelection blockSel)
           {
           }

           private void OnDidBreakBlock(IServerPlayer byPlayer, BlockSelection blockSel, ref float dropQuantityMultiplier, ref EnumHandling handling)
           {
           }

           private void OnPlayerTriedToSleep(IServerPlayer player, BlockPos bedPos)
           {
           }

           private void OnPlayerWokeUp(IServerPlayer player, BlockPos bedPos)
           {
           }


           public override void StartClientSide(ICoreClientAPI api)
           {
             Mod.Logger.Notification("Hello from template mod client side: " + Lang.Get("simplebedspawn:hello"));
           }
          */

    }
}