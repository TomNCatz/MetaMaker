using System;
using Godot;
using LibT;
using LibT.Serialization;
using LibT.Services;

namespace MetaMaker
{
	public class KeyLinkSlot : Container, IField
	{
		private enum EmptyHandling
		{
			EMPTY_STRING,
			SKIP
		}

		[Export] public NodePath _titlePath;
		private Label _label;
		[Export] public NodePath _fieldPath;
		private Label _field;
		private EmptyHandling emptyHandling;
		private readonly ServiceInjection<MainView> _builder = new ServiceInjection<MainView>();
		private GenericDataArray _parentModel;
		public event System.Action OnValueUpdated;
		
		public override void _Ready()
		{
			_label = this.GetNodeFromPath<Label>( _titlePath );
			_field = this.GetNodeFromPath<Label>( _fieldPath );
			_field.Connect( "gui_input", this, nameof(_GuiInput) );
		}

		public override void _GuiInput( InputEvent @event )
		{
			if (@event is InputEventMouseButton mouseButton && mouseButton.Doubleclick)
			{
				if (string.IsNullOrEmpty(_field.Text)) return;

				_builder.Get.CenterViewOnKeyedNode(_field.Text);
			}
		}

		public bool AddKey(string link)
		{
			if( !string.IsNullOrEmpty( _field.Text ) ) return false;

			_field.Text = link;
			UpdateField(_parentModel);

			return true;
		}

		public bool RemoveKey(string link)
		{
			if( string.IsNullOrEmpty( _field.Text ) ) return false;

			_field.Text = string.Empty;
			UpdateField(_parentModel);

			return true;
		}

		public void Init(GenericDataArray template, GenericDataArray parentModel)
		{
			template.GetValue( "label", out string label );
			_label.Text = label;
			_parentModel = parentModel;
			
			if( template.values.ContainsKey( "emptyHandling" ) )
			{
				template.GetValue( "emptyHandling", out emptyHandling );
			}

			if(parentModel.values.ContainsKey(_label.Text))
			{
				parentModel.GetValue( _label.Text, out string key );

				if( string.IsNullOrEmpty( key ) ) return;
				
				_builder.Get.loadingLinks.Add( new Tuple<KeyLinkSlot, string>( this, key ) );
			}
		}

		private void UpdateField( GenericDataArray objData )
		{
			if( string.IsNullOrEmpty( _field.Text ) )
			{
				switch(emptyHandling)
				{
					case EmptyHandling.EMPTY_STRING :
						objData.AddValue( _label.Text, _field.Text );
						break;
					case EmptyHandling.SKIP : 
						objData.RemoveValue(_label.Text);
						break;
					default : throw new ArgumentOutOfRangeException();
				}
			}
			else
			{
				objData.AddValue( _label.Text, _field.Text );
			}
			OnValueUpdated?.Invoke();
		}
	}
}