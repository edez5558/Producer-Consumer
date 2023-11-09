using Godot;
public interface IWorkable 
{
    
    public int nextIndex {get; set;}
    public int leftWork {get; set;}
    public Plot currentPlot {get; set;}
    public bool canWork(int amount);
    public void setState(State state);
    public void sleep();
}
