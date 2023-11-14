using Godot;
using System;
using System.Collections;

public partial class CardInfo : Node2D
{
    private Label label;
    private Sprite2D sprite;

    [Export]
    public int index = 0;
    public void changeState(State state){
        switch(state){
            case State.none:
                label.Text = "....";
                sprite.FrameCoords = new Vector2I(2,index);
                break;
            case State.dormido:
                label.Text = "Durmiendo";
                sprite.FrameCoords = new Vector2I(0,index);
                break;
            case State.intento:
                label.Text = "Esperando";
                sprite.FrameCoords = new Vector2I(1,index);
                break;
            case State.trabajando:
                label.Text = "Trabajando";
                sprite.FrameCoords = new Vector2I(2,index);
                break;
        }
    }

    public override async void _Ready(){
        label = GetNode<Label>("Panel/Label");
        sprite = GetChild<Sprite2D>(0);

        label.Text = "....";
        sprite.FrameCoords = new Vector2I(2,index);
    }
}
