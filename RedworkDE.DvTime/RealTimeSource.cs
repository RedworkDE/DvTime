using System;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace RedworkDE.DvTime
{
	public class RealTimeSource : ITimeSource
	{
		internal RealTimeSource()
		{
			
		}

		private float _timeScale;

		public float TimeScale
		{
			get => _timeScale;
			set
			{
				if (Mathf.Approximately(_timeScale, value)) return;
				BaseTime = DateTime.Now;
				Offset += CurrentDelta;
				_timeScale = value;
			}
		}

		public DateTime BaseTime { get; set; }
		public TimeSpan Offset { get; set; }

		public string Id => "realtime";
		public void Save(JObject shared, JObject settings)
		{
			shared[nameof(TimeScale)] = TimeScale;

			settings[nameof(BaseTime)] = BaseTime;
			settings[nameof(Offset)] = Offset;
		}

		public void Load(JObject shared, JObject settings)
		{
			TimeScale = shared[nameof(TimeScale)]?.ToObject<float>() ?? 1;

			BaseTime = settings[nameof(BaseTime)]?.ToObject<DateTime>() ?? DateTime.Now;
			Offset = settings[nameof(Offset)]?.ToObject<TimeSpan>() ?? TimeSpan.Zero;
		}

		public DateTime LocalTime
		{
			get
			{
				var result = BaseTime + Offset + CurrentDelta;

				return result;
			}
		}

		private TimeSpan CurrentDelta => new TimeSpan((long) ((DateTime.Now - BaseTime).Ticks * (double) TimeScale));
	}
}