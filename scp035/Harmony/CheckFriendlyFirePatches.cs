using Exiled.API.Features;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using scp035.API;
using UnityEngine;
using GameCore;
using InventorySystem;
using System.Reflection.Emit;
using static HarmonyLib.AccessTools;
using InventorySystem.Items.ThrowableProjectiles;

namespace scp035.Harmony
{
	[HarmonyPatch(typeof(HitboxIdentity), nameof(HitboxIdentity.CheckFriendlyFire), new[] { typeof(ReferenceHub), typeof(ReferenceHub), typeof(bool) })]
	static class CheckFriendlyFirePatches
	{
		public static bool Prefix(Scp106PlayerScript __instance, ReferenceHub attacker, ReferenceHub victim, bool ignoreConfig, ref bool __result)
		{
			if ((Player.Get(attacker) == EventHandlers.scpPlayer && Player.Get(victim).Team != Team.SCP) || (Player.Get(victim) == EventHandlers.scpPlayer && Player.Get(attacker).Team != Team.SCP))
			{
				__result = true;
				return false;
			}
			else
			{
				return true;
			}
		}
	}

	[HarmonyPatch(typeof(HitboxIdentity), nameof(HitboxIdentity.Damage))]
	static class HitboxIdentityDamagePatch
	{
		public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
		{
			var newInstructions = instructions.ToList();
			var offsetIndex = newInstructions.FindIndex(code => code.opcode == OpCodes.Beq_S) + 1;

			newInstructions.RemoveRange(offsetIndex, 19);

			newInstructions.InsertRange(offsetIndex, new[] 
			{
				new CodeInstruction(OpCodes.Ldarg_2),
				new CodeInstruction(OpCodes.Ldfld, Field(typeof(Footprinting.Footprint), nameof(Footprinting.Footprint.Hub))),
				new CodeInstruction(OpCodes.Ldarg_0),
				new CodeInstruction(OpCodes.Callvirt, PropertyGetter(typeof(HitboxIdentity), nameof(HitboxIdentity.TargetHub))),
				new CodeInstruction(OpCodes.Ldc_I4_0),
				new CodeInstruction(OpCodes.Call, Method(typeof(HitboxIdentity), nameof(HitboxIdentity.CheckFriendlyFire), new[] { typeof(ReferenceHub), typeof(ReferenceHub), typeof(bool) }))
			});

			foreach (var code in newInstructions)
				yield return code;
		}
	}

	[HarmonyPatch(typeof(ExplosionGrenade), nameof(ExplosionGrenade.ExplodeDestructible))]
	static class ExplosionGrenadeExplodeDestructiblePatch
	{
		public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
		{
			var newInstructions = instructions.ToList();
			var offsetIndex = newInstructions.FindIndex(code => code.opcode == OpCodes.Stloc_S &&
				((LocalBuilder)code.operand).LocalIndex == 5) + 6;

			newInstructions.RemoveRange(offsetIndex, 8);

			newInstructions.InsertRange(offsetIndex, new[]
			{
				new CodeInstruction(OpCodes.Ldarg_0),
				new CodeInstruction(OpCodes.Ldflda, Field(typeof(ExplosionGrenade), nameof(ExplosionGrenade.PreviousOwner))),
				new CodeInstruction(OpCodes.Ldfld, Field(typeof(Footprinting.Footprint), nameof(Footprinting.Footprint.Hub))),
				new CodeInstruction(OpCodes.Ldloc_3),
				new CodeInstruction(OpCodes.Ldc_I4_0),
				new CodeInstruction(OpCodes.Call, Method(typeof(HitboxIdentity), nameof(HitboxIdentity.CheckFriendlyFire), new[] { typeof(ReferenceHub), typeof(ReferenceHub), typeof(bool) }))
			});

			foreach (var code in newInstructions)
				yield return code;
		}
	}

	[HarmonyPatch(typeof(FlashbangGrenade), nameof(FlashbangGrenade.PlayExplosionEffects))]
	static class FlashbangGrenadePlayExplosionEffectsPatch
	{
		public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
		{
			var newInstructions = instructions.ToList();
			var offsetIndex = newInstructions.FindLastIndex(code => code.opcode == OpCodes.Brtrue_S) + 1;

			newInstructions.RemoveRange(offsetIndex, 9);

			newInstructions.InsertRange(offsetIndex, new[]
			{
				new CodeInstruction(OpCodes.Ldarg_0),
				new CodeInstruction(OpCodes.Ldflda, Field(typeof(FlashbangGrenade), nameof(FlashbangGrenade.PreviousOwner))),
				new CodeInstruction(OpCodes.Ldfld, Field(typeof(Footprinting.Footprint), nameof(Footprinting.Footprint.Hub))),
				new CodeInstruction(OpCodes.Ldloca_S, 2),
				new CodeInstruction(OpCodes.Call, PropertyGetter(typeof(KeyValuePair<GameObject, ReferenceHub>), nameof(KeyValuePair<GameObject, ReferenceHub>.Value))),
				new CodeInstruction(OpCodes.Ldc_I4_0),
				new CodeInstruction(OpCodes.Call, Method(typeof(HitboxIdentity), nameof(HitboxIdentity.CheckFriendlyFire), new[] { typeof(ReferenceHub), typeof(ReferenceHub), typeof(bool) }))
			});

			foreach (var code in newInstructions)
				yield return code;
		}
	}

	[HarmonyPatch(typeof(Scp018Projectile), nameof(Scp018Projectile.DetectPlayers))]
	static class Scp018ProjectileDetectPlayersPatch
	{
		public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
		{
			var newInstructions = instructions.ToList();
			var offsetIndex = newInstructions.FindLastIndex(code => code.opcode == OpCodes.Ldloca_S &&
				((LocalBuilder)code.operand).LocalIndex == 1) + 5;

			newInstructions.RemoveRange(offsetIndex, 7);

			newInstructions.InsertRange(offsetIndex, new[]
			{
				new CodeInstruction(OpCodes.Ldflda, Field(typeof(Scp018Projectile), nameof(Scp018Projectile.PreviousOwner))),
				new CodeInstruction(OpCodes.Ldfld, Field(typeof(Footprinting.Footprint), nameof(Footprinting.Footprint.Hub))),
				new CodeInstruction(OpCodes.Ldloc_1),
				new CodeInstruction(OpCodes.Ldc_I4_0),
				new CodeInstruction(OpCodes.Call, Method(typeof(HitboxIdentity), nameof(HitboxIdentity.CheckFriendlyFire), new[] { typeof(ReferenceHub), typeof(ReferenceHub), typeof(bool) }))
			});

			foreach (var code in newInstructions)
				yield return code;
		}
	}
}
