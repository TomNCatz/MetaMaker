using System.Collections.Generic;
using Godot;
using LibT;
using LibT.Services;

namespace MetaMaker
{
	public partial class LinkToParentSlot : Control
	{
		[Export] public NodePath _indexPath;
		private Label _indexLabel;
		[Export] public NodePath _upButtonPath;
		private Button _upButton;
		[Export] public NodePath _downButtonPath;
		private Button _downButton;
		
		[Injectable] private MainView _mainView;

		private List<LinkToChildSlot> _links = new List<LinkToChildSlot>();

		public bool IsLinked => _links.Count > 0;
		
		public int LinkType {get;set;}

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
			_upButton.Connect("pressed",new Callable(this,nameof(OnUpPressed)));
			_downButton.Connect("pressed",new Callable(this,nameof(OnDownPressed)));
			UpdateDisplay();
		}

		public void Link(LinkToChildSlot link)
		{
			_links.Add(link);
			if(link.parentListing != null)
			{
				link.parentListing.OnValueUpdated += UpdateDisplay;
			}
			UpdateDisplay();
		}

		public void Unlink(LinkToChildSlot link)
		{
			if(_links.Contains(link))
			{
				if(link.parentListing != null)
				{
					link.parentListing.OnValueUpdated -= UpdateDisplay;
				}
				_links.RemoveAt(link);
			}

			UpdateDisplay();
		}

		private void UpdateDisplay()
		{
			if(LinkType == _mainView.CurrentParentIndex)
			{
				_indexLabel.Text = "GLOBAL";
				_upButton.Visible = false;
				_downButton.Visible = false;
			}
			else if(_links.Count == 0)
			{
				_indexLabel.Text = "UNLINKED";
				_upButton.Visible = false;
				_downButton.Visible = false;
			}
			else if(_links.Count == 1)
			{
				_indexLabel.Text = _links[0].Label;
				//_upButton.Visible = true;
				//_downButton.Visible = true;
			}
			else
			{
				_indexLabel.Text = "MULTIPLE";
				_upButton.Visible = false;
				_downButton.Visible = false;
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