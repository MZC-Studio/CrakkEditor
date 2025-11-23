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

	[Export] public AudioStreamPlayer player;

	private float bpm;
	private int beatNum;

	private bool isPlaying = false;
	private bool isDragging = false;
	
	private int currentPlaceNoteIndex = 0;
	private List<string> noteTypeList = new List<string>();

	private float songTime;
	
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
		bpm = Convert.ToSingle(bpmEdit.Text);
		beatNum = Convert.ToInt32(beatNumEdit.Text);
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
		var fileDialog = new FileDialog();
		fileDialog.FileMode = FileDialog.FileModeEnum.OpenFile;
		fileDialog.Access = FileDialog.AccessEnum.Filesystem;
		fileDialog.AddFilter("*.wav;音频文件");
		AddChild(fileDialog);
		fileDialog.FileSelected += OnMusicSelected;
		fileDialog.PopupCentered(new Vector2I(1920, 1080));
		void OnMusicSelected(string path)
		{
			var audio = AudioStreamWav.LoadFromFile(path);
			player.Stream = audio;
			nameLabel.Text = path.GetFile();
			timeSlider.MaxValue = audio.GetLength();
			tipMask.Hide();
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
