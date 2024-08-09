public static class MyEditorMenu
{
	[Menu("Editor", "$title/My Menu Option")]
	public static void OpenMyMenu()
	{
		//EditorUtility.DisplayDialog("It worked!", "This is being called from your library's editor code!");
	}
}

/*[Tool("My Custom Editor", "rocket", "Edits custom things!")]
public partial class MainWindow : DockWindow
{
	public MainWindow()
	{
		DeleteOnClose = true;
		Size = new Vector2(480, 360);
		Title = "My Custom Editor";
	}
}*/

[Dock("Editor", "My Custom Dock", "snippet_folder")]
public class MyCustomDock : Widget
{
	public LineEdit GordonsName;
	public MyCustomDock(Widget parent) : base(parent)
	{
		//Layout.Add(new Label("1. What is the name of the character you play as?", this));

		//single line text input
		GordonsName = Layout.Add(new LineEdit(this) { PlaceholderText = "Firstname Lastname" });

		var button = new Button("Ping", "arrow_upward");
		button.Clicked = () =>
		{
			Log.Info("Pong");
		};
	}
}