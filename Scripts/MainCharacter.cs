using Godot;
using System;
using System.Threading.Tasks;
using static Godot.GD;

public partial class MainCharacter : CharacterBody2D
{
    [Export]
    public float Speed = 300.0f;

    [Export]
    public float JumpVelocity = -400.0f;

    enum State
    {
        Idle,
        Running,
        Jumping,
        Falling,
        Landed,
        Attacking
    }

    // Get the gravity from the project settings to be synced with RigidBody nodes.
    public float gravity = ProjectSettings.GetSetting("physics/2d/default_gravity").AsSingle();
    private AnimationPlayer animationPlayer;
    private Sprite2D sprite;
    State currentState = State.Idle;
    State nextState;

    bool isAttacking = false;

    [Export]
    float timeInAir = 0f;

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
        if (
            @event is InputEventMouseButton mouseButton
            && mouseButton.ButtonIndex == MouseButton.Left
            && mouseButton.Pressed
        )
        {

            nextState = State.Attacking;
            isAttacking = true;
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

        // Get the input direction and handle the movement/deceleration.
        // As good practice, you should replace UI actions with custom gameplay actions.
        float direction = Input.GetAxis("left", "right");
        if (direction != 0)
        {
            velocity.X = direction * Speed;
            sprite.FlipH = direction < 0;
        }
        else
        {
            velocity.X = Mathf.MoveToward(Velocity.X, 0, Speed);
        }

        if (IsOnFloor())
        {
            //don't switch to these states if attacking
            if (!IsCharacterAttacking())
            {
                if (
                    currentState == State.Falling
                    && animationPlayer.CurrentAnimation == "CharacterFalling"
                    && timeInAir > 1f
                )
                {
                    nextState = State.Landed;
                }
                else if (animationPlayer.CurrentAnimation != "CharacterLanding")
                {
                    if (velocity.X != 0)
                    {
                        nextState = State.Running;
                    }
                    else
                    {
                        nextState = State.Idle;
                    }
                }
            }
            timeInAir = 0f;
        }
        else
        {
            //don't switch to these states if attacking
            if (!IsCharacterAttacking())
            {
                if (velocity.Y >= 0)
                {
                    nextState = State.Falling;
                }
                else
                {
                    nextState = State.Jumping;
                }
            }

            timeInAir += (float)delta;
        }

        if (Input.IsActionJustPressed("space") && IsOnFloor())
        {
            velocity.Y = JumpVelocity;
            nextState = State.Jumping;
            if (IsCharacterAttacking())
            {
                FinishAttacking();
            }
        }
        if (nextState != currentState)
        {
            switch (nextState)
            {
                case State.Idle:
                    animationPlayer.Play("CharacterIdle");
                    break;
                case State.Running:
                    animationPlayer.Play("CharacterRunning");
                    break;
                case State.Jumping:
                    animationPlayer.Play("CharacterJumpUp");
                    break;
                case State.Falling:
                    if (currentState != State.Attacking)
                        animationPlayer.Play("CharacterJumpDown");
                    animationPlayer.Queue("CharacterFalling");
                    break;
                case State.Landed:
                    animationPlayer.Play("CharacterLanding");
                    break;
            }
        }

        currentState = nextState;

        // Set the velocity.
        Velocity = velocity;
        MoveAndSlide();
    }

    bool IsCharacterAttacking()
    {
        return isAttacking;
    }

    // called at the last frame of animation 'CharacterAttacking'
    void FinishAttacking()
    {
        isAttacking = false;
    }
}
