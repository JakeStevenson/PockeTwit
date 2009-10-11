using System;

using System.Collections.Generic;
using System.Text;
using Microsoft.Win32;

namespace PockeTwit
{
    class TodayScreenRegistrySetup
    {
        public static void CheckTodayScreenInstalled()
        {
            if(DetectDevice.DeviceType!=DeviceType.Professional)
                return;

            try
            {
                var infoKey = Registry.LocalMachine.OpenSubKey(@"Software\\Microsoft\\Today\\Items\\PockeTwit");
                
                if(infoKey==null)
                {
                    InstallKey();
                }
            }
            catch
            {
                //Unable to install today screen.
            }
        }

        private static void InstallKey()
        {
            var infoKey = Registry.LocalMachine.CreateSubKey("Software\\Microsoft\\Today\\Items\\PockeTwit");
            infoKey.SetValue("Dll","\\Windows\\PockeTwitTodayPlugin.dll");
            infoKey.SetValue("Enabled", 0);
            infoKey.SetValue("Flags", 0);
            infoKey.SetValue("Options", 1);
            infoKey.SetValue("Order", 0);
            infoKey.SetValue("Selectability", 2);
            infoKey.SetValue("Type", 4);
            infoKey.SetValue("UseCompactTodayPlugin", 0);
        }
    }
}
