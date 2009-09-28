using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration.Install;
using Microsoft.Win32;
using System.Reflection;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;


//namespace CustomInstallLibCS2
//{
//    [RunInstaller(true)]
//    public partial class CustomInstaller : Installer
//    {
//        public CustomInstaller()
//        {
//            InitializeComponent();
//        }
//    }
//}

namespace CustomInstallLib
{
    [RunInstaller(true)]
    public partial class CustomInstaller : Installer
    {
        public CustomInstaller()
        {
            InitializeComponent();
        }
        // After Desktop install this fires to run CEAppMgr to install on phone
        protected override void OnAfterInstall(IDictionary savedState)
        {
            string programName = "\"" + CEAppMgrExe + "\"";
            string programArg = "\"" + IniFilePath + "\"";

            //MessageBox.Show(programName + " " + programArg);

            Process.Start(programName, programArg);

            base.OnAfterInstall(savedState);
        }

        const string _ceAppMgrRegistryPath = @"software\Microsoft\Windows\CurrentVersion\App Paths\CEAppMgr.exe";
        const string _iniFileName = "PockeTwitSetup.ini";
        const string _filePrefix = @"file:\";

        // Gets the path for the CEAppMgr.exe 
        string CEAppMgrExe
        {
            get
            {
                RegistryKey subkey = null;
                string exePath = null;
                try
                {
                    subkey = Registry.LocalMachine.OpenSubKey(_ceAppMgrRegistryPath);
                    exePath =  (string) subkey.GetValue("");
                }
                finally
                {
                    if (subkey != null)
                        subkey.Close();
                }
                return exePath;

            }
        }

        // Gets the Path for the ini file required by the CEAppMgr
        string IniFilePath
        {
            get
            {
                string installfolder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().GetName().CodeBase);
                string iniFilePath = Path.Combine(installfolder, _iniFileName);

                if (iniFilePath.StartsWith(_filePrefix))
                {
                    int index = _filePrefix.Length;
                    iniFilePath = iniFilePath.Substring(index);
                    
                }
                return iniFilePath;
            }

        }





    }
}
