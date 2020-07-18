using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using UnityEngine;
using HarmonyLib;

namespace RedworkDE.DvTime
{
	public class TimeUpdater : AutoCreateMonoBehaviour<TimeUpdater>
	{
		public const float DEFAULT_TIME_SCALE = 1440;
		private const string SAVE_GAME_KEY = "RedworkDE.DvTime.TimeUpdater";
		private const string SAVE_GAME_KEY_SOURCE = SAVE_GAME_KEY + ".source";

		private static TimeUpdater? _instance;
		public static TimeUpdater Instance
		{
			get
			{
				if (_instance != null) return _instance;
				var go = new GameObject(nameof(TimeUpdater));
				return _instance = go.AddComponent<TimeUpdater>();
			}
		}

		private ITimeSource? _timeSource;

		public ITimeSource? TimeSource
		{
			get => _timeSource;
			set
			{
				Logger.LogDebug($"Set TimeSource: {value}");
				Save();
				_timeSource = value;
				Load();
			}
		}


		public static RealTimeSource RealTime = new RealTimeSource();
		public static PlayTimeSource PlayTime = new PlayTimeSource();
		public List<ITimeSource> TimeSources { get; } = new List<ITimeSource>() {RealTime, PlayTime};

		void Awake()
		{
			_instance = this;
			TimeSource = RealTime;
			SaveGameManager.Loaded += ()=>
			{
				var type = SaveGameManager.data.GetString(SAVE_GAME_KEY_SOURCE);
				if (type is { } && type != TimeSource?.Id)
				{
					foreach (var timeSource in TimeSources)
					{
						if (timeSource.Id == type)
						{
							_timeSource = timeSource;
							return;
						}
					}
				}

				Load();
			};
		}

		void Update()
		{
			var ts = TimeSource;
			if (ts is null) return;

			CurrentTime.Time = ts.LocalTime;
		}

		private void Save()
		{
			Logger.LogDebug($"Save: {TimeSource is {}}");

			if (TimeSource is null) return;

			var sourceKey = SAVE_GAME_KEY + "." + TimeSource.Id;

			var shared = SaveGameManager.data.GetJObject(SAVE_GAME_KEY) ?? new JObject();
			var source = SaveGameManager.data.GetJObject(sourceKey) ?? new JObject();

			TimeSource.Save(shared, source);

			SaveGameManager.data.SetJObject(SAVE_GAME_KEY, shared);
			SaveGameManager.data.SetJObject(sourceKey, source);
			SaveGameManager.data.SetString(SAVE_GAME_KEY_SOURCE, TimeSource?.Id ?? "null");


			Logger.LogDebug($"Saved: {shared} // {source}");
		}

		private void Load()
		{
			Logger.LogDebug($"Load: {SaveGameManager.data is {}} {TimeSource is { }}");

			if (SaveGameManager.data is null) return;

			if (TimeSource is null) return;

			var sourceKey = SAVE_GAME_KEY + "." + TimeSource.Id;

			var shared = SaveGameManager.data.GetJObject(SAVE_GAME_KEY) ?? new JObject();
			var source = SaveGameManager.data.GetJObject(sourceKey) ?? new JObject();

			TimeSource.Load(shared, source);


			Logger.LogDebug($"Loaded: {shared} // {source}");
		}

		[HarmonyPatch(typeof(SaveGameManager), nameof(SaveGameManager.DoSaveIO)), HarmonyPrefix]
		private static void SaveGameManager_DoSaveIO_Patch()
		{
			Instance.Save();
		}
		
		[HarmonyPatch(typeof(SaveGameManager), nameof(SaveGameManager.DoLoadIO)), HarmonyPostfix]
		private static void SaveGameManager_DoLoadIO_Patch()
		{
			Instance.Load();
		}
	}
	
	public interface ITimeSource
	{
		public string Id { get; }
		public void Save(JObject shared, JObject settings);
		public void Load(JObject shared, JObject settings);
		public DateTime LocalTime { get; }
	}
}