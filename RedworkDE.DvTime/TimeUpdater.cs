using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using UnityEngine;
using HarmonyLib;

namespace RedworkDE.DvTime
{
	[AutoCreate]
	public class TimeUpdater : MonoBehaviour
	{
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
		private readonly List<ITimeSource> _timeSources = new List<ITimeSource>();

		public ITimeSource? TimeSource
		{
			get => _timeSource ?? TimeSources.FirstOrDefault();
			set
			{
				Log.Debug($"Set TimeSource: {value}");
				Save();
				_timeSource = value;
				Load();
			}
		}

		public static RealTimeSource RealTime = new RealTimeSource();
		public static PlayTimeSource PlayTime = new PlayTimeSource();

		public static event Action<List<ITimeSource>>? RegisterTimeSource;

		public List<ITimeSource> TimeSources
		{
			get
			{
				if (_timeSources.Count == 0)
				{
					_timeSources.Add(RealTime);
					_timeSources.Add(PlayTime);
					RegisterTimeSource?.Invoke(_timeSources);
				}
				return _timeSources;
			}
		}

		void Awake()
		{
			_instance = this;
			Hooks.OnGameLoad += LoadInitial;
			Hooks.OnGameSave += Save;
		}

		private void LoadInitial(Hooks.GameLoadData gameLoadData)
		{
			var type = SaveGameManager.data?.GetString(SAVE_GAME_KEY_SOURCE);
			Log.Debug($"source to load: {type}");
			if (type is { } && type != TimeSource?.Id)
			{
				foreach (var timeSource in TimeSources)
				{
					if (timeSource.Id == type)
					{
						Log.Debug($"found source: {timeSource.Id}");
						_timeSource = timeSource;

						break;
					}

					Log.Debug($"wrong source: {timeSource.Id}");
				}
			}

			Load();
		}

		void Update()
		{
			var ts = TimeSource;
			if (ts is null) return;

			CurrentTime.Time = ts.LocalTime;
		}

		private void Save(Hooks.GameSaveData gameSaveData) => Save();
		private void Save()
		{
			Log.Debug($"Save: {TimeSource is {}}");

			if (TimeSource is null) return;

			var sourceKey = SAVE_GAME_KEY + "." + TimeSource.Id;

			var shared = SaveGameManager.data.GetJObject(SAVE_GAME_KEY) ?? new JObject();
			var source = SaveGameManager.data.GetJObject(sourceKey) ?? new JObject();

			TimeSource.Save(shared, source);

			SaveGameManager.data.SetJObject(SAVE_GAME_KEY, shared);
			SaveGameManager.data.SetJObject(sourceKey, source);
			SaveGameManager.data.SetString(SAVE_GAME_KEY_SOURCE, _timeSource?.Id);
			
			Log.Debug($"Saved: {shared} // {source}");
		}

		private void Load()
		{
			Log.Debug($"Load: {SaveGameManager.data is {}} {TimeSource is { }}");

			if (SaveGameManager.data is null) return;

			if (TimeSource is null) return;

			var sourceKey = SAVE_GAME_KEY + "." + TimeSource.Id;

			var shared = SaveGameManager.data.GetJObject(SAVE_GAME_KEY) ?? new JObject();
			var source = SaveGameManager.data.GetJObject(sourceKey) ?? new JObject();

			TimeSource.Load(shared, source);


			Log.Debug($"Loaded: {shared} // {source}");
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