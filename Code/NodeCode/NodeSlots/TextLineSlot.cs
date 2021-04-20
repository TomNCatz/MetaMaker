using Godot;
using LibT;
using LibT.Serialization;

namespace MetaMaker
{
	public class TextLineSlot : Container, IField
	{
		[Export] public NodePath _labelPath;
		private Label _label;
		[Export] public NodePath _fieldPath;
		private LineEdit _field;
		private GenericDataArray _parentModel;
		public event System.Action OnValueUpdated;
		
		public override void _Ready()
		{
			_label = this.GetNodeFromPath<Label>( _labelPath );
			_field = this.GetNodeFromPath<LineEdit>( _fieldPath );
			_field.Connect("text_changed",this,nameof(OnChanged));
		}

		public void Init(GenericDataArray template, GenericDataArray parentModel)
		{
			template.GetValue( "label", out string label );
			_label.Text = label;

			_parentModel = parentModel;
			if(parentModel.values.ContainsKey(_label.Text))
			{
				parentModel.GetValue( _label.Text, out string value );
				_field.Text = value;
			}
			else
			{
				template.GetValue( "defaultValue", out string value );
				_field.Text = value;
				_parentModel.AddValue(_label.Text, value);
			}
		}

		private void OnChanged( string text )
		{
			_parentModel.AddValue(_label.Text, text);
			OnValueUpdated?.Invoke();
		}
	}
}