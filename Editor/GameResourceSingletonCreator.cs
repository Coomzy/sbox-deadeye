using System;
using System.Reflection;
using Editor;
using Sandbox;
using static Editor.EditorEvent;

public static class GameResourceSingletonCreator
{
	[Hotload]
	public static void CreateGameResourceSingletonFiles()
	{
		var gameResourceClasses = GetAllSubclasses(typeof(GameResourceSingleton<>));

		foreach(var gameResourceClass in gameResourceClasses)
		{
			//Log.Info($"gameResoureClass = {gameResourceClass.Name}");
			var instance = CreateInstance(gameResourceClass) as GameResource;

			Type constructedType = typeof(GameResourceSingleton<>).MakeGenericType(gameResourceClass);
			var filePathProperty = constructedType.GetProperty("fullFilePathWithoutExtension", BindingFlags.Static | BindingFlags.Public);
			var fileExtensionProperty = constructedType.GetProperty("fileExtension", BindingFlags.Static | BindingFlags.Public);

			string filePath = filePathProperty != null ? filePathProperty.GetValue(null) as string : null;
			string fileExtension = fileExtensionProperty != null ? fileExtensionProperty.GetValue(null) as string : null;

			//Log.Info($"gameResoureClass = {gameResourceClass.Name}, filePath = {filePath}, fileExtension = {fileExtension}");			
			AssetSystem.CreateResource(fileExtension, filePath);
		}
	}

	public static IEnumerable<Type> GetAllSubclasses(Type genericBaseType)
	{
		if (!genericBaseType.IsGenericType)
			throw new ArgumentException("The provided type must be a generic type.", nameof(genericBaseType));

		return Assembly.GetAssembly(genericBaseType)
					   .GetTypes()
					   .Where(type => type.IsClass && !type.IsAbstract && IsSubclassOfGeneric(type, genericBaseType));
	}

	static bool IsSubclassOfGeneric(Type type, Type genericBaseType)
	{
		while (type != null && type != typeof(object))
		{
			var cur = type.IsGenericType ? type.GetGenericTypeDefinition() : type;
			if (genericBaseType == cur)
			{
				return true;
			}
			type = type.BaseType;
		}
		return false;
	}

	public static object CreateInstance(Type type)
	{
		if (type == null)
			throw new ArgumentNullException(nameof(type));

		// Ensure the type has a parameterless constructor
		if (type.GetConstructor(Type.EmptyTypes) == null)
			throw new InvalidOperationException($"The type {type.FullName} must have a parameterless constructor.");

		return Activator.CreateInstance(type);
	}
}
