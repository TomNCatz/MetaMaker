using System;
using Godot;
using Godot.Collections;
using LibT;
using LibT.Services;

namespace MetaMaker
{
    public class SearchPopup : AcceptDialog
    {
		[Export] public NodePath _searchFieldPath;
		private LineEdit _searchField;
		[Export] public NodePath _matchCaseCheckPath;
		private CheckBox _matchCaseCheck;
		[Export] public NodePath _searchButtonPath;
		private Button _searchButton;
		[Export] public NodePath _resultsListPath;
		private Container _resultsList;

        [Injectable] private MainView _mainView;
        [Injectable] private App _app;

		public override void _Ready()
        {
	        _searchField = this.GetNodeFromPath<LineEdit>( _searchFieldPath );
	        _matchCaseCheck = this.GetNodeFromPath<CheckBox>( _matchCaseCheckPath );
	        _searchButton = this.GetNodeFromPath<Button>( _searchButtonPath );
	        _resultsList = this.GetNodeFromPath<Container>( _resultsListPath );
	        
	        _searchField.Connect( "text_entered", this, nameof(OnSearch) );
	        _searchButton.Connect( "pressed", this, nameof(OnSearchPress) );

			Connect( "about_to_show", this, nameof(OnPrep) );
			Connect( "confirmed", this, nameof(OnConfirm) );
        }

        public void OnPrep()
        {
	        if (!string.IsNullOrEmpty(_searchField.Text) || string.IsNullOrEmpty(_mainView.SearchString)) return;
	        
	        _searchField.Text = _mainView.SearchString;
	        OnSearch(_searchField.Text);
        }

        public void OnConfirm()
        {
        }

        private void OnSearchPress()
        {
	        OnSearch(_searchField.Text);
        }
        
        
        private void OnSearch( string text )
        {
	        if(string.IsNullOrEmpty(text)) return;

	        var results = new System.Collections.Generic.Dictionary<string, SlottedGraphNode>();
	        _mainView.OnSearch(text, _matchCaseCheck.Pressed, results);

	        var children = _resultsList.GetChildren();
	        foreach (var child in children)
	        {
		        (child as Node)?.QueueFree();
	        }
	        
	        foreach (var result in results)
	        {
		        var item = new Button {Text = result.Key};
		        item.SizeFlagsHorizontal = 0;
		        var binds = new Godot.Collections.Array {result.Value};
		        
		        item.Connect( "pressed", this, nameof(OnPress), binds );
		        _resultsList.AddChild(item);
	        }
        }

        private void OnPress(SlottedGraphNode target)
        {
	        _mainView.CenterViewOnNode(target);
        }
    }
}