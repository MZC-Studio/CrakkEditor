using Godot;
using System;

public partial class BeatLine : Control
{
	[Export] public Label numberlabel;
	
	public EditorMain controller;
	public int lineNumber, index;
	
	public void Init()
	{
		if (lineNumber != -2)
		{
			numberlabel.Text = lineNumber.ToString();
		}
		else
		{
			numberlabel.QueueFree();
		}
	}
}
