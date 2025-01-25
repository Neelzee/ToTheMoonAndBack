using Godot;
using System;

public partial class Player : CharacterBody2D
{
	private CharacterBody2D _body;
	private float _speed = 100;
	public override void _Ready()
	{
		_body = GetNode<CharacterBody2D>(".");
		if (_body == null) throw new NullReferenceException("CharacterBody2D is null");
	}
	
	public override void _PhysicsProcess(double delta)
	{
		var velocity = _body.Velocity;
		velocity.Y -= (float)(_speed * delta);
		if (velocity.Length() >= _speed * 2)
		{
			velocity = velocity.Normalized() * _speed * 2;
		}
		_body.Velocity = velocity;
		MoveAndSlide();
	}
}
