using Godot;
using LibT;
using LibT.Serialization;

namespace MetaMaker
{
	public class Vector2Slot : Container, IField
	{
		[Export] public NodePath _labelPath;
		private Label _label;
		[Export] public NodePath _xPath;
		private SpinBox _x;
		[Export] public NodePath _yPath;
		private SpinBox _y;
		private GenericDataDictionary _model;

		public string Label { get => _label.Text; set => _label.Text = value; }
		public event System.Action OnValueUpdated;

		public override void _Ready()
		{
			_label = this.GetNodeFromPath<Label>( _labelPath );
			
			_x = this.GetNodeFromPath<SpinBox>( _xPath );
			_y = this.GetNodeFromPath<SpinBox>( _yPath );
			
			_x.Connect("value_changed",this,nameof(OnChanged));
			_y.Connect("value_changed",this,nameof(OnChanged));
		}
		
		public void Init(GenericDataDictionary template, GenericDataObject parentModel)
		{
			template.GetValue( "label", out string label );
			_label.Text = label;
			
			template.GetValue( "info", out string info );
			_label.HintTooltip = info;
			_x.HintTooltip = info;
			_y.HintTooltip = info;

			parentModel.TryGetValue(_label.Text, out GenericDataDictionary model);
			if(model != null)
			{
				_model = model;
				_model.GetValue( out Vector2 vector );
				_x.Value = vector.x;
				_y.Value = vector.y;
			}
			else
			{
			
				template.GetValue( "defaultValue", out Vector2 vector );
				_model = parentModel.TryAddValue(_label.Text, vector) as GenericDataDictionary;
				_x.Value = vector.x;
				_y.Value = vector.y;
			}
		}
		
		private void OnChanged(float value)
		{
			LibT.Maths.Vector2 vector = new Vector2((float)_x.Value,(float)_y.Value);
			_model.CopyFrom(GenericDataObject.CreateGdo(vector));
			OnValueUpdated?.Invoke();
		}
	}
}