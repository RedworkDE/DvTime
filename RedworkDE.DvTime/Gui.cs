using System;
using System.Linq;
using UnityEngine;
using UnityModManagerNet;

namespace RedworkDE.DvTime
{
#if UMM
	public class Gui
	{
		[AutoLoad]
		static void Init()
		{
			Log.Message("Settings GUI");
			Hooks.UMMOnGUI += OnGui;
		}

		private static void OnGui(UnityModManager.ModEntry entry)
		{
			var timeSource = TimeUpdater.Instance.TimeSources.IndexOf(TimeUpdater.Instance.TimeSource!);
			UnityModManager.UI.ToggleGroup(timeSource, TimeUpdater.Instance.TimeSources.Select(ts => ts.GetType().Name).ToArray(), n => TimeUpdater.Instance.TimeSource = TimeUpdater.Instance.TimeSources[n]);

			switch (TimeUpdater.Instance.TimeSource)
			{
				case RealTimeSource rts:
					UnityModManager.UI.DrawFloatField(rts.TimeScale, "Timescale", n => rts.TimeScale = n);
					var offset = rts.Offset.ToString("g");
					GUILayout.Label("Offset: ");
					offset = GUILayout.TextField(offset);
					if (TimeSpan.TryParse(offset, out var ts)) rts.Offset = ts;
					break;
				case PlayTimeSource pts:
					UnityModManager.UI.DrawFloatField(pts.TimeScale, "Timescale", n => pts.TimeScale = n);
					break;
			}

			GUILayout.Label("Pocket Watch Integration: ");
			UnityModManager.UI.ToggleGroup((int)PocketWatchPatches.Mode, Enum.GetNames(typeof(PocketWatchPatches.IntegrationMode)), num => PocketWatchPatches.Mode = (PocketWatchPatches.IntegrationMode)num);
		}
	}
#endif
}