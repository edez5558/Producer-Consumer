using System;
using Godot;
public interface IWorkable 
{
    
    public int nextIndex {get; set;}
    public int leftWork {get; set;}
    public float distanceToRest { get; set;}
    public Plot currentPlot {get; set;}
    public Vector2 moveTo {get; set;}
    public int amountAdd {get; set;}
    public String name {get; set;}
    public bool canWork(int amount);
    public void setState(State state);
    public void turnMoving();
    public void sleep();
}
