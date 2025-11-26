using System;
using Godot;
using System.Collections.Generic;
using Environment = System.Environment;

public partial class EditorMain : Control
{
	[Export] public LineEdit bpmEdit, beatNumEdit;
	[Export] public LineEdit trackEdit, holdTimeEdit, noteBPMEdit, xPosEdit, yPosEdit;
	
	[Export] public Button playButton, pauseButton;
	[Export] public Button saveButton, loadChartButton, loadMusicButton, previewButton, reloadButton;
	[Export] public Button[] switchButtons = new Button[5];
	
	[Export] public Label nameLabel, timeLabel, currentTypeLabel, FPSLabel;
	[Export] public HSlider timeSlider;
	[Export] public ColorRect tipMask;

	[Export] public Control camEventsParent, lineParent, judgeLine;
	[Export] public PackedScene beatLine, note;
	
	[Export] public FileDialog fileDialog;

	[Export] public AudioStreamPlayer player;
	
	private float bpm;
	private int beatNum;

	private bool isPlaying = false;
	private bool isDragging = false;
	
	private int currentPlaceNoteIndex = 0;
	private List<string> noteTypeList = new List<string>();

	private float songTime;
	private float parentStartPos, topPos;
	
	private float distanceOfBeats;
	private float scrollSpeedPerMin;
	private float speed = 600f;

	public override void _Ready()
	{
		tipMask.Show();
		
		bpm = 120;
		beatNum = 4;

		bpmEdit.Text = bpm.ToString();
		beatNumEdit.Text = beatNum.ToString();
		
		for (var index = 0; index < switchButtons.Length; index++)
		{
			var button = switchButtons[index];
			var index1 = index;
			noteTypeList.Add(button.Text);
			button.Pressed += () => LoadType(index1);
		}

		parentStartPos = lineParent.Position.Y;
		
		trackEdit.Show();
		noteBPMEdit.Show();
			
		holdTimeEdit.Hide();
		xPosEdit.Hide();
		yPosEdit.Hide();
		
		bpmEdit.TextSubmitted += t => OnDataChanged();
		beatNumEdit.TextSubmitted += t => OnDataChanged();

		playButton.Pressed += () =>  { ChangePlayState(true); };
		pauseButton.Pressed += () =>  { ChangePlayState(false); };

		loadMusicButton.Pressed += SelectMusic;

		timeSlider.DragStarted += TimeSliderStartDrag;
		timeSlider.DragEnded += TimeSliderEndDrag;
	}

	public override void _Process(double delta)
	{
		FPSLabel.Text = "FPS  " + (int)Performance.GetMonitor(Performance.Monitor.TimeFps);
		if (player.GetStream() != null)
		{
			if (!isDragging)
			{
				if (player.IsPlaying())
				{
					timeLabel.Text = SecondsToMMSS((int)player.GetPlaybackPosition()) 
					                 + "/" + SecondsToMMSS((int)player.GetStream().GetLength());
					timeSlider.Value = player.GetPlaybackPosition();

					lineParent.Position = lineParent.Position with { Y = parentStartPos + distanceOfBeats * bpm / 60 * player.GetPlaybackPosition() };
					camEventsParent.Position = camEventsParent.Position with { Y = parentStartPos + distanceOfBeats * bpm / 60 * player.GetPlaybackPosition() };
				}
				else
				{
					timeLabel.Text = SecondsToMMSS((int)songTime) 
					                 + "/" + SecondsToMMSS((int)player.GetStream().GetLength());
					timeSlider.Value = songTime;
				}
			}
			else
			{
				songTime = (float)timeSlider.Value;
				timeLabel.Text = SecondsToMMSS((int)songTime) 
				                 + "/" + SecondsToMMSS((int)player.GetStream().GetLength());
			}
		}
	}

	public override void _Input(InputEvent @event)
	{
		if (@event is InputEventMouseButton eventKey)
		{
			if (eventKey.ButtonIndex == MouseButton.WheelUp)
			{
				if (lineParent.Position.Y - parentStartPos >= speed / 3)
				{
					if (player.IsPlaying())
					{
						player.Stop();
					}
					lineParent.Position += Vector2.Up * speed / 3; 
					camEventsParent.Position += Vector2.Up * speed / 3;
					songTime = (lineParent.Position.Y - parentStartPos) / distanceOfBeats / bpm * 60;
				}
				else
				{
					lineParent.Position = lineParent.Position with { Y = parentStartPos };
					camEventsParent.Position = camEventsParent.Position with { Y = parentStartPos };
				}
			}
			else if (eventKey.ButtonIndex == MouseButton.WheelDown)
			{
				if(player.IsPlaying())
				{					
					player.Stop();
				}
				lineParent.Position += Vector2.Down * speed / 3; 
				camEventsParent.Position += Vector2.Down * speed / 3;
				songTime = (lineParent.Position.Y - parentStartPos) / distanceOfBeats / bpm * 60;
			}
		}
	}

	private void LoadType(int index)
	{
		currentPlaceNoteIndex = index;
		currentTypeLabel.Text = "目前：" + noteTypeList[currentPlaceNoteIndex];
		if (currentPlaceNoteIndex is 0 or 2)
		{
			trackEdit.Show();
			noteBPMEdit.Show();
			
			holdTimeEdit.Hide();
			xPosEdit.Hide();
			yPosEdit.Hide();
		}
		else if(currentPlaceNoteIndex is 1)
		{
			trackEdit.Show();
			noteBPMEdit.Show();
			holdTimeEdit.Show();
			
			xPosEdit.Hide();
			yPosEdit.Hide();
		}
		else
		{
			trackEdit.Show();
			noteBPMEdit.Show();
			xPosEdit.Show();
			yPosEdit.Show();
			
			holdTimeEdit.Hide();
		}
	}

	private void OnDataChanged()
	{
		player.Stop();
		songTime = 0;
		
		bpm = Convert.ToSingle(bpmEdit.Text);
		beatNum = Convert.ToInt32(beatNumEdit.Text);
		
		GenBeatLine();
	}

	private void ChangePlayState(bool isPlay)
	{
		isPlaying = isPlay;
		playButton.Disabled = isPlaying;
		pauseButton.Disabled = !isPlaying;
		if (isPlaying)
		{
			player.Play(songTime);
		}
		else
		{
			songTime = player.GetPlaybackPosition();
			player.Stop();
		}
	}
	
	private void SelectMusic()
	{
		fileDialog.FileMode = FileDialog.FileModeEnum.OpenFile;
		fileDialog.Access = FileDialog.AccessEnum.Filesystem;
		fileDialog.AddFilter("*.wav;音频文件");
		fileDialog.FileSelected += OnMusicSelected;
		fileDialog.PopupCentered();

		void OnMusicSelected(string path)
		{
			var audio = AudioStreamWav.LoadFromFile(path);
			player.Stream = audio;
			nameLabel.Text = path.GetFile();
			timeSlider.MaxValue = audio.GetLength();
			tipMask.Hide();
			GenBeatLine();
		}
	}

	private void GenBeatLine()
	{
		lineParent.Position = lineParent.Position with { Y = parentStartPos };
		camEventsParent.Position = camEventsParent.Position with { Y = parentStartPos };
		
		foreach (var child in lineParent.GetChildren())
		{
			if(child.Name != judgeLine.Name) child.QueueFree();
		}
		
		float beatsCount = (float)(player.GetStream().GetLength() / 60f * bpm);
		distanceOfBeats = (float)(player.GetStream().GetLength() * speed / beatsCount);
		scrollSpeedPerMin = distanceOfBeats * bpm;
		
		float lineNum = beatsCount + 2 + beatsCount * (beatNum - 1);
		float bottomPos = judgeLine.Position.Y;
		int i = 0;
		int number = 0;
		while (i < (int)lineNum)
		{
			Control line = (Control)beatLine.Instantiate();
			lineParent.AddChild(line);
			line.Position = new Vector2(judgeLine.Position.X, bottomPos - i * distanceOfBeats / beatNum);
			
			if (i % beatNum == 0)
			{
				((BeatLine)line).lineNumber = number;
				number++;
			}
			else
			{
				((BeatLine)line).lineNumber = -2;
				line.SelfModulate = new Color(0.5f, 0.5f, 0.5f);
			}

			((BeatLine)line).controller = this;
			((BeatLine)line).index = i;
			((BeatLine)line).Init();
			if (i + 1 == (int)lineNum)
			{
				topPos = line.Position.Y;
			}
			i++;
		}
	}

	private void TimeSliderEndDrag(bool a)
	{
		player.Play(songTime);
		isDragging = false;
	}	
	
	private void TimeSliderStartDrag()
	{
		player.Stop();
		isDragging = true;
	}
	
	private string SecondsToMMSS(int totalSeconds)
	{
		int minutes = totalSeconds / 60;
		int seconds = totalSeconds % 60;
		return $"{minutes:D2}:{seconds:D2}";
	}
}