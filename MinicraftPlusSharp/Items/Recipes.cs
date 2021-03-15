using System.Collections.Generic;

namespace MinicraftPlusSharp.Items
{
    public class Recipes
    {
        public static readonly List<Recipe> anvilRecipes = new();
        public static readonly List<Recipe> ovenRecipes = new();
        public static readonly List<Recipe> furnaceRecipes = new();
        public static readonly List<Recipe> workbenchRecipes = new();
        public static readonly List<Recipe> enchantRecipes = new();
        public static readonly List<Recipe> craftRecipes = new();
        public static readonly List<Recipe> loomRecipes = new();

        static Recipes()
        {
            craftRecipes.Add(new Recipe("Workbench_1", "Wood_10"));
            craftRecipes.Add(new Recipe("Torch_2", "Wood_1", "coal_1"));
            craftRecipes.Add(new Recipe("plank_2", "Wood_1"));
            craftRecipes.Add(new Recipe("Plank Wall_1", "plank_3"));
            craftRecipes.Add(new Recipe("Wood Door_1", "plank_5"));

            workbenchRecipes.Add(new Recipe("Torch_2", "Wood_1", "coal_1"));
            workbenchRecipes.Add(new Recipe("Lantern_1", "Wood_8", "slime_4", "glass_3"));
            workbenchRecipes.Add(new Recipe("Stone Brick_2", "Stone_2"));
            workbenchRecipes.Add(new Recipe("Stone Wall_1", "Stone Brick_3"));
            workbenchRecipes.Add(new Recipe("Stone Door_1", "Stone Brick_5"));
            workbenchRecipes.Add(new Recipe("Obsidian Wall_1", "Obsidian Brick_3"));
            workbenchRecipes.Add(new Recipe("Obsidian Door_1", "Obsidian Brick_5"));
            workbenchRecipes.Add(new Recipe("Oven_1", "Stone_15"));
            workbenchRecipes.Add(new Recipe("Furnace_1", "Stone_20"));
            workbenchRecipes.Add(new Recipe("Enchanter_1", "Wood_5", "string_2", "Lapis_10"));
            workbenchRecipes.Add(new Recipe("Chest_1", "Wood_20"));
            workbenchRecipes.Add(new Recipe("Anvil_1", "iron_5"));
            workbenchRecipes.Add(new Recipe("Tnt_1", "Gunpowder_10", "Sand_8"));
            workbenchRecipes.Add(new Recipe("Loom_1", "Wood_10", "Wool_5"));
            workbenchRecipes.Add(new Recipe("Wood Fishing Rod_1", "Wood_10", "string_3"));
            workbenchRecipes.Add(new Recipe("Iron Fishing Rod_1", "Iron_10", "string_3"));
            workbenchRecipes.Add(new Recipe("Gold Fishing Rod_1", "Gold_10", "string_3"));
            workbenchRecipes.Add(new Recipe("Gem Fishing Rod_1", "Gem_10", "string_3"));

            workbenchRecipes.Add(new Recipe("Wood Sword_1", "Wood_5"));
            workbenchRecipes.Add(new Recipe("Wood Axe_1", "Wood_5"));
            workbenchRecipes.Add(new Recipe("Wood Hoe_1", "Wood_5"));
            workbenchRecipes.Add(new Recipe("Wood Pickaxe_1", "Wood_5"));
            workbenchRecipes.Add(new Recipe("Wood Shovel_1", "Wood_5"));
            workbenchRecipes.Add(new Recipe("Wood Bow_1", "Wood_5", "string_2"));
            workbenchRecipes.Add(new Recipe("Rock Sword_1", "Wood_5", "Stone_5"));
            workbenchRecipes.Add(new Recipe("Rock Axe_1", "Wood_5", "Stone_5"));
            workbenchRecipes.Add(new Recipe("Rock Hoe_1", "Wood_5", "Stone_5"));
            workbenchRecipes.Add(new Recipe("Rock Pickaxe_1", "Wood_5", "Stone_5"));
            workbenchRecipes.Add(new Recipe("Rock Shovel_1", "Wood_5", "Stone_5"));
            workbenchRecipes.Add(new Recipe("Rock Bow_1", "Wood_5", "Stone_5", "string_2"));

            workbenchRecipes.Add(new Recipe("arrow_3", "Wood_2", "Stone_2"));
            workbenchRecipes.Add(new Recipe("Leather Armor_1", "leather_10"));
            workbenchRecipes.Add(new Recipe("Snake Armor_1", "scale_15"));

            loomRecipes.Add(new Recipe("string_2", "Wool_1"));
            loomRecipes.Add(new Recipe("red wool_1", "Wool_1", "rose_1"));
            loomRecipes.Add(new Recipe("blue wool_1", "Wool_1", "Lapis_1"));
            loomRecipes.Add(new Recipe("green wool_1", "Wool_1", "Cactus_1"));
            loomRecipes.Add(new Recipe("yellow wool_1", "Wool_1", "Flower_1"));
            loomRecipes.Add(new Recipe("black wool_1", "Wool_1", "coal_1"));
            loomRecipes.Add(new Recipe("Bed_1", "Wood_5", "Wool_3"));

            loomRecipes.Add(new Recipe("blue clothes_1", "cloth_5", "Lapis_1"));
            loomRecipes.Add(new Recipe("green clothes_1", "cloth_5", "Cactus_1"));
            loomRecipes.Add(new Recipe("yellow clothes_1", "cloth_5", "Flower_1"));
            loomRecipes.Add(new Recipe("black clothes_1", "cloth_5", "coal_1"));
            loomRecipes.Add(new Recipe("orange clothes_1", "cloth_5", "rose_1", "Flower_1"));
            loomRecipes.Add(new Recipe("purple clothes_1", "cloth_5", "Lapis_1", "rose_1"));
            loomRecipes.Add(new Recipe("cyan clothes_1", "cloth_5", "Lapis_1", "Cactus_1"));
            loomRecipes.Add(new Recipe("reg clothes_1", "cloth_5"));

            loomRecipes.Add(new Recipe("Leather Armor_1", "leather_10"));

            anvilRecipes.Add(new Recipe("Iron Armor_1", "iron_10"));
            anvilRecipes.Add(new Recipe("Gold Armor_1", "gold_10"));
            anvilRecipes.Add(new Recipe("Gem Armor_1", "gem_65"));
            anvilRecipes.Add(new Recipe("Empty Bucket_1", "iron_5"));
            anvilRecipes.Add(new Recipe("Iron Lantern_1", "iron_8", "slime_5", "glass_4"));
            anvilRecipes.Add(new Recipe("Gold Lantern_1", "gold_10", "slime_5", "glass_4"));
            anvilRecipes.Add(new Recipe("Iron Sword_1", "Wood_5", "iron_5"));
            anvilRecipes.Add(new Recipe("Iron Claymore_1", "Iron Sword_1", "shard_15"));
            anvilRecipes.Add(new Recipe("Iron Axe_1", "Wood_5", "iron_5"));
            anvilRecipes.Add(new Recipe("Iron Hoe_1", "Wood_5", "iron_5"));
            anvilRecipes.Add(new Recipe("Iron Pickaxe_1", "Wood_5", "iron_5"));
            anvilRecipes.Add(new Recipe("Iron Shovel_1", "Wood_5", "iron_5"));
            anvilRecipes.Add(new Recipe("Iron Bow_1", "Wood_5", "iron_5", "string_2"));
            anvilRecipes.Add(new Recipe("Gold Sword_1", "Wood_5", "gold_5"));
            anvilRecipes.Add(new Recipe("Gold Claymore_1", "Gold Sword_1", "shard_15"));
            anvilRecipes.Add(new Recipe("Gold Axe_1", "Wood_5", "gold_5"));
            anvilRecipes.Add(new Recipe("Gold Hoe_1", "Wood_5", "gold_5"));
            anvilRecipes.Add(new Recipe("Gold Pickaxe_1", "Wood_5", "gold_5"));
            anvilRecipes.Add(new Recipe("Gold Shovel_1", "Wood_5", "gold_5"));
            anvilRecipes.Add(new Recipe("Gold Bow_1", "Wood_5", "gold_5", "string_2"));
            anvilRecipes.Add(new Recipe("Gem Sword_1", "Wood_5", "gem_50"));
            anvilRecipes.Add(new Recipe("Gem Claymore_1", "Gem Sword_1", "shard_15"));
            anvilRecipes.Add(new Recipe("Gem Axe_1", "Wood_5", "gem_50"));
            anvilRecipes.Add(new Recipe("Gem Hoe_1", "Wood_5", "gem_50"));
            anvilRecipes.Add(new Recipe("Gem Pickaxe_1", "Wood_5", "gem_50"));
            anvilRecipes.Add(new Recipe("Gem Shovel_1", "Wood_5", "gem_50"));
            anvilRecipes.Add(new Recipe("Gem Bow_1", "Wood_5", "gem_50", "string_2"));
            anvilRecipes.Add(new Recipe("Shear_1", "Iron_4"));

            furnaceRecipes.Add(new Recipe("iron_1", "iron Ore_4", "coal_1"));
            furnaceRecipes.Add(new Recipe("gold_1", "gold Ore_4", "coal_1"));
            furnaceRecipes.Add(new Recipe("glass_1", "sand_4", "coal_1"));

            ovenRecipes.Add(new Recipe("cooked pork_1", "raw pork_1", "coal_1"));
            ovenRecipes.Add(new Recipe("steak_1", "raw beef_1", "coal_1"));
            ovenRecipes.Add(new Recipe("cooked fish_1", "raw fish_1", "coal_1"));
            ovenRecipes.Add(new Recipe("bread_1", "wheat_4"));
            ovenRecipes.Add(new Recipe("Baked Potato_1", "Potato_1"));

            enchantRecipes.Add(new Recipe("Gold Apple_1", "apple_1", "gold_8"));
            enchantRecipes.Add(new Recipe("potion_1", "glass_1", "Lapis_3"));
            enchantRecipes.Add(new Recipe("speed potion_1", "potion_1", "Cactus_5"));
            enchantRecipes.Add(new Recipe("light potion_1", "potion_1", "slime_5"));
            enchantRecipes.Add(new Recipe("swim potion_1", "potion_1", "raw fish_5"));
            enchantRecipes.Add(new Recipe("haste potion_1", "potion_1", "Wood_5", "Stone_5"));
            enchantRecipes.Add(new Recipe("lava potion_1", "potion_1", "Lava Bucket_1"));
            enchantRecipes.Add(new Recipe("energy potion_1", "potion_1", "gem_25"));
            enchantRecipes.Add(new Recipe("regen potion_1", "potion_1", "Gold Apple_1"));
            enchantRecipes.Add(new Recipe("Health Potion_1", "potion_1", "GunPowder_2", "Leather Armor_1"));
            enchantRecipes.Add(new Recipe("Escape Potion_1", "potion_1", "GunPowder_3", "Lapis_7"));
        }
    }
}