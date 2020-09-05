using Godot;
using System;
using LibT;
using LibT.Debugging;

public class Growl : Node
{
	[Export]private Log.LogLevel _lowestLogLevel = Log.LogLevel.WARNINGS;
	[Export]private string _logFile = string.Empty;

	private static bool isLoaded;
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		// todo find out why when I load certain files this script gets started over with no settings applied
		if(isLoaded) return;
		isLoaded = true; 
		GD.Print( "Growl Ready" );
		
		
		Log.lowestDebug = _lowestLogLevel;
		Log.PassLogs += LogOnPassLogs;
		
		GD.Print( OS.GetUserDataDir()+"/"+_logFile );

		if( !string.IsNullOrEmpty( _logFile ) ) new LogToCsv( OS.GetUserDataDir() + "/" + _logFile );
	}

	private void LogOnPassLogs( LogArgs args )
	{
		string message;
		
		if( args.level == Log.LogLevel.EXCEPTIONS )
		{
			args.message = $"{args.except.Message}\n{args.except.StackTrace}";
		}
		if( args.context != null )
		{
			message = $"-- {args.context.GetType()} -- {args.message}";
		}
		else
		{
			message = args.message;
		}
		switch(args.level)
		{
			case Log.LogLevel.NONE : GD.PrintErr( message ); break;
			case Log.LogLevel.ASSERTS : GD.PrintErr( $"AST {message}" ); break;
			case Log.LogLevel.ERRORS : GD.PrintErr( $"ERR {message}" ); break;
			case Log.LogLevel.EXCEPTIONS : GD.PrintErr( $"EXP {message}" ); break;
			case Log.LogLevel.WARNINGS : GD.Print( $"WARN {message}"  );break;
			case Log.LogLevel.LOGS : GD.Print( $"LOG {message}" );break;
			case Log.LogLevel.VERBOSE : GD.Print( $"VER {message}"  );break;
			case Log.LogLevel.ALL : GD.Print( $"ALL {message}"  );break;
			default : throw new ArgumentOutOfRangeException();
		}
	}
}
