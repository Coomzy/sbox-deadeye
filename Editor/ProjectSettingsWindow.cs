[Dock("Editor", "Project Settings", "snippet_folder")]
public class ProjectSettingsWindow : DockWindow
{
	public ProjectSettingsWindow()
	{
		//var widget = new DockedWidget(this);
		//DockManager.AddDock(null, widget);
	}
}

public class DockedWidget : Widget
{
	public DockedWidget(Widget parent) : base(parent)
	{
		WindowTitle = "Docked Widget";
	}
}