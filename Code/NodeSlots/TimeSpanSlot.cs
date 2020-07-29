using System;
using Godot;
using LibT;
using LibT.Serialization;

public class TimeSpanSlot : Container, IGdaLoadable, IGdoConvertible
{
	[Export] private NodePath _labelPath;
	private Label _label;
	[Export] private NodePath _fieldPath;
	private Label _field;
	[Export] private NodePath _popupPath;
	private Popup _popup;
	[Export] private NodePath _dayPath;
	private SpinBox _day;
	[Export] private NodePath _hourPath;
	private SpinBox _hour;
	[Export] private NodePath _minutePath;
	private SpinBox _minute;
	[Export] private NodePath _secondPath;
	private SpinBox _second;
	[Export] private NodePath _milisecondPath;
	private SpinBox _milisecond;
	[Export] private NodePath _cancelPath;
	private Button _cancel;

	private TimeSpan span;

	public override void _Ready()
    {
	    _label = this.GetNodeFromPath<Label>( _labelPath );
	    _field = this.GetNodeFromPath<Label>( _fieldPath );
	    _field.Connect( "gui_input", this, nameof(_GuiInput) );

	    _popup = this.GetNodeFromPath<Popup>( _popupPath );
	    _popup.Connect( "popup_hide", this, nameof(UpdateDate) );
	    
	    _day = this.GetNodeFromPath<SpinBox>( _dayPath );
	    _hour = this.GetNodeFromPath<SpinBox>( _hourPath );
	    _minute = this.GetNodeFromPath<SpinBox>( _minutePath );
	    _second = this.GetNodeFromPath<SpinBox>( _secondPath );
	    _milisecond = this.GetNodeFromPath<SpinBox>( _milisecondPath );
	    
	    _cancel = this.GetNodeFromPath<Button>( _cancelPath );
	    _cancel.Connect( "pressed", this, nameof(OnCancelButton) );
    }

	public override void _GuiInput( InputEvent @event )
    {
	    InputEventMouseButton mouseButton = @event as InputEventMouseButton;
	    
	    if( mouseButton != null && !mouseButton.Pressed )
	    {
		    SetDateInPopup(span);
		    _popup.Popup_();
		    _popup.RectPosition = GetGlobalMousePosition();
	    }
    }

    private void OnCancelButton()
    {
	    SetDateInPopup(span);
	    _popup.Hide();
    }

    private void SetDateInPopup(TimeSpan time)
    {
	    _day.Value = time.Days;
	    _hour.Value = time.Hours;
	    _minute.Value = time.Minutes;
	    _second.Value = time.Seconds;
	    _milisecond.Value = time.Milliseconds;
    }

    private void GetDateInPopup()
    {
	    span = new TimeSpan(
		    (int)_day.Value,
		    (int)_hour.Value,
		    (int)_minute.Value,
		    (int)_second.Value,
		    (int)_milisecond.Value
		    );
    }

    private void UpdateDate()
    {
	    GetDateInPopup();
	    UpdateDisplay();
    }

    private void UpdateDisplay()
    {
	    _field.Text = span.ToString("c");
    }

    public void LoadFromGda( GenericDataArray data )
    {
	    data.GetValue( "label", out string label );
	    _label.Text = label;

	    data.GetValue( "defaultValue", out span );

	    UpdateDisplay();
    }

    public void GetObjectData( GenericDataArray objData )
    {
	    objData.AddValue( _label.Text, span );
    }

    public void SetObjectData( GenericDataArray objData )
    {
	    objData.GetValue( _label.Text, out span );
	    UpdateDisplay();
    }
}
