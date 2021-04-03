using Godot;
using LibT;
using LibT.Serialization;

namespace MetaMaker
{
	public class AutoSlot : Container, IField, IGdoConvertible, IStringRetriever
	{
		[Export] private readonly NodePath _labelPath;
		private Label _label;
		[Export] private readonly NodePath _fieldPath;
		private Label _field;

		public override void _Ready()
		{
			_label = this.GetNodeFromPath<Label>( _labelPath );
			_field = this.GetNodeFromPath<Label>( _fieldPath );
		}
		
		public void Init(GenericDataArray template, GenericDataArray parentModel)
		{
			template.GetValue( "label", out string label );
			_label.Text = label;
			
			template.GetValue( "defaultValue", out string text );
			_field.Text = text;
			
			parentModel.AddValue(_label.Text, text);
		}

		public void GetObjectData( GenericDataArray objData )
		{
			objData.AddValue( _label.Text, _field.Text );
		}

		public void SetObjectData( GenericDataArray objData )
		{
		}

		public string GetString()
		{
			return _field.Text;
		}
	}
}