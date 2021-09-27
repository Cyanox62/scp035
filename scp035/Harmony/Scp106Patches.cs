using System;
using HarmonyLib;
using Exiled.API.Features;
using UnityEngine;
using scp035.API;
using Exiled.Events.EventArgs;
using Exiled.Events;
using CustomPlayerEffects;
using InventorySystem.Items.MicroHID;
using System.Collections.Generic;
using MapGeneration;

namespace scp035.Harmony
{
	[HarmonyPatch(typeof(Scp106PlayerScript), nameof(Scp106PlayerScript.UserCode_CmdMovePlayer))]
	[HarmonyPriority(Priority.Last)]
	static class Scp106Patch
	{
		public static bool Prefix(Scp106PlayerScript __instance, GameObject ply, int t)
		{
			try
			{
				if (!__instance._iawRateLimit.CanExecute(true) || !__instance.iAm106 || !ServerTime.CheckSynchronization(t))
				{
					return false;
				}
				if (ply == null)
				{
					return false;
				}
				ReferenceHub hub = ReferenceHub.GetHub(ply);
				CharacterClassManager characterClassManager = hub.characterClassManager;
				if (characterClassManager == null || characterClassManager.GodMode || !characterClassManager.IsHuman())
				{
					return false;
				}
				Vector3 position = ply.transform.position;
				float num = Vector3.Distance(__instance._hub.playerMovementSync.RealModelPosition, position);
				float num2 = Math.Abs(__instance._hub.playerMovementSync.RealModelPosition.y - position.y);
				if ((num >= 3.1f && num2 < 1.02f) || (num >= 3.4f && num2 < 1.95f) || (num >= 3.7f && num2 < 2.2f) || (num >= 3.9f && num2 < 3f) || num >= 4.2f)
				{
					__instance._hub.characterClassManager.TargetConsolePrint(__instance.connectionToClient, string.Format("106 MovePlayer command rejected - too big distance (code: T.1). Distance: {0}, Y Diff: {1}.", num, num2), "gray");
					return false;
				}
				if (Physics.Linecast(__instance._hub.playerMovementSync.RealModelPosition, ply.transform.position, MicroHIDItem.WallMask))
				{
					__instance._hub.characterClassManager.TargetConsolePrint(__instance.connectionToClient, string.Format("106 MovePlayer command rejected - collider found between you and the target (code: T.2). Distance: {0}, Y Diff: {1}.", num, num2), "gray");
					return false;
				}
				__instance._hub.characterClassManager.RpcPlaceBlood(ply.transform.position, 1, 2f);
				__instance.TargetHitMarker(__instance.connectionToClient, __instance.captureCooldown);
				__instance._currentServerCooldown = __instance.captureCooldown;
				if (Scp106PlayerScript._blastDoor.isClosed)
				{
					__instance._hub.characterClassManager.RpcPlaceBlood(ply.transform.position, 1, 2f);
					__instance._hub.playerStats.HurtPlayer(new PlayerStats.HitInfo(500f, null, DamageTypes.Scp106, __instance._hub.playerId, false), ply, false, true);
				}
				else
				{
					__instance._hub.playerStats.HurtPlayer(new PlayerStats.HitInfo(40f, null, DamageTypes.Scp106, __instance._hub.playerId, false), ply, false, true);
					hub.playerMovementSync.OverridePosition(Vector3.down * 1998.5f, 0f, true);
					hub.playerEffectsController.EnableEffect<Corroding>(0f, false);
					foreach (Scp079PlayerScript scp079PlayerScript in Scp079PlayerScript.instances)
					{
						Scp079Interactable.InteractableType[] filter = new Scp079Interactable.InteractableType[]
						{
							Scp079Interactable.InteractableType.Door,
							Scp079Interactable.InteractableType.Light,
							Scp079Interactable.InteractableType.Lockdown,
							Scp079Interactable.InteractableType.Tesla,
							Scp079Interactable.InteractableType.ElevatorUse
						};
						bool flag = false;
						using (IEnumerator<Scp079Interaction> enumerator2 = scp079PlayerScript.ReturnRecentHistory(12f, filter).GetEnumerator())
						{
							while (enumerator2.MoveNext())
							{
								if (RoomIdUtils.IsTheSameRoom(enumerator2.Current.interactable.transform.position, ply.transform.position))
								{
									flag = true;
								}
							}
						}
						if (flag)
						{
							scp079PlayerScript.RpcGainExp(ExpGainType.PocketAssist, characterClassManager.CurClass);
						}
					}
				}
				return false;
			}
			catch (Exception e)
			{
				Exiled.API.Features.Log.Error($"{typeof(Scp106Patch).FullName}:\n{e}");

				return true;
			}
		}
	}
}
