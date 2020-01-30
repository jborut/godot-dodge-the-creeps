using Godot;
using System;

public class Player : Area2D
{
	[Export]
	public int Speed = 400; // How fast the player will move (pixels/sec).
	
	[Export]
	public Vector2 StartPosition = new Vector2(250, 450); // Start position
	
	[Signal]
	public delegate void Hit();
	
	private Vector2 _screenSize; // Size of the game window.
	private Vector2 _velocity; // movement direction
	
	// Node Position is immutable so we can only set the new object for Position
	// because we don't want to create new object each frame we will keep the position
	// in this local variable
	private Vector2 _position; // current position
	
	private AnimatedSprite _sprite;
	private CollisionShape2D _collisionShape;
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		_screenSize = GetViewport().Size;
		_velocity = new Vector2();
		_sprite = GetNode<AnimatedSprite>("AnimatedSprite");
		_collisionShape = GetNode<CollisionShape2D>("CollisionShape2D");
		Hide();
	}
	
	public void Start()
	{
		// set the position of the player
		_position = StartPosition;
		
		// show the player sprite
		Show();
		
		// enable collisions so that when the mob hits us we can die
		_collisionShape.Disabled = false;
	}

//  // Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(float delta)
	{
		setVelocity();
		setAnimation();
		setPosition(delta);
	}
	
	private void setVelocity()
	{
		_velocity.x = 0;
		_velocity.y = 0;
	
		if (Input.IsActionPressed("ui_right"))
		{
			_velocity.x += 1;
		}
		else if (Input.IsActionPressed("ui_left"))
		{
			_velocity.x -= 1;
		}
	
		if (Input.IsActionPressed("ui_down"))
		{
			_velocity.y += 1;
		}
		else if (Input.IsActionPressed("ui_up"))
		{
			_velocity.y -= 1;
		}
	}
	
	private void setAnimation()
	{
		if (_velocity.x != 0)
		{
			_sprite.Animation = "right";
			_sprite.FlipH = _velocity.x < 0;
		}
		else if (_velocity.y != 0)
		{
			_sprite.Animation = "up";
			_sprite.FlipV = _velocity.y > 0;
		}
	
		if (_velocity.Length() > 0)
		{
			_velocity = _velocity.Normalized() * Speed;
			_sprite.Play();
		}
		else
		{
			_sprite.Stop();
		}
	}
	
	private void setPosition(float delta)
	{
		_position.x = Mathf.Clamp(_position.x + _velocity.x * delta, 0, _screenSize.x);
		_position.y = Mathf.Clamp(_position.y + _velocity.y * delta, 0, _screenSize.y);
		Position = _position;
	}
	
	private void onPlayerBodyEntered(object body)
	{
		// we have to check that the mob hit us and not any other object (i.e. bomb)
		if (body is Mob)
		{
			EmitSignal("Hit");
			// remove the mob from the screen
			_collisionShape.SetDeferred("disabled", true);
		}
	}
}
