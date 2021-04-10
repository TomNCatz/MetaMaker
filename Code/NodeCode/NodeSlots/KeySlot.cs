using System;
using Godot;
using LibT;
using LibT.Serialization;
using LibT.Services;

namespace MetaMaker
{
	public class KeySlot : Container, IGdaLoadable, IGdoConvertible, IStringRetriever
	{
		[Export] private readonly NodePath _labelPath;
		private Label _label;
		[Export] private readonly NodePath _fieldPath;
		private Label _field;

		public string GetKey => _field.Text;
		
		private string _keyPrefix = String.Empty;
		private int _keySize = 1;
		private readonly ServiceInjection<App> _app = new ServiceInjection<App>();
		
		public override void _Ready()
		{
			_label = this.GetNodeFromPath<Label>( _labelPath );
			_field = this.GetNodeFromPath<Label>( _fieldPath );
		}

		public void LoadFromGda( GenericDataArray data )
		{
			data.GetValue( "label", out string label );
			_label.Text = label;
			
			data.GetValue( "keyPrefix", out _keyPrefix );
			data.GetValue( "keySize", out _keySize );

			SetKey();
		}

		public override void _ExitTree()
		{
			if( _app.Get.generatedKeys.ContainsKey( _field.Text ) )
			{
				_app.Get.generatedKeys.Remove( _field.Text );
			}
			base._ExitTree();
		}

		private void SetKey( string force = null )
		{
			if( _app.Get.pastingData )
			{
				force = null;
			}
			
			if(string.IsNullOrEmpty( force ))
			{
				force = Tool.GetUniqueKey( _keySize, _app.Get.generatedKeys.ContainsKey, 4, _keyPrefix );
			}
			else if( _app.Get.generatedKeys.ContainsKey( force ) )
			{
				string alt = Tool.GetUniqueKey( _keySize, _app.Get.generatedKeys.ContainsKey, 4, _keyPrefix );

				_app.Get.generatedKeys[alt] = _app.Get.generatedKeys[force];
			}

			if( _app.Get.generatedKeys.ContainsKey( _field.Text ) )
			{
				_app.Get.generatedKeys.Remove( _field.Text );
			}
			
			_field.Text = force;
			_app.Get.generatedKeys[_field.Text] = this;
		}

		public void GetObjectData( GenericDataArray objData )
		{
			objData.AddValue( _label.Text, _field.Text );
		}

		public void SetObjectData( GenericDataArray objData )
		{
			objData.GetValue( _label.Text, out string key );
			
			SetKey( key );
		}

		public string GetString()
		{
			return _field.Text;
		}
	}
}