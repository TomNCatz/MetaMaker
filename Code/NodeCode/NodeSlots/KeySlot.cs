using System;
using Godot;
using LibT;
using LibT.Serialization;
using LibT.Services;

namespace MetaMaker
{
	public class KeySlot : KeyAbstraction
	{
		[Export] public NodePath _fieldPath;
		private Label _field;

		[Injectable] private App _app;
		public override string GetKey => _field.Text;
		
		public override int LinkType => _linkType;
		private int _linkType;

		private string _keyPrefix = string.Empty;
		private int _keySize = 1;
		private GenericDataObject<string> _model;
		
		public override void _Ready()
		{
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

		public override void Init(GenericDataDictionary template, GenericDataObject parentModel)
		{
			template.GetValue( "label", out string label );
			_label.Text = label;
			
			template.GetValue( "info", out string info );
			_field.HintTooltip = info;
			
			template.GetValue( "keyPrefix", out _keyPrefix );
			template.GetValue( "keySize", out _keySize );
			template.GetValue( "slotType", out _linkType );

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
			ValueUpdate();
		}
	}
}