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

		private GenericDataArray _parentModel;
		public event Action OnValueUpdated;

		private Mask32 _value;
		private List<FlagItem> items = new List<FlagItem>();

		public override void _Ready()
		{
			_label = this.GetNodeFromPath<Label>( _titlePath );
			_current = this.GetNodeFromPath<Label>( _currentPath );
		}

		public void Init(GenericDataArray template, GenericDataArray parentModel)
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

			_parentModel = parentModel;
			if(parentModel.values.ContainsKey(_label.Text))
			{
				parentModel.GetValue( _label.Text, out int value );
				_value = value;
			}
			else
			{
				template.GetValue( "defaultValue", out int value );
				_value = value;
				_parentModel.AddValue(_label.Text, value);
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
			_parentModel.AddValue(_label.Text, _value);
			OnValueUpdated?.Invoke();
		}
	}
}