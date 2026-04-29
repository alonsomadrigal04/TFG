using Godot;
using System;
[GlobalClass]
public partial class ItemSlotBehaviour : Control
{
    [Export] TextureRect iconItem;
    [Export] Label nameItem;
    public ObjectData ObjectData;

    public void SetItem(ObjectData data)
    {
        iconItem.Texture = data.Icon;
        nameItem.Text = data.Name;
        ObjectData = data;
    }

}
