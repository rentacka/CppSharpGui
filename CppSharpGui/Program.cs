using CppSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CppSharp_Builder
{
    public class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            var form = new MainForm();
            Application.Run(form);
        }
    }
}
