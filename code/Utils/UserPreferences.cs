
using Sandbox.Citizen;
using Sandbox.Internal;
using System.IO;
using System;
using Sandbox;
using static Sandbox.Gizmo;
using Sandbox.Audio;
using System.Runtime.CompilerServices;

public abstract class UserPreferences<T> : IHotloadManaged where T : UserPreferences<T>
{
	//static string fileName => typeof(T).ToSimpleString(false);
	static string fileName => $"{typeof(T).ToSimpleString(false)}.json";

#pragma warning disable SB3000 // Hotloading not supported
	public static T _instance;
#pragma warning restore SB3000 // Hotloading not supported
	public static T instance
	{
		get
		{
			if (Utils.isEditTime)
			{
				_instance = null;
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
					_instance.OnLoad();
					return _instance;
				}
			}

			T newInst = Activator.CreateInstance<T>();
			_instance = newInst;
			_instance.OnLoad();
			_instance.Save();
			return _instance;
		}
	}

	public void Save()
	{
		OnSave();
		FileSystem.Data.WriteJson(fileName, this as T);
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
}
