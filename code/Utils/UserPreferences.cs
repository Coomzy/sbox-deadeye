
using Sandbox.Citizen;
using Sandbox.Internal;
using System.IO;
using System;
using Sandbox;
using static Sandbox.Gizmo;
using Sandbox.Audio;
using System.Runtime.CompilerServices;
using System.Linq;
using System.Reflection;

public abstract class UserPreferences<T> : IHotloadManaged where T : UserPreferences<T>
{
	//static string fileName => typeof(T).ToSimpleString(false);
	static string fileName => $"{typeof(T).ToSimpleString(false)}.json";

#pragma warning disable SB3000 // Hotloading not supported
	static T _instance = null;
#pragma warning restore SB3000 // Hotloading not supported
	public static T instance
	{
		get
		{
			if (!Game.IsPlaying)
			{
				//_instance = null;
			}

			if (_instance != null)
			{
				return _instance;
			}

			if (FileSystem.Data.FileExists(fileName))
			{
				var fileInst = FileSystem.Data.ReadJson<T>(fileName);

				if (fileInst != null)
				{
					_instance = fileInst;
					_instance.Load();
					return _instance;
				}
			}

			T newInst = Activator.CreateInstance<T>();
			_instance = newInst;
			_instance.Load();
			_instance.Save();
			return _instance;
		}
	}

	public void Save()
	{
		OnSave();
		FileSystem.Data.WriteJson<T>(fileName, this as T);
	}

	public void Load()
	{
		UserPreferencesSystem.onClear += Clear;
		OnLoad();
	}

	protected virtual void OnSave(){}
	protected virtual void OnLoad(){}

	void IHotloadManaged.Created(IReadOnlyDictionary<string, object> state)
	{
		_instance = null;
	}

	void IHotloadManaged.Destroyed(Dictionary<string, object> state)
	{
		_instance = null;
	}

	public void Clear()
	{
		_instance = null;
	}

	~UserPreferences()
	{
		UserPreferencesSystem.onClear -= Clear;
	}
}

public class UserPreferencesSystem : GameObjectSystem
{
	public static event Action onClear;

	public UserPreferencesSystem(Scene scene) : base(scene)
	{
		if (onClear != null)
		{
			onClear();
		}
	}
}