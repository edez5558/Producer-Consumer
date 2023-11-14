using Godot;
using System;

public partial class Plot : Node2D
{
	[Export]
	public float labelRadius;
	private Sprite2D sprite;
	public void displaceLabel(float angle){
		Label aux = GetChild<Label>(1);

		aux.SetPosition(new Vector2(
								aux.Position.X + labelRadius*Mathf.Cos(angle),
								aux.Position.Y + labelRadius*Mathf.Sin(angle)));
	}

	public void setLabel(int number){
		Label aux = GetChild<Label>(1);

		aux.Text = number.ToString("D2");
	}
	public Vector2 getDirToMe(Vector2 player){
		return player.DirectionTo(Position);
	}
	public void setGrow(){
		sprite.FrameCoords = new Vector2I(5,9);
	}
	public void setUnGrow(){
		sprite.FrameCoords = new Vector2I(5,8);
	}
	public override void _Ready()
	{
		sprite = GetChild<Sprite2D>(0);
	}

}
