using System;
using System.Reflection;
using Editor;
using Sandbox;
using static Editor.EditorEvent;

public static class UserPreferencesChecker
{
	[Hotload]
	public static void CheckUserPreferences()
	{
		var userPreferencesClasses = GetAllSubclasses(typeof(UserPreferences<>));

		foreach(var userPreferencesClass in userPreferencesClasses)
		{
			//Log.Info($"userPreferencesClass = {userPreferencesClass.Name}");

			var fields = userPreferencesClass.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

			foreach (var field in fields)
			{
				if (field.Name.Contains("k__BackingField"))
				{
					continue;
				}

				if (field.IsDefined(typeof(NonSerializedAttribute), false))
				{
					continue;
				}

				Log.Error($"UserPreferences '{userPreferencesClass.Name}' contains a field '{field.Name}' which will not be serialized by JSON. If this is intentional add the NonSerialized attribute");
			}
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
}
