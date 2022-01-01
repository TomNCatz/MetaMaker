using System;
using System.Collections.Generic;
using Godot;
using LibT;
using LibT.Maths;
using LibT.Serialization;

namespace MetaMaker
{
	public class FlagsSlot : Container, IField
	{
		[Export] public PackedScene flagsScene;
		[Export] public NodePath _titlePath;
		private Label _label;
		[Export] public NodePath _currentPath;
		private Label _current;

		private GenericDataObject<int> _model;

		public string Label { get => _label.Text; set => _label.Text = value; }
		public event Action OnValueUpdated;

		private Mask32 _value;
		private List<FlagItem> items = new List<FlagItem>();

		public override void _Ready()
		{
			_label = this.GetNodeFromPath<Label>( _titlePath );
			_current = this.GetNodeFromPath<Label>( _currentPath );
		}

		public void Init(GenericDataDictionary template, GenericDataObject parentModel)
		{
			template.GetValue( "label", out string label );
			_label.Text = label;

			template.GetValue("flags", out List<string> flags);
			foreach(var flag in flags)
			{
				FlagItem item = flagsScene.Instance() as FlagItem;
				AddChild(item);
				item.Label = flag;
				item.OnValueUpdated += OnValueChange;
				items.Add(item);
			}

			parentModel.TryGetValue(_label.Text, out GenericDataObject<int> model);
			if(model != null)
			{
				_model = model;
				_value = _model.value;
			}
			else
			{
				template.GetValue( "defaultValue", out int value );
				_value = value;
				_model = parentModel.TryAddValue(_label.Text, value) as GenericDataObject<int>;
			}

			for(int i = 0; i < items.Count; i++)
			{
				items[i].Value = _value[i];
			}
			_current.Text = _value.flags.ToString();
		}

		private void OnValueChange()
		{
			_value = 0;
			for(int i = 0; i < items.Count; i++)
			{
				_value[i] = items[i].Value;
			}

			_current.Text = _value.flags.ToString();
			_model.value = _value.flags;
			OnValueUpdated?.Invoke();
		}
	}
}