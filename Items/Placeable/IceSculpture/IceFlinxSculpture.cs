using SpiritMod.Items.Sets.CryoliteSet;
using SpiritMod.Items.Placeable.Tiles;
using SpiritMod.Tiles.Ambient.IceSculpture;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace SpiritMod.Items.Placeable.IceSculpture
{
	[Sacrifice(1)]
	public class IceFlinxSculpture : ModItem
	{
		public override void SetDefaults()
		{
			Item.width = 30;
			Item.height = 40;
			Item.value = Item.sellPrice(0, 0, 15, 0);
			Item.maxStack = Item.CommonMaxStack;
			Item.useStyle = ItemUseStyleID.Swing;
			Item.useTime = 10;
			Item.useAnimation = 15;
			Item.useTurn = true;
			Item.autoReuse = true;
			Item.consumable = true;
			Item.createTile = ModContent.TileType<IceFlinxDecor>();
		}

		public override void AddRecipes()
		{
			Recipe recipe = CreateRecipe();
			recipe.AddIngredient(ModContent.ItemType<CreepingIce>(), 20);
			recipe.AddIngredient(ModContent.ItemType<CryoliteBar>(), 2);
			recipe.AddTile(TileID.Anvils);
			recipe.DisableDecraft();
			recipe.Register();
		}
	}
}