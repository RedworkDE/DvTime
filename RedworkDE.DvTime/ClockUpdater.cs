using System;
using System.Linq;
using UnityEngine;

namespace RedworkDE.DvTime
{
	public class ClockUpdater : MonoBehaviour
	{
		public Transform[] _hourTransforms = null!;
		public Quaternion[] _hourRot = null!;
		public Transform[] _minuteTransforms = null!;
		public Quaternion[] _minuteRot = null!;
		public Transform[] _secondTransforms = null!;
		public Quaternion[] _secondRot = null!;
		public Vector3 BaseRotation;

		void Awake()
		{
			_hourTransforms = transform.OfType<Transform>().Where(tf => tf.name.IndexOf("hour", StringComparison.OrdinalIgnoreCase) != -1).ToArray();
			_minuteTransforms = transform.OfType<Transform>().Where(tf => tf.name.IndexOf("minute", StringComparison.OrdinalIgnoreCase) != -1).ToArray();
			_secondTransforms = transform.OfType<Transform>().Where(tf => tf.name.IndexOf("second", StringComparison.OrdinalIgnoreCase) != -1).ToArray();
			
			Log.Info($"Found Clock {gameObject}, {_hourTransforms.Length} / {_minuteTransforms.Length} / {_secondTransforms.Length}");
		}

		void LateUpdate()
		{
			_hourRot ??= _hourTransforms.Select(t => t.localRotation).ToArray();
			_minuteRot ??= _minuteTransforms.Select(t => t.localRotation).ToArray();
			_secondRot ??= _secondTransforms.Select(t => t.localRotation).ToArray();

			var time = CurrentTime.Time;


			var hourAngle = time.GetHour() / 12f * 360f;
			var minuteAngle = time.GetMinute() / 60f * 360f;
			var secondAngle = time.GetSecond() / 60f * 360f;


			for (var i = 0; i < _hourTransforms.Length; i++) _hourTransforms[i].localRotation = _hourRot[i] * Quaternion.Euler(BaseRotation * hourAngle);
			for (var i = 0; i < _minuteTransforms.Length; i++) _minuteTransforms[i].localRotation = _minuteRot[i] * Quaternion.Euler(BaseRotation * minuteAngle);
			for (var i = 0; i < _secondTransforms.Length; i++) _secondTransforms[i].localRotation = _secondRot[i] * Quaternion.Euler(BaseRotation * secondAngle);
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