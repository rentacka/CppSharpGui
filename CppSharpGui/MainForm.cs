using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;
using System.IO;

using CppSharp;
using CppSharp.AST;
using CppSharp.Generators;
using Mono.Options;
using System.Xml;
using GGD_Wpf.Strorage.VsStorage.ProjectCondition;

namespace CppSharp_Builder
{
    public partial class MainForm : Form
    {
        Module module = null;

        List<string> sources = new List<string>();
        string bindPath;
        
        public MainForm()
        {
            InitializeComponent();
        }

        private void RunButton_Click(object sender, EventArgs e)
        {
            if (module != null)
            {
                GeneratorKind mode = GeneratorKind.CSharp;

                if (radioButton4.Checked)
                {
                    mode = GeneratorKind.CSharp;
                }
                if (radioButton5.Checked)
                    mode = GeneratorKind.CLI;
                if (radioButton6.Checked)
                    mode = GeneratorKind.C;
                if (radioButton7.Checked)
                    mode = GeneratorKind.CPlusPlus;
                if (radioButton8.Checked)
                    mode = GeneratorKind.ObjectiveC;
                if (radioButton9.Checked)
                    mode = GeneratorKind.Java;

                // std.cs生成
                File.Copy(Application.StartupPath + @"\Std.cs", bindPath + @"\Std.cs", true);
                // SymbolResolver.cs生成
                File.Copy(Application.StartupPath + @"\SymbolResolver.cs", bindPath + @"\SymbolResolver.cs", true);


                var builder = new BuildLib(module, mode);
                ConsoleDriver.Run(builder);
            }

            Process.Start(bindPath);

            module = null;

            Environment.Exit(0);
        }

        private void MainForm_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effect = DragDropEffects.Copy;
            }
        }

        private void MainForm_DragDrop(object sender, DragEventArgs e)
        {
            Errorlabel.Text = null;
            sources.Clear();

            // ドラッグ＆ドロップされたファイル
            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);

            module = new Module("Sample");
            module.OutputNamespace = module.LibraryName;
            module.SharedLibraryName = module.LibraryName;

            foreach (string file in files)
            {
                var ext = Path.GetExtension(file);
                var fileName = Path.GetFileName(file);
                var fileNameNoExt = Path.GetFileNameWithoutExtension(file);
                var fileDir = Path.GetDirectoryName(file);

                bool isFile = false;
                if (ext == ".h")
                {
                    if (module.IncludeDirs.Contains(fileDir) == false)
                        module.IncludeDirs.Add(fileDir);

                    //module.Headers.Add(fileName);
                    module.Headers.Add(file);

                    isFile = true;
                }

                if (ext == ".cs" || ext == ".cpp")
                {
                    // たぶんC#は.hと.cppかDLLやLibで変換前にC++の参照を全部追加しておく必要があるんかな?
                    //module.CodeFiles.Add(fileName);
                    module.CodeFiles.Add(file);

                    isFile = true;
                }

                if (ext == ".lib")
                {
                    if (module.LibraryDirs.Contains(fileDir) == false)
                        module.LibraryDirs.Add(fileDir);

                    module.Libraries.Add(fileName);
                    //module.Libraries.Add(file);

                    isFile = true;
                }

                if (ext == ".dll")
                {
                    if (module.LibraryDirs.Contains(fileDir) == false)
                        module.LibraryDirs.Add(fileDir);

                    //module.Libraries.Add(fileName);
                    module.Libraries.Add(file);

                    isFile = true;
                }

                if (isFile == false)
                {
                    if (System.IO.Directory.Exists(file))
                    {
                        var di = new DirectoryInfo(file);
                        var allDir = di.GetDirectories("*.*", SearchOption.AllDirectories);

                        bool isTrudir = false;
                        var truName = di.Name.ToLower();
                        if (truName == "include" || truName == "inc")
                        {
                            if (module.IncludeDirs.Contains(fileDir) == false)
                                module.IncludeDirs.Add(fileDir);

                            isTrudir = true;
                        }

                        if (truName == "lib" || truName == "library")
                        {
                            if (module.LibraryDirs.Contains(fileDir) == false)
                                module.LibraryDirs.Add(fileDir);

                            isTrudir = true;
                        }

                        if (isTrudir == false)
                        {
                            if (module.IncludeDirs.Contains(di.FullName) == false)
                                module.IncludeDirs.Add(di.FullName);
                        }

                        foreach (var otherdir in allDir)
                        {
                            if (module.IncludeDirs.Contains(otherdir.FullName) == false)
                            {
                                module.IncludeDirs.Add(otherdir.FullName);
                            }

                            /*
                            foreach (var topFile in otherdir.GetFiles("*.h", SearchOption.TopDirectoryOnly))
                                if (module.Headers.Contains(topFile.FullName) == false)
                                    module.Headers.Add(topFile.FullName);
                            */
                        }
                    }
                }
            }

            FileCountlabel.Text = "Files:" + (module.CodeFiles.Count + module.Libraries.Count + module.Headers.Count + module.LibraryDirs.Count + module.IncludeDirs.Count).ToString();
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            AllowDrop = true;

            bindPath = System.IO.Path.Combine(System.Windows.Forms.Application.StartupPath, "Output");
            System.IO.Directory.CreateDirectory(bindPath);
        }

        private void RapperCreateButton_Click(object sender, EventArgs e)
        {
            var swigForm = new SwigForGui.Form1();
            swigForm.ShowDialog();
        }

        #region CopyDirectory
        public static void CopyDirectory(
            string sourceDirName, string destDirName)
        {
            //コピー先のディレクトリがないときは作る
            if (!System.IO.Directory.Exists(destDirName))
            {
                System.IO.Directory.CreateDirectory(destDirName);
                //属性もコピー
                System.IO.File.SetAttributes(destDirName,
                    System.IO.File.GetAttributes(sourceDirName));
            }

            //コピー先のディレクトリ名の末尾に"\"をつける
            if (destDirName[destDirName.Length - 1] !=
                    System.IO.Path.DirectorySeparatorChar)
                destDirName = destDirName + System.IO.Path.DirectorySeparatorChar;

            //コピー元のディレクトリにあるファイルをコピー
            string[] files = System.IO.Directory.GetFiles(sourceDirName);
            foreach (string file in files)
                System.IO.File.Copy(file,
                    destDirName + System.IO.Path.GetFileName(file), true);

            //コピー元のディレクトリにあるディレクトリについて、再帰的に呼び出す
            string[] dirs = System.IO.Directory.GetDirectories(sourceDirName);
            foreach (string dir in dirs)
                CopyDirectory(dir, destDirName + System.IO.Path.GetFileName(dir));
        }
        #endregion CopyDirectory
    }
}
