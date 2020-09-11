using System;
using System.Security.AccessControl;
using Godot;
using LibT;
using LibT.Serialization;

public class DateTimeOffsetSlot : Container, IGdaLoadable, IGdoConvertible, StringRetriever
{
	[Export] private NodePath _labelPath;
	private Label _label;
	[Export] private NodePath _fieldPath;
	private Label _field;
	[Export] private NodePath _popupPath;
	private Popup _popup;
	[Export] private NodePath _yearPath;
	private SpinBox _year;
	[Export] private NodePath _monthPath;
	private OptionButton _month;
	[Export] private NodePath _dayPath;
	private SpinBox _day;
	[Export] private NodePath _hourPath;
	private OptionButton _hour;
	[Export] private NodePath _minutePath;
	private SpinBox _minute;
	[Export] private NodePath _secondPath;
	private SpinBox _second;
	[Export] private NodePath _timezonePath;
	private OptionButton _timezone;
	[Export] private NodePath _nowPath;
	private Button _now;
	[Export] private NodePath _cancelPath;
	private Button _cancel;

	private bool isOffset;
	private DateTimeOffset dtOffset;

	public override void _Ready()
    {
	    _label = this.GetNodeFromPath<Label>( _labelPath );
	    _field = this.GetNodeFromPath<Label>( _fieldPath );
	    _field.Connect( "gui_input", this, nameof(_GuiInput) );

	    _popup = this.GetNodeFromPath<Popup>( _popupPath );
	    _popup.Connect( "popup_hide", this, nameof(UpdateDate) );
	    
	    _year = this.GetNodeFromPath<SpinBox>( _yearPath );
	    _month = this.GetNodeFromPath<OptionButton>( _monthPath );
	    _day = this.GetNodeFromPath<SpinBox>( _dayPath );
	    _hour = this.GetNodeFromPath<OptionButton>( _hourPath );
	    _minute = this.GetNodeFromPath<SpinBox>( _minutePath );
	    _second = this.GetNodeFromPath<SpinBox>( _secondPath );
	    
	    _now = this.GetNodeFromPath<Button>( _nowPath );
	    _now.Connect( "pressed", this, nameof(OnNowButton) );
	    
	    _cancel = this.GetNodeFromPath<Button>( _cancelPath );
	    _cancel.Connect( "pressed", this, nameof(OnCancelButton) );

	    isOffset = !_timezonePath.IsEmpty();
	    if( isOffset )
	    {
		    _timezone = this.GetNodeFromPath<OptionButton>( _timezonePath );
	    }

	    FillOptions();
    }

	private void FillOptions()
	{
		_month.Clear();
		_month.AddItem( "01-January", 1 );
		_month.AddItem( "02-February", 2 );
		_month.AddItem( "03-March", 3 );
		_month.AddItem( "04-April", 4 );
		_month.AddItem( "05-May", 5 );
		_month.AddItem( "06-June", 6 );
		_month.AddItem( "07-July", 7 );
		_month.AddItem( "08-August", 8 );
		_month.AddItem( "09-September", 9 );
		_month.AddItem( "10-October", 10 );
		_month.AddItem( "11-November", 11 );
		_month.AddItem( "12-December", 12 );
		
		
		_hour.Clear();
		_hour.AddItem( "00-12AM",0 );
		_hour.AddItem( "01-1AM", 1 );
		_hour.AddItem( "02-2AM", 2 );
		_hour.AddItem( "03-3AM", 3 );
		_hour.AddItem( "04-4AM", 4 );
		_hour.AddItem( "05-5AM", 5 );
		_hour.AddItem( "06-6AM", 6 );
		_hour.AddItem( "07-7AM", 7 );
		_hour.AddItem( "08-8AM", 8 );
		_hour.AddItem( "09-9AM", 9 );
		_hour.AddItem( "10-10AM", 10 );
		_hour.AddItem( "11-11AM", 11 );
		_hour.AddItem( "12-12PM", 12 );
		_hour.AddItem( "13-1PM", 13 );
		_hour.AddItem( "14-2PM", 14 );
		_hour.AddItem( "15-3PM", 15 );
		_hour.AddItem( "16-4PM", 16 );
		_hour.AddItem( "17-5PM", 17 );
		_hour.AddItem( "18-6PM", 18 );
		_hour.AddItem( "19-7PM", 19 );
		_hour.AddItem( "20-8PM", 20 );
		_hour.AddItem( "21-9PM", 21 );
		_hour.AddItem( "22-10PM",22 );
		_hour.AddItem( "23-11PM",23 );

		if( !isOffset ) return;

		_timezone.Clear();
		_timezone.AddItem( "UTC -12" );
		_timezone.AddItem( "UTC -11" );
		_timezone.AddItem( "UTC -10" );
		_timezone.AddItem( "UTC -9" );
		_timezone.AddItem( "UTC -8" );
		_timezone.AddItem( "UTC -7" );
		_timezone.AddItem( "UTC -6" );
		_timezone.AddItem( "UTC -5" );
		_timezone.AddItem( "UTC -4" );
		_timezone.AddItem( "UTC -3" );
		_timezone.AddItem( "UTC -2" );
		_timezone.AddItem( "UTC -1" );
		_timezone.AddItem( "UTC" );
		_timezone.AddItem( "UTC +1" );
		_timezone.AddItem( "UTC +2" );
		_timezone.AddItem( "UTC +3" );
		_timezone.AddItem( "UTC +4" );
		_timezone.AddItem( "UTC +5" );
		_timezone.AddItem( "UTC +6" );
		_timezone.AddItem( "UTC +7" );
		_timezone.AddItem( "UTC +8" );
		_timezone.AddItem( "UTC +9" );
		_timezone.AddItem( "UTC +10" );
		_timezone.AddItem( "UTC +11" );
	}

    public override void _GuiInput( InputEvent @event )
    {
	    InputEventMouseButton mouseButton = @event as InputEventMouseButton;
	    
	    if( mouseButton != null && !mouseButton.Pressed )
	    {
		    SetDateInPopup(dtOffset);
		    _popup.PopupCentered();
	    }
    }

    private void OnNowButton()
    {
	    SetDateInPopup(DateTimeOffset.Now);
    }

    private void OnCancelButton()
    {
	    SetDateInPopup(dtOffset);
	    _popup.Hide();
    }

    private void SetDateInPopup(DateTimeOffset time)
    {
	    _year.Value = time.Year;
	    _month.Selected = time.Month-1;
	    _day.Value = time.Day;
	    _hour.Selected = time.Hour;
	    _minute.Value = time.Minute;
	    _second.Value = time.Second;

	    if( isOffset )
	    {
		    _timezone.Selected = time.Offset.Hours+12;
	    }
    }

    private void GetDateInPopup()
    {
	    int offset = 0;
	    if( isOffset )
	    {
		    offset = _timezone.Selected-12;
	    }
	    dtOffset = new DateTimeOffset( (int)_year.Value,
		    _month.Selected+1,
		    (int)_day.Value,
		    _hour.Selected,
		    (int)_minute.Value,
		    (int)_second.Value,
		    new TimeSpan(0,offset, 0, 0) );
    }

    private void UpdateDate()
    {
	    GetDateInPopup();
	    UpdateDisplay();
    }

    private void UpdateDisplay()
    {
	    if( isOffset )
	    {
		    _field.Text = dtOffset.ToString("yyyy-MM-dd HH:mm:ss zz");
	    }
	    else
	    {
		    _field.Text = dtOffset.ToString("yyyy-MM-dd HH:mm:ss");
	    }
    }

    public void LoadFromGda( GenericDataArray data )
    {
	    data.GetValue( "label", out string label );
	    _label.Text = label;

	    if( isOffset )
	    {
		    data.GetValue( "defaultValue", out dtOffset );
	    }
	    else
	    {
		    data.GetValue( "defaultValue", out DateTime dateTime );
		    dtOffset = new DateTimeOffset(dateTime);
	    }

	    UpdateDisplay();
    }

    public void GetObjectData( GenericDataArray objData )
    {
	    if( isOffset )
	    {
		    objData.AddValue( _label.Text, dtOffset );
	    }
	    else
	    {
		    objData.AddValue( _label.Text, dtOffset.DateTime );
	    }
    }

    public void SetObjectData( GenericDataArray objData )
    {
	    if( isOffset )
	    {
		    objData.GetValue( _label.Text, out dtOffset );
	    }
	    else
	    {
		    objData.GetValue( _label.Text, out DateTime dateTime );
		    dtOffset = new DateTimeOffset(dateTime);
	    }

	    UpdateDisplay();
    }

    public string GetString()
    {
	    if( isOffset )
	    {
		    return dtOffset.ToString("yyyy-MM-dd HH:mm:ss zz");
	    }

	    return dtOffset.ToString("yyyy-MM-dd HH:mm:ss");
    }
}
