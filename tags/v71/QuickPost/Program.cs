using System;

using System.Collections.Generic;
using System.Text;

namespace QuickPost
{
    class Program
    {
        static void Main(string[] args)
        {
            string Path = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase);
            System.Diagnostics.Process p = new System.Diagnostics.Process();
            p.StartInfo.Arguments = "/QuickPost";
            p.StartInfo.FileName = Path + "\\PockeTwit.exe";
            p.Start();
        }
    }
}
