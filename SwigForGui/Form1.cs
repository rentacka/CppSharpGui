using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SwigForGui
{
    public partial class Form1 : Form
    {
        string GenerateMode = "-csharp";

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_DragDrop(object sender, DragEventArgs e)
        {
            var bindPath = System.IO.Path.Combine(System.Windows.Forms.Application.StartupPath, "Output");
            System.IO.Directory.CreateDirectory(bindPath);

            // ドラッグ＆ドロップされたファイル
            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);

            List<string> headerFiles = new List<string>();
            // Dllのモジュールを作成
            if (files.Length == 1)
            {
                var file = files.First();
                if (System.IO.Directory.Exists(file))
                {
                    files = System.IO.Directory.GetFiles(file,"*.h", System.IO.SearchOption.AllDirectories);
                }
            }

            foreach (var file in files)
            {
                var ext = System.IO.Path.GetExtension(file);
                if (ext == ".h")
                    // たぶんVisualStudioやとファイル名だけでいいはず？
                    headerFiles.Add(System.IO.Path.GetFileName(file));
            }

            if (files.Length > 0)
            {
                // Swig用のインターフェイスモジュールの作成
                StringBuilder sb = new StringBuilder();
                string moduleName = ModuleTextBox1.Text;
                if (moduleName.Length == 0)
                    moduleName = Guid.NewGuid().ToString();
                sb.AppendLine("%module " + moduleName);
                // ヘッダシンボル
                sb.AppendLine("%{");
                foreach (var header in headerFiles)
                {
                    sb.AppendLine("#include " + @"""" + header + @"""");
                }
                sb.AppendLine("%}");
                foreach (var header in headerFiles)
                {
                    sb.AppendLine("%include " + @"""" + header + @"""");
                }

                //文字コード(ここでは、Shift JIS)
                System.Text.Encoding enc = System.Text.Encoding.GetEncoding("shift_jis");
                System.IO.File.WriteAllText(System.IO.Path.Combine(bindPath, moduleName + ".i"), sb.ToString(), enc);

                /*
                System.Diagnostics.ProcessStartInfo info = new System.Diagnostics.ProcessStartInfo();
                info.Arguments = GenerateMode;
                info.CreateNoWindow = true;
                info.FileName = System.IO.Path.Combine(Application.StartupPath, @"swigwin-3.0.12\swig.exe");
                System.Diagnostics.Process.Start(info);
                */

                System.Diagnostics.Process.Start(bindPath);
            }
            else
            {
                MessageBox.Show("ヘッダファイルなし");
            }
        }

        private void Form1_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effect = DragDropEffects.Copy;
            }
        }

        private void radioButton1_CheckedChanged(object sender, EventArgs e)
        {
            var radiobutton = (RadioButton)sender;
            GenerateMode =  "-" + radiobutton.Text;
        }
    }
}
