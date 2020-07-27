using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using CommandTerminal;
using HarmonyLib;

namespace RedworkDE.DvTime
{
	public class ConsoleCommands : AutoLoad<ConsoleCommands>
	{
		private static List<Tuple<string, CommandInfo>>? _commands = new List<Tuple<string, CommandInfo>>();

		public static void RegisterCommand(string name, CommandInfo command)
		{
			_commands?.Add(Tuple.Create(name, command));
			if (_commands is null)
			{
				Terminal.Shell.AddCommand(name, command);
				Terminal.Autocomplete.Register(name);
			}
		}

		public static void RegisterCommand(string name, Action<CommandArg[]> func)
		{
			RegisterCommand(name, new CommandInfo() { proc = func, max_arg_count = -1 });
		}


		[HarmonyPatch(typeof(CommandShell), "RegisterCommands")]
		[HarmonyPrefix]
		private static void CommandShell_RegisterCommands_Patch()
		{
			if (!Terminal.Shell.Commands.ContainsKey("q")) Terminal.Shell.AddCommand("q", args => Process.GetCurrentProcess().Kill(), help: "Kill the game process");

			_commands?.ForEach(c => Terminal.Shell.AddCommand(c.Item1, c.Item2));
			_commands = null;
		}

        static ConsoleCommands()
		{
			RegisterCommand("time.source", args =>
			{
				if (args.Length == 0) Terminal.Log($"Current source: {TimeUpdater.Instance.TimeSource?.Id ?? "none"}");
				else
				{
					var sourceName = args[0].String;
					var source = TimeUpdater.Instance.TimeSources.FirstOrDefault(s => s.Id == sourceName);
					if (source is null) Terminal.Log($"Cannot find source type {sourceName}, available sources are: {string.Join(", ", TimeUpdater.Instance.TimeSources.Select(s => s.Id))}");
					else TimeUpdater.Instance.TimeSource = source;
				}
			});
			RegisterCommand("time.scale", args =>
			{
				var source = TimeUpdater.Instance.TimeSource;
				if (source is RealTimeSource rts) if (args.Length == 0) Terminal.Log($"Current time scale: {rts.TimeScale}"); else rts.TimeScale = args[0].Float;
				else if (source is PlayTimeSource pts) if (args.Length == 0) Terminal.Log($"Current time scale: {pts.TimeScale}"); else pts.TimeScale = args[0].Float;
			});
			RegisterCommand("time.current", args =>
			{
				var source = TimeUpdater.Instance.TimeSource;

				if (args.Length == 0)
				{
					Terminal.Log($"Current time: {CurrentTime.Time:s}");
					return;
				}

				if (!DateTime.TryParse(args[0].String, CultureInfo.CurrentCulture, DateTimeStyles.AssumeLocal | DateTimeStyles.AllowWhiteSpaces, out var targetDate))
				{
					Terminal.Log($"Failed to parse provided date/time");
					return;
				}

				if (source is RealTimeSource rts)
				{
					rts.Offset += (targetDate - CurrentTime.Time);
				}
				else if (source is PlayTimeSource pts)
				{
					pts.CurrentTime = targetDate;
				}
			});
			RegisterCommand("time.reset", args =>
			{
				var source = TimeUpdater.Instance.TimeSource;

				if (source is RealTimeSource rts)
				{
					rts.TimeScale = 1;
					rts.Offset = TimeSpan.Zero;
					rts.BaseTime = DateTime.Now;
				}
				else if (source is PlayTimeSource pts)
				{
					pts.TimeScale = 4;
					pts.CurrentTime = DateTime.Now;
				}
			});
		}
	}
}