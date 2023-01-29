using System;
using Godot;
using LibT.Serialization;

namespace MetaMaker
{
	public abstract class KeyAbstraction : Container, IField
	{
		[Export] public NodePath _labelPath;
		protected Label _label;

		public abstract string GetKey {get;}
		
		public abstract int LinkType {get;}

		public string Label { get => _label.Text; set => _label.Text = value; }
		public event System.Action OnValueUpdated;

		protected void ValueUpdate()
		{
			OnValueUpdated?.Invoke();
		}

		public abstract void Init(GenericDataDictionary template, GenericDataObject parentModel);
	}
}