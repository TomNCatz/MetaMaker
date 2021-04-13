using Godot;
using System;
using System.Collections.Generic;
using LibT;
using LibT.Serialization;

public class SerializationTest : Node, IGdoConvertible
{
	public enum Kind {UNKIND,MEAN,KIND,ASSHOLE}
	
	
	[Export] public NodePath _displayButtonPath;
	private Button _displayButton;
	[Export] public NodePath _saveButtonPath;
	private Button _saveButton;
	[Export] public NodePath _clearButtonPath;
	private Button _clearButton;
	[Export] public NodePath _loadButtonPath;
	private Button _loadButton;
	[Export] public NodePath _textPath;
	private Label _text;
	[Export] public NodePath _originPath;
	private Label _origin;
	
	[Export] public string id;
	[Export] public int count;
	[Export] public float value;
	[Export] public bool isThing;
	[Export] public Kind kind;
	public List<int> numbers = new List<int> {1,2,3,4};
	public string[] strings = new[] {"test","this","mess"};
	public Dictionary<string,int> common = new Dictionary<string, int>(){{"four",4},{"seven",7}};
	public NestedTest nested = new NestedTest();
	public List<NestedTest> nestedList = new List<NestedTest>();
	public Dictionary<string,NestedTest> nestedDic = new Dictionary<string, NestedTest>();

	public override void _Ready()
	{
		_displayButton = this.GetNodeFromPath<Button>( _displayButtonPath );
		_displayButton.Connect( "pressed", this, nameof(Display) );
		
		_saveButton = this.GetNodeFromPath<Button>( _saveButtonPath );
		_saveButton.Connect( "pressed", this, nameof(Save) );
		
		_clearButton = this.GetNodeFromPath<Button>( _clearButtonPath );
		_clearButton.Connect( "pressed", this, nameof(Clear) );
		
		_loadButton = this.GetNodeFromPath<Button>( _loadButtonPath );
		_loadButton.Connect( "pressed", this, nameof(Load) );
		
		_text = this.GetNodeFromPath<Label>( _textPath );
		_origin = this.GetNodeFromPath<Label>( _originPath );
		
		nested.id = "Nested";
		nested.numbers.Add( 5 );
		
		nestedList.Add( nested );
		nestedList.Add( new NestedTest() );

		nestedDic["test"] = nested;
		nestedDic["num2"] = new NestedTest();
		
		Display();
	}
	
	public void GetObjectData( GenericDataArray objData )
	{
		objData.AddValue( "id", id );
		objData.AddValue( "count", count );
		objData.AddValue( "value", value );
		objData.AddValue( "isThing", isThing );
		objData.AddValue( "kind", kind );
		objData.AddValue( "numbers", numbers );
		objData.AddValue( "strings", strings );
		objData.AddValue( "common", common );
		objData.AddValue( "nested", nested );
		objData.AddValue( "nestedList", nestedList );
		objData.AddValue( "nestedDic", nestedDic );
	}

	public void SetObjectData( GenericDataArray objData )
	{
		objData.GetValue( "id", out id );
		objData.GetValue( "count", out count );
		objData.GetValue( "value", out value );
		objData.GetValue( "isThing", out isThing );
		objData.GetValue( "kind", out kind );
		objData.GetValue( "numbers", out  numbers );
		objData.GetValue( "strings", out  strings );
		objData.GetValue( "common", out  common );
		objData.GetValue( "nested", out  nested );
		objData.GetValue( "nestedList", out  nestedList );
		objData.GetValue( "nestedDic", out  nestedDic );
	}

	private void Display()
	{
		_text.Text = this.GetObjectData().ToJson();
	}

	private void Save()
	{
		_origin.Text = this.GetObjectData().ToJson();
	}

	private void Clear()
	{
		id = null;
		count = 0;
		value = 0;
		isThing = false;
		kind = Kind.ASSHOLE;
		numbers.Clear();
		strings = new string[0];
		common.Clear();
		nested = null;
		nestedList.Clear();
		nestedDic.Clear();
	}

	private void Load()
	{
		GenericDataArray gda = new GenericDataArray();
		gda.FromJson( _origin.Text );
		SetObjectData( gda );
	}
	
	
	public class NestedTest : IGdoConvertible
	{
		[Export] public string id;
		[Export] public int count;
		[Export] public float value;
		[Export] public bool isThing;
		[Export] public Kind kind;
		public List<int> numbers = new List<int> {1,2,3,4};
		public string[] strings = new[] {"test","this","mess"};
		public Dictionary<string,int> common = new Dictionary<string, int>(){{"four",4},{"seven",7}};

		public void GetObjectData( GenericDataArray objData )
		{
			objData.AddValue( "id", id );
			objData.AddValue( "count", count );
			objData.AddValue( "value", value );
			objData.AddValue( "isThing", isThing );
			objData.AddValue( "kind", kind );
			objData.AddValue( "numbers", numbers );
			objData.AddValue( "strings", strings );
			objData.AddValue( "common", common );
		}

		public void SetObjectData( GenericDataArray objData )
		{
			objData.GetValue( "id", out id );
			objData.GetValue( "count", out count );
			objData.GetValue( "value", out value );
			objData.GetValue( "isThing", out isThing );
			objData.GetValue( "kind", out kind );
			objData.GetValue( "numbers", out  numbers );
			objData.GetValue( "strings", out  strings );
			objData.GetValue( "common", out  common );
		}
	}
}
