using System;
using System.Globalization;
using System.Linq;
using CommandTerminal;

namespace RedworkDE.DvTime
{
	public class Commands
	{
		[AutoLoad]
        static void Init()
		{
			ConsoleCommands.Register("time.source", args =>
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
			ConsoleCommands.Register("time.scale", args =>
			{
				var source = TimeUpdater.Instance.TimeSource;
				if (source is RealTimeSource rts) if (args.Length == 0) Terminal.Log($"Current time scale: {rts.TimeScale}"); else rts.TimeScale = args[0].Float;
				else if (source is PlayTimeSource pts) if (args.Length == 0) Terminal.Log($"Current time scale: {pts.TimeScale}"); else pts.TimeScale = args[0].Float;
			});
			ConsoleCommands.Register("time.current", args =>
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
			ConsoleCommands.Register("time.reset", args =>
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