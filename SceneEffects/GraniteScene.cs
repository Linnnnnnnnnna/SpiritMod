using SpiritMod.Biomes;
using SpiritMod.Utilities;
using Terraria;
using Terraria.ModLoader;

namespace SpiritMod.SceneEffects;

internal class GraniteScene : ModSceneEffect
{
	public override int Music => MusicLoader.GetMusicSlot(Mod, "Sounds/Music/GraniteBiome");
	public override SceneEffectPriority Priority => SceneEffectPriority.BiomeMedium;
	public override bool IsSceneEffectActive(Player player) => BiomeTileCounts.InGranite && ModContent.GetInstance<SpiritMusicConfig>().GraniteMusic;
}
