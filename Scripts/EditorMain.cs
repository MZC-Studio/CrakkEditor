using Godot;
using System;

public partial class EditorMain : Control
{
	[Export] public LineEdit bpmEdit, beatNumEdit, trackEdit, holdTimeEdit, noteBPMEdit, xPosEdit, yPosEdit;
	[Export] public Button bpmListButton, playButton, pauseButton, saveButton, loadChartButton, loadMusicButton, previewButton, reloadButton;
	[Export] public Button[] switchButtons = new Button[5];
	[Export] public Label nameLabel, timeLabel, currentTypeLabel, FPSLabel;
	[Export] public HSlider timeSlider;

	public override void _Ready()
	{
	}

	public override void _Process(double delta)
	{
	}
}
