using Exiled.API.Features;
using Exiled.Events;
using System;
using System.Reflection;

namespace scp035
{
	public class scp035 : Plugin<Config>
	{
		internal static scp035 instance;

		private HarmonyLib.Harmony hInstance;

		private EventHandlers ev;

		private MethodBase cmdmoveplayer;

		public override void OnEnabled()
		{
			base.OnEnabled();

			//HarmonyLib.Harmony.DEBUG = true;
			instance = this;

			foreach (MethodBase method in Events.Instance.Harmony.GetPatchedMethods())
				if (method.DeclaringType?.Name == "Scp106PlayerScript" && method.Name == "CallCmdMovePlayer")
				{
					cmdmoveplayer = method;
					Events.DisabledPatchesHashSet.Add(method);
					break;
				}
			try
			{
				Events.Instance.ReloadDisabledPatches();
			}
			catch (Exception e)
			{
				Log.Error(e);
			}

			hInstance = new HarmonyLib.Harmony("cyanox.scp035");
			hInstance.PatchAll();

			ev = new EventHandlers(this);

			Exiled.Events.Handlers.Server.RoundStarted += ev.OnRoundStart;
			Exiled.Events.Handlers.Player.PickingUpItem += ev.OnPickupItem;
			Exiled.Events.Handlers.Server.RoundEnded += ev.OnRoundEnd;
			Exiled.Events.Handlers.Player.Died += ev.OnPlayerDie;
			Exiled.Events.Handlers.Player.Hurting += ev.OnPlayerHurt;
			Exiled.Events.Handlers.Player.EnteringPocketDimension += ev.OnPocketDimensionEnter;
			Exiled.Events.Handlers.Server.EndingRound += ev.OnCheckRoundEnd;
			Exiled.Events.Handlers.Player.ChangingRole += ev.OnSetClass;
			Exiled.Events.Handlers.Player.Destroying += ev.OnPlayerLeave;
			Exiled.Events.Handlers.Scp106.Containing += ev.OnContain106;
			Exiled.Events.Handlers.Player.ActivatingGenerator += ev.OnActivatingGenerator;
			Exiled.Events.Handlers.Player.FailingEscapePocketDimension += ev.OnPocketDimensionDie;
			Exiled.Events.Handlers.Player.EscapingPocketDimension += ev.OnPocketDimensionEscape;
			Exiled.Events.Handlers.Player.Shooting += ev.OnShoot;
			Exiled.Events.Handlers.Player.UsingItem += ev.OnUsingItem;
			Exiled.Events.Handlers.Player.ItemUsed += ev.OnItemUsed;
			Exiled.Events.Handlers.Player.Escaping += ev.OnEscaping;
			Exiled.Events.Handlers.Player.Spawning += ev.OnSpawning;
			Exiled.Events.Handlers.Player.Dying += ev.OnDying;
		}

		public override void OnDisabled()
		{
			base.OnDisabled();

			Exiled.Events.Handlers.Server.RoundStarted -= ev.OnRoundStart;
			Exiled.Events.Handlers.Player.PickingUpItem -= ev.OnPickupItem;
			Exiled.Events.Handlers.Server.RoundEnded -= ev.OnRoundEnd;
			Exiled.Events.Handlers.Player.Died -= ev.OnPlayerDie;
			Exiled.Events.Handlers.Player.Hurting -= ev.OnPlayerHurt;
			Exiled.Events.Handlers.Player.EnteringPocketDimension -= ev.OnPocketDimensionEnter;
			Exiled.Events.Handlers.Server.EndingRound -= ev.OnCheckRoundEnd;
			Exiled.Events.Handlers.Player.ChangingRole -= ev.OnSetClass;
			Exiled.Events.Handlers.Player.Destroying -= ev.OnPlayerLeave;
			Exiled.Events.Handlers.Scp106.Containing -= ev.OnContain106;
			Exiled.Events.Handlers.Player.ActivatingGenerator -= ev.OnActivatingGenerator;
			Exiled.Events.Handlers.Player.FailingEscapePocketDimension -= ev.OnPocketDimensionDie;
			Exiled.Events.Handlers.Player.EscapingPocketDimension -= ev.OnPocketDimensionEscape;
			Exiled.Events.Handlers.Player.Shooting -= ev.OnShoot;
			Exiled.Events.Handlers.Player.UsingItem -= ev.OnUsingItem;
			Exiled.Events.Handlers.Player.ItemUsed -= ev.OnItemUsed;
			Exiled.Events.Handlers.Player.Escaping -= ev.OnEscaping;
			Exiled.Events.Handlers.Player.Spawning -= ev.OnSpawning;
			Exiled.Events.Handlers.Player.Dying -= ev.OnDying;

			Events.DisabledPatchesHashSet.Remove(cmdmoveplayer);

			hInstance.UnpatchAll();
			hInstance = null;

			ev = null;
		}

		public override string Name => "scp035";
		public override string Author => "Cyanox";
	}
}
