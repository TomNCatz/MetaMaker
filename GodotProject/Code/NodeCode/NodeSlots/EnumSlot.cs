using System.Collections.Generic;
using Godot;
using LibT;
using LibT.Serialization;

namespace MetaMaker
{
	public partial class EnumSlot : Container, IField
	{
		[Export] public NodePath _labelPath;
		private Label _label;
		[Export] public NodePath _fieldPath;
		private OptionButton _field;
		private GenericDataObject<string> _model;

		public string Label { get => _label.Text; set => _label.Text = value; }
		public event System.Action OnValueUpdated;
		
		public override void _Ready()
		{
			_label = this.GetNodeFromPath<Label>( _labelPath );
			_field = this.GetNodeFromPath<OptionButton>( _fieldPath );
			_field.Connect("item_selected",new Callable(this,nameof(OnChanged)));
		}

		public void Init(GenericDataDictionary template, GenericDataObject parentModel)
		{
			template.GetValue( "label", out string label );
			_label.Text = label;
			
			template.GetValue( "info", out string info );
			_label.TooltipText = info;
			_field.TooltipText = info;
			
			template.GetValue( "expandedField", out bool expandedField );
			_label.SizeFlagsHorizontal = expandedField ? (int) SizeFlags.Fill : (int) SizeFlags.ExpandFill;
			_field.Align = expandedField ? Godot.Button.TextAlign.Right : Godot.Button.TextAlign.Left;
			
			template.GetValue( "values", out List<string> values );
			foreach( string value in values )
			{
				_field.AddItem( value );
			}
			parentModel.TryGetValue(_label.Text, out GenericDataObject<string> model);
			if(model != null)
			{
				_model = model;
				_model.GetValue(out string choice);

				for( int i = 0; i < _field.GetItemCount(); i++ )
				{
					if( _field.GetItemText( i ).Equals( choice ) )
					{
						_field.Select( i );
					}	
				}
			}
			else
			{
				_model = parentModel.TryAddValue( _label.Text, GetSelection() ) as GenericDataObject<string>;
			}
		}

		private void OnChanged(int value)
		{
			_model.value = GetSelection();
			OnValueUpdated?.Invoke();
		}

		private string GetSelection()
		{
			if(_field.GetItemCount() == 0)
			{
				return string.Empty;
			}

			return _field.GetItemText(_field.Selected);
		}
	}
}