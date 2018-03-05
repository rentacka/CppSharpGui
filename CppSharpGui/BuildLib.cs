using CppSharp;
using CppSharp.AST;
using CppSharp.Generators;
using Mono.Options;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CppSharp_Builder
{
    class BuildLib : ILibrary
    {
        Module module = null;

        public GeneratorKind GeneratorMode = GeneratorKind.CSharp;

        public BuildLib(Module target, GeneratorKind outPutMode = GeneratorKind.CSharp)
        {
            module = target;
            GeneratorMode = outPutMode;
        }

        public void Postprocess(Driver driver, ASTContext ctx)
        {
        }

        public void Preprocess(Driver driver, ASTContext ctx)
        {
        }

        public void Setup(Driver driver)
        {
            if (module != null)
            {
                var options = driver.Options;
                options.GeneratorKind = GeneratorMode;

                // 呼び出し元のC++関数をC#化
                options.CheckSymbols = true;
                options.Compilation.Platform = TargetPlatform.Windows;
                /*
                options.Compilation.Target = CompilationTarget.SharedLibrary;
                options.Compilation.VsVersion = VisualStudioVersion.VS2017;
                */

                options.GenerateSingleCSharpFile = true;

                var bindPath = System.IO.Path.Combine(System.Windows.Forms.Application.StartupPath, "Output");
                System.IO.Directory.CreateDirectory(bindPath);
                options.OutputDir = bindPath;

                options.Modules.Clear();

                options.Modules.Add(module);
            }
        }

        public void SetupPasses(Driver driver)
        {
        }
    }
}
