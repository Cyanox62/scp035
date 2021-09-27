using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommandSystem;
using Exiled.API.Features;
using MEC;
using RemoteAdmin;

namespace scp035.Commands
{
	[CommandHandler(typeof(RemoteAdminCommandHandler))]
	class Spawn035 : ICommand
	{
		public string[] Aliases { get; set; } = Array.Empty<string>();

		public string Description { get; set; } = "Spawn Scp035";

		string ICommand.Command { get; } = "spawn035";

		public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
		{
			if (EventHandlers.scpPlayer == null)
			{
				Player player = null;
				if (arguments.Count >= 1)
				{
					if (int.TryParse(arguments.ElementAt(0), out int classid) && classid >= 0 && classid <= 20)
					{
						Log.Info((RoleType)classid);
						bool full = false;
						if (arguments.Count >= 2 && !bool.TryParse(arguments.ElementAt(1), out full))
						{
							player = Player.Get(arguments.ElementAt(1));
							if (arguments.Count == 3 && !bool.TryParse(arguments.ElementAt(2), out full))
							{
								response = "Error: Invalid value for full.";
								return true;
							}
							if (player != null)
							{
								player.SetRole((RoleType)classid);
								Timing.CallDelayed(0.8f, () =>
								{
									EventHandlers.Spawn035(player, null, full);
								});
								
								response = $"Spawned '{player.Nickname}' as SCP-035.";
								return true;
							}
							else
							{
								response = "Error: Invalid player.";
								return true;
							}
						}
						else
						{
							player = Player.List.ElementAt(UnityEngine.Random.Range(0, Player.List.Count()));
							player.SetRole((RoleType)classid);
							Timing.CallDelayed(0.8f, () =>
							{
								EventHandlers.Spawn035(player, null, full);
							});
							response = $"Spawned '{player.Nickname}' as SCP-035.";
							return true;
						}
					}
					else
					{
						response = "Error: Invalid ClassID.";
						return true;
					}
				}
				else
				{
					response = "Usage: SPAWN035 (CLASSID) [PLAYER / PLAYERID / STEAMID] [full TRUE/FALSE]";
					return true;
				}
			}
			else
			{
				response = "Error: SCP-035 is currently alive.";
				return true;
			}
		}
	}
}
