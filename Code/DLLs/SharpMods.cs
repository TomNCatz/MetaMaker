using System.CodeDom.Compiler;
using System.Reflection;
using System;
using System.IO;
using LibT.Serialization;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis;
using System.Collections.Generic;
using System.Linq;

namespace LibT
{
    public class AssemblyCopileArgs
    {
        public CSharpCompilationOptions Options = new CSharpCompilationOptions(
                    OutputKind.DynamicallyLinkedLibrary,
                    reportSuppressedDiagnostics: true,
                    optimizationLevel: OptimizationLevel.Release,
                    generalDiagnosticOption: ReportDiagnostic.Error
                );

        public List<PortableExecutableReference> references = new List<PortableExecutableReference>();
        
        public string code;
    }

    public static class SharpMods
    {
		
        public static string classCode = @"
using System;
using LibT;
namespace MyApp
{
    public class Test1: SharpMods.IHello
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

        public static Assembly RoslynAttempt(AssemblyCopileArgs args)
        {
            var compilation = CSharpCompilation.Create(
                "_" + Guid.NewGuid().ToString("D"),
                references: args.references,
                syntaxTrees: new SyntaxTree[] { CSharpSyntaxTree.ParseText(args.code) },
                options: args.Options
            );

            using (var ms = new MemoryStream())
            {
                var compilationResult = compilation.Emit(ms);

                if (compilationResult.Success)
                {
                    ms.Seek(0, SeekOrigin.Begin);
                    return Assembly.Load(ms.ToArray());
                }

                throw new Exception($"Assembly could not be created. {compilationResult.Diagnostics}");
            }
        }

        public static PortableExecutableReference GetPortableReferenceToType(Type type)
        {
            if(type == null) return null;

            return MetadataReference.CreateFromFile(type.Assembly.Location);
        }
    }
}