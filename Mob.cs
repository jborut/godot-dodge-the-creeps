using Godot;
using System;

public class Mob : RigidBody2D
{
	[Export]
	public int MinSpeed = 150; // Minimum speed range.

	[Export]
	public int MaxSpeed = 250; // Maximum speed range.

	// types of mobs, each will have different animation
	private String[] _mobTypes = {"walk", "swim", "fly"};
	
	static private Random _random = new Random();
	
	private AnimatedSprite _sprite;
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		_sprite = GetNode<AnimatedSprite>("AnimatedSprite");
		// set appropriate animation based on random number and play it
		_sprite.Animation = _mobTypes[_random.Next(0, _mobTypes.Length)];
		_sprite.Play();
	}
	
	// once the mob goes out of the screen, remove it from the scene
	public void OnVisibilityScreenExited()
	{
		QueueFree();
	}
}
