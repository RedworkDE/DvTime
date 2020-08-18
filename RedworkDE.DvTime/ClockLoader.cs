using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using HarmonyLib;

namespace RedworkDE.DvTime
{
	public class ClockLoader
	{
		private static AssetBundle _wallClockBundle;

		[AutoLoad]
		static void Init()
		{
			_wallClockBundle = AssetBundle.LoadFromStream(typeof(ClockLoader).Assembly.GetManifestResourceStream(typeof(ClockLoader), "clocks"));

			WorldStreamingInit.LoadingFinished += () =>
			{
				var scene = SceneManager.GetActiveScene();
				Log.Debug($"searching scene: {scene.isLoaded} // {scene.name} // {scene.rootCount}");
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
			Log.Info($"Loaded streamer scence {sceneGO}");

			AddStationClocks(sceneGO.transform);
		}

		private static void AddStationClocks(Transform transform)
		{
			if (transform.name.StartsWith("StationClock")) AddStationClock(transform.gameObject);
			else
			{
				foreach (var tf in transform.OfType<Transform>())
					AddStationClocks(tf);
			}
		}

		private static void AddStationClock(GameObject go)
		{
			var originalHour = go.transform.Find("StationClock_hour_LOD0");
			var originalMinute = go.transform.Find("StationClock_minute_LOD0");

			var hourAsset = _wallClockBundle.LoadAsset<GameObject>("StationClock_hour_LOD0");
			var hour1 = Object.Instantiate(hourAsset, originalHour.position, Quaternion.identity, go.transform);
			var hour2 = Object.Instantiate(hourAsset, originalHour.position, Quaternion.identity, go.transform);
			hour1.transform.localEulerAngles = new Vector3(0, 0, 0);
			hour2.transform.localEulerAngles = new Vector3(0, 180, 0);

			var minuteAsset = _wallClockBundle.LoadAsset<GameObject>("StationClock_minute_LOD0");
			var minute1 = Object.Instantiate(minuteAsset, originalMinute.position, Quaternion.identity, go.transform);
			var minute2 = Object.Instantiate(minuteAsset, originalMinute.position, Quaternion.identity, go.transform);
			minute1.transform.localEulerAngles = new Vector3(0, 0, 0);
			minute2.transform.localEulerAngles = new Vector3(0, 180, 0);

			hour1.GetComponentInChildren<MeshRenderer>().material = hour2.GetComponentInChildren<MeshRenderer>().material = originalHour.GetComponent<MeshRenderer>().material;
			Object.Destroy(originalHour.gameObject);
			Object.Destroy(go.transform.Find("StationClock_hour_LOD1").gameObject);

			minute1.GetComponentInChildren<MeshRenderer>().material = minute2.GetComponentInChildren<MeshRenderer>().material = originalMinute.GetComponent<MeshRenderer>().material;
			Object.Destroy(originalMinute.gameObject);
			Object.Destroy(go.transform.Find("StationClock_minute_LOD1").gameObject);


			var upd1 = go.AddComponent<ClockUpdater>();
			upd1.BaseRotation = new Vector3(-1, 0, 0);
			upd1._hourTransforms = new[] {hour1.transform};
			upd1._minuteTransforms = new[] {minute1.transform};

			var upd2 = go.AddComponent<ClockUpdater>();
			upd2.BaseRotation = new Vector3(-1, 0, 0);
			upd2._hourTransforms = new[] { hour2.transform };
			upd2._minuteTransforms = new[] { minute2.transform };
		}
	}
}