using Godot;
using LibT;
using LibT.Serialization;
using LibT.Services;

public class ColorSlot : Container, IGdaLoadable, IGdoConvertible
{
	[Export] private NodePath _labelPath;
	private Label _label;
	[Export] private NodePath _colorRectPath;
	private ColorRect _colorRect;
	
	private ServiceInjection<JsonBuilder> _builder = new ServiceInjection<JsonBuilder>();

	private bool asHtml;

	public override void _Ready()
    {
	    _label = this.GetNodeFromPath<Label>( _labelPath );
	    
	    _colorRect = this.GetNodeFromPath<ColorRect>( _colorRectPath );
	    _colorRect.Connect( "gui_input", this, nameof(_GuiInput) );
    }

    public override void _GuiInput( InputEvent @event )
    {
	    InputEventMouseButton mouseButton = @event as InputEventMouseButton;
	    
	    if( mouseButton != null && !mouseButton.Pressed )
	    {
		    _builder.Get.GetColorFromUser( _colorRect.Color )
			    .Then( color => { _colorRect.Color = color; } );
	    }
    }

    public void LoadFromGda( GenericDataArray data )
    {
	    data.GetValue( "label", out string label );
	    _label.Text = label;

	    data.GetValue( "defaultValue", out Color color );
	    _colorRect.Color = color;

	    data.GetValue( "asHtml", out asHtml );
    }

    public void GetObjectData( GenericDataArray objData )
    {
	    if( asHtml )
	    {
		    objData.AddValue( _label.Text, $"#{_colorRect.Color.ToHtml()}" );
	    }
	    else
	    {
		    objData.AddValue( _label.Text, _colorRect.Color );
	    }
    }

    public void SetObjectData( GenericDataArray objData )
    {
	    Color color = new Color();
	    if( asHtml )
	    {
		    objData.GetValue( _label.Text, out string colorData );
		    color = new Color(colorData);
	    }
	    else
	    {
		    objData.GetValue( _label.Text, out color );
	    }

	    _colorRect.Color = color;
    }
}
