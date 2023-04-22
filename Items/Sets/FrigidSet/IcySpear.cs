using SpiritMod.Projectiles.Thrown.Charge;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace SpiritMod.Items.Sets.FrigidSet
{
	public class IcySpear : ModItem
	{
		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Frigid Javelin");
			Tooltip.SetDefault("Hold and release to throw\nHold longer for more velocity and damage\nOccasionally inflicts Frostburn");
		}

		public override void SetDefaults()
		{
			Item.damage = 12;
			Item.crit = 6;
			Item.noMelee = true;
			Item.channel = true;
			Item.rare = ItemRarityID.Blue;
			Item.width = 18;
			Item.height = 18;
			Item.useTime = 15;
			Item.useAnimation = 45;
			Item.useStyle = ItemUseStyleID.Swing;
			Item.useTime = Item.useAnimation = 24;
			Item.knockBack = 6f;
			Item.DamageType = DamageClass.Melee;
			Item.noMelee = true;
			Item.autoReuse = false;
			Item.noUseGraphic = true;
			Item.shoot = ModContent.ProjectileType<FrigidJavelinProj>();
			Item.shootSpeed = 0f;
		}

		public override void AddRecipes()
		{
			Recipe recipe = CreateRecipe();
			recipe.AddIngredient(ModContent.ItemType<FrigidFragment>(), 9);
			recipe.AddTile(TileID.Anvils);
			recipe.Register();
		}
	}
}
