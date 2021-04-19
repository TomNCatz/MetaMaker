using System;
using Godot;
using LibT;
using LibT.Serialization;

namespace MetaMaker
{
	public class TimeSpanSlot : Container, IField, IGdoConvertible
	{
		[Export] public NodePath _labelPath;
		private Label _label;
		[Export] public NodePath _fieldPath;
		private Label _field;
		[Export] public NodePath _popupPath;
		private ConfirmationDialog _popup;
		[Export] public NodePath _dayPath;
		private SpinBox _day;
		[Export] public NodePath _hourPath;
		private SpinBox _hour;
		[Export] public NodePath _minutePath;
		private SpinBox _minute;
		[Export] public NodePath _secondPath;
		private SpinBox _second;
		[Export] public NodePath _milisecondPath;
		private SpinBox _milisecond;

		private TimeSpan span;
		private GenericDataArray _parentModel;
		private bool asSeconds = false;

		public event System.Action OnValueUpdated;

		public override void _Ready()
		{
			_label = this.GetNodeFromPath<Label>( _labelPath );
			_field = this.GetNodeFromPath<Label>( _fieldPath );
			_field.Connect( "gui_input", this, nameof(_GuiInput) );

			_popup = this.GetNodeFromPath<ConfirmationDialog>( _popupPath );
			_popup.Connect( "confirmed", this, nameof(UpdateDate) );
			
			_day = this.GetNodeFromPath<SpinBox>( _dayPath );
			_hour = this.GetNodeFromPath<SpinBox>( _hourPath );
			_minute = this.GetNodeFromPath<SpinBox>( _minutePath );
			_second = this.GetNodeFromPath<SpinBox>( _secondPath );
			_milisecond = this.GetNodeFromPath<SpinBox>( _milisecondPath );
		}

		public override void _GuiInput( InputEvent @event )
		{
			if (@event is InputEventMouseButton mouseButton && !mouseButton.Pressed)
			{
				SetDateInPopup(span);
				_popup.Popup_();
				_popup.RectPosition = GetGlobalMousePosition();
			}
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
			if(asSeconds)
			{
				_parentModel.AddValue( _label.Text, span.TotalSeconds );
			}
			else
			{
				_parentModel.AddValue( _label.Text, span );
			}
			OnValueUpdated?.Invoke();
		}

		public void Init(GenericDataArray template, GenericDataArray parentModel)
		{
			template.GetValue( "label", out string label );
			_label.Text = label;

			_parentModel = parentModel;

			template.GetValue( "defaultValue", out span );

			template.GetValue( "asSeconds", out asSeconds );

			UpdateDisplay();
		}

		public void GetObjectData( GenericDataArray objData )
		{
		}
		
		public void SetObjectData( GenericDataArray objData )
		{
			objData.GetValue( _label.Text, out span );
			UpdateDisplay();
		}
	}
}