using Godot;
using LibT;
using LibT.Serialization;

namespace MetaMaker
{
	public class TextAreaRichSlot : Container, IField, IGdoConvertible, IStringRetriever
	{
		[Export] private readonly NodePath _labelPath;
		private Label _label;
		[Export] private readonly NodePath _fieldPath;
		private TextEdit _field;
		[Export] private readonly NodePath _displayPath;
		private RichTextLabel _display;
		private GenericDataArray _parentModel;
		
		
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

			_parentModel = parentModel;
			
			template.GetValue( "minHeight", out float height );
			_field.RectMinSize = new Vector2(0,height);
			_display.RectMinSize = new Vector2(0,height);
			
			template.GetValue( "defaultValue", out string text );
			_field.Text = text;
			OnTextChanged();
		}

		public void GetObjectData( GenericDataArray objData )
		{
			objData.AddValue( _label.Text, _field.Text );
		}

		public void SetObjectData( GenericDataArray objData )
		{
			objData.GetValue( _label.Text, out string text );
			_field.Text = text;
		}

		private void OnTextChanged()
		{
			_display.BbcodeText = _field.Text;
			_parentModel.AddValue(_label.Text, _field.Text);
		}

		public string GetString()
		{
			return _field.Text;
		}
	}
}