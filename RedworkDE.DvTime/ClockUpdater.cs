using System;
using System.Linq;
using UnityEngine;

namespace RedworkDE.DvTime
{
	public class ClockUpdater : MonoBehaviour<ClockUpdater>
	{
		private Transform[] _hourTransforms = null!;
		private Transform[] _minuteTransforms = null!;
		private Transform[] _secondTransforms = null!;
		public Vector3 BaseRotation;

		void Awake()
		{
			_hourTransforms = transform.OfType<Transform>().Where(tf => tf.name.IndexOf("hour", StringComparison.OrdinalIgnoreCase) != -1).ToArray();
			_minuteTransforms = transform.OfType<Transform>().Where(tf => tf.name.IndexOf("minute", StringComparison.OrdinalIgnoreCase) != -1).ToArray();
			_secondTransforms = transform.OfType<Transform>().Where(tf => tf.name.IndexOf("second", StringComparison.OrdinalIgnoreCase) != -1).ToArray();
			
			Logger.LogInfo($"Found Clock {gameObject}, {_hourTransforms.Length} / {_minuteTransforms.Length} / {_secondTransforms.Length}");
		}

		void LateUpdate()
		{
			var time = CurrentTime.Time;


			var hourAngle = time.GetHour() / 12f * 360f;
			var minuteAngle = time.GetMinute() / 60f * 360f;
			var secondAngle = time.GetSecond() / 60f * 360f;


			foreach (var transform in _hourTransforms) transform.localEulerAngles = BaseRotation * hourAngle;
			foreach (var transform in _minuteTransforms) transform.localEulerAngles = BaseRotation * minuteAngle;
			foreach (var transform in _secondTransforms) transform.localEulerAngles = BaseRotation * secondAngle;
		}
	}

	public static class DateTimeExtender
	{
		public static float GetHour(this DateTime date)
		{
			return (float) ((date.Ticks % TimeSpan.TicksPerDay) / (double) TimeSpan.TicksPerHour);
		}

		public static float GetMinute(this DateTime date)
		{
			return (float) ((date.Ticks % TimeSpan.TicksPerHour) / (double) TimeSpan.TicksPerMinute);
		}

		public static float GetSecond(this DateTime date)
		{
			return (float) ((date.Ticks % TimeSpan.TicksPerMinute) / (double) TimeSpan.TicksPerSecond);
		}
	}
}