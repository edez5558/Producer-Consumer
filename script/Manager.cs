using System;
using Godot;
using Godot.Collections;
public partial class Manager : Node
{
	[Export]
	public PackedScene PlotScene {get; set;}

	[Export]
	public float radius = 225;
	[Export]
	public float radiusP = 210;

	[Export]
	public float nContenedores = 35;
	private Productor productor;
	private CardInfo cardProductor;
	private Consumidor consumidor;
	private CardInfo cardConsumidor;
	private Array<Plot> contenedor;
	private bool isUsingContendor;
	private int tileUsing;
	private int workLeft;
	private int[] currentIndex;
	private IWorkable workerWaiting;
	private Label bufferLabel;
	private Label infoLabel;
	private Timer timerClean;
	private Array<String> logs;

	public override void _Ready()
	{
		workerWaiting = null;
		currentIndex = new int[2];

		float randias = 2*Mathf.Pi / nContenedores;	
		float sumRandias= - Mathf.Pi / 2.0f;

		isUsingContendor = false;
		currentIndex[0] = 0;
		currentIndex[1] = 0;
		workLeft = 0;
		tileUsing = 0;
		
		logs = new Array<string>();
		Node2D farmer = GetChild<Node2D>(0);
		Node2D cow = GetChild<Node2D>(1);
		bufferLabel = GetChild<Label>(2);
		infoLabel = GetChild<Label>(3);
		bufferLabel.Text = "";
		infoLabel.Text = "";

		timerClean = new Timer();
		timerClean.WaitTime = 2;
		timerClean.Timeout += cleanMessage;

		AddChild(timerClean);

		cardProductor = GetNode<CardInfo>("../CardFarmer");
		cardConsumidor = GetNode<CardInfo>("../CardCow");



		productor = farmer as Productor;
		consumidor = cow as Consumidor;

		if(productor == null){
			GD.Print("Warning: Productor null");
		}

		farmer.Position = new Vector2(
									radiusP*Mathf.Cos(sumRandias),
									radiusP*Mathf.Sin(sumRandias));

		cow.Position = new Vector2(
									radiusP*Mathf.Cos(sumRandias),
									radiusP*Mathf.Sin(sumRandias));


		contenedor = new Array<Plot>();
		for(int i = 0; i < nContenedores; i++){
			Node2D node2d = PlotScene.Instantiate<Node2D>();
			Plot plot = node2d as Plot;

			contenedor.Add(plot);

			node2d.Position = new Vector2(
										radius*Mathf.Cos(sumRandias),
										radius*Mathf.Sin(sumRandias));

			plot.displaceLabel(sumRandias);
			plot.setLabel(i+1);

			sumRandias += randias;
			AddChild(node2d);
		}
	}
	private void showMessage(String msg){
		lock(logs){
			logs.Add(msg);
		}

		updateMessage();

		lock(timerClean){
			if(timerClean.IsStopped())
				timerClean.Start();
		}
	}
	private void updateMessage(){
		lock(logs){
			infoLabel.Text = "";
			foreach(String msg in logs){
				infoLabel.Text += msg + '\n';
			}
		}
	}
	private void cleanMessage(){
		lock(logs){
			logs.RemoveAt(0);

			lock(timerClean){
				if(logs.Count == 0)
					timerClean.Stop();
			}
		}

		updateMessage();
	}
	private int nextIndex(int current){
		return (current + 1)%35;
	}
	public void nextPlot(IWorkable trabajador){
		if(trabajador.leftWork > 0 && trabajador.canWork(tileUsing)){
			trabajador.currentPlot = contenedor[trabajador.nextIndex];
			trabajador.nextIndex = nextIndex(trabajador.nextIndex);
			trabajador.leftWork--;
			trabajador.moveTo = trabajador.currentPlot.Position;

			trabajador.turnMoving();
			tileUsing += trabajador.amountAdd;
		}else{
			GD.Print("Trabajador no puede trabajar mas");
			if(trabajador.leftWork > 0){
				showMessage(trabajador.name + " no puede trabajar mas");
			}
			Vector2 nextPosition = contenedor[trabajador.nextIndex].Position;
			trabajador.sleep();
			trabajador.moveTo =	nextPosition + 
								nextPosition.DirectionTo(Vector2.Zero) * 
								trabajador.distanceToRest;
			trabajador.turnMoving();

			isUsingContendor = false;
			bufferLabel.Text = "Buffer libre";
			showMessage(trabajador.name + " saliendo del buffer");

			if(workerWaiting == null) return;

			tryAction(workerWaiting);
			workerWaiting = null;
		}
	}

	public void tryAction(IWorkable trabajador){
		lock(this){
			if(!trabajador.canWork(tileUsing)){
				GD.Print("Trabajador no puede trabajar");
				showMessage(trabajador.name + " no puede trabajar");
				trabajador.sleep();
				return;
			}


			if(isUsingContendor){
				GD.Print("Trabajador intentando trabajar");
				showMessage(trabajador.name + " intentando trabajar");

				workerWaiting = trabajador;

				Vector2 nextPosition = contenedor[trabajador.nextIndex].Position;
				trabajador.moveTo =	nextPosition + 
								nextPosition.DirectionTo(Vector2.Zero) * 
								(trabajador.distanceToRest - 20.0f);

				trabajador.turnMoving();

				trabajador.setState(State.intento);
				return;
			}


			isUsingContendor = true;
			bufferLabel.Text = "Buffer ocupado";

			showMessage(trabajador.name + " trabajando " + trabajador.leftWork + " espacios");

			nextPlot(trabajador);
			trabajador.setState(State.trabajando);
		}
	}
	public void tryActionProductor(Productor productor){
		tryAction(productor);
	}
	public void nextWorkProductor(Productor productor){
		nextPlot(productor);
	}
	public void tryActionConsumidor(Consumidor consumidor){
		tryAction(consumidor);
	}
	public void nextWorkConsumidor(Consumidor consumidor){
		nextPlot(consumidor);
	}

	public void _on_button_pressed(){
		Node nodeButtom = GetNode<Node>("../Button");
		nodeButtom.QueueFree();

		productor.ProductorTryAction += tryActionProductor;
		productor.ProductorNextWork += nextWorkProductor;

		consumidor.ConsumidorTryAction += tryActionConsumidor;
		consumidor.ConsumidorNextWork += nextWorkConsumidor;

		consumidor.ConsumidorChangeState += cardConsumidor.changeState;
		productor.ProductorChangeState += cardProductor.changeState;

		bufferLabel.Text = "Buffer libre";

		Vector2 nextPosition = contenedor[productor.nextIndex].Position;

		productor.sleep();
		productor.moveTo =	nextPosition + 
							nextPosition.DirectionTo(Vector2.Zero) * 
							productor.distanceToRest;
		productor.turnMoving();


		nextPosition = contenedor[consumidor.nextIndex].Position;
		consumidor.sleep();
		consumidor.moveTo =	nextPosition + 
							nextPosition.DirectionTo(Vector2.Zero) * 
							consumidor.distanceToRest;
		consumidor.turnMoving();
	}

	public override void _Process(double delta)
	{
	}

	public override void _Input(InputEvent @event){
    	if (@event is InputEventKey keyEvent && keyEvent.Pressed)
    	{
			if(keyEvent.Keycode == Key.Escape){
				GetTree().Quit();
			}
    }
}
}
