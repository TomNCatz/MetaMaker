using System;
using System.Collections.Generic;
using Godot;
using LibT;
using LibT.Maths;
using LibT.Serialization;

namespace MetaMaker
{
	public class FlagsSlot : Container, IField, IGdoConvertible
	{
		[Export] public PackedScene flagsScene;
		[Export] public NodePath _titlePath;
		private Label _title;
		[Export] public NodePath _currentPath;
		private Label _current;

		private GenericDataArray _parentModel;
		public event Action OnValueUpdated;

		private Mask32 _value;
		private List<FlagItem> items = new List<FlagItem>();

		public override void _Ready()
		{
			_title = this.GetNodeFromPath<Label>( _titlePath );
			_current = this.GetNodeFromPath<Label>( _currentPath );
		}

		public void Init(GenericDataArray template, GenericDataArray parentModel)
		{
			template.GetValue( "label", out string label );
			_title.Text = label;

			_parentModel = parentModel;

			template.GetValue("flags", out List<string> flags);
			foreach(var flag in flags)
			{
				FlagItem item = flagsScene.Instance() as FlagItem;
				AddChild(item);
				item.Label = flag;
				item.OnValueUpdated += OnValueChange;
				items.Add(item);
			}
			
			template.GetValue( "defaultValue", out Mask32 value );
			_value = 0;
			for(int i = 0; i < items.Count; i++)
			{
				items[i].Value = value[i];
			}
			_parentModel.AddValue(_title.Text, _value);
		}

		public void GetObjectData( GenericDataArray objData )
		{
		}

		public void SetObjectData( GenericDataArray objData )
		{
			objData.GetValue( _title.Text, out Mask32 value );
			_value = 0;
			for(int i = 0; i < items.Count; i++)
			{
				items[i].Value = value[i];
			}

			_current.Text = _value.flags.ToString();
			_parentModel.AddValue(_title.Text, _value);
		}

		private void OnValueChange()
		{
			_value = 0;
			for(int i = 0; i < items.Count; i++)
			{
				_value[i] = items[i].Value;
			}

			_current.Text = _value.flags.ToString();
			_parentModel.AddValue(_title.Text, _value);
			OnValueUpdated?.Invoke();
		}
	}
}