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
		private GenericDataObject<string> _model;

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

				_builder.Get.CenterViewOnKeyedNode(_field.Text);
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
				template.GetValue( "emptyHandling", out emptyHandling );
			}

			parentModel.TryGetValue(_label.Text, out GenericDataObject<string> model);
			if(model != null)
			{
				_model = model;

				if( string.IsNullOrEmpty( model.value ) ) return;
				
				_builder.Get.loadingLinks.Add( new Tuple<KeyLinkSlot, string>( this, model.value ) );
			}
			else
			{
				_model = parentModel.TryAddValue(_label.Text, string.Empty) as GenericDataObject<string>;
			}
		}

		private void UpdateField()
		{
			// TODO : We lost the option to skip empties, I would like something similar back somehow
			_model.value = _field.Text;
			OnValueUpdated?.Invoke();
		}
	}
}