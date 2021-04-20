using Godot;
using LibT;
using LibT.Serialization;

namespace MetaMaker
{
	public class TextAreaRichSlot : Container, IField
	{
		[Export] public NodePath _labelPath;
		private Label _label;
		[Export] public NodePath _fieldPath;
		private TextEdit _field;
		[Export] public NodePath _displayPath;
		private RichTextLabel _display;
		private GenericDataArray _parentModel;
		public event System.Action OnValueUpdated;
		
		public override void _Ready()
		{
			_label = this.GetNodeFromPath<Label>( _labelPath );
			_field = this.GetNodeFromPath<TextEdit>( _fieldPath );
			_display = this.GetNodeFromPath<RichTextLabel>( _displayPath );

			_field.Connect( "text_changed", this, nameof(OnTextChanged) );
		}

		public void Init(GenericDataArray template, GenericDataArray parentModel)
		{
			template.GetValue( "label", out string label );
			_label.Text = label;
			
			template.GetValue( "minHeight", out float height );
			_field.RectMinSize = new Vector2(0,height);
			_display.RectMinSize = new Vector2(0,height);

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
			_display.BbcodeText = _field.Text;
		}

		private void OnTextChanged()
		{
			_display.BbcodeText = _field.Text;
			_parentModel.AddValue(_label.Text, _field.Text);
			OnValueUpdated?.Invoke();
		}
	}
}