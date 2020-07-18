using System;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using UnityEngine;
#if BepInEx
using BepInEx;
using BepInEx.Logging;
#else
using UnityModManagerNet;
#endif

namespace RedworkDE.DvTime
{
	/// <summary>
	/// Main Entry point of the mod, ensure that is type is a MonoBehaviour and add it to an GameObject
	/// </summary>
#if BepInEx
	[BepInPlugin("61BFE110-1B58-4CD1-895F-A4163B4F0641", "RedworkDE.DvTime", "$Version")]
#endif
	public class AutoLoadManager
#if BepInEx
		: BaseUnityPlugin
#else
	: MonoBehaviour<AutoLoadManager>
#endif
	{
#if UMM
    public static bool Load(UnityModManager.ModEntry mod)
    {
        new GameObject("__AutoLoadManager").AddComponent<AutoLoadManager>();

        mod.OnGUI += entry =>
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
        };


		return true;
    }
#endif

		void Awake()
		{
			foreach (var type in Assembly.GetExecutingAssembly().GetTypes())
			{
				try
				{
					if (type.BaseType is null) continue;

					if (type.BaseType.IsGenericType && type.BaseType.GetGenericTypeDefinition() == typeof(AutoLoad<>))
					{
						Logger.LogDebug($"Loading {type}");
						RuntimeHelpers.RunClassConstructor(type.TypeHandle);
					}

					if (type.BaseType.IsGenericType && type.BaseType.GetGenericTypeDefinition() == typeof(AutoLoadMonoBehaviour<>))
					{
						Logger.LogDebug($"Loading {type}");
						RuntimeHelpers.RunClassConstructor(type.TypeHandle);
					}

					if (type.BaseType.IsGenericType && type.BaseType.GetGenericTypeDefinition() == typeof(AutoCreateMonoBehaviour<>))
					{
						Logger.LogDebug($"Loading {type}");
						RuntimeHelpers.RunClassConstructor(type.TypeHandle);
						gameObject.AddComponent(type);
					}
				}
				catch (Exception ex)
				{
					Logger.LogDebug($"Error checking {type}: {ex}");
				}
			}
		}
	}

	/// <summary>
	/// MonoBehaviour that has a logger for type <typeparamref name="T"/>
	/// </summary>
	public class MonoBehaviour<T> : MonoBehaviour where T : MonoBehaviour<T>
	{
		protected static readonly ManualLogSource Logger = Logging<T>.Logger;
	}

	/// <summary>
	/// Class that has a logger for type <typeparamref name="T"/>
	/// </summary>
	public class HasLogger<T> where T : HasLogger<T>
	{
		protected static readonly ManualLogSource Logger = Logging<T>.Logger;
	}

	/// <summary>
	/// MonoBehaviour whose static Constructor is is ran automatically
	/// </summary>
	public class AutoLoadMonoBehaviour<T> : MonoBehaviour<T> where T : AutoLoadMonoBehaviour<T>
	{
	}

	/// <summary>
	/// MonoBehaviour that is automatically added to some GameObject
	/// </summary>
	public class AutoCreateMonoBehaviour<T> : MonoBehaviour<T> where T : AutoCreateMonoBehaviour<T>
	{
	}

	/// <summary>
	/// Class whose static Constructor is is ran automatically
	/// </summary>
	public class AutoLoad<T> : HasLogger<T> where T : AutoLoad<T>
	{
	}

#if UMM
	public class ManualLogSource
	{
		public ManualLogSource(string name)
		{
			_prefix = $"[{name}]: ";
		}

		private readonly string _prefix;

		public void LogFatal(object str)
		{
			UnityModManager.Logger.Error(str?.ToString(), _prefix);
		}

		public void LogError(object str)
		{
			UnityModManager.Logger.Error(str?.ToString(), _prefix);
		}

		public void LogWarning(object str)
		{
			UnityModManager.Logger.Error(str?.ToString(), _prefix);
		}

		public void LogMessage(object str)
		{
			UnityModManager.Logger.Log(str?.ToString(), _prefix);
		}

		public void LogInfo(object str)
		{
			UnityModManager.Logger.Log(str?.ToString(), _prefix);
		}

		public void LogDebug(object str)
		{
			UnityModManager.Logger.Log(str?.ToString(), _prefix);
		}
	}
#endif
}