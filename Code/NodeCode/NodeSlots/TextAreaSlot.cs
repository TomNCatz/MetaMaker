using Godot;
using LibT;
using LibT.Serialization;

namespace MetaMaker
{
	public class TextAreaSlot : Container, IField, IGdoConvertible
	{
		[Export] public NodePath _labelPath;
		private Label _label;
		[Export] public NodePath _fieldPath;
		private TextEdit _field;
		private GenericDataArray _parentModel;
		public event System.Action OnValueUpdated;
		
		public override void _Ready()
		{
			_label = this.GetNodeFromPath<Label>( _labelPath );
			_field = this.GetNodeFromPath<TextEdit>( _fieldPath );
			_field.Connect("text_changed",this,nameof(OnChanged));
		}

		public void Init(GenericDataArray template, GenericDataArray parentModel)
		{
			template.GetValue( "label", out string label );
			_label.Text = label;

			_parentModel = parentModel;
			
			template.GetValue( "defaultValue", out string text );
			_field.Text = text;
			_parentModel.AddValue(_label.Text, _field.Text);
			
			template.GetValue( "minHeight", out float height );
			_field.RectMinSize = new Vector2(0,height);
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

		private void OnChanged( string text )
		{
			_parentModel.AddValue(_label.Text, text);
			OnValueUpdated?.Invoke();
		}
	}
}