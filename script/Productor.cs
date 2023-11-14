using Godot;
using System;
using System.Diagnostics.Tracing;

public partial class Productor : Node2D, IWorkable
{
	[Signal]
	public delegate void ProductorTryActionEventHandler(Productor trabajador);
	[Signal]
	public delegate void ProductorNextWorkEventHandler(Productor trabajador);
	[Signal]
	public delegate void ProductorChangeStateEventHandler(State state);
	
	private Timer timer;
	private Timer timerWork;
	private RandomNumberGenerator random;
	private AnimationPlayer animator;
	public int nextIndex { get; set; }
	public Plot currentPlot { get; set; }
	public Vector2 moveTo { get; set; }
	[Export]
	public float distanceFromPlot = 30.0f;
	public float distanceToRest {get; set;}
	public bool isMoving {get; set; }

    public int amountAdd {get; set;}

	public string name {get; set;}
	private State state;

	private String lastDirection;
	public void setState(State state){
        this.state = state;

		EmitSignal(SignalName.ProductorChangeState,(int)this.state);
	}
    public int leftWork { get; set; }

    public void awake(){
		timer.Stop();

		leftWork = random.RandiRange(3,10);
		GD.Print("Productor trabajara: "+ leftWork);
		EmitSignal(SignalName.ProductorTryAction,this);

		GD.Print("Productor Awake");
	}
	public void sleep(){
		timer.WaitTime = random.RandfRange(0.1f,15f);
		setState(State.dormido);

		GD.Print("Productor durmiendo: " + timer.WaitTime);
		timer.Start();
	}
	public override async void _Ready()
	{
		name = "Productor";
		amountAdd = 1;
		distanceToRest = distanceFromPlot;
		timer = new Timer();
		timerWork = new Timer();

		random = new RandomNumberGenerator();
		animator = GetNode<AnimationPlayer>("Sprite2D/AnimationPlayer");
		
		timer.Timeout += awake;

		timerWork.WaitTime = 0.7;
		timerWork.Timeout += endWork;

		AddChild(timer);
		AddChild(timerWork);
	}
	public void endWork(){
		timerWork.Stop();
		currentPlot.setGrow();
		EmitSignal(SignalName.ProductorNextWork,this);
	}
	private string getDirectionMove(){
		if(moveTo.X - Position.X > 0){
			return "Right";
		}

		return "Left";
	}
	public void turnMoving(){
		lastDirection = getDirectionMove();
		animator.Play("Walking/" + lastDirection);
		isMoving = true;
	}
	private void isComeToPosition(){
		switch(state){
			case State.trabajando:
				animator.Play("Working/" + lastDirection);
				timerWork.Start();
				break;
			case State.dormido:
				animator.Play("Sleeping");
				break;
		}

	}

	public override void _Process(double delta)
	{
		if(isMoving){
			Position = Position.MoveToward(moveTo,(float)delta * 100.0f);

			if(Position.DistanceTo(moveTo) <= 0.1){
				isComeToPosition();
				isMoving = false;
			}
		}
	}

    public bool canWork(int amount)
    {
		return amount < 35;
    }

}
