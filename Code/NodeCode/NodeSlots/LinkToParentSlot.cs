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

		private LinkToChildSlot _link;

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
			UpdateDisplay();
		}

		public void Link(LinkToChildSlot link)
		{
			IsLinked = true;

			_link = link;
			if(_link.parentListing != null)
			{
				_link.parentListing.OnValueUpdated += ListingChange;
			}
			UpdateDisplay(_link.Label);
		}

		public void Unlink()
		{
			IsLinked = false;

			if(_link != null)
			{
				if(_link.parentListing != null)
				{
					_link.parentListing.OnValueUpdated -= ListingChange;
				}
				_link = null;
			}

			UpdateDisplay();
		}

		private void ListingChange()
		{
			UpdateDisplay(_link.Label);
		}

		private void UpdateDisplay(string link = null)
		{
			if(string.IsNullOrEmpty(link))
			{
				_indexLabel.Text = "UNLINKED";
				_upButton.Visible = false;
				_downButton.Visible = false;
			}
			else
			{
				_indexLabel.Text = link;
				//_upButton.Visible = true;
				//_downButton.Visible = true;
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