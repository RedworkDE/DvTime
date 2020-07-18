using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using HarmonyLib;

namespace RedworkDE.DvTime
{
	public class ClockLoader : AutoLoad<ClockLoader>
	{
		private static AssetBundle _wallClockBundle;

		static ClockLoader()
		{
			_wallClockBundle = AssetBundle.LoadFromStream(typeof(ClockLoader).Assembly.GetManifestResourceStream(typeof(ClockLoader), "wallclock"));

			WorldStreamingInit.LoadingFinished += () =>
			{
				var scene = SceneManager.GetActiveScene();
				Logger.LogDebug($"searching scene: {scene.isLoaded} // {scene.name} // {scene.rootCount}");
				foreach (var root in scene.GetRootGameObjects()) AddWallClocks(root.transform);
			};
		}

		// wall clock

		private static void AddWallClocks(Transform transform)
		{
			if (transform.name == "WallClock") AddWallClock(transform.gameObject);
			else
			{
				foreach (var tf in transform.OfType<Transform>()) AddWallClocks(tf);
			}
		}

		private static void AddWallClock(GameObject go)
		{
			var renderer = go.GetComponentInChildren<MeshRenderer>();
			var materials = renderer.materials;

			Object.Destroy(renderer.gameObject);

			var newClock = Object.Instantiate(_wallClockBundle.LoadAsset<GameObject>("wallclock"), go.transform);
			newClock.transform.localEulerAngles = new Vector3(90, 0, 180);

			foreach (Transform child in newClock.transform)
			{
				var cr = child.GetComponent<MeshRenderer>();

				cr.material = child.name.Contains("Glass") ? materials[1] : materials[0];
			}

			newClock.AddComponent<ClockUpdater>().BaseRotation = new Vector3(0, 1, 0);
		}

		// station clocks

		[HarmonyPatch(typeof(Streamer), nameof(Streamer.AddSceneGO)), HarmonyPrefix]
		private static void Streamer_AddSceneGO_Patch(GameObject sceneGO)
		{
			Logger.LogInfo($"Loaded streamer scence {sceneGO}");

			AddStationClocks(sceneGO.transform);
		}

		private static void AddStationClocks(Transform transform)
		{
			if (transform.name.StartsWith("StationClock")) transform.gameObject.AddComponent<ClockUpdater>().BaseRotation = new Vector3(-1, 0, 0);
			else
			{
				foreach (var tf in transform.OfType<Transform>())
					AddStationClocks(tf);
			}
		}
	}
}