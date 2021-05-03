using System;
using Godot;
using LibT;
using LibT.Serialization;

namespace MetaMaker
{
	public class TimeSpanSlot : Container, IField
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
		private GenericDataObject _model;
		private bool asSeconds = false;


		public string Label { get => _label.Text; set => _label.Text = value; }
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
			_field.Text = span.ToString("c");
			UpdateData();
		}

		private void UpdateData()
		{
			if( asSeconds )
			{
				_model.CopyFrom(GenericDataObject.CreateGdo((int)Math.Round(span.TotalSeconds)));
			}
			else
			{
				_model.CopyFrom(GenericDataObject.CreateGdo(span));
			}
			OnValueUpdated?.Invoke();
		}

		public void Init(GenericDataDictionary template, GenericDataObject parentModel)
		{
			template.GetValue( "label", out string label );
			_label.Text = label;

			template.GetValue( "asSeconds", out asSeconds );

			GenericDataObject gdo = parentModel.TryGetRelativeGdo(_label.Text);
			if(gdo != null)
			{
				if(gdo.GetValue(out int seconds))
				{
					span = new TimeSpan(0,0,seconds);
					if(asSeconds)
					{
						_model = gdo;
					}
					else
					{
						_model = new GenericDataDictionary();
					}
				}
				else if(gdo.GetValue(out span))
				{
					if(asSeconds)
					{
						_model = new GenericDataObject<int>();
					}
					else
					{
						_model = gdo;
					}
				}
			}
			else
			{
				template.GetValue( "defaultValue", out span );
				if( asSeconds )
				{
					_model = parentModel.TryAddValue( _label.Text, (int)Math.Round(span.TotalSeconds) );
				}
				else
				{
					_model = parentModel.TryAddValue( _label.Text, span );
				}
			}

			_field.Text = span.ToString("c");
		}
	}
}