using Godot;
using LibT;
using LibT.Serialization;

public class ColorSlot : Container, IGdaLoadable, IGdoConvertible
{
	[Export] private NodePath _labelPath;
	private Label _label;
	[Export] private NodePath _colorRectPath;
	private ColorRect _colorRect;
	[Export] private NodePath _popupPath;
	private Popup _popup;
	[Export] private NodePath _pickerPath;
	private ColorPicker _picker;

	private bool asHtml;

	public override void _Ready()
    {
	    _label = this.GetNodeFromPath<Label>( _labelPath );
	    
	    _colorRect = this.GetNodeFromPath<ColorRect>( _colorRectPath );
	    _colorRect.Connect( "gui_input", this, nameof(_GuiInput) );
	    
	    _popup = this.GetNodeFromPath<Popup>( _popupPath );
	    _popup.Connect( "popup_hide", this, nameof(SelectColor) );
	    
	    _picker = this.GetNodeFromPath<ColorPicker>( _pickerPath );
    }

    public override void _GuiInput( InputEvent @event )
    {
	    InputEventMouseButton mouseButton = @event as InputEventMouseButton;
	    
	    if( mouseButton != null && !mouseButton.Pressed )
	    {
		    _picker.Color = _colorRect.Color;
		    _popup.Popup_();
		    _popup.RectPosition = GetGlobalMousePosition();
	    }
    }

    private void SelectColor()
    {
	    _colorRect.Color = _picker.Color;
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
