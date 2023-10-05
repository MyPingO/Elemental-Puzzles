using Godot;
using System;

public partial class ResetScene : Button
{
	public Callable OnButtonPressed { get; private set; }

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		OnButtonPressed = new Callable(this, nameof(ReloadThisScene));
		Connect("pressed", OnButtonPressed);
	}

	public void ReloadThisScene()
	{
		GetTree().ReloadCurrentScene();
	}
}
