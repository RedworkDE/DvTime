using System;
using Newtonsoft.Json.Linq;

namespace RedworkDE.DvTime
{
	public class PlayTimeSource : ITimeSource
	{
		internal PlayTimeSource()
		{
			
		}

		public float TimeScale { get; set; }
		public DateTime CurrentTime { get;  set; }

		public string Id => "playtime";
		public void Save(JObject shared, JObject settings)
		{
			settings[nameof(TimeScale)] = TimeScale;
			settings[nameof(CurrentTime)] = CurrentTime;
		}

		public void Load(JObject shared, JObject settings)
		{
			TimeScale = (settings[nameof(TimeScale)] ?? shared[nameof(TimeScale)])?.ToObject<float>() ?? 4;
			CurrentTime = settings[nameof(CurrentTime)]?.ToObject<DateTime>() ?? DateTime.Now;
		}

		public DateTime LocalTime
		{
			get
			{
				return CurrentTime += new TimeSpan((long) (UnityEngine.Time.deltaTime * TimeScale * (double) TimeSpan.TicksPerSecond));
			}
		}
	}
}