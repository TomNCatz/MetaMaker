using Godot;
using LibT;
using LibT.Serialization;
using LibT.Services;

namespace MetaMaker
{
	public class KeyManualSlot : KeyAbstraction
	{
		[Export] public NodePath _prefixLabelPath;
		private Label _prefixLabel;
		[Export] public NodePath _fieldPath;
		private LineEdit _field;
		private GenericDataObject<string> _model;

		public override string GetKey => _keyPrefix + _key;
		private string _key;
		private string _keyPrefix;
		public override int LinkType => _linkType;
		private int _linkType;
		
		public override void _Ready()
		{
			_label = this.GetNodeFromPath<Label>( _labelPath );
			_prefixLabel = this.GetNodeFromPath<Label>( _prefixLabelPath );
			_field = this.GetNodeFromPath<LineEdit>( _fieldPath );
			_field.Connect("text_changed",this,nameof(OnChanged));
		}

		public override void Init(GenericDataDictionary template, GenericDataObject parentModel)
		{
			template.GetValue( "label", out string label );
			_label.Text = label;
			
			template.GetValue( "keyPrefix", out _keyPrefix );
			_prefixLabel.Text = _keyPrefix;

			template.GetValue( "slotType", out _linkType );

			parentModel.TryGetValue(_label.Text, out GenericDataObject<string> model);
			if(model != null)
			{
				_model = model;
				_field.Text = _model.value.Substring(_keyPrefix.Length, _model.value.Length - _keyPrefix.Length);
				OnChanged(_field.Text);
			}
			else
			{
				_model = parentModel.TryAddValue(_label.Text, string.Empty) as GenericDataObject<string>;
				_field.Text = string.Empty;
				OnChanged(_field.Text);
			}
		}

		private void OnChanged( string text )
		{
			bool problem = string.IsNullOrEmpty(text);
			problem |= ServiceInjection<App>.Service.ContainsKey(text);
			IndicateTrouble(problem);
			ServiceInjection<App>.Service.RemoveKey(this);

			if(problem) return;

			_key = text;
			_model.value = GetKey;
			ServiceInjection<App>.Service.AddKey(this);
			ValueUpdate();
		}

		private void IndicateTrouble(bool trouble = true)
		{
			var color = new Color(1,1,1);
			if(trouble)
			{
				color = new Color(1,0,0,0.5f);
			}

			_label.Modulate = color;
			_field.Modulate = color;
		}
	}
}