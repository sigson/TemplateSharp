using Godot;
using System;

public partial class DebuggerDelay : Node
{
    [Signal]
    public delegate void RunGameEventHandler();

    public event RunGameEventHandler RunGameEvent;
	[Export]
	public bool AutoRun;
    [Export]
    public string MainScene;
    public override void _Ready()
	{
#if !DEBUG
		AutoRun = true;
#endif
    }

	public override void _Process(double delta)
	{
		if(AutoRun)
		{
            GetTree().ChangeSceneToFile(MainScene);
        }
	}

    public void RunButton()
    {
        AutoRun = true;
    }
}
