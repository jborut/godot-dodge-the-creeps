using Godot;
using System;

public class HUD : CanvasLayer
{
	[Signal]
	public delegate void StartGame();
	
	private Label _scoreLabel;
	private Label _bombsLabel;
	private Label _messageLabel;
	private Button _startButton;
	private Timer _messageTimer;
	
	// this variable tracks the state of the game
	// because we have a listener for Enter key
	// that makes sense to react to only when the game is in menu state
	private bool _isRunning = false;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		_scoreLabel = GetNode<Label>("ScoreLabel");
		_bombsLabel = GetNode<Label>("BombsLabel");
		_messageLabel = GetNode<Label>("MessageLabel");
		_startButton = GetNode<Button>("StartButton");
		_messageTimer = GetNode<Timer>("MessageTimer");
		
		// initially hide counters, we will show them during the play
		_scoreLabel.Hide();
		_bombsLabel.Hide();
	}
	
	// this method is used to display various messages
	public void ShowMessage(string text, int time = 1)
	{
		// set the message and show it
		_messageLabel.Text = text;
		_messageLabel.Show();

		// set the timer when to hide it again
		_messageTimer.WaitTime = time;
		_messageTimer.Start();
	}
	
	// this method is async because we will wait for game over message to be displayed
	async public void ShowGameOver()
	{
		// show message game over and wait until it is done
		ShowMessage("Game Over");
		await ToSignal(_messageTimer, "timeout");

		// now show the title again
		_messageLabel.Text = "Dodge the\nCreeps!";
		_messageLabel.Show();

		// also show the start button
		_startButton.Show();
		
		// set the isRunning to false
		_isRunning = false;
	}
	
	// this method is used to show the score value
	public void UpdateScore(int score)
	{
		_scoreLabel.Text = score.ToString();
	}
	
	// this method is used to show number of available bombs
	public void UpdateBombs(int bombs)
	{
		_bombsLabel.Text = bombs.ToString();
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(float delta)
	{
		// if user presses enter we will start the game
		// there is only one button on menu screen so there is no other option
		if (Input.IsActionJustPressed("ui_accept") && !_isRunning)
		{
			startButtonPressed();
		}
	}

	// hide message
	private void MessageTimerTimeout()
	{
		_messageLabel.Hide();
	}

	// hide the menu screen and emit StartGame signal
	private void startButtonPressed()
	{
		_isRunning = true;
		_startButton.Hide();
		_messageLabel.Hide();
		_scoreLabel.Show();
		_bombsLabel.Show();
		
		EmitSignal("StartGame");
	}
}
