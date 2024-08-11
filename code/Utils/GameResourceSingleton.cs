
using Sandbox.Citizen;
using Sandbox.Internal;
using System.IO;
using System;
using Sandbox;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.IO.Enumeration;

public abstract partial class GameResourceSingleton<T> : GameResource, IHotloadManaged where T : GameResourceSingleton<T>
{
	public static string fileName => $@"{typeof(T).ToSimpleString(false)}";
	public static string filePath => $@"{filePathWithoutExtension}.{fileExtension}";
	public static string filePathWithoutExtension => $@"ProjectSettings\{fileName}";
	public static string fullFilePath => $@"{Project.Current.GetAssetsPath()}\{filePath}";
	public static string fullFilePathWithoutExtension => $@"{Project.Current.GetAssetsPath()}\{filePathWithoutExtension}";

	public static string fileExtension
	{
		get
		{

			Type type = typeof(T);

			if (!Attribute.IsDefined(type, typeof(GameResourceAttribute)))
			{
				Log.Error($"Type '{typeof(T)}' does not have the required GameResourceAttribute on it!");
			}
			var gameResourceAttribute = (GameResourceAttribute)Attribute.GetCustomAttribute(typeof(T), typeof(GameResourceAttribute));

			if (gameResourceAttribute == null)
			{
				Log.Error($"Type '{typeof(T)}' does not have the required GameResourceAttribute on it!");
				return "";
			}

			return gameResourceAttribute.Extension;
		}
	}

#pragma warning disable SB3000 // Hotloading not supported
	static T _instance;
#pragma warning restore SB3000 // Hotloading not supported
	public static T instance
	{
		get
		{
			//Log.Info($"Game.IsEditor = {Game.IsEditor}, Application.IsEditor {Application.IsEditor}");

			// TODO: Figure out how to make this only at edit time!
			if (Game.IsEditor)
			{
				_instance = null;
			}

			if (_instance != null)
			{
				return _instance;
			}

			Type type = typeof(T);

			if (ResourceLibrary.TryGet(filePath, out T inst))
			{
				_instance = inst;
				return _instance;
			}

			if (Game.IsEditor)
			{
				Log.Error($"GameResourceSingleton<{type.ToSimpleString(false)}> need a file in /ProjectSettings/");
				return null;
			}

			T newInst = Activator.CreateInstance<T>();
			//newInst.ResourceName = fileName;
			//newInst.ResourcePath = filePath;
			//var sceneAsset = AssetSystem.CreateResource("prefab", location);
			
			

			if (Game.IsEditor)
			{
				Log.Error($"GameResourceSingleton<{type.ToSimpleString(false)}> need a file in /ProjectSettings/");
				return newInst;
			}

			return newInst;
		}
	}

	void IHotloadManaged.Created(IReadOnlyDictionary<string, object> state)
	{
		_instance = null;
	}

	void IHotloadManaged.Destroyed(Dictionary<string, object> state)
	{
		_instance = null;
	}
}
