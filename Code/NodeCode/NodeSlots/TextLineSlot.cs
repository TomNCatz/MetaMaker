using Godot;
using LibT;
using LibT.Serialization;

namespace MetaMaker
{
	public class TextLineSlot : Container, IField, IGdoConvertible, IStringRetriever
	{
		[Export] private readonly NodePath _labelPath;
		private Label _label;
		[Export] private readonly NodePath _fieldPath;
		private LineEdit _field;
		private GenericDataArray _parentModel;
		
		
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
			
			template.GetValue( "defaultValue", out string text );
			_field.Text = text;
			_parentModel.AddValue(_label.Text, _field.Text);
		}

		public void GetObjectData( GenericDataArray objData )
		{
			objData.AddValue( _label.Text, _field.Text );
		}

		public void SetObjectData( GenericDataArray objData )
		{
			objData.GetValue( _label.Text, out string text );
			_field.Text = text;
			OnChanged( text );
		}

		private void OnChanged( string text )
		{
			_parentModel.AddValue(_label.Text, text);
		}

		public string GetString()
		{
			return _field.Text;
		}
	}
}