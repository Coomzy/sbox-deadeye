public sealed class Test : Component
{
	[Button("Hi")]
	private void CreateObj()
	{
		var newObj = GameObject.Scene.CreateObject();
		newObj.SetPrefabSource("cube.prefab");
		newObj.UpdateFromPrefab();
	}
}