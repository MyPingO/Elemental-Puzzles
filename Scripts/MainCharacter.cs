using Godot;
using System;
using System.Threading.Tasks;

public partial class MainCharacter : CharacterBody2D
{
	[Export]
	public float Speed = 300.0f;

	[Export]
	public float JumpVelocity = -400.0f;

	// Get the gravity from the project settings to be synced with RigidBody nodes.
	public float gravity = ProjectSettings.GetSetting("physics/2d/default_gravity").AsSingle();
	private AnimationPlayer animationPlayer;
	private Sprite2D sprite;
	bool isFalling = false;

	public override void _Ready()
	{
		// Get a reference to the AnimationPlayer node.
		animationPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
		sprite = GetNode<Sprite2D>("Sprite2D");
		// Call animation called CharacterIdle.
		animationPlayer.Play("CharacterIdle");
	}

	public override void _Input(InputEvent @event)
	{
		// Handle the mouse click.
		if (
			@event is InputEventMouseButton mouseButton
			&& mouseButton.ButtonIndex == MouseButton.Left
			&& mouseButton.Pressed
		)
		{
			GD.Print("Attack");
			animationPlayer.Play("CharacterAttack");
		}
	}

	public override void _PhysicsProcess(double delta)
	{
		// Movement
		Vector2 velocity = Velocity;

		// Add the gravity.
		if (!IsOnFloor())
			velocity.Y += gravity * (float)delta;

		if (!IsOnFloor() && velocity.Y > 0 && !isFalling)
		{
			HandleFallingAnimation();
		}

		// Get the input direction and handle the movement/deceleration.
		// As good practice, you should replace UI actions with custom gameplay actions.
		float direction = Input.GetAxis("left", "right");
		if (direction != 0)
		{
			velocity.X = direction * Speed;
			if (direction != 0)
			{
				sprite.FlipH = direction < 0;
			}

			if (IsOnFloor() && velocity.X != 0 && !IsCharacterAttacking())
				animationPlayer.Play("CharacterRunning");
		}
		else
		{
			velocity.X = Mathf.MoveToward(Velocity.X, 0, Speed);
			if (velocity.X == 0 && IsOnFloor() && !IsCharacterAttacking())
			{
				animationPlayer.Play("CharacterIdle");
			}
		}

		// Handle Jump.
		if (Input.IsActionJustPressed("space") && IsOnFloor())
		{
			velocity.Y = JumpVelocity;
			animationPlayer.Play("CharacterJumpUp");
		}
		Velocity = velocity;
		MoveAndSlide();
	}

	bool IsCharacterAttacking()
	{
		return animationPlayer.CurrentAnimation == "CharacterAttack";
	}

	public async void HandleFallingAnimation()
	{

		if (animationPlayer.CurrentAnimation != "CharacterJumpDown")
		{
			isFalling = true;
			animationPlayer.Play("CharacterJumpDown");
			await ToSignal(animationPlayer, "animation_finished");

			// if still moving down
			while (!IsOnFloor() && Velocity.Y > 0)
			{
				GD.Print("Falling Down");
				animationPlayer.Play("CharacterFalling");
				await ToSignal(animationPlayer, "animation_finished");
				GD.Print("Falling Down Finished");				
			}

			GD.Print("Landed");
			animationPlayer.Play("CharacterLanding");
			await ToSignal(animationPlayer, "animation_finished");

			isFalling = false;
		}
	}
}
