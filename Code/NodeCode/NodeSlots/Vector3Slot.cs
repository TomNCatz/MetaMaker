using Godot;
using LibT;
using LibT.Serialization;

namespace MetaMaker
{
	public partial class Vector3Slot : Container, IField
	{
		[Export] public NodePath _labelPath;
		private Label _label;
		[Export] public NodePath _xPath;
		private SpinBox _x;
		[Export] public NodePath _yPath;
		private SpinBox _y;
		[Export] public NodePath _zPath;
		private SpinBox _z;
		private GenericDataDictionary _model;

		public string Label { get => _label.Text; set => _label.Text = value; }
		public event System.Action OnValueUpdated;

		public override void _Ready()
		{
			_label = this.GetNodeFromPath<Label>( _labelPath );
			
			_x = this.GetNodeFromPath<SpinBox>( _xPath );
			_y = this.GetNodeFromPath<SpinBox>( _yPath );
			_z = this.GetNodeFromPath<SpinBox>( _zPath );

			_x.Connect("value_changed",new Callable(this,nameof(OnChanged)));
			_y.Connect("value_changed",new Callable(this,nameof(OnChanged)));
			_z.Connect("value_changed",new Callable(this,nameof(OnChanged)));
		}
		
		public void Init(GenericDataDictionary template, GenericDataObject parentModel)
		{
			template.GetValue( "label", out string label );
			_label.Text = label;
			
			template.GetValue( "info", out string info );
			_label.TooltipText = info;
			_x.TooltipText = info;
			_y.TooltipText = info;
			_z.TooltipText = info;

			parentModel.TryGetValue(_label.Text, out GenericDataDictionary model);
			if(model != null)
			{
				_model = model;
				_model.GetValue( out Vector3 vector );
				_x.Value = vector.x;
				_y.Value = vector.y;
				_z.Value = vector.z;
			}
			else
			{
				template.GetValue( "defaultValue", out Vector3 vector );
				_model = parentModel.TryAddValue(_label.Text, vector) as GenericDataDictionary;
				_x.Value = vector.x;
				_y.Value = vector.y;
				_z.Value = vector.z;
			}
		}

		private void OnChanged(float value)
		{
			LibT.Maths.Vector3 vector = new Vector3((float)_x.Value,(float)_y.Value,(float)_z.Value);
			_model.CopyFrom(GenericDataObject.CreateGdo(vector));
			OnValueUpdated?.Invoke();
		}
	}
}