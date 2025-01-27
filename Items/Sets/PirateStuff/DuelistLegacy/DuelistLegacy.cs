using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using SpiritMod.Utilities;
using Terraria;
using Terraria.Audio;
using Terraria.Enums;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.DataStructures;
using SpiritMod.Items.Sets.GunsMisc.LadyLuck;
using Terraria.Localization;

namespace SpiritMod.Items.Sets.PirateStuff.DuelistLegacy
{
	public class DuelistLegacy : ModItem
	{
		public bool ChargeReady => charge % 3 == 2;

		private int charge;

		public override void SetStaticDefaults() => ItemID.Sets.ShimmerTransformToItem[Type] = ModContent.ItemType<LadyLuck>();

		public override void SetDefaults()
		{
			Item.damage = 90;
			Item.DamageType = DamageClass.Melee;
			Item.width = 36;
			Item.height = 44;
			Item.useTime = 12;
			Item.useAnimation = 12;
			Item.reuseDelay = 10;
			Item.channel = true;
			Item.useStyle = ItemUseStyleID.Shoot;
			Item.knockBack = 10f;
			Item.value = Item.sellPrice(0, 1, 80, 0);
			Item.crit = 4;
			Item.rare = ItemRarityID.Pink;
			Item.shootSpeed = 1f;
			Item.autoReuse = true;
			Item.shoot = ModContent.ProjectileType<DuelistSlash>();
			Item.noUseGraphic = true;
			Item.noMelee = true;
		}

		public override bool AltFunctionUse(Player player) => true;

		public override bool CanUseItem(Player player)
		{
			if (player.altFunctionUse == 2)
				Item.shoot = ModContent.ProjectileType<DuelistGun>();
			else
				Item.shoot = ModContent.ProjectileType<DuelistSlash>();

			return base.CanUseItem(player);
		}

		public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback) 
		{
			if (player.altFunctionUse == 2)
			{
				Vector2 direction = velocity;
				if (ChargeReady)
				{
					SoundEngine.PlaySound(new SoundStyle("SpiritMod/Sounds/Shotgun2") with { PitchVariance = 0.2f, Volume = 0.4f}, player.Center);

					for (int i = 0; i < 15; i++)
					{
						Dust dust = Dust.NewDustDirect(player.Center + (direction * 20), 0, 0, ModContent.DustType<DuelistBubble2>());
						dust.velocity = direction.RotatedBy(Main.rand.NextFloat(-0.4f, 0.4f)) * Main.rand.NextFloat(2, 10);
					}
					player.GetModPlayer<MyPlayer>().Shake += 12;
					Projectile.NewProjectile(source, position + (direction * 20) + (direction.RotatedBy(-1.57f * player.direction) * 15), direction, ModContent.ProjectileType<DuelistBlastSpecial>(), damage * 2, knockback * 1.5f, player.whoAmI);
				}
				else
				{
					SoundEngine.PlaySound(SoundID.Item14 with { PitchVariance = 0.2f, Volume = 0.35f }, player.Center);
					SoundEngine.PlaySound(new SoundStyle("SpiritMod/Sounds/Shotgun1") with { PitchVariance = 0.6f, Volume = 0.46f }, player.Center);

					for (int i = 0; i < 10; i++)
					{
						Dust dust = Dust.NewDustDirect(player.Center + (direction * 20), 0, 0, ModContent.DustType<DuelistSmoke>());
						dust.velocity = direction.RotatedBy(Main.rand.NextFloat(-0.4f,0.4f)) * Main.rand.NextFloat(5, 15);
						dust.scale = Main.rand.NextFloat(0.5f, 0.75f);
						dust.alpha = 40 + Main.rand.Next(40);
						dust.rotation = Main.rand.NextFloat(6.28f);
					}
					Projectile.NewProjectile(source, position + (direction * 20) + (direction.RotatedBy(-1.57f * player.direction) * 15), direction, ModContent.ProjectileType<DuelistBlast>(), damage, knockback, player.whoAmI);
				}
				charge = 0;
				return true;
			}
			else
			{
				Vector2 direction = velocity;
				SoundEngine.PlaySound(SoundID.Item1 with { PitchVariance = 0.5f }, player.Center);
				Projectile proj = Projectile.NewProjectileDirect(source, position + (direction * 20) + (direction.RotatedBy(-1.57f * player.direction) * 20), Vector2.Zero, ModContent.ProjectileType<DuelistSlash>(), damage, knockback, player.whoAmI);
				var mp = proj.ModProjectile as DuelistSlash;
				mp.Phase = charge % 3;
				charge++;
				return false;
			}
		}
	}

	internal class DuelistSlash : ModProjectile
	{
		public const float SwingRadians = MathHelper.Pi * 0.75f; //Total radians of the sword's arc

		public int Phase;

		public int Timer;

		public Player Player => Main.player[Projectile.owner];

		public bool Empowered => Phase == 2;

		Vector2 direction = Vector2.Zero;

		private bool initialized = false;

		private float rotation;

		private bool flip = false;

		public int MaxFrames
		{
			get
			{
				switch (Phase)
				{
					case 0:
						return 11;
					case 1:
						return 11;
					case 2:
						return 8;
					default:
						return 19;
				}
			}
		}

		public int SwingDirection
		{
			get
			{
				switch (Phase)
				{
					case 0:
						return -1 * Math.Sign(direction.X);
					case 1:
						return 1 * Math.Sign(direction.X);
					default:
						return 0;

				}
			}
		}

		public int SwingTime => MaxFrames * 2;

		public override LocalizedText DisplayName => Language.GetText("Mods.SpiritMod.Items.DuelistLegacy.DisplayName");

		public override void SetStaticDefaults() => Main.projFrames[Projectile.type] = 1;//11, 11, 9, 19

		public override void SetDefaults()
		{
			Projectile.friendly = true;
			Projectile.DamageType = DamageClass.Melee;
			Projectile.tileCollide = false;
			Projectile.Size = new Vector2(150, 250);
			Projectile.penetrate = -1;
			Projectile.usesLocalNPCImmunity = true;
			Projectile.localNPCHitCooldown = 16;
			Projectile.ownerHitCheck = true;
		}

		public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
		{
			Vector2 lineDirection = rotation.ToRotationVector2();
			float collisionPoint = 0;
			if (Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(), Player.Center, Player.Center + (lineDirection * Projectile.width), Projectile.height, ref collisionPoint))
				return true;
			return false;
		}
		public override bool? CanCutTiles()
		{
			return true;
		}

		// Plot a line from the start of the Solar Eruption to the end of it, to change the tile-cutting collision logic. (Don't change this.)
		public override void CutTiles()
		{
			Vector2 lineDirection = rotation.ToRotationVector2();
			DelegateMethods.tilecut_0 = TileCuttingContext.AttackProjectile;
			Utils.PlotTileLine(Player.Center, Player.Center + (lineDirection * Projectile.width), Projectile.height, DelegateMethods.CutTiles);
		}
		public override void AI()
		{
			Lighting.AddLight(Projectile.position, Color.Cyan.ToVector3() * 0.5f);

			Projectile.velocity = Vector2.Zero;
			Player.itemTime = Player.itemAnimation = 5;
			Player.heldProj = Projectile.whoAmI;

			if (!initialized)
			{
				initialized = true;
				direction = Player.DirectionTo(Main.MouseWorld);
				direction.Normalize();
				Projectile.rotation = direction.ToRotation();
				switch (Phase)
				{
					case 0:
						Projectile.Size = new Vector2(150, 50);
						break;
					case 1:
						Projectile.Size = new Vector2(150, 50);
						flip = !flip;
						break;
					case 2:
						Projectile.Size = new Vector2(150, 250);
						Projectile.Center -= (direction * 150);
						break;
				}

				if (direction.X < 0)
					flip = !flip;

				if (Empowered)
				{
					Projectile.damage *= 2;

					SoundEngine.PlaySound(new SoundStyle("SpiritMod/Sounds/Slash1") with { PitchVariance = 0.1f, Volume = 0.4f }, Projectile.Center);
					Player.GetModPlayer<MyPlayer>().Shake += 12;
				}
			}

			if (Empowered)
				Projectile.Center = Player.Center - (direction * 50);
			else
				Projectile.Center = Player.Center + (direction.RotatedBy(-1.57f) * 20);

			Timer += 2;
			float progress = Math.Min(Timer / (float)SwingTime, 1);
			progress = EaseFunction.EaseCircularInOut.Ease(progress);
			rotation = Projectile.rotation + MathHelper.Lerp(SwingRadians / 2 * SwingDirection, -SwingRadians / 2 * SwingDirection, progress);

			Player.direction = Math.Sign(rotation.ToRotationVector2().X);

			Player.itemRotation = rotation;
			if (Player.direction != 1)
			{
				Player.itemRotation -= 3.14f;
			}
			Player.itemRotation = MathHelper.WrapAngle(Player.itemRotation);

			if (!Empowered)
			{
				if (Projectile.frame > 2 && Projectile.frame < 5)
				{
					SoundEngine.PlaySound(SoundID.Item85 with { PitchVariance = 0.2f, Volume = 0.15f }, Projectile.Center);

					for (int i = 0; i < 2; i++)
						Dust.NewDustPerfect(Player.Center + ((rotation - 0.4f).ToRotationVector2() * 95), ModContent.DustType<DuelistBubble>(), Main.rand.NextVector2Circular(1, 1));
				}
			}

			Projectile.frameCounter++;
			if (Projectile.frameCounter % 2 == 0)
				Projectile.frame++;
			if (Projectile.frame >= MaxFrames)
			{
				if (Phase == 1)
					Projectile.NewProjectile(Projectile.GetSource_FromAI(), Player.Center, Vector2.Zero, ModContent.ProjectileType<DuelistActivation>(), 0, 0, Player.whoAmI);
				Projectile.active = false;
			}

			if (Projectile.frame >= 2 && Projectile.frame < 7)
				Projectile.friendly = true;
			else
				Projectile.friendly = false;
		}
		public override Color? GetAlpha(Color lightColor) => Color.White;

		public override bool PreDraw(ref Color lightColor)
		{
			Color color = Color.White;
			color.A = 120;
			color *= 0.8f;

			if (!Empowered)
			{
				Texture2D tex2 = TextureAssets.Projectile[Projectile.type].Value;
				if (flip)
				{
					Main.spriteBatch.Draw(tex2, Player.Center - Main.screenPosition, null, lightColor * .5f, rotation + 2.355f, new Vector2(tex2.Width, tex2.Height), Projectile.scale, SpriteEffects.FlipHorizontally, 0f);
					Main.spriteBatch.Draw(tex2, Player.Center - Main.screenPosition, null, lightColor * .125f, rotation + 2.355f, new Vector2(tex2.Width, tex2.Height), Projectile.scale * 1.15f, SpriteEffects.FlipHorizontally, 0f);
				}
				else
				{
					Main.spriteBatch.Draw(tex2, Player.Center - Main.screenPosition, null, lightColor, rotation + 0.785f, new Vector2(0, tex2.Height), Projectile.scale, SpriteEffects.None, 0f);
					Main.spriteBatch.Draw(tex2, Player.Center - Main.screenPosition, null, lightColor * .125f, rotation + 0.785f, new Vector2(0, tex2.Height), Projectile.scale * 1.15f, SpriteEffects.None, 0f);
				}
			}

			Texture2D tex = Phase switch
			{
				0 => ModContent.Request<Texture2D>(Texture + "One", ReLogic.Content.AssetRequestMode.ImmediateLoad).Value,
				1 => ModContent.Request<Texture2D>(Texture + "Two", ReLogic.Content.AssetRequestMode.ImmediateLoad).Value,
				2 => ModContent.Request<Texture2D>(Texture + "Special", ReLogic.Content.AssetRequestMode.ImmediateLoad).Value,
				_ => TextureAssets.Projectile[Projectile.type].Value,
			};

			int frameHeight = tex.Height / MaxFrames;
			Rectangle frame = new Rectangle(0, frameHeight * Projectile.frame, tex.Width, frameHeight);
			if (flip)
			{
				if (direction.X > 0)
				{
					Main.spriteBatch.Draw(tex, Projectile.Center - Main.screenPosition, frame, color * .6f, Projectile.rotation, new Vector2(0, frameHeight / 2), Projectile.scale, SpriteEffects.None, 0f);
					Main.spriteBatch.Draw(tex, Projectile.Center - Main.screenPosition, frame, color * .126f, Projectile.rotation, new Vector2(0, frameHeight / 2), Projectile.scale * 1.05f, SpriteEffects.None, 0f);
				}
				else
				{
					Main.spriteBatch.Draw(tex, Projectile.Center - Main.screenPosition, frame, color * .6f, Projectile.rotation + 3.14f, new Vector2(tex.Width, frameHeight / 2), Projectile.scale, SpriteEffects.FlipHorizontally, 0f);
					Main.spriteBatch.Draw(tex, Projectile.Center - Main.screenPosition, frame, color * .126f, Projectile.rotation + 3.14f, new Vector2(tex.Width, frameHeight / 2), Projectile.scale * 1.05f, SpriteEffects.FlipHorizontally, 0f);
				}
			}
			else
			{
				if (direction.X > 0)
				{
					Main.spriteBatch.Draw(tex, Projectile.Center - Main.screenPosition - (direction.RotatedBy(-1.57f) * 15), frame, color * .6f, Projectile.rotation, new Vector2(0, frameHeight / 2), Projectile.scale, SpriteEffects.None, 0f);
					Main.spriteBatch.Draw(tex, Projectile.Center - Main.screenPosition - (direction.RotatedBy(-1.57f) * 15), frame, color * .126f, Projectile.rotation, new Vector2(0, frameHeight / 2), Projectile.scale * 1.05f, SpriteEffects.None, 0f);

				}
				else
				{
					Main.spriteBatch.Draw(tex, Projectile.Center - Main.screenPosition - (direction.RotatedBy(-1.57f) * 15), frame, color * .6f, Projectile.rotation + 3.14f, new Vector2(tex.Width, frameHeight / 2), Projectile.scale, SpriteEffects.FlipHorizontally, 0f);
					Main.spriteBatch.Draw(tex, Projectile.Center - Main.screenPosition - (direction.RotatedBy(-1.57f) * 15), frame, color * .126f, Projectile.rotation + 3.14f, new Vector2(tex.Width, frameHeight / 2), Projectile.scale *1.05f, SpriteEffects.FlipHorizontally, 0f);
				}
			}
			return false;
		}

		public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
		{
			modifiers.HitDirectionOverride = Math.Sign(direction.X);
		}
	}

	internal class DuelistGun : ModProjectile
	{
		public float Recoil = 0f;

		private Vector2 initialDirection = Vector2.Zero;

		private Vector2 CurrentDirection => initialDirection.RotatedBy(Recoil);

		private bool initialized = false;

		private Player Player => Main.player[Projectile.owner];

		public override LocalizedText DisplayName => Language.GetText("Mods.SpiritMod.Items.DuelistLegacy.DisplayName");

		public override void SetStaticDefaults() => Main.projFrames[Projectile.type] = 1;

		public override void SetDefaults()
		{
			Projectile.friendly = false;
			Projectile.DamageType = DamageClass.Melee;
			Projectile.tileCollide = false;
			Projectile.Size = new Vector2(32, 32);
			Projectile.penetrate = -1;
			Projectile.usesLocalNPCImmunity = true;
			Projectile.localNPCHitCooldown = 16;
			Projectile.ownerHitCheck = true;
			Projectile.timeLeft = 40;
		}

		public override void AI()
		{
			Player.itemTime = Player.itemAnimation = 5;
			Player.heldProj = Projectile.whoAmI;
			Projectile.Center = Player.Center;

			if (!initialized)
			{
				initialized = true;
				initialDirection = Player.DirectionTo(Main.MouseWorld);
				initialDirection.Normalize();
				Recoil = Player.direction * -0.75f;
			}

			Player.itemRotation = CurrentDirection.ToRotation();
			if (Player.direction != 1)
			{
				Player.itemRotation -= 3.14f;
			}
			Recoil *= 0.95f;
		}

		public override bool PreDraw(ref Color lightColor)
		{
			Player player = Main.player[Projectile.owner];
			Texture2D texture = TextureAssets.Projectile[Projectile.type].Value;

			int height = texture.Height / Main.projFrames[Projectile.type];
			int y2 = height * Projectile.frame;
			Vector2 position = (player.Center + (CurrentDirection * 15)) - Main.screenPosition;

			if (player.direction == 1)
			{
				SpriteEffects effects1 = SpriteEffects.None;
				Main.spriteBatch.Draw(texture, position, new Microsoft.Xna.Framework.Rectangle?(new Microsoft.Xna.Framework.Rectangle(0, y2, texture.Width, height)), lightColor * .91f, CurrentDirection.ToRotation(), new Vector2((float)texture.Width / 2f, (float)height / 2f), Projectile.scale, effects1, 0.0f);
				Main.spriteBatch.Draw(texture, position, new Microsoft.Xna.Framework.Rectangle?(new Microsoft.Xna.Framework.Rectangle(0, y2, texture.Width, height)), Color.Gray * .05f, CurrentDirection.ToRotation(), new Vector2((float)texture.Width / 2f, (float)height / 2f), Projectile.scale * 1.2f, effects1, 0.0f);
			}

			else
			{
				SpriteEffects effects1 = SpriteEffects.FlipHorizontally;
				Main.spriteBatch.Draw(texture, position, new Microsoft.Xna.Framework.Rectangle?(new Microsoft.Xna.Framework.Rectangle(0, y2, texture.Width, height)), lightColor * .91f, CurrentDirection.ToRotation() - 3.14f, new Vector2((float)texture.Width / 2f, (float)height / 2f), Projectile.scale, effects1, 0.0f);
				Main.spriteBatch.Draw(texture, position, new Microsoft.Xna.Framework.Rectangle?(new Microsoft.Xna.Framework.Rectangle(0, y2, texture.Width, height)), Color.Gray * .05f, CurrentDirection.ToRotation() - 3.14f, new Vector2((float)texture.Width / 2f, (float)height / 2f), Projectile.scale * 1.2f, effects1, 0.0f);

			}
			return false;
		}
	}

	internal class DuelistBlast : ModProjectile
	{
		protected virtual Color color => Color.White;

		int direction = 0;

		public override LocalizedText DisplayName => Language.GetText("Mods.SpiritMod.Items.DuelistLegacy.DisplayName");

		public override void SetStaticDefaults() => Main.projFrames[Projectile.type] = 6;

		public override void SetDefaults()
		{
			Projectile.friendly = true;
			Projectile.DamageType = DamageClass.Melee;
			Projectile.tileCollide = false;
			Projectile.Size = new Vector2(225, 75);
			Projectile.penetrate = -1;
			Projectile.usesLocalNPCImmunity = true;
			Projectile.localNPCHitCooldown = 16;
		}
		public override void AI()
		{
			if (Projectile.velocity != Vector2.Zero)
			{
				direction = Math.Sign(Projectile.velocity.X);
				Projectile.rotation = Projectile.velocity.ToRotation();
			}
			Projectile.velocity = Vector2.Zero;
			Projectile.frameCounter++;
			if (Projectile.frameCounter % 3 == 0)
				Projectile.frame++;
			if (Projectile.frame >= Main.projFrames[Projectile.type])
				Projectile.active = false;
			if (Projectile.frame >= Main.projFrames[Projectile.type] / 2)
				Projectile.friendly = false;

			CreateParticles();
		}

		protected virtual void CreateParticles()
		{
			Vector2 lineDirection = Projectile.rotation.ToRotationVector2() * (Projectile.width * 0.7f);
			Vector2 lineOffshoot = (Projectile.rotation + 1.57f).ToRotationVector2() * Projectile.height * 0.3f;
			for (int i = 0; i < 3; i++)
			{
				Vector2 position = Projectile.Center + (lineDirection * Main.rand.NextFloat()) + (lineOffshoot * Main.rand.NextFloat(-1f, 1f));
				Dust.NewDustPerfect(position, 6, Main.rand.NextVector2Circular(1, 1) + ((Projectile.rotation + Main.rand.NextFloat(-0.35f,0.35f)).ToRotationVector2() * 5), 0, default, 1.3f).noGravity = true;
			}
		}
		public override Color? GetAlpha(Color lightColor) => Color.White;

		public override bool PreDraw(ref Color lightColor)
		{
			Texture2D tex = TextureAssets.Projectile[Projectile.type].Value;
			int frameHeight = tex.Height / Main.projFrames[Projectile.type];
			Rectangle frame = new Rectangle(0, frameHeight * Projectile.frame, tex.Width, frameHeight);
			if (direction == 1)
			{
				Main.spriteBatch.Draw(TextureAssets.Projectile[Projectile.type].Value, Projectile.Center - Main.screenPosition, frame, color * .75f, Projectile.rotation, new Vector2(0, frameHeight / 2), Projectile.scale, SpriteEffects.None, 0f);
				Main.spriteBatch.Draw(TextureAssets.Projectile[Projectile.type].Value, Projectile.Center - Main.screenPosition, frame, color * .1f, Projectile.rotation, new Vector2(0, frameHeight / 2), Projectile.scale * 1.05f, SpriteEffects.None, 0f);
			}
			else
			{
				Main.spriteBatch.Draw(TextureAssets.Projectile[Projectile.type].Value, Projectile.Center - Main.screenPosition, frame, color * .75f, Projectile.rotation + 3.14f, new Vector2(tex.Width, frameHeight / 2), Projectile.scale, SpriteEffects.FlipHorizontally, 0f);
				Main.spriteBatch.Draw(TextureAssets.Projectile[Projectile.type].Value, Projectile.Center - Main.screenPosition, frame, color * .1f, Projectile.rotation + 3.14f, new Vector2(tex.Width, frameHeight / 2), Projectile.scale * 1.05f, SpriteEffects.FlipHorizontally, 0f);
			}
			return false;
		}

		public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
		{
			Vector2 lineDirection = Projectile.rotation.ToRotationVector2();
			float collisionPoint = 0;
			if (Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(), Projectile.Center, Projectile.Center + (lineDirection * Projectile.width), Projectile.height, ref collisionPoint))
				return true;
			return false;
		}


		public override bool? CanCutTiles()
		{
			return true;
		}

		// Plot a line from the start of the Solar Eruption to the end of it, to change the tile-cutting collision logic. (Don't change this.)
		public override void CutTiles()
		{
			Vector2 lineDirection = Projectile.rotation.ToRotationVector2();
			DelegateMethods.tilecut_0 = TileCuttingContext.AttackProjectile;
			Utils.PlotTileLine(Projectile.Center, Projectile.Center + (lineDirection * Projectile.width), Projectile.height, DelegateMethods.CutTiles);
		}
		public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
		{
			modifiers.HitDirectionOverride = direction;
		}
	}

	internal class DuelistBlastSpecial : DuelistBlast
	{
		protected override Color color => new Color(255, 255, 255, 120) * 0.8f;

		public override LocalizedText DisplayName => Language.GetText("Mods.SpiritMod.Items.DuelistLegacy.DisplayName");

		public override void SetStaticDefaults() => Main.projFrames[Projectile.type] = 11;

		public override void SetDefaults()
		{
			Projectile.friendly = true;
			Projectile.DamageType = DamageClass.Melee;
			Projectile.tileCollide = false;
			Projectile.penetrate = -1;
			Projectile.Size = new Vector2(300, 100);
			Projectile.usesLocalNPCImmunity = true;
			Projectile.localNPCHitCooldown = 16;
		}

		protected override void CreateParticles()
		{
			Vector2 lineDirection = Projectile.rotation.ToRotationVector2() * (Projectile.width * 0.7f);
			Vector2 lineOffshoot = (Projectile.rotation + 1.57f).ToRotationVector2() * Projectile.height * 0.3f;
			if (Projectile.frame < 7)
			{
				SoundEngine.PlaySound(SoundID.Item85 with { PitchVariance = 0.2f, Volume = 0.15f }, Projectile.Center);
				Vector2 position = Projectile.Center + (lineDirection * Main.rand.NextFloat()) + (lineOffshoot * Main.rand.NextFloat(-1f, 1f));
				Dust.NewDustPerfect(position, ModContent.DustType<DuelistBubble>(), Main.rand.NextVector2Circular(1, 1) + (Projectile.rotation.ToRotationVector2() * 2));
			}
		}
	}

	internal class DuelistActivation : ModProjectile
	{
		public override LocalizedText DisplayName => Language.GetText("Mods.SpiritMod.Items.DuelistLegacy.DisplayName");

		public override void SetStaticDefaults() => Main.projFrames[Projectile.type] = 19;

		public override void SetDefaults()
		{
			Projectile.friendly = false;
			Projectile.DamageType = DamageClass.Melee;
			Projectile.tileCollide = false;
			Projectile.Size = new Vector2(225, 75);
			Projectile.penetrate = -1;
			Projectile.usesLocalNPCImmunity = true;
			Projectile.localNPCHitCooldown = 16;
			Projectile.ownerHitCheck = true;
		}
		public override void AI()
		{
			Player player = Main.player[Projectile.owner];
			if (player.HeldItem.ModItem is DuelistLegacy modItem && !modItem.ChargeReady)
				Projectile.active = false;

			Vector2 direction = player.DirectionTo(Main.MouseWorld);
			direction.Normalize();
			Projectile.rotation = direction.ToRotation();

			Projectile.Center = player.Center + (direction * 15);
			Projectile.velocity = Vector2.Zero;
			Projectile.frameCounter++;
			if (Projectile.frameCounter % 3 == 0)
				Projectile.frame++;
			if (Projectile.frame >= Main.projFrames[Projectile.type])
				Projectile.active = false;
		}
		public override bool PreDraw(ref Color lightColor)
		{
			Color color = Color.White;
			color.A = 120;
			color *= 0.8f;

			Texture2D tex = TextureAssets.Projectile[Projectile.type].Value;
			int frameHeight = tex.Height / Main.projFrames[Projectile.type];
			Rectangle frame = new Rectangle(0, frameHeight * Projectile.frame, tex.Width, frameHeight);
			Main.spriteBatch.Draw(TextureAssets.Projectile[Projectile.type].Value, Projectile.Center - Main.screenPosition, frame, color, Projectile.rotation, new Vector2(0, frameHeight / 2), Projectile.scale, SpriteEffects.None, 0f);
			return false;
		}
	}
}