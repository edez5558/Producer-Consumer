using Godot;
using System;
using System.Diagnostics.Tracing;

public partial class Productor : Node2D, IWorkable
{
	[Signal]
	public delegate void ProductorTryActionEventHandler(Productor trabajador);
	[Signal]
	public delegate void ProductorNextWorkEventHandler(Productor trabajador);
	
	private Timer timer;
	private Timer timerWork;
	private RandomNumberGenerator random;
	private AnimationPlayer animator;
	public int nextIndex { get; set; }
	public Plot currentPlot { get; set; }

	private State state;
	public void setState(State state){
        this.state = state;

		switch(state){
			case State.none: animator.Stop();
				break;

			case State.moviendose: animator.Play("Walk_Right");
				break;
			case State.trabajando: animator.Play("Shovel_Right");
				break;
		
		}
	}
    public int leftWork { get; set; }

    public void awake(){
		timer.Stop();

		EmitSignal(SignalName.ProductorTryAction,this);

		GD.Print("Productor Awake");
	}
	public void sleep(){
		timer.WaitTime = random.RandiRange(1,6);

		GD.Print("Productor durmiendo: " + timer.WaitTime);
		timer.Start();
	}
	public override async void _Ready()
	{
		timer = new Timer();
		timerWork = new Timer();

		random = new RandomNumberGenerator();
		animator = GetNode<AnimationPlayer>("Sprite2D/AnimationPlayer");
		
		timer.Timeout += awake;


		timerWork.WaitTime = 1;
		timerWork.Timeout += endWork;

		AddChild(timer);
		AddChild(timerWork);
	}
	public void endWork(){
		timerWork.Stop();
		EmitSignal(SignalName.ProductorNextWork,this);
	}

	public override void _Process(double delta)
	{
		switch(state){
			case State.none: break;
			case State.moviendose:{

				Position = Position.MoveToward(currentPlot.Position,0.5f);

				if(Position.DistanceTo(currentPlot.Position) <= 0.1){
					setState(State.trabajando);
					timerWork.Start();
				}
			}

			break;

			case State.trabajando:{

			}

			break;
		}
	}

    public bool canWork(int amount)
    {
		return amount < 35;
    }

}
