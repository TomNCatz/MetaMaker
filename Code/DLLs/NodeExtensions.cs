using System;
using Godot;

namespace LibT
{
	public static class NodeExtensions
	{
		public static T GetNodeFromPath<T>( this Node node, NodePath nodePath)where T:class
		{
			try
			{
				string path = String.Empty;
				int length = nodePath.GetNameCount();

				if( length > 1 && nodePath.GetName( 0  ).Equals( ".." ))
				{
					for( int i = 2; i < length; i++ )
					{
						path += nodePath.GetName( i ) + "/";
					}
				
					path = path.Substring( 0, path.Length - 1 );
				}
				else
				{
					path = nodePath.ToString();
				}

				return node.GetNode<T>( path );
			}
			catch( Exception e )
			{
				Log.Except( e );
				Log.Error( nodePath.ToString() );
				throw;
			}
		}
		
		public static async void DelayedInvoke(this Node node, float delay, Action callback )
		{
			var t = new Timer();
			t.WaitTime = delay;
			t.OneShot = true;
			node.AddChild( t );
			t.Start();
		
			await node.ToSignal(t, "timeout");
		
			callback.Invoke();
		
			t.QueueFree();
		}
	}
}