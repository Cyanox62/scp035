using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using MEC;
using Exiled.API.Features;
using Exiled.API.Enums;
using Exiled.API.Features.Items;
using InventorySystem;
using InventorySystem.Items;
using InventorySystem.Items.Pickups;
using Exiled.API.Extensions;

namespace scp035
{
	partial class EventHandlers
	{
		private static void RemovePossessedItems()
		{
			for (int i = 0; i < scpPickups.Count; i++)
			{
				Pickup p = scpPickups[i];
				if (p != null) p.Destroy();
			}
			scpPickups.Clear();
		}

		private static ItemPickupBase GetRandomItem()
		{
			List<ItemPickupBase> pickups = GameObject.FindObjectsOfType<ItemPickupBase>().Where(x => !scpPickups.Contains(Pickup.Get(x))).ToList();
			return pickups[rand.Next(pickups.Count)];
		}

		private static void RefreshItems()
		{
			RemovePossessedItems();
			if (scp035.instance.Config.SelfInfect || Player.List.Where(x => x.Team == Team.RIP && !x.ReferenceHub.serverRoles.OverwatchEnabled).ToList().Count > 0)
			{
				for (int i = 0; i < scp035.instance.Config.InfectedItemCount; i++)
				{
					ItemPickupBase p = GetRandomItem();
					Pickup a = new Item(scp035.instance.Config.PossibleItems[rand.Next(scp035.instance.Config.PossibleItems.Count)]).Spawn(p.transform.position);
					scpPickups.Add(a);
				}
			}
		}

		private static void KillScp035(bool setRank = true)
		{
			Player player = scpPlayer;
			scpPlayer = null;
			Scp035JustDied = player;
			Timing.CallDelayed(10f, () =>
			{
				Scp035JustDied = null;
			});
			if (player != null)
			{
				if (setRank)
				{
					player.CustomInfo = string.Empty;
					player.ReferenceHub.nicknameSync.ShownPlayerInfo |= PlayerInfoArea.Role;
					if (isHidden) player.ReferenceHub.characterClassManager.UserCode_CmdRequestHideTag();
				}
				if (scp035.instance.Config.CanHealBeyondHostHp)
				{
					player.MaxHealth = maxHP;
				}
			}
			isRotating = true;
			RefreshItems();
			if (Scp173.TurnedPlayers.Contains(player)) Scp173.TurnedPlayers.Remove(player);
			if (Scp096.TurnedPlayers.Contains(player)) Scp096.TurnedPlayers.Remove(player);
		}

		public static void Spawn035(Player p035, Player player = null, bool full = true)
		{
			if (full)
			{
				if (player != null && p035 != player)
				{
					p035.ClearInventory();
					Vector3 pos = player.Position;
					p035.ChangeRole(player.Role, true);
					Timing.CallDelayed(0.5f, () => p035.Position = pos);

					Timing.CallDelayed(0.8f, () =>
					{
						foreach (Item item in player.Items) p035.AddItem(item);
					});
					
				}
				maxHP = player?.MaxHealth ?? p035.MaxHealth;
				p035.Health = scp035.instance.Config.Health;
				p035.Ammo[ItemType.Ammo12gauge] = 60;
				p035.Ammo[ItemType.Ammo44cal] = 60;
				p035.Ammo[ItemType.Ammo556x45] = 60;
				p035.Ammo[ItemType.Ammo762x39] = 60;
				p035.Ammo[ItemType.Ammo9x19] = 60;
			}

			if (!string.IsNullOrEmpty(p035.ReferenceHub.serverRoles.HiddenBadge))
			{
				isHidden = true;
				p035.BadgeHidden = false;
			}
			p035.ReferenceHub.nicknameSync.ShownPlayerInfo &= ~PlayerInfoArea.Role;
			p035.CustomInfo = "<color=#FF0000>SCP-035</color>";

			if (!Scp173.TurnedPlayers.Contains(p035)) Scp173.TurnedPlayers.Add(p035);
			if (!Scp096.TurnedPlayers.Contains(p035)) Scp096.TurnedPlayers.Add(p035);

			p035.Broadcast(scp035.instance.Config.Scp035PlayerMessageTime, scp035.instance.Config.Scp035PlayerMessage);

			scpPlayer = p035;

			if (scp035.instance.Config.CorrodeHost)
			{
				coroutines.Add(Timing.RunCoroutine(CorrodeHost()));
			}

			Player scp966 = Player.List.FirstOrDefault(p => p.SessionVariables.ContainsKey("is966") && (bool)p.SessionVariables["is966"]);
			if (scp966 != null)
			{
				p035.TargetGhostsHashSet.Remove(scp966.Id);
			}
		}

		public static void InfectPlayer(Player player, Pickup pItem)
		{
			if (scp035.instance.Config.SelfInfect)
			{
				pItem.Destroy();

				Spawn035(player, player);

				isRotating = false;

				RemovePossessedItems();

				if (scp035.instance.Config.CorrodeHost)
				{
					coroutines.Add(Timing.RunCoroutine(CorrodeHost()));
				}
			}
			else
			{
				List<Player> pList = Player.List.Where(x => x.Role == RoleType.Spectator && !x.ReferenceHub.serverRoles.OverwatchEnabled && x.UserId != null && x.UserId != string.Empty).ToList();
				if (pList.Count > 0 && scpPlayer == null)
				{
					pItem.Destroy();

					Spawn035(pList[rand.Next(pList.Count)], player);

					isRotating = false;

					player.ClearInventory();
					player.ChangeRole(RoleType.Spectator);
					player.Broadcast(scp035.instance.Config.InfectedPlayerMessageTime, scp035.instance.Config.InfectedPlayerMessage);

					RemovePossessedItems();
				}
			}
		}

		private static IEnumerator<float> CorrodeHost()
		{
			while (scpPlayer != null)
			{
				scpPlayer.Health -= scp035.instance.Config.CorrodeHostAmount;
				if (scpPlayer.Health <= 0)
				{
					scpPlayer.ChangeRole(RoleType.Spectator);
					KillScp035();
				}
				yield return Timing.WaitForSeconds(scp035.instance.Config.CorrodeHostInterval);
			}
		}

		private IEnumerator<float> RotatePickup()
		{
			while (isRoundStarted)
			{
				if (isRotating)
				{
					RefreshItems();
				}
				yield return Timing.WaitForSeconds(scp035.instance.Config.RotateInterval);
			}
		}

		private IEnumerator<float> CorrodeUpdate()
		{
			if (scp035.instance.Config.CorrodePlayers)
			{
				while (isRoundStarted)
				{
					if (scpPlayer != null)
					{
						IEnumerable<Player> pList = Player.List.Where(x => x.Id != scpPlayer.Id && x.IsAlive);
						if (!scp035.instance.Config.ScpFriendlyFire) pList = pList.Where(x => x.Team != Team.SCP);
						if (!scp035.instance.Config.TutorialFriendlyFire) pList = pList.Where(x => x.Team != Team.TUT);
						foreach (Player player in pList)
						{
							if (player != null && Vector3.Distance(scpPlayer.Position, player.Position) <= scp035.instance.Config.CorrodeDistance)
							{
								CorrodePlayer(player);
							}
						}
					}
					yield return Timing.WaitForSeconds(scp035.instance.Config.CorrodeInterval);
				}
			}
		}

		private void CorrodePlayer(Player player)
		{
			if (scp035.instance.Config.CorrodeLifeSteal && scpPlayer != null)
			{
				int currHP = (int)scpPlayer.Health;
				scpPlayer.Health = currHP + scp035.instance.Config.CorrodeDamage > scp035.instance.Config.Health ? scp035.instance.Config.Health : currHP + scp035.instance.Config.CorrodeDamage;
			}
			player.Damage(scp035.instance.Config.CorrodeDamage, DamageTypes.Nuke);
		}

		private void GrantFF(Player player)
		{
			player.IsFriendlyFireEnabled = true;
			ffPlayers.Add(player.Id);
		}

		private void RemoveFF(Player player)
		{
			player.IsFriendlyFireEnabled = false;
			ffPlayers.Remove(player.Id);
		}

		private void ExitPD(Player player)
		{
			if (!Warhead.IsDetonated) player.Position = RoleType.Scp096.GetRandomSpawnProperties().Item1;
			else player.Kill();
		}
	}
}
