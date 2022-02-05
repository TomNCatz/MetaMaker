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
			NULL_STRING,
			SKIP
		}

		[Export] public NodePath _titlePath;
		private Label _label;
		[Export] public NodePath _fieldPath;
		private Label _field;
		private EmptyHandling _emptyHandling;
		private GenericDataObject _parentModel;
		
		[Injectable] private App _app;
		[Injectable] private MainView _mainView;

		public string Label { get => _label.Text; set => _label.Text = value; }
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

				_mainView.CenterViewOnKeyedNode(_field.Text);
			}
		}

		public bool AddKey(string link)
		{
			if( !string.IsNullOrEmpty( _field.Text ) ) return false;

			_field.Text = link;
			UpdateField();

			return true;
		}

		public bool RemoveKey(string link)
		{
			if( string.IsNullOrEmpty( _field.Text ) ) return false;

			_field.Text = string.Empty;
			UpdateField();

			return true;
		}

		public void Init(GenericDataDictionary template, GenericDataObject parentModel)
		{
			template.GetValue( "label", out string label );
			_label.Text = label;
			
			if( template.values.ContainsKey( "emptyHandling" ) )
			{
				template.GetValue( "emptyHandling", out _emptyHandling );
			}

			_parentModel = parentModel;
			parentModel.TryGetValue(_label.Text, out GenericDataObject<string> model);
			
			if(!_app.pastingData &&
			   model != null &&
			   !string.IsNullOrEmpty( model.value ))
			{				
				_mainView.loadingLinks.Add( new Tuple<KeyLinkSlot, string>( this, model.value ) );
			}
			else
			{
				UpdateField();
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