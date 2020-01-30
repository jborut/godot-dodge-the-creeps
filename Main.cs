using Godot;
using System;

public class Main : Node2D
{
	[Export]
	public PackedScene Mob;
	
	[Export]
	public PackedScene Bomb;
	
	[Export]
	public int Bombs = 5; // initial number of available bombs
	
	[Export]
	public int BombHitScore = 10; // how many scores we get when we kill one mob with a bomb
	
	[Export]
	public int BombProximityDistance = 100; // kill zone distance from the center of the bomb

	private int _score;	
	private int _bombs;
	
	private Random _random = new Random();
	private Player _player;
	
	private Timer _startTimer;
	private Timer _mobTimer;
	private Timer _scoreTimer;
	
	private AudioStreamPlayer _bgMusic;
	private AudioStreamPlayer _gameOverMusic;
	
	private HUD _hud;
	
	private PathFollow2D _mobSpawnLocation;
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		_player = GetNode<Player>("Player");
		_mobTimer = GetNode<Timer>("MobTimer");
		_startTimer = GetNode<Timer>("StartTimer");
		_scoreTimer = GetNode<Timer>("ScoreTimer");
		_bgMusic = GetNode<AudioStreamPlayer>("BgMusic");
		_gameOverMusic = GetNode<AudioStreamPlayer>("GameOverMusic");
		_hud = GetNode<HUD>("CanvasLayer");
		_mobSpawnLocation = GetNode<PathFollow2D>("MobPath/MobSpawnLocation");
	}
	
	public void NewGame()
	{
		// set initial score and number of bombs
		_score = 0;
		_bombs = Bombs;
		
		// display those values
		_hud.UpdateScore(_score);
		_hud.UpdateBombs(_bombs);
		
		// show basic instructions before the game starts
		_hud.ShowMessage("Arrows to move, Space for Bombs!", 2);

		_startTimer.Start();
		_bgMusic.Play();
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(float delta)
	{
		// if user presses space and he has some bombs left, we will drop it
		if (Input.IsActionJustPressed("ui_select") && _bombs > 0)
		{
			// decrease the number of available bombs
			_bombs--;
			
			// create new bomb instance and add it to the scene
			var bombInstance = (RigidBody2D)Bomb.Instance();
			AddChild(bombInstance);
			
			// place it under the player
			bombInstance.Position = _player.Position;
			
			// subscribe to the explode signal from the bomb
			bombInstance.Connect("Explode", this, nameof(onBombExplode));
		}
	}

	// this will return a random number in a given range
	private float RandRange(float min, float max)
	{
		return (float)_random.NextDouble() * (max - min) + min;
	}
	
	private void gameOver()
	{
		// lets clear up the scene
		foreach(var node in GetChildren())
		{
			if (node is Mob)
			{
				((Mob)node).QueueFree();
			}
			else if (node is Bomb)
			{
				((Bomb)node).QueueFree();
			}
		}
		
		// stop all the timers
		_mobTimer.Stop();
		_scoreTimer.Stop();
		
		// show a game over message
		_hud.ShowGameOver();
		
		// stop background music music
		_bgMusic.Stop();
		
		// and play game over music
		_gameOverMusic.Play();
		
		// hide the player
		_player.Hide();		
	}
	
	private void mobTimerTimeout()
	{
		// Choose a random location on Path2D.
		_mobSpawnLocation.Offset = _random.Next();

		// Create a Mob instance and add it to the scene.
		var mobInstance = (RigidBody2D)Mob.Instance();
		AddChild(mobInstance);

		// Set the mob's direction perpendicular to the path direction.
		float direction = _mobSpawnLocation.Rotation + Mathf.Pi / 2;

		// Set the mob's position to a random location.
		mobInstance.Position = _mobSpawnLocation.Position;

		// Add some randomness to the direction.
		direction += RandRange(-Mathf.Pi / 4, Mathf.Pi / 4);
		mobInstance.Rotation = direction;

		// Choose the velocity.
		mobInstance.LinearVelocity = new Vector2(RandRange(150f, 250f), 0).Rotated(direction);
		
		// decrease timer
		_mobTimer.WaitTime = _score > 30 ? 0.5f : (-0.03333f * _score + 1.5f);
	}

	private void scoreTimerTimeout()
	{
		// every second we will increase score by 1
		_score++;
		
		// every 10 seconds we will get one bomb
		if (_score % 10 == 0)
			_bombs++;
			
		// display current values
		_hud.UpdateScore(_score);
		_hud.UpdateBombs(_bombs);
	}
	
	private void startTimerTimeout()
	{
		// start the game, show player and start mob spawning timer
		_player.Start();
		_mobTimer.Start();
		_scoreTimer.Start();
	}
	
	private void CanvasLayerStartGame()
	{
		// this is a connection from a HUD signal when player hits Start button
		NewGame();
	}

	private void onBombExplode(Vector2 position)
	{
		// we need to go thru all the nodes to see if there are some that are 
		// in close proximity of the bomb that just exploded
		foreach(var node in GetChildren())
		{
			if (node is Player)
			{
				// if node is player and it is close to the bomb, game over
				if (((Player)node).Position.DistanceTo(position) < BombProximityDistance)
				{
					gameOver();
				}
			}
			else if (node is Mob)
			{
				// if node is mob and it is close to the bomb, remove it from the sceen and increase the score
				if (((Mob)node).Position.DistanceTo(position) < BombProximityDistance)
				{
					((Mob)node).QueueFree();
					_score += BombHitScore;
				}
			}
		}
	}
}
