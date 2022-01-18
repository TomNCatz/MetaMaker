using System;
using Godot;
using LibT;
using LibT.Serialization;
using LibT.Services;

namespace MetaMaker
{
	public class KeySelectSlot : Container, IField
	{
		private enum EmptyHandling
		{
			EMPTY_STRING,
			NULL_STRING,
			SKIP
		}

		[Export] public NodePath _titlePath;
		private Label _label;
		[Export] public NodePath _fieldPath;
		private Label _field;
		[Export] public NodePath _buttonPath;
		private Button _button;
		[Export] public NodePath _popupPath;
		private ConfirmationDialog _popup;
		[Export] public NodePath _dropDownPath;
		private OptionButton _dropDown;
		[Export] public NodePath _clearButtonPath;
		private Button _clearButton;
		private EmptyHandling _emptyHandling;
		private GenericDataObject _parentModel;
		
		public int LinkType {get; private set;}

		public string Label { get => _label.Text; set => _label.Text = value; }
		public event System.Action OnValueUpdated;
		
		public override void _Ready()
		{
			_label = this.GetNodeFromPath<Label>( _titlePath );
			_field = this.GetNodeFromPath<Label>( _fieldPath );
			_button = this.GetNodeFromPath<Button>( _buttonPath );
			_button.Connect( "pressed", this, nameof(OpenPopup) );
			_popup = this.GetNodeFromPath<ConfirmationDialog>( _popupPath );
			_popup.Connect( "confirmed", this, nameof(PullKey) );
			_dropDown = this.GetNodeFromPath<OptionButton>( _dropDownPath );
			_clearButton = this.GetNodeFromPath<Button>( _clearButtonPath );
			_clearButton.Connect( "pressed", this, nameof(ClearSelection) );
			_field.Connect( "gui_input", this, nameof(_GuiInput) );
		}

		public override void _GuiInput( InputEvent @event )
		{
			if (@event is InputEventMouseButton mouseButton && mouseButton.Doubleclick)
			{
				if (string.IsNullOrEmpty(_field.Text)) return;

				ServiceInjection<MainView>.Service.CenterViewOnKeyedNode(_field.Text);
			}
		}
		
		private void OpenPopup()
		{
			FillOptions();
			_popup.PopupCentered();
		}

		private void FillOptions()
		{
			_dropDown.Clear();
			_dropDown.AddItem( "NONE", 0 );

			int select = 0;

			var keys = ServiceInjection<App>.Service.AvailableKeys(LinkType);

			for(int i = 0; i < keys.Count; i++)
			{
				if(keys[i] == _field.Text)
				{
					select = i+1;
				}
				_dropDown.AddItem( keys[i], i+1 );
			}
			
			_dropDown.Selected = select;
		}

		private void ClearSelection()
		{
			_dropDown.Selected = 0;
		}

		private void PullKey()
		{
			if(_dropDown.Selected == 0)
			{
				_field.Text = string.Empty;
			}
			else
			{
				_field.Text = _dropDown.GetItemText(_dropDown.Selected);
			}
			UpdateField();
		}

		public void Init(GenericDataDictionary template, GenericDataObject parentModel)
		{
			template.GetValue( "label", out string label );
			_label.Text = label;

			template.GetValue( "slotType", out int slotType );
			LinkType = slotType;
			
			if( template.values.ContainsKey( "emptyHandling" ) )
			{
				template.GetValue( "emptyHandling", out _emptyHandling );
			}

			_parentModel = parentModel;
			parentModel.TryGetValue(_label.Text, out GenericDataObject<string> model);
			if(model == null)
			{
				_field.Text = string.Empty;
			}
			else
			{
				_field.Text = model.value;
			}
		}

		private void UpdateField()
		{
			if( string.IsNullOrEmpty( _label.Text ) )
			{
				_parentModel.TryRemoveValue( _label.Text );
				switch (_emptyHandling)
				{
					case EmptyHandling.EMPTY_STRING:
						_parentModel.TryAddValue( _label.Text, string.Empty );
						break;
					case EmptyHandling.NULL_STRING:
						_parentModel.TryAddValue( _label.Text, new GenericDataObject<string>() );
						break;
					case EmptyHandling.SKIP:
						_parentModel.TryAddValue( _label.Text, new GenericDataSkip() );
						break;
					default:
						break;
				}
			}
			else
			{
				_parentModel.TryRemoveValue( _label.Text );
				_parentModel.TryAddValue( _label.Text, _field.Text );
			}
			OnValueUpdated?.Invoke();
		}
	}
}