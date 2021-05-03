using Godot;
using LibT;

namespace MetaMaker
{
	public class LinkToParentSlot : Control
	{
		[Export] public NodePath _indexPath;
		private Label _indexLabel;
		[Export] public NodePath _upButtonPath;
		private Button _upButton;
		[Export] public NodePath _downButtonPath;
		private Button _downButton;

		public bool IsLinked { get; private set; }
		public int Index
		{
			get => _index;
			set
			{
				_index = value;

				UpdateDisplay();
			}
		}
		private int _index;
		
		public override void _Ready()
		{
			_indexLabel = this.GetNodeFromPath<Label>( _indexPath );
			_upButton = this.GetNodeFromPath<Button>( _upButtonPath );
			_downButton = this.GetNodeFromPath<Button>( _downButtonPath );
			_upButton.Connect( "pressed", this, nameof(OnUpPressed) );
			_downButton.Connect( "pressed", this, nameof(OnDownPressed) );
		}

		public void Link(int index)
		{
			IsLinked = true;
			Index = index;
		}

		public void Unlink()
		{
			IsLinked = false;
			Index = -1;
		}

		private void UpdateDisplay()
		{
			if(_index < 0)
			{
				_indexLabel.Text = "UNLINKED";
			}
			else
			{
				_indexLabel.Text = _index.ToString();
			}
		}

		private void OnUpPressed()
		{

		}

		private void OnDownPressed()
		{
			
		}
	}
}