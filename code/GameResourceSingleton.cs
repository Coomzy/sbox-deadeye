
using Sandbox.Citizen;
using Sandbox.Internal;
using System.IO;
using System;
using Sandbox;

public abstract class GameResourceSingleton<T> : GameResource where T : GameResourceSingleton<T>
{
	static T _instance;
	public static T instance
	{
		get
		{
			//Log.Info($"Game.IsEditor = {Game.IsEditor}, Application.IsEditor {Application.IsEditor}");
			if (Game.IsEditor)
			{
				_instance = null;
			}

			if (_instance != null)
			{
				return _instance;
			}

			Type type = typeof(T);

			if (!Attribute.IsDefined(type, typeof(GameResourceAttribute)))
			{
				Log.Error($"Type '{type}' does not have the required GameResourceAttribute on it!");
			}
			var gameResourceAttribute = (GameResourceAttribute)Attribute.GetCustomAttribute(type, typeof(GameResourceAttribute));

			if (gameResourceAttribute == null)
			{
				Log.Error($"Type '{type}' does not have the required GameResourceAttribute on it!");
			}

			string fileName = $@"{type.ToSimpleString(false)}";
			string filePath = $@"ProjectSettings\{fileName}.{gameResourceAttribute.Extension}";
			string fullFilePath = $@"{Project.Current.GetAssetsPath()}\{filePath}";

			if (ResourceLibrary.TryGet(filePath, out T inst))
			{
				_instance = inst;
				return _instance;
			}

			/*T newInst = Activator.CreateInstance<T>();
			newInst.ResourceName = fileName;
			newInst.ResourcePath = filePath;
			var sceneAsset = AssetSystem.CreateResource("prefab", location);

			return newInst;*/

			return null;
		}
	}
}
