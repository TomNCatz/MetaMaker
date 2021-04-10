using System;
using LibT;
using LibT.Debugging;
using NUnit.Framework;

namespace LibT.Tests
{
	[TestFixture] public class SharpModsTests
	{
		private LogToConsole _console;
		[SetUp] public void Init()
		{
			_console = new LogToConsole( Log.LogLevel.ALL, false );
		}

		[TearDown] public void Dispose()
		{
			_console.TearDown();
		}
		
        public string classCode = @"
using System;
using LibT.Tests;
namespace MyApp
{
    public class Test1: SharpModsTests.IHello
    {
        public string HelloWorld(string name) 
        {
            //return $@""Hello {name}"";   // This doesn't work in in classic - C# 6+ syntax
            return ""Hello "" + name + "" from the classic compiler."";
        }
    }
}";
        public interface IHello
        {
            string HelloWorld(string name);
        }

		[Test] public void SingleFileCompileAndRunTest([Values("","Tom", "word","this\ntest\nis\nmultiline","Phaine of Catz")]string name)
		{
			var args = new AssemblyCopileArgs();
			args.references.Add(SharpMods.GetPortableReferenceToType(typeof(Guid)));
			args.references.Add(SharpMods.GetPortableReferenceToType(typeof(IHello)));
			args.code = classCode;
			var assembly = SharpMods.RoslynAttempt(args);

			var type = assembly.GetType("MyApp.Test1");

			var obj = Activator.CreateInstance(type, null);
			IHello speaker = (IHello)obj;
			Log.Error(speaker.HelloWorld(name));
		}

		public void SingleFileCompileAndRunManualTest(string name = "Jeff")
		{
			var args = new AssemblyCopileArgs();
			args.references.Add(SharpMods.GetPortableReferenceToType(typeof(Guid)));
			args.references.Add(SharpMods.GetPortableReferenceToType(typeof(IHello)));
			args.code = classCode;
			var assembly = SharpMods.RoslynAttempt(args);

			var type = assembly.GetType("MyApp.Test1");

			var obj = Activator.CreateInstance(type, null);
			IHello speaker = (IHello)obj;
			Log.Error(speaker.HelloWorld(name));
		}
	}
}