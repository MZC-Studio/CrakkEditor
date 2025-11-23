using Godot;
using System;

public partial class EditorMain : Control
{
	[Export] public LineEdit bpmEdit, beatNumEdit, trackEdit, holdTimeEdit, noteBPMEdit, xPosEdit, yPosEdit;
	[Export] public Button playButton, pauseButton, saveButton, loadChartButton, loadMusicButton, reloadButton;
	[Export] public Button[] switchButtons = new Button[5];
	[Export] public Label nameLabel, timeLabel, currentTypeLabel, FPSLabel;
	[Export] public HSlider timeSlider;
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
}
