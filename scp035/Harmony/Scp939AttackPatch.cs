using Exiled.API.Features;
using HarmonyLib;
using UnityEngine;

namespace scp035.Harmony
{
	[HarmonyPatch(typeof(PlayableScps.Scp939), nameof(PlayableScps.Scp939.ServerAttack))]
	class Scp939AttackPatch
	{
		public static void Postfix(PlayableScps.Scp939 __instance, Mirror.NetworkConnection conn, PlayableScps.Messages.Scp939AttackMessage msg)
		{
			Player player = Player.Get(msg.Victim);
			if (player != null && player.Role == RoleType.Tutorial && !scp035.instance.Config.ScpFriendlyFire)
			{
				player.ReferenceHub.playerEffectsController.DisableEffect<CustomPlayerEffects.Amnesia>();
			}
		}
	}

	//[HarmonyPatch(typeof(Scp939PlayerScript), nameof(Scp939PlayerScript.UserCode_CmdShoot))]
	//class Scp939AttackPatch
	//{
	//	public static void Postfix(Scp939PlayerScript __instance, GameObject target)
	//	{
	//		Player player = Player.Get(target);
	//		if (player != null && player.Role == RoleType.Tutorial && !scp035.instance.Config.ScpFriendlyFire)
	//		{
	//			player.ReferenceHub.playerEffectsController.DisableEffect<CustomPlayerEffects.Amnesia>();
	//		}
	//	}
	//}
}
