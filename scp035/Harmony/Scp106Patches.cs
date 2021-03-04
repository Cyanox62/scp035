﻿using System;
using HarmonyLib;
using Exiled.API.Features;
using UnityEngine;
using scp035.API;
using Exiled.Events.EventArgs;
using Exiled.Events;
using CustomPlayerEffects;

namespace scp035.Harmony
{
	[HarmonyPatch(typeof(Scp106PlayerScript), nameof(Scp106PlayerScript.CallCmdMovePlayer))]
	static class Scp106Patch
	{
		public static bool Prefix(Scp106PlayerScript __instance, GameObject ply, int t)
		{
			try
			{
				if (!__instance._iawRateLimit.CanExecute(true) || ply == null)
				{
					return false;
				}

				ReferenceHub hub = ReferenceHub.GetHub(ply);
				CharacterClassManager ccm = hub != null ? hub.characterClassManager : null;

				if (ccm == null)
				{
					return false;
				}

				if (!ServerTime.CheckSynchronization(t) || !__instance.iAm106 ||
					Vector3.Distance(hub.playerMovementSync.RealModelPosition, ply.transform.position) >= 3f ||
					!ccm.IsHuman() || ccm.GodMode || ccm.CurRole.team == Team.SCP)
				{
					return false;
				}

				Vector3 position = ply.transform.position;
				float num1 = Vector3.Distance(__instance._hub.playerMovementSync.RealModelPosition, position);
				float num2 = Math.Abs(__instance._hub.playerMovementSync.RealModelPosition.y - position.y);
				if ((num1 >= 1.8179999589920044 && num2 < 1.0199999809265137) ||
					(num1 >= 2.0999999046325684 && num2 < 1.9500000476837158) ||
					((num1 >= 2.6500000953674316 && num2 < 2.200000047683716) ||
					 (num1 >= 3.200000047683716 && num2 < 3.0)) || num1 >= 3.640000104904175)
				{
					__instance._hub.characterClassManager.TargetConsolePrint(__instance.connectionToClient, $"106 MovePlayer command rejected - too big distance (code: T1). Distance: {num1}, Y Diff: {num2}.", "gray");
				}
				else if (Physics.Linecast(__instance._hub.playerMovementSync.RealModelPosition, ply.transform.position, __instance._hub.weaponManager.raycastServerMask))
				{
					__instance._hub.characterClassManager.TargetConsolePrint(__instance.connectionToClient, $"106 MovePlayer command rejected - collider found between you and the target (code: T2). Distance: {num1}, Y Diff: {num2}.", "gray");
				}
				else
				{
					var instanceHub = ReferenceHub.GetHub(__instance.gameObject);
					instanceHub.characterClassManager.RpcPlaceBlood(ply.transform.position, 1, 2f);
					__instance.TargetHitMarker(__instance.connectionToClient, __instance.captureCooldown);

					if (Scp106PlayerScript._blastDoor.isClosed)
					{
						instanceHub.characterClassManager.RpcPlaceBlood(ply.transform.position, 1, 2f);
						instanceHub.playerStats.HurtPlayer(new PlayerStats.HitInfo(500f, instanceHub.LoggedNameFromRefHub(), DamageTypes.Scp106, instanceHub.playerId), ply);
					}
					else
					{
						Scp079Interactable.ZoneAndRoom otherRoom = hub.scp079PlayerScript.GetOtherRoom();
						Scp079Interactable.InteractableType[] filter = new Scp079Interactable.InteractableType[]
						{
							Scp079Interactable.InteractableType.Door, Scp079Interactable.InteractableType.Light,
							Scp079Interactable.InteractableType.Lockdown, Scp079Interactable.InteractableType.Tesla,
							Scp079Interactable.InteractableType.ElevatorUse,
						};

						foreach (Scp079PlayerScript scp079PlayerScript in Scp079PlayerScript.instances)
						{
							bool flag = false;

							foreach (Scp079Interaction scp079Interaction in scp079PlayerScript.ReturnRecentHistory(12f, filter))
							{
								foreach (Scp079Interactable.ZoneAndRoom zoneAndRoom in scp079Interaction.interactable
									.currentZonesAndRooms)
								{
									if (zoneAndRoom.currentZone == otherRoom.currentZone &&
										zoneAndRoom.currentRoom == otherRoom.currentRoom)
									{
										flag = true;
									}
								}
							}

							if (flag)
							{
								scp079PlayerScript.RpcGainExp(ExpGainType.PocketAssist, ccm.CurClass);
							}
						}

						var ev = new EnteringPocketDimensionEventArgs(Player.Get(ply), Vector3.down * 1998.5f, Player.Get(instanceHub));

						Exiled.Events.Handlers.Player.OnEnteringPocketDimension(ev);

						if (!ev.IsAllowed)
							return false;

						hub.playerMovementSync.OverridePosition(ev.Position, 0f, true);

						instanceHub.playerStats.HurtPlayer(new PlayerStats.HitInfo(40f, instanceHub.LoggedNameFromRefHub(), DamageTypes.Scp106, instanceHub.playerId), ply);

						PlayerEffectsController effectsController = hub.playerEffectsController;
						effectsController.GetEffect<Corroding>().IsInPd = true;
						effectsController.EnableEffect<Corroding>(0.0f, false);
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
}
