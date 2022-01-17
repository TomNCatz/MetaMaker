using System;
using Godot;
using LibT;
using LibT.Serialization;
using LibT.Services;

namespace MetaMaker
{
	public class KeySlot : Container, IField
	{
		[Export] public NodePath _labelPath;
		private Label _label;
		[Export] public NodePath _fieldPath;
		private Label _field;

		public string GetKey => _field.Text;
		
		public int LinkType {get; private set;}
		
		private string _keyPrefix = String.Empty;
		private int _keySize = 1;
		private App _app;
		private GenericDataObject<string> _model;

		public string Label { get => _label.Text; set => _label.Text = value; }
		public event System.Action OnValueUpdated;
		
		public override void _Ready()
		{
			_app = ServiceInjection<App>.Service;
			_label = this.GetNodeFromPath<Label>( _labelPath );
			_field = this.GetNodeFromPath<Label>( _fieldPath );
		}

		public override void _GuiInput( InputEvent @event )
		{
			if (@event is InputEventMouseButton mouseButton && mouseButton.Doubleclick)
			{
				OS.Clipboard = GetKey;
			}
		}

		public void Init(GenericDataDictionary template, GenericDataObject parentModel)
		{
			template.GetValue( "label", out string label );
			_label.Text = label;
			
			template.GetValue( "keyPrefix", out _keyPrefix );
			template.GetValue( "keySize", out _keySize );
			template.GetValue( "slotType", out int slotType );
			LinkType = slotType;

			parentModel.TryGetValue(_label.Text, out GenericDataObject<string> model);
			if(model != null)
			{
				_model = model;
				SetKey( _model.value );
				_model.value = GetKey;
			}
			else
			{
				SetKey();
				_model = parentModel.TryAddValue(_label.Text, GetKey) as GenericDataObject<string>;
			}
		}

		public override void _ExitTree()
		{
			_app.RemoveKey(this);
			base._ExitTree();
		}

		private void SetKey( string force = null )
		{
			if( _app.pastingData )
			{
				force = null;
			}
			
			if(string.IsNullOrEmpty( force ))
			{
				force = _app.GenerateKey(_keySize,_keyPrefix);
			}
			
			_field.Text = force;
			_app.AddKey(this);
		}
	}
}