using Godot;
using Godot.Collections;
using System;
using System.Collections.Generic;
using System.Threading;

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
	private Array<Plot> contenedor;
	private bool isUsingContendor;
	private int tileUsing;
	private int workLeft;
	private int[] currentIndex;

	

	public override void _Ready()
	{
		currentIndex = new int[2];

		float randias = 2*Mathf.Pi / nContenedores;	
		float sumRandias= - Mathf.Pi / 2.0f;

		isUsingContendor = false;
		currentIndex[0] = 0;
		currentIndex[1] = 0;
		workLeft = 0;
		tileUsing = 0;
		

		Node2D farmer = GetChild<Node2D>(0);

		productor = farmer as Productor;

		if(productor == null){
			GD.Print("Warning: Productor null");
		}



		farmer.Position = new Vector2(
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
	public void nextPlot(IWorkable trabajador){
		trabajador.currentPlot = contenedor[trabajador.nextIndex];
		trabajador.nextIndex = (trabajador.nextIndex + 1)%35;

		trabajador.setState(State.moviendose);
	}

	public void tryAction(IWorkable trabajador){
		lock(this){
			if(!trabajador.canWork(tileUsing)){
				GD.Print("Trabajador no puede trabajar");
				trabajador.sleep();
				return;
			}


			if(isUsingContendor){
				GD.Print("Trabajador intentado trabajar");

				trabajador.setState(State.intento);
				return;
			}


			isUsingContendor = true;

			nextPlot(trabajador);
		}
	}
	public void tryActionProductor(Productor productor){
		tryAction(productor);
	}
	public void nextWorkProductor(Productor productor){
		nextPlot(productor);
	}

	public void _on_button_pressed(){
		Node nodeButtom = GetNode<Node>("../Button");
		nodeButtom.QueueFree();

		productor.ProductorTryAction += tryActionProductor;
		productor.ProductorNextWork += nextWorkProductor;

		productor.sleep();
	}

	public override void _Process(double delta)
	{
	}
}
