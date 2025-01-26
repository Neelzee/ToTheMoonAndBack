using Godot;
using System;
using System.Collections.Generic;

public partial class BobMovement3 : CharacterBody2D
{
	[Export] public float Speed = 300.0f;
	public const float MaxSpeed = 1500.0f;
	public const float Friction = 600f;
	[Export] public int radiusMax = 500;
	[Export] public int radiusFace = 200;
	[Export] public int radiusMin = 30;
	public const float bounceVar = 0.7f;
	public int state = 0; // 0 - Normal, 1 - blow-weak, 2 - Blow hard
	public int prevState = 0;
	[Export] public Godot.Collections.Array<Texture2D> faces;
	[Export] public AudioStreamPlayer2D walkingSound;
	[Export] public AudioStream walkingStrong;
	[Export] public AudioStream walkingWeak;
	[Export] public Sprite2D faceManager;
	
	private Vector2 InputDirection = Vector2.Zero;

	private void UpdateFace(float distance) {
		if (Input.IsActionPressed("cursor_left") && distance < radiusMax)
		{
			if (distance > radiusFace)
			{
				state = 1;
			}
			else
			{
				state = 2;
			}
		}
		else 
		{
			state = 0;
		}
		if (prevState != state) {
			faceManager.SetTexture(faces[state]);
			prevState = state;
		}
	}
	private Vector2 GetInpDir(){
		Vector2 relativeVector = Vector2.Zero;
		float dist = 0;
		if(Input.IsActionPressed("cursor_left")){
			Vector2 mousePosition = GetGlobalMousePosition();
			Vector2 playerPosition = this.GlobalPosition;
			relativeVector = new Vector2(playerPosition.X - mousePosition.X, playerPosition.Y - mousePosition.Y);
			dist = Mathf.Sqrt(Mathf.Pow(relativeVector.X, 2) + Mathf.Pow(relativeVector.Y, 2));
		
			//relativeVector = relativeVector.Normalized();
			
			if(dist < radiusMax){
				float scale = Mathf.Min(radiusMax - dist, radiusMax - radiusMin)/(radiusMax - radiusMin);
				relativeVector.X = relativeVector.X;
				relativeVector.Y = relativeVector.Y;
				relativeVector = relativeVector.Normalized() * scale;
			}
			else{
				relativeVector = Vector2.Zero;
			}
		}
		UpdateFace(dist);
		PlayWalkingSound(dist);
		return relativeVector;
	}

	public void UpdateVelocity(double delta){
		Vector2 InputDirection = GetInpDir();
		Vector2 vel = Vector2.Zero;
		Vector2 new_vel = GetVelocity();
		float scaleSpeedY = Mathf.Abs(InputDirection.Y);
		float scaleSpeedX = Mathf.Abs(InputDirection.X);
		
		
		vel = InputDirection * MaxSpeed;

		if(InputDirection.X != 0){
			new_vel.X = (float)Mathf.MoveToward(Velocity.X, vel.X, delta*Speed*scaleSpeedX);
		}
		else{
			new_vel.X = (float)Mathf.MoveToward(Velocity.X, 0, delta*(Friction/MaxSpeed)*Speed);
		}
		if(InputDirection.Y != 0){
			new_vel.Y = (float)Mathf.MoveToward(Velocity.Y, vel.Y, delta*Speed*scaleSpeedY);
		}
		else{
			new_vel.Y = (float)Mathf.MoveToward(Velocity.Y, 0, delta*(Friction/MaxSpeed)*Speed);
		}
		
		SetVelocity(new_vel);
	}
	
	public override void _PhysicsProcess(double delta){
		// We save velocity before collision, to bounce on it
		Vector2 bounceVelocity = GetVelocity() * bounceVar;
		MoveAndSlide();
		if (GetSlideCollisionCount() > 0){
			var collision = GetSlideCollision(0);
			if (collision != null){
				Velocity = bounceVelocity.Bounce(collision.GetNormal());
			}
			
		}
		UpdateVelocity(delta);
	}

	private void PlayWalkingSound(float distance)
	{
		if (Input.IsActionPressed("cursor_left") && distance < radiusMax)
		{
			if (!walkingSound.IsPlaying())
			{
				if (distance > radiusFace)
				{
					walkingSound.Stream = walkingWeak;
				}
				else
				{
					walkingSound.Stream = walkingStrong;
				}

				walkingSound.SetPitchScale((float)GD.RandRange(0.8f, 1.5f));
				walkingSound.Play();
			}
		}
		else
		{
			walkingSound.Stop();
		}

	}


	public void OnBodyEnteredKillZone(PhysicsBody2D body){
		GetTree().CallDeferred("reload_current_scene");
	}
};
