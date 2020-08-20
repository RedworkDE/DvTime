using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RedworkDE.DvTime
{
	class PocketWatchPatches
	{
		public static IntegrationMode Mode = IntegrationMode.Exclusive;

		[HarmonyPatch(typeof(PocketWatchController), MethodType.Constructor), HarmonyPostfix]
		public static void PocketWatchController_ctor_Patch(PocketWatchController __instance)
		{
			if (Mode != IntegrationMode.None) __instance.state = PocketWatchController.PocketWatchState.CLOCK;
		}

		[HarmonyPatch(typeof(PocketWatchController), nameof(PocketWatchController.OnButtonPressed)), HarmonyPrefix]
		public static bool PocketWatchController_OnButtonPressed_Patch(PocketWatchController __instance)
		{
			Log.Debug($"State before: {__instance.state}");

			switch (Mode)
			{
				case IntegrationMode.Exclusive:
					__instance.UpdateState(PocketWatchController.PocketWatchState.CLOCK);
					return false;
				case IntegrationMode.AddAfterTimer:
					if (__instance.state == PocketWatchController.PocketWatchState.TIMER)
					{
						Log.Debug("Changing to CLOCK");
						__instance.UpdateState(PocketWatchController.PocketWatchState.CLOCK);
						return false;
					}
					else if (__instance.state == PocketWatchController.PocketWatchState.CLOCK)
					{
						Log.Debug("Changing to IDLE");
						__instance.UpdateState(PocketWatchController.PocketWatchState.IDLE);
						return false;
					}
					break;
				case IntegrationMode.ReplaceStopwatch:
					if (__instance.state == PocketWatchController.PocketWatchState.IDLE && __instance.time == 0)
					{
						__instance.UpdateState(PocketWatchController.PocketWatchState.CLOCK);
						return false;
					}
					else if (__instance.state == PocketWatchController.PocketWatchState.CLOCK)
					{
						__instance.UpdateState(PocketWatchController.PocketWatchState.IDLE);
						return false;
					}
					break;
				case IntegrationMode.None:
					if (__instance.state == PocketWatchController.PocketWatchState.CLOCK)
					{
						__instance.UpdateState(PocketWatchController.PocketWatchState.IDLE);
						return false;
					}
					break;
			}
			return true;
		}

		[HarmonyPatch(typeof(PocketWatchController), nameof(PocketWatchController.Update)), HarmonyPrefix]
		public static bool PocketWatchController_Update_Patch(PocketWatchController __instance)
		{
			if (!__instance.isInitialized || __instance.state != PocketWatchController.PocketWatchState.CLOCK) return true;
			// dont use the default update for clock, as it will use DateTime.Now and mess up the saved time

			var time = CurrentTime.Time;

			__instance.SetHours(time.GetHour() % 12); // SetHours only works for hours 0-18 for some reason
			__instance.SetMinutes(time.GetMinute());
			__instance.SetSeconds(time.GetSecond());

			return false;
		}

		public enum IntegrationMode
		{
			Exclusive,
			AddAfterTimer,
			ReplaceStopwatch,
			None,
		}
	}
}
