using Godot;
using System;

public class Bomb : RigidBody2D
{
	// notify that the bomb has exploded
	[Signal]
	public delegate void Explode(Vector2 position);
	
	private AnimatedSprite _bombSprite;
	private CollisionShape2D _collisionShape;
	private AudioStreamPlayer _bombPlaceSound;
	private AudioStreamPlayer _bombExplodeSound;
	private Timer _removeTimer;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		_bombSprite = GetNode<AnimatedSprite>("BombSprite");
		_collisionShape = GetNode<CollisionShape2D>("CollisionShape");
		
		// play the ticking animation
		_bombSprite.Animation = "default";
		_bombSprite.Play();
		
		// play the bomb drop sound
		_bombPlaceSound = GetNode<AudioStreamPlayer>("BombPlaceStreamPlayer");
		_bombPlaceSound.Play();	
		
		_bombExplodeSound = GetNode<AudioStreamPlayer>("BombExplosionStreamPlayer");
		_removeTimer = GetNode<Timer>("RemoveTimer");
	}

	private void onExplodeTimerTimeout()
	{
		// change the animation to explosion
		_bombSprite.Animation = "explode";
		
		// start the timer that will remove the bomb
		_removeTimer.Start();
		
		// play explosion sound
		_bombExplodeSound.Play();
		
		// emit Explode signal
		EmitSignal("Explode", Position);
	}
	
	private void onRemoveTimerTimeout()
	{
		// remove the bomb from scene
		QueueFree();
	}
}
