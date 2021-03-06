using MinicraftPlusSharp.Core;
using MinicraftPlusSharp.Core.IO;
using MinicraftPlusSharp.Entities.Furniture;
using MinicraftPlusSharp.Entities.Particle;
using MinicraftPlusSharp.Gfx;
using MinicraftPlusSharp.Items;
using MinicraftPlusSharp.Levels;
using MinicraftPlusSharp.Levels.Tiles;
using MinicraftPlusSharp.Networking;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using static MinicraftPlusSharp.Entities.Direction;

namespace MinicraftPlusSharp.Entities.Mobs
{
    public class Player : Mob, ItemHolder, ClientTickable
    {
        private class PlayerInventory : Inventory
        {
            public override void Add(int idx, Item item)
            {
                if (Game.IsMode("creative"))
                {
                    if (Count(item) > 0)
                    {
                        return;
                    }

                    item = item.Clone();

                    if (item is StackableItem stackable)
                    {
                        stackable.count = 1;
                    }
                }

                base.Add(idx, item);
            }

            public override Item Remove(int idx)
            {
                if (Game.IsMode("creative"))
                {
                    Item cur = Get(idx);

                    if (cur is StackableItem stackable)
                    {
                        stackable.count = 1;
                    }

                    if (Count(cur) == 1)
                    {
                        base.Remove(idx);
                        base.Add(0, cur);
                        return cur.Clone();
                    }
                }

                return base.Remove(idx);
            }
        }

        protected InputHandler input;

        private static readonly int playerHurtTime = 30;
        public static readonly int INTERACT_DIST = 12;
        private static readonly int ATTACK_DIST = 20;

        private static readonly int mtm = 300; // time given to increase multiplier before it goes back to 1.
        public static readonly int MAX_MULTIPLIER = 50; // maximum score multiplier.

        public double moveSpeed = 1; // the number of coordinate squares to move; each tile is 16x16.
        private int score; // the player's score

        private int multipliertime = mtm; // Time left on the current multiplier.
        private int multiplier = 1; // Score multiplier

        //These 2 ints are ints saved from the first spawn - this way the spawn pos is always saved.
        public int spawnx = 0, spawny = 0; // these are stored as tile coordinates, not entity coordinates.
                                           //public bool bedSpawn = false;

        //private bool hasSetHome = false;
        public bool skinon;
        //private int homeSetX, homeSetY;

        // the maximum stats that the player can have.
        public static readonly int maxStat = 10;
        public static new readonly int maxHealth = maxStat;
        public static readonly int maxStamina = maxStat, maxHunger = maxStat;
        public static readonly int maxArmor = 100;

        public static new MobSprite[][] sprites = MobSprite.CompileMobSpriteAnimations(0, 16);
        private static MobSprite[][] carrySprites = MobSprite.CompileMobSpriteAnimations(0, 18); // the sprites while carrying something.
        private static MobSprite[][] suitSprites = MobSprite.CompileMobSpriteAnimations(8, 16); // the "airwizard suit" sprites.
        private static MobSprite[][] carrySuitSprites = MobSprite.CompileMobSpriteAnimations(8, 18); // the "airwizard suit" sprites.

        private Inventory inventory;

        public Item activeItem;
        public Item attackItem; // attackItem is useful again b/c of the power glove.
        private Item prevItem; // holds the item held before using the POW glove.

        public int attackTime;
        public Direction attackDir;

        private int onStairDelay; // the delay before changing levels.
        private int onFallDelay; // the delay before falling b/c we're on an InfiniteFallTile

        public int hunger, stamina, armor; // the current stats
        public int armorDamageBuffer;
        public ArmorItem curArmor; // the color/type of armor to be displayed.

        private int staminaRecharge; // the ticks before charging a bolt of the player's stamina
        private static readonly int maxStaminaRecharge = 10; // cutoff value for staminaRecharge
        public int staminaRechargeDelay; // the recharge delay ticks when the player uses up their stamina.

        private int hungerStamCnt, stamHungerTicks; // tiers of hunger penalties before losing a burger.
        private static readonly int maxHungerTicks = 400; // the cutoff value for stamHungerTicks
        private static readonly int[] maxHungerStams = { 10, 7, 5 }; // hungerStamCnt required to lose a burger.
        private static readonly int[] hungerTickCount = { 120, 30, 10 }; // ticks before decrementing stamHungerTicks.
        private static readonly int[] hungerStepCount = { 8, 3, 1 }; // steps before decrementing stamHungerTicks.
        private static readonly int[] minStarveHealth = { 5, 3, 0 }; // min hearts required for hunger to hurt you.
        private int stepCount; // used to penalize hunger for movement.
        private int hungerChargeDelay; // the delay between each time the hunger bar increases your health
        private int hungerStarveDelay; // the delay between each time the hunger bar decreases your health

        public Dictionary<PotionType, int> potioneffects; // the potion effects currently applied to the player
        public bool showpotioneffects; // whether to display the current potion effects on screen
        private int cooldowninfo; // prevents you from toggling the info pane on and off super fast.
        private int regentick; // counts time between each time the regen potion effect heals you.

        //private readonly int acs = 25; // default ("start") arrow count
        public int shirtColor = Color.Get(1, 51, 51, 0); // player shirt color.

        public bool isFishing = false;
        public const int maxFishingTicks = 120;
        public int fishingTicks = maxFishingTicks;
        public int fishingLevel;

        public Player(Player previousInstance, InputHandler input)
            : base(sprites, Player.maxHealth)
        {
            x = 24;
            y = 24;
            this.input = input;
            inventory = new PlayerInventory();

            //if(previousInstance == null)
            //	inventory.Add(Items.arrowItem, acs);

            potioneffects = new();
            showpotioneffects = true;

            cooldowninfo = 0;
            regentick = 0;

            attackDir = dir;
            armor = 0;
            curArmor = null;
            armorDamageBuffer = 0;
            stamina = maxStamina;
            hunger = maxHunger;

            hungerStamCnt = maxHungerStams[Settings.getIdx("diff")];
            stamHungerTicks = maxHungerTicks;

            if (Game.IsMode("creative"))
            {
                Items.Items.FillCreativeInv(inventory);
            }

            if (previousInstance != null)
            {
                spawnx = previousInstance.spawnx;
                spawny = previousInstance.spawny;
            }
        }

        public string GetDebugHunger()
        {
            return hungerStamCnt + "_" + stamHungerTicks;
        }

        public int GetMultiplier()
        {
            return Game.IsMode("score") ? multiplier : 1;
        }

        private void ResetMultiplier()
        {
            multiplier = 1;
            multipliertime = mtm;
        }

        public void AddMultiplier(int value)
        {
            if (!Game.IsMode("score"))
            {
                return;
            }

            multiplier = Math.Min(MAX_MULTIPLIER, multiplier + value);
            multipliertime = Math.Max(multipliertime, mtm - 5);
        }

        public void TickMultiplier()
        {
            if ((Game.ISONLINE || !Updater.paused) && multiplier > 1)
            {
                if (multipliertime != 0) multipliertime--;
                if (multipliertime <= 0) ResetMultiplier();
            }
        }

        public int GetScore()
        {
            return score;
        }

        public void SetScore(int score)
        {
            this.score = score;
        }

        public void AddScore(int points)
        {
            if (!Game.IsValidClient()) // the server will handle the score.
            {
                score += points * GetMultiplier();
            }
        }

        /**
         * Adds a new potion effect to the player.
         * @param type Type of potion.
         * @param duration How long the effect lasts.
         */
        public void AddPotionEffect(PotionType type, int duration)
        {
            potioneffects.Add(type, duration);
        }

        /**
         * Adds a potion effect to the player.
         * @param type Type of effect.
         */
        public void AddPotionEffect(PotionType type)
        {
            AddPotionEffect(type, type.duration);
        }

        /**
         * Returns all the potion effects currently affecting the player.
         * @return all potion effects on the player.
         */
        public Dictionary<PotionType, int> GetPotionEffects()
        {
            return potioneffects;
        }

        public override void Tick()
        {
            if (level == null || IsRemoved())
            {
                return;
            }

            if (Game.GetMenu() != null && !Game.ISONLINE)
            {
                return; // don't tick player when menu is open
            }

            base.Tick(); // ticks Mob.cs

            if (!Game.IsValidClient())
            {
                TickMultiplier();
            }

            if (potioneffects.Count > 0 && !Bed.InBed(this))
            {
                foreach (PotionType potionType in potioneffects.Keys)
                {
                    if (potioneffects[potionType] <= 1) // if time is zero (going to be set to 0 in a moment)...
                    {
                        PotionItem.ApplyPotion(this, potionType, false); // automatically removes this potion effect.
                    }
                    else
                    {
                        potioneffects[potionType] = potioneffects[potionType] - 1; // otherwise, replace it with one less.
                    }
                }
            }

            if (isFishing)
            {
                if (!Bed.InBed(this) && !IsSwimming())
                {
                    fishingTicks--;

                    if (fishingTicks <= 0)
                    {
                        // checks to make sure that the client doesn't drop a "fake" item
                        if (!Game.IsConnectedClient())
                        {
                            GoFishing();
                        }
                    }
                }
                else
                {
                    isFishing = false;
                    fishingTicks = maxFishingTicks;
                }
            }

            if (cooldowninfo > 0)
            {
                cooldowninfo--;
            }

            if (input.GetKey("potionEffects").clicked && cooldowninfo == 0)
            {
                cooldowninfo = 10;
                showpotioneffects = !showpotioneffects;
            }

            Tile onTile = level.GetTile(x >> 4, y >> 4); // gets the current tile the player is on.

            if (onTile == Tiles.Get("Stairs Down") || onTile == Tiles.Get("Stairs Up"))
            {
                if (onStairDelay <= 0)
                { // when the delay time has passed...
                    World.ScheduleLevelChange((onTile == Tiles.Get("Stairs Up")) ? 1 : -1); // decide whether to go up or down.

                    onStairDelay = 10; // resets delay, since the level has now been changed.

                    return; // SKIPS the rest of the tick() method.
                }

                onStairDelay = 10; //resets the delay, if on a stairs tile, but the delay is greater than 0. In other words, this prevents you from ever activating a level change on a stair tile, UNTIL you get off the tile for 10+ ticks.
            }
            else if (onStairDelay > 0)
            {
                onStairDelay--; // decrements stairDelay if it's > 0, but not on stair tile... does the player get removed from the tile beforehand, or something?
            }

            if (onTile == Tiles.Get("Infinite Fall") && !Game.IsMode("creative"))
            {
                if (onFallDelay <= 0)
                {
                    World.ScheduleLevelChange(-1);

                    onFallDelay = 40;

                    return;
                }
            }
            else if (onFallDelay > 0)
            {
                onFallDelay--;
            }

            if (Game.IsMode("creative"))
            {
                // prevent stamina/hunger decay in creative mode.
                stamina = maxStamina;
                hunger = maxHunger;
            }

            // remember: staminaRechargeDelay is a penalty delay for when the player uses up all their stamina.
            // staminaRecharge is the rate of stamina recharge, in some sort of unknown units.
            if (stamina <= 0 && staminaRechargeDelay == 0 && staminaRecharge == 0)
            {
                staminaRechargeDelay = 40; // delay before resuming adding to stamina.
            }

            if (staminaRechargeDelay > 0 && stamina < maxStamina)
            {
                staminaRechargeDelay--;
            }

            if (staminaRechargeDelay == 0)
            {
                staminaRecharge++; // ticks since last recharge, accounting for the time potion effect.

                if (IsSwimming() && !potioneffects.ContainsKey(PotionType.Swim))
                {
                    staminaRecharge = 0; //don't recharge stamina while swimming.
                }

                // recharge a bolt for each multiple of maxStaminaRecharge.
                while (staminaRecharge > maxStaminaRecharge)
                {
                    staminaRecharge -= maxStaminaRecharge;

                    if (stamina < maxStamina)
                    {
                        stamina++; // recharge one stamina bolt per "charge".
                    }
                }
            }

            int diffIdx = Settings.GetIdx("diff");

            if (hunger < 0)
            {
                hunger = 0; // error correction
            }

            if (stamina < maxStamina)
            {
                stamHungerTicks -= diffIdx; // affect hunger if not at full stamina; this is 2 levels away from a hunger "burger".

                if (stamina == 0)
                {
                    stamHungerTicks -= diffIdx; // double effect if no stamina at all.
                }
            }

            // this if statement encapsulates the hunger system
            if (!Bed.InBed(this))
            {
                if (hungerChargeDelay > 0)
                { // if the hunger is recharging health...
                    stamHungerTicks -= 2 + diffIdx; // penalize the hunger

                    if (hunger == 0)
                    {
                        stamHungerTicks -= diffIdx; // further penalty if at full hunger
                    }
                }

                if (Updater.tickCount % Player.hungerTickCount[diffIdx] == 0)
                {
                    stamHungerTicks--; // hunger due to time.
                }

                if (stepCount >= Player.hungerStepCount[diffIdx])
                {
                    stamHungerTicks--; // hunger due to exercise.
                    stepCount = 0; // reset.
                }

                if (stamHungerTicks <= 0)
                {
                    stamHungerTicks += maxHungerTicks; // reset stamHungerTicks
                    hungerStamCnt--; // enter 1 level away from burger.
                }

                while (hungerStamCnt <= 0)
                {
                    hunger--; // reached burger level.
                    hungerStamCnt += maxHungerStams[diffIdx];
                }

                /// system that heals you depending on your hunger
                if (health < maxHealth && hunger > maxHunger / 2)
                {
                    hungerChargeDelay++;

                    if (hungerChargeDelay > 20 * Math.Pow(maxHunger - hunger + 2, 2))
                    {
                        health++;
                        hungerChargeDelay = 0;
                    }
                }
                else
                {
                    hungerChargeDelay = 0;
                }

                if (hungerStarveDelay == 0)
                {
                    hungerStarveDelay = 120;
                }

                if (hunger == 0 && health > minStarveHealth[diffIdx])
                {
                    if (hungerStarveDelay > 0)
                    {
                        hungerStarveDelay--;
                    }

                    if (hungerStarveDelay == 0)
                    {
                        Hurt(this, 1, Direction.NONE); // do 1 damage to the player
                    }
                }
            }

            // regen health
            if (potioneffects.ContainsKey(PotionType.Regen))
            {
                regentick++;

                if (regentick > 60)
                {
                    regentick = 0;

                    if (health < 10)
                    {
                        health++;
                    }
                }
            }

            if (Updater.savecooldown > 0 && !Updater.saving)
            {
                Updater.savecooldown--;
            }

            if (Game.GetMenu() == null && !Bed.InBed(this))
            {
                // this is where movement detection occurs.
                int xmov = 0, ymov = 0;

                if (onFallDelay <= 0)
                { // prevent movement while falling
                    if (input.GetKey("move-up").down) ymov--;
                    if (input.GetKey("move-down").down) ymov++;
                    if (input.GetKey("move-left").down) xmov--;
                    if (input.GetKey("move-right").down) xmov++;
                }

                //executes if not saving; and... essentially halves speed if out of stamina.
                if ((xmov != 0 || ymov != 0) && (staminaRechargeDelay % 2 == 0 || IsSwimming()) && !Updater.saving)
                {
                    double spd = moveSpeed * (potioneffects.ContainsKey(PotionType.Speed) ? 1.5D : 1);
                    int xd = (int)(xmov * spd);
                    int yd = (int)(ymov * spd);
                    Direction newDir = Direction.GetDirection(xd, yd);

                    if (newDir == Direction.NONE)
                    {
                        newDir = dir;
                    }

                    if ((xd != 0 || yd != 0 || newDir != dir) && Game.IsConnectedClient() && this == Game.player)
                    {
                        Game.client.move(this, x + xd, y + yd);
                    }

                    bool moved = Move(xd, yd); // THIS is where the player moves; part of Mob.java

                    if (moved)
                    {
                        stepCount++;
                    }
                }


                if (IsSwimming() && tickTime % 60 == 0 && !potioneffects.ContainsKey(PotionType.Swim))
                { // if drowning... :P
                    if (stamina > 0)
                    {
                        PayStamina(1); // take away stamina
                    }
                    else
                    {
                        Hurt(this, 1, Direction.NONE); // if no stamina, take damage.
                    }
                }

                if (activeItem != null && (input.GetKey("drop-one").clicked || input.GetKey("drop-stack").clicked))
                {
                    Item drop = activeItem.Clone();

                    if (input.GetKey("drop-one").clicked && drop is StackableItem && ((StackableItem)drop).count > 1)
                    {
                        // drop one from stack
                        ((StackableItem)activeItem).count--;
                        ((StackableItem)drop).count = 1;
                    }
                    else if (!Game.IsMode("creative"))
                    {
                        activeItem = null; // remove it from the "inventory"
                    }

                    if (Game.IsValidClient())
                    {
                        Game.client.dropItem(drop);
                    }
                    else
                    {
                        level.DropItem(x, y, drop);
                    }
                }

                if ((activeItem == null || !activeItem.used_pending) && (input.GetKey("attack").clicked) && stamina != 0 && onFallDelay <= 0)
                { // this only allows attacks when such action is possible.
                    if (!potioneffects.ContainsKey(PotionType.Energy))
                    {
                        stamina--;
                    }

                    staminaRecharge = 0;

                    Attack();

                    if (Game.ISONLINE && activeItem != null && activeItem.InteractsWithWorld() && !(activeItem is ToolItem))
                    {
                        activeItem.used_pending = true;
                    }
                }

                if (input.GetKey("menu").clicked && activeItem != null)
                {
                    inventory.Add(0, activeItem);
                    activeItem = null;
                }

                if (Game.GetMenu() == null)
                {
                    if (input.GetKey("menu").clicked && !Use()) // !use() = no furniture in front of the player; this prevents player inventory from opening (will open furniture inventory instead)
                    {
                        Game.SetMenu(new PlayerInvDisplay(this));
                    }

                    if (input.GetKey("pause").clicked)
                    {
                        Game.SetMenu(new PauseDisplay());
                    }

                    if (input.GetKey("craft").clicked && !Use())
                    {
                        Game.SetMenu(new CraftingDisplay(Recipes.craftRecipes, "Crafting", this, true));
                    }

                    if (input.GetKey("info").clicked)
                    {
                        Game.SetMenu(new InfoDisplay());
                    }

                    if (input.GetKey("quicksave").clicked && !Updater.saving && this is RemotePlayer && !Game.IsValidClient())
                    {
                        Updater.saving = true;
                        LoadingDisplay.SetPercentage(0);
                        new Save(WorldSelectDisplay.GetWorldName());
                    }

                    //debug feature:
                    if (Game.debug && input.GetKey("shift-p").clicked)
                    { // remove all potion effects
                        foreach (PotionType potionType in potioneffects.Keys)
                        {
                            PotionItem.ApplyPotion(this, potionType, false);

                            if (Game.IsConnectedClient() && this == Game.player)
                            {
                                Game.client.sendPotionEffect(potionType, false);
                            }
                        }
                    }

                    if (input.GetKey("pickup").clicked && (activeItem == null || !activeItem.used_pending))
                    {
                        if (!(activeItem is PowerGloveItem))
                        { // if you are not already holding a power glove (aka in the middle of a separate interaction)...
                            prevItem = activeItem; // then save the current item...
                            activeItem = new PowerGloveItem(); // and replace it with a power glove.
                        }

                        Attack(); // attack (with the power glove)

                        if (!Game.ISONLINE)
                        {
                            ResolveHeldItem();
                        }
                    }
                }

                if (attackTime > 0)
                {
                    attackTime--;

                    if (attackTime == 0)
                    {
                        attackItem = null; // null the attackItem once we are done attacking.
                    }
                }
            }

            if (Game.IsConnectedClient() && this == Game.player)
            {
                Game.client.sendPlayerUpdate(this);
            }
        }

        /**
         * Removes an held item and places it back into the inventory.
         * Looks complicated to so it can handle the powerglove.
         */
        public void ResolveHeldItem()
        {
            if (activeItem is not PowerGloveItem)
            { // if you are now holding something other than a power glove...
                if (prevItem != null && !Game.IsMode("creative")) // and you had a previous item that we should care about...
                {
                    inventory.Add(0, prevItem); // then add that previous item to your inventory so it isn't lost.
                }
                // if something other than a power glove is being held, but the previous item is null, then nothing happens; nothing added to inventory, and current item remains as the new one.
            }
            else
            {
                activeItem = prevItem; // otherwise, if you're holding a power glove, then the held item didn't change, so we can remove the power glove and make it what it was before.
            }

            prevItem = null; // this is no longer of use.

            if (activeItem is PowerGloveItem) // if, for some odd reason, you are still holding a power glove at this point, then null it because it's useless and shouldn't remain in hand.
            {
                activeItem = null;
            }
        }

        /**
         * This method is called when we press the attack button.
         */
        protected void Attack()
        {
            // walkDist is not synced, so this can happen for both the client and server.
            walkDist += 8; // increase the walkDist (changes the sprite, like you moved your arm)

            if (isFishing)
            {
                isFishing = false;
                fishingTicks = maxFishingTicks;
            }

            // bit of a FIXME for fishing to work on servers
            if (activeItem is FishingRodItem && Game.IsValidClient())
            {
                Point t = GetInteractionTile();
                Tile tile = level.GetTile(t.x, t.y);

                activeItem.InteractOn(tile, level, t.x, t.y, this, attackDir);
            }

            if (activeItem != null && !activeItem.InteractsWithWorld())
            {
                attackDir = dir; // make the attack direction equal the current direction
                attackItem = activeItem; // make attackItem equal activeItem
                                         //if (Game.debug) Console.WriteLine(Network.onlinePrefix()+"player is using reflexive item: " + activeItem);
                activeItem.InteractOn(Tiles.Get("rock"), level, 0, 0, this, attackDir);

                if (!Game.IsMode("creative") && activeItem.IsDepleted())
                {
                    activeItem = null;
                }

                return;
            }

            // if this is a multiplayer game, than the server will execute the full method instead.
            if (Game.IsConnectedClient())
            {
                attackDir = dir;
                attackTime = activeItem != null ? 10 : 5;
                attackItem = activeItem;

                Game.client.requestInteraction(this);

                // we are going to use an arrow.
                if (activeItem is ToolItem toolItem // Is the player currently holding a tool?
                    && ((stamina - 1) >= 0) // Does the player have any more stamina left?
                    && (toolItem.type == ToolType.Bow) // Is the item a bow?
                    && (inventory.Count(Items.Items.ArrowItem) > 0))
                { // Does the player have an arrow in its inventory?
                    inventory.RemoveItem(Items.Items.ArrowItem); // Remove the arrow from the inventory.
                }

                return;
            }

            attackDir = dir; // make the attack direction equal the current direction
            attackItem = activeItem; // make attackItem equal activeItem

            // If the player is holding a tool, and has stamina available do this.
            if (activeItem is ToolItem tool && stamina - 1 >= 0)
            {
                if (tool.type == ToolType.Bow && tool.dur > 0 && inventory.Count(Items.Items.ArrowItem) > 0)
                { // if the player is holding a bow, and has arrows...
                    if (!Game.IsMode("creative"))
                    {
                        inventory.RemoveItem(Items.Items.ArrowItem);
                    }

                    level.Add(new Arrow(this, attackDir, tool.level));
                    attackTime = 10;

                    if (!Game.IsMode("creative"))
                    {
                        tool.dur--;
                    }

                    return; // we have attacked!
                }
            }

            // if we are simply holding an item...
            if (activeItem != null)
            {
                attackTime = 10; // attack time will be set to 10.
                bool done = false;

                // if the interaction between you and an entity is successful, then return.
                if (Interact(GetInteractionBox(INTERACT_DIST)))
                {
                    return;
                }

                // otherwise, attempt to interact with the tile.
                Point t = GetInteractionTile();

                if (t.x >= 0 && t.y >= 0 && t.x < level.w && t.y < level.h)
                { // if the target coordinates are a valid tile...
                    List<Entity> tileEntities = level.GetEntitiesInTiles(t.x, t.y, t.x, t.y, false, e => e is ItemEntity);

                    if (tileEntities.Count == 0 || tileEntities.Count == 1 && tileEntities[0] == this)
                    {
                        Tile tile = level.GetTile(t.x, t.y);

                        if (activeItem.InteractOn(tile, level, t.x, t.y, this, attackDir))
                        { // returns true if your held item successfully interacts with the target tile.
                            done = true;
                        }
                        else
                        { // item can't interact with tile
                            if (tile.Interact(level, t.x, t.y, this, activeItem, attackDir))
                            { // returns true if the target tile successfully interacts with the item.
                                done = true;
                            }
                        }
                    }

                    if (Game.IsValidServer() && this is RemotePlayer remotePlayer)
                    {// only do this if no interaction was actually made; b/c a tile update packet will generally happen then anyway.
                        MinicraftServerThread thread = Game.server.GetAssociatedThread(remotePlayer);
                        //if(thread != null)
                        thread.SendTileUpdate(level, t.x, t.y); /// FIXME this part is as a semi-temporary fix for those odd tiles that don't update when they should; instead of having to make another system like the entity additions and removals (and it wouldn't quite work as well for this anyway), this will just update whatever tile the player interacts with (and fails, since a successful interaction changes the tile and therefore updates it anyway).
                    }

                    if (!Game.IsMode("creative") && activeItem.IsDepleted())
                    {
                        // if the activeItem has 0 items left, then "destroy" it.
                        activeItem = null;
                    }
                }

                if (done)
                {
                    return; // skip the rest if interaction was handled
                }
            }

            if (activeItem == null || activeItem.CanAttack())
            { // if there is no active item, OR if the item can be used to attack...
                attackTime = 5;
                // attacks the enemy in the appropriate direction.
                bool used = Hurt(GetInteractionBox(ATTACK_DIST));

                // attempts to hurt the tile in the appropriate direction.
                Point t = GetInteractionTile();

                if (t.x >= 0 && t.y >= 0 && t.x < level.w && t.y < level.h)
                {
                    Tile tile = level.GetTile(t.x, t.y);
                    used = tile.Hurt(level, t.x, t.y, this, random.NextInt(3) + 1, attackDir) || used;
                }

                if (used && activeItem is ToolItem toolItem)
                {
                    toolItem.PayDurability();
                }
            }
        }

        private Rectangle GetInteractionBox(int range)
        {
            int x = this.x, y = this.y - 2;

            //noinspection UnnecessaryLocalVariable
            int paraClose = 4, paraFar = range;
            int perpClose = 0, perpFar = 8;

            int xClose = x + dir.GetX() * paraClose + dir.GetY() * perpClose;
            int yClose = y + dir.GetY() * paraClose + dir.GetX() * perpClose;
            int xFar = x + dir.GetX() * paraFar + dir.GetY() * perpFar;
            int yFar = y + dir.GetY() * paraFar + dir.GetX() * perpFar;

            return new Rectangle(Math.Min(xClose, xFar), Math.Min(yClose, yFar), Math.Max(xClose, xFar), Math.Max(yClose, yFar), Rectangle.CORNERS);
        }

        private Point GetInteractionTile()
        {
            int x = this.x, y = this.y - 2;

            x += dir.GetX() * INTERACT_DIST;
            y += dir.GetY() * INTERACT_DIST;

            return new Point(x >> 4, y >> 4);
        }

        private void GoFishing()
        {
            int fcatch = random.NextInt(100);

            bool caught = false;

            // figure out which table to roll for
            string[] data = null;

            if (fcatch > FishingRodItem.GetChance(0, fishingLevel))
            {
                data = FishingData.fishData;
            }
            else if (fcatch > FishingRodItem.GetChance(1, fishingLevel))
            {
                data = FishingData.junkData;
            }
            else if (fcatch > FishingRodItem.GetChance(2, fishingLevel))
            {
                data = FishingData.toolData;
            }
            else if (fcatch >= FishingRodItem.GetChance(3, fishingLevel))
            {
                data = FishingData.rareData;
            }

            if (data != null)
            { // if you've caught something
                foreach (string line in data)
                {
                    // check all the entries in the data
                    // the number is a percent, if one fails, it moves down the list
                    // for entries with a "," it chooses between the options
                    int chance = int.Parse(line.Split(":")[0]);
                    string itemData = line.Split(":")[1];

                    if (random.NextInt(100) + 1 <= chance)
                    {
                        if (itemData.Contains(","))
                        { // if it has multiple items choose between them
                            string[] extendedData = itemData.Split(",");
                            int randomChance = random.NextInt(extendedData.Length);
                            itemData = extendedData[randomChance];
                        }
                        if (itemData.StartsWith(";"))
                        {
                            // for secret messages :=)
                            Game.notifications.Add(itemData[1..]);
                        }
                        else
                        {
                            level.DropItem(x, y, Items.Items.Get(itemData));
                            caught = true;
                            break; // don't let people catch more than one thing with one use
                        }
                    }
                }
            }
            else
            {
                caught = true; // end this fishing session
            }

            if (caught)
            {
                isFishing = false;

                if (Game.IsValidServer())
                {
                    Game.server.BroadcastStopFishing(this.eid);
                }
            }

            fishingTicks = maxFishingTicks; // if you didn't catch anything, try again in 120 ticks
        }

        private bool Use()
        {
            return Use(GetInteractionBox(INTERACT_DIST));
        }

        /** called by other use method; this serves as a buffer in case there is no entity in front of the player. */
        private bool Use(Rectangle area)
        {
            List<Entity> entities = level.GetEntitiesInRect(area); // gets the entities within the 4 points

            foreach (Entity e in entities)
            {
                if (e is Furniture.Furniture furniture && furniture.Use(this))
                {
                    return true; // if the entity is not the player, then call it's use method, and return the result. Only some furniture classes use this.
                }
            }

            return false;
        }

        /** same, but for interaction. */
        private bool Interact(Rectangle area)
        {
            List<Entity> entities = level.GetEntitiesInRect(area);

            foreach (Entity e in entities)
            {
                if (e != this && e.Interact(this, activeItem, attackDir))
                {
                    return true; // this is the ONLY place that the Entity.interact method is actually called.
                }
            }

            return false;
        }

        /** same, but for attacking. */
        private bool Hurt(Rectangle area)
        {
            List<Entity> entities = level.GetEntitiesInRect(area);

            int maxDmg = 0;

            foreach (Entity e in entities)
            {
                if (e != this && e is Mob mob)
                {
                    int dmg = GetAttackDamage(e);

                    maxDmg = Math.Max(dmg, maxDmg);
                    mob.Hurt(this, dmg, attackDir);
                }

                if (e is Furniture.Furniture)
                {
                    e.Interact(this, null, attackDir);
                }
            }

            return maxDmg > 0;
        }

        /**
         * Calculates how much damage the player will do.
         * @param e Entity being attacked.
         * @return How much damage the player does.
         */
        private int GetAttackDamage(Entity e)
        {
            int dmg = random.NextInt(2) + 1;

            if (activeItem != null && activeItem is ToolItem toolItem)
            {
                dmg += toolItem.GetAttackDamageBonus(e); // sword/axe are more effective at dealing damage.
            }

            return dmg;
        }

        public override void Render(Screen screen)
        {
            MobSprite[][] spriteSet; // the default, walking sprites.

            if (activeItem is FurnitureItem)
            {
                spriteSet = skinon ? carrySuitSprites : carrySprites;
            }
            else
            {
                spriteSet = skinon ? suitSprites : sprites;
            }

            /* offset locations to start drawing the sprite relative to our position */
            int xo = x - 8; // horizontal
            int yo = y - 11; // vertical

            // Renders swimming
            if (IsSwimming())
            {
                yo += 4; // y offset is moved up by 4

                if (level.GetTile(x / 16, y / 16) == Tiles.Get("water"))
                {
                    screen.Render(xo + 0, yo + 3, 5 + 2 * 32, 0, 3); // render the water graphic
                    screen.Render(xo + 8, yo + 3, 5 + 2 * 32, 1, 3); // render the mirrored water graphic to the right.
                }
                else if (level.GetTile(x / 16, y / 16) == Tiles.Get("lava"))
                {
                    screen.Render(xo + 0, yo + 3, 6 + 2 * 32, 1, 3); // render the water graphic
                    screen.Render(xo + 8, yo + 3, 6 + 2 * 32, 0, 3); // render the mirrored water graphic to the right.
                }
            }

            // Renders indicator for what tile the item will be placed on
            if (activeItem is TileItem)
            {
                Point t = GetInteractionTile();

                screen.Render(t.x * 16 + 4, t.y * 16 + 4, 3 + 4 * 32, 0, 3);
            }

            // Makes the player white if they have just gotten hurt
            if (hurtTime > playerHurtTime - 10)
            {
                col = Color.WHITE; // make the sprite white.
            }

            // Renders falling
            MobSprite curSprite;

            if (onFallDelay > 0)
            {
                // what this does is make falling look really cool
                float spriteToUse = onFallDelay / 2f;

                while (spriteToUse > spriteSet.Length - 1)
                {
                    spriteToUse -= 4;
                }

                curSprite = spriteSet[(int)Math.Round(spriteToUse)][(walkDist >> 3) & 1];
            }
            else
            {
                curSprite = spriteSet[dir.GetDir()][(walkDist >> 3) & 1]; // gets the correct sprite to render.
            }

            // render each corner of the sprite
            if (!IsSwimming())
            { // don't render the bottom half if swimming.
                curSprite.render(screen, xo, yo - 4 * onFallDelay, -1, shirtColor);
            }
            else
            {
                curSprite.renderRow(0, screen, xo, yo, -1, shirtColor);
            }

            // renders slashes:
            if (attackTime > 0)
            {
                if (attackDir == UP)  // if currently attacking upwards...
                {
                    screen.Render(xo + 0, yo - 4, 3 + 2 * 32, 0, 3); //render left half-slash
                    screen.Render(xo + 8, yo - 4, 3 + 2 * 32, 1, 3); //render right half-slash (mirror of left).
                    if (attackItem != null && !(attackItem is PowerGloveItem))
                    { // if the player had an item when they last attacked...
                        attackItem.sprite.Render(screen, xo + 4, yo - 4, 1); // then render the icon of the item, mirrored
                    }
                }

                if (attackDir == LEFT)
                {  // attacking to the left... (Same as above)
                    screen.Render(xo - 4, yo, 4 + 2 * 32, 1, 3);
                    screen.Render(xo - 4, yo + 8, 4 + 2 * 32, 3, 3);

                    if (attackItem != null && !(attackItem is PowerGloveItem))
                    {
                        attackItem.sprite.Render(screen, xo - 4, yo + 4, 1);
                    }
                }

                if (attackDir == RIGHT)
                {  // attacking to the right (Same as above)
                    screen.Render(xo + 8 + 4, yo, 4 + 2 * 32, 0, 3);
                    screen.Render(xo + 8 + 4, yo + 8, 4 + 2 * 32, 2, 3);

                    if (attackItem != null && !(attackItem is PowerGloveItem))
                    {
                        attackItem.sprite.Render(screen, xo + 8 + 4, yo + 4);
                    }
                }

                if (attackDir == DOWN) // attacking downwards (Same as above)
                {
                    screen.Render(xo + 0, yo + 8 + 4, 3 + 2 * 32, 2, 3);
                    screen.Render(xo + 8, yo + 8 + 4, 3 + 2 * 32, 3, 3);

                    if (attackItem != null && !(attackItem is PowerGloveItem))
                    {
                        attackItem.sprite.Render(screen, xo + 4, yo + 8 + 4);
                    }
                }
            }

            // Renders the fishing rods when fishing
            if (isFishing)
            {
                if (dir == UP)
                {
                    screen.Render(xo + 4, yo - 4, fishingLevel + 11 * 32, 1);
                }

                if (dir == LEFT)
                {
                    screen.Render(xo - 4, yo + 4, fishingLevel + 11 * 32, 1);
                }

                if (dir == RIGHT)
                {
                    screen.Render(xo + 8 + 4, yo + 4, fishingLevel + 11 * 32, 0);
                }

                if (dir == DOWN)
                {
                    screen.Render(xo + 4, yo + 8 + 4, fishingLevel + 11 * 32, 0);
                }
            }

            // Renders the furniture if the player is holding one.
            if (activeItem is FurnitureItem furnitureItem)
            {
                Furniture.Furniture furniture = furnitureItem.furniture;

                furniture.x = x;
                furniture.y = yo - 4;

                furniture.Render(screen);
            }
        }

        /** What happens when the player interacts with a itemEntity */
        public void PickupItem(ItemEntity itemEntity)
        {
            Sound.pickup.Play();
            itemEntity.Remove();

            AddScore(1);

            if (Game.IsMode("creative"))
            {
                return; // we shall not bother the inventory on creative mode.
            }

            if (itemEntity.item is StackableItem stackableItem && activeItem is StackableItem otherStackableItem && stackableItem.stacksWith(activeItem)) // picked up item equals the one in your hand
            {
                otherStackableItem.count += stackableItem.count;
            }
            else
            {
                inventory.Add(itemEntity.item); // add item to inventory
            }
        }

        // the player can swim.
        public override bool CanSwim()
        {
            return true;
        }

        // can walk on wool tiles..? quickly..?
        public override bool CanWool()
        {
            return true;
        }

        /**
         * Finds a starting position for the player.
         * @param level Level which the player wants to start in.
         * @param spawnSeed Spawnseed.
         */
        public void FindStartPos(Level level, long spawnSeed)
        {
            random.SetSeed(spawnSeed);

            FindStartPos(level);
        }

        /**
         * Finds the starting position for the player in a level.
         * @param level The level.
         */
        public void FindStartPos(Level level)
        {
            FindStartPos(level, true);
        }

        public void FindStartPos(Level level, bool setSpawn)
        {
            Point spawnPos;

            List<Point> spawnTilePositions = level.GetMatchingTiles(Tiles.Get("grass"));

            if (spawnTilePositions.Count == 0)
            {
                spawnTilePositions.AddRange(level.GetMatchingTiles((t, x, y) => t.MaySpawn()));
            }

            if (spawnTilePositions.Count == 0)
            {
                spawnTilePositions.AddRange(level.GetMatchingTiles((t, x, y) => t.MayPass(level, x, y, this)));
            }

            // there are no tiles in the entire map which the player is allowed to stand on. Not likely.
            if (spawnTilePositions.Count == 0)
            {
                spawnPos = new Point(random.NextInt(level.w / 4) + level.w * 3 / 8, random.NextInt(level.h / 4) + level.h * 3 / 8);
                level.SetTile(spawnPos.x, spawnPos.y, Tiles.Get("grass"));
            }
            else // gets random valid spawn tile position.
            {
                spawnPos = spawnTilePositions[random.NextInt(spawnTilePositions.Count)];
            }

            if (setSpawn)
            {
                // used to save (tile) coordinates of spawnpoint outside of this method.
                spawnx = spawnPos.x;
                spawny = spawnPos.y;
            }

            // set (entity) coordinates of player to the center of the tile.
            this.x = spawnPos.x * 16 + 8; // conversion from tile coords to entity coords.
            this.y = spawnPos.y * 16 + 8;
        }

        /**
         * Finds a location where the player can respawn in a given level.
         * @param level The level.
         * @return true
         */
        public bool Respawn(Level level)
        {
            if (!level.GetTile(spawnx, spawny).MaySpawn())
            {
                FindStartPos(level); // if there's no bed to spawn from, and the stored coordinates don't point to a grass tile, then find a new point.
            }

            // move the player to the spawnpoint
            this.x = spawnx * 16 + 8;
            this.y = spawny * 16 + 8;

            return true; // again, why the "return true"'s for methods that never return false?
        }

        /**
         * Uses an amount of stamina to do an action.
         * @param cost How much stamina the action requires.
         * @return true if the player had enough stamina, false if not.
         */
        public bool PayStamina(int cost)
        {
            if (potioneffects.ContainsKey(PotionType.Energy))
            {
                return true; // if the player has the potion effect for infinite stamina, return true (without subtracting cost).
            }
            else if (stamina <= 0)
            {
                return false; // if the player doesn't have enough stamina, then return false; failure.
            }

            if (cost < 0)
            {
                cost = 0; // error correction
            }

            stamina -= Math.Min(stamina, cost); // subtract the cost from the current stamina

            if (Game.IsValidServer() && this is RemotePlayer remotePlayer)
            {
                Game.server.GetAssociatedThread(remotePlayer).SendStaminaChange(cost);
            }

            return true; // success
        }

        /** 
         * Gets the player's light radius underground 
         */
        public override int GetLightRadius()
        {
            int r = 5; // the radius of the light.

            if (activeItem != null && activeItem is FurnitureItem furnitureItem)
            { // if player is holding furniture
                int rr = furnitureItem.furniture.GetLightRadius(); // gets furniture light radius

                if (rr > r)
                {
                    r = rr; // brings player light up to furniture light, if less, since the furnture is not yet part of the level and so doesn't emit light even if it should.
                }
            }

            return r; // return light radius
        }

        /** What happens when the player dies */
        public override void Die()
        {
            score -= score / 3; // subtracts score penalty (minus 1/3 of the original score)

            ResetMultiplier();

            //make death chest
            DeathChest dc = new DeathChest(this);

            if (activeItem != null)
            {
                dc.GetInventory().Add(activeItem);
            }

            if (curArmor != null)
            {
                dc.GetInventory().Add(curArmor);
            }

            Sound.playerDeath.Play();

            if (!Game.ISONLINE)
            {
                World.levels[Game.currentLevel].Add(dc);
            }
            else if (Game.IsConnectedClient())
            {
                Game.client.SendPlayerDeath(this, dc);
            }

            base.Die(); // calls the die() method in Mob.cs
        }

        public override void Hurt(Tnt tnt, int dmg)
        {
            base.Hurt(tnt, dmg);

            PayStamina(dmg * 2);
        }

        /** Hurt the player.
         * @param damage How much damage to do to player.
         * @param attackDir What direction to attack.
         */
        public void Hurt(int damage, Direction attackDir)
        {
            DoHurt(damage, attackDir);
        }

        protected override void DoHurt(int damage, Direction attackDir)
        {
            if (Game.IsMode("creative") || hurtTime > 0 || Bed.InBed(this))
            {
                return; // can't get hurt in creative, hurt cooldown, or while someone is in bed
            }

            if (Game.IsValidServer() && this is RemotePlayer)
            {
                // let the clients deal with it.
                Game.server.BroadcastPlayerHurt(eid, damage, attackDir);

                return;
            }

            bool fullPlayer = !(Game.IsValidClient() && this != Game.player);

            int healthDam = 0, armorDam = 0;

            if (fullPlayer)
            {
                if (curArmor == null)
                { // no armor
                    healthDam = damage; // subtract that amount
                }
                else
                { // has armor
                    armorDamageBuffer += damage;
                    armorDam += damage;

                    while (armorDamageBuffer >= curArmor.level + 1)
                    {
                        armorDamageBuffer -= curArmor.level + 1;
                        healthDam++;
                    }
                }

                // adds a text particle telling how much damage was done to the player, and the armor.
                if (armorDam > 0)
                {
                    level.Add(new TextParticle("" + damage, x, y, Color.GRAY));
                    armor -= armorDam;

                    if (armor <= 0)
                    {
                        healthDam -= armor; // adds armor damage overflow to health damage (minus b/c armor would be negative)
                        armor = 0;
                        armorDamageBuffer = 0; // ensures that new armor doesn't inherit partial breaking from this armor.
                        curArmor = null; // removes armor
                    }
                }
            }

            if (healthDam > 0 || !fullPlayer)
            {
                level.Add(new TextParticle("" + damage, x, y, Color.Get(-1, 504)));

                if (fullPlayer)
                {
                    base.DoHurt(healthDam, attackDir); // sets knockback, and takes away health.
                }
            }

            Sound.playerHurt.Play();
            hurtTime = playerHurtTime;
        }

        public override void Remove()
        {
            if (Game.debug)
            {
                Console.WriteLine(Network.OnlinePrefix() + "Removing player from level " + GetLevel());
                //Thread.dumpStack();
            }

            base.Remove();
        }

        protected override string GetUpdateString()
        {
            string updates = base.GetUpdateString() + ";";
            updates += "skinon," + skinon +
            ";shirtColor," + shirtColor +
            ";armor," + armor +
            ";stamina," + stamina +
            ";health," + health +
            ";hunger," + hunger +
            ";attackTime," + attackTime +
            ";attackDir," + attackDir.Ordinal +
            ";activeItem," + (activeItem == null ? "null" : activeItem.GetData()) +
            ";isFishing," + (isFishing ? "1" : "0");

            return updates;
        }

        protected override bool UpdateField(string field, string val)
        {
            if (base.UpdateField(field, val))
            {
                return true;
            }

            switch (field)
            {
                case "skinon": skinon = bool.Parse(val); return true;
                case "shirtColor": shirtColor = int.Parse(val); return true;
                case "armor": armor = int.Parse(val); return true;
                case "stamina": stamina = int.Parse(val); return true;
                case "health": health = int.Parse(val); return true;
                case "hunger": hunger = int.Parse(val); return true;
                case "score": score = int.Parse(val); return true;
                case "mult": multiplier = int.Parse(val); return true;
                case "attackTime": attackTime = int.Parse(val); return true;
                case "attackDir": attackDir = Direction.All[int.Parse(val)]; return true;
                case "activeItem":
                    activeItem = Items.Items.Get(val, true);
                    attackItem = activeItem != null && activeItem.CanAttack() ? activeItem : null;
                    return true;
                case "isFishing": isFishing = int.Parse(val) == 1; return true;
                case "potioneffects":
                    potioneffects.Clear();
                    foreach (string potion in val.Split(":"))
                    {
                        string[] parts = potion.Split("_");
                        potioneffects.Add(PotionType.All[int.Parse(parts[0])], int.Parse(parts[1]));
                    }
                    return true;
            }

            return false;
        }

        public string GetPlayerData()
        {
            List<string> datalist = new();
            StringBuilder playerdata = new();

            playerdata.Append(Game.VERSION).Append("\n");

            Save.WritePlayer(this, datalist);

            foreach (string str in datalist)
            {
                if (str.Length > 0)
                {
                    playerdata.Append(str).Append(",");
                }
            }

            playerdata = new StringBuilder(playerdata.ToString(0, playerdata.Length - 1) + "\n");

            Save.WriteInventory(this, datalist);

            foreach (string str in datalist)
            {
                if (str.Length > 0)
                {
                    playerdata.Append(str).Append(",");
                }
            }

            if (datalist.Count == 0)
            {
                playerdata.Append("null");
            }
            else
            {
                playerdata = new StringBuilder(playerdata.ToString(0, playerdata.Length - 1));
            }

            return playerdata.ToString();
        }

        public Inventory GetInventory()
        {
            return inventory;
        }
    }
}
