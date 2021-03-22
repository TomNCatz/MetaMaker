using Godot;
using LibT;
using RSG;
using RSG.Exceptions;

public class FileDialogExtended : FileDialog
{
	private Promise<string> _promise;
	
    public override void _Ready()
    {
	    Connect( "file_selected", this, nameof(OnSelect) );
	    Connect( "popup_hide", this, nameof(OnClose) );
    }

    public IPromise<string> Show(string[] filters, bool save, string title = null)
    {
	    if( string.IsNullOrEmpty( title ) )
	    {
		    ModeOverridesTitle = true;
	    }
	    else
	    {
		    ModeOverridesTitle = false;
		    WindowTitle = title;
	    }
	    
	    if( save )
	    {
		    Mode = ModeEnum.SaveFile;
	    }
	    else
	    {
		    Mode = ModeEnum.OpenFile;
	    }
	    
	    Filters = filters;
	    
	    PopupCentered();
	    _promise = new Promise<string>();

	    return _promise;
    }
    
    private void OnSelect(string path)
    {
	    _promise.Resolve( path );
    }
    
    private void OnClose()
    {
	    if( _promise )
	    {
		    _promise.Reject( new PromiseExpectedError("canceled") );
	    }
    }
}
