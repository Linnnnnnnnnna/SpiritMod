using Terraria.ID;
using Terraria.ModLoader;

namespace SpiritMod.Items.BossLoot.ScarabeusDrops
{
	[Sacrifice(1)]
	public class Trophy1 : ModItem
	{
		public override void SetStaticDefaults() => DisplayName.SetDefault("Scarabeus Trophy");

		public override void SetDefaults()
		{
			Item.width = 30;
			Item.height = 30;
			Item.maxStack = 99;
			Item.useTurn = true;
			Item.autoReuse = true;
			Item.useAnimation = 15;
			Item.useTime = 10;
			Item.useStyle = ItemUseStyleID.Swing;
			Item.consumable = true;
			Item.value = Terraria.Item.sellPrice(0, 1, 0, 0);
			Item.rare = ItemRarityID.Blue;
			Item.createTile = Mod.Find<ModTile>("Trophy1Tile").Type;
			Item.placeStyle = 0;
		}
	}
}