﻿using Microsoft.Win32;
using System;
using System.IO;
using System.IO.Pipes;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;

namespace EasyExploits
{
    public class Module
    {
        private WebClient wc = new WebClient();

        private bool CheckLastestDll(RegistryKey registryKey)
        {
            string[] strArray = this.wc.DownloadString("https://raw.githubusercontent.com/GreenMs02/Update/master/Module.txt").Split("\r\n".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
            if (!(strArray[2] == "true") || !Directory.Exists(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "\\Roblox\\Versions\\" + strArray[3]))
                return false;
            registryKey.SetValue("Ver", (object)strArray[3]);
            return true;
        }

        private bool CheckDllUpdate()
        {
            string[] strArray = this.wc.DownloadString("https://raw.githubusercontent.com/GreenMs02/Update/master/Module.txt").Split("\r\n".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
            RegistryKey registryKey = Registry.CurrentUser.OpenSubKey("SOFTWARE\\\\CoCO", true);
            if (registryKey == null)
            {
                Registry.CurrentUser.CreateSubKey("SOFTWARE\\\\CoCO").SetValue("Ver", (object)"0");
            }
            else
            {
                if (registryKey.GetValue("Ver").ToString() != strArray[0])
                {
                    registryKey.SetValue("Ver", (object)strArray[0]);
                    return true;
                }
                if (registryKey.GetValue("Ver").ToString() != strArray[3] && this.CheckLastestDll(registryKey))
                    return true;
            }
            return !System.IO.File.Exists("EasyExploitsDLL.dll");
        }

        private bool DownloadDLL()
        {
            RegistryKey registryKey = Registry.CurrentUser.OpenSubKey("SOFTWARE\\\\CoCO", true);
            string[] strArray = this.wc.DownloadString("https://raw.githubusercontent.com/GreenMs02/Update/master/Module.txt").Split("\r\n".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
            if (registryKey.GetValue("Ver").ToString() == strArray[3] && this.CheckLastestDll(registryKey))
                this.wc.DownloadFile(strArray[4], "EasyExploitsDLL.dll");
            else
                this.wc.DownloadFile(strArray[1], "EasyExploitsDLL.dll");
            return System.IO.File.Exists("EasyExploitsDLL.dll");
        }

        public void ExecuteScript(string Script)
        {
            if (Module.namedPipeExist("ocybedam"))
            {
                using (NamedPipeClientStream pipeClientStream = new NamedPipeClientStream(".", "ocybedam", PipeDirection.Out))
                {
                    pipeClientStream.Connect();
                    using (StreamWriter streamWriter = new StreamWriter((Stream)pipeClientStream, Encoding.Default, 999999))
                    {
                        streamWriter.Write(Script);
                        streamWriter.Dispose();
                    }
                    pipeClientStream.Dispose();
                }
            }
            else if (System.IO.File.Exists("EasyExploitsDLL.dll"))
            {
                int num1 = (int)MessageBox.Show("You need to inject the script first!", "[ERROR] Not Injected", MessageBoxButtons.OK, MessageBoxIcon.Hand);
            }
            else
            {
                int num2 = (int)MessageBox.Show("Your AntiVirus might be blocking the DLL, try adding the DLL to exceptions or turn it off entirely", "DLL Error", MessageBoxButtons.OK, MessageBoxIcon.Hand);
            }
        }

        private void InjectDLL()
        {
            switch (DLLInjection.DllInjector.GetInstance.Inject("RobloxPlayerBeta", Application.StartupPath + "\\EasyExploitsDLL.dll"))
            {
                case DLLInjection.DllInjectionResult.DllNotFound:
                    int num1 = (int)MessageBox.Show("Couldn't find the dll!", "[ERROR] DLL Not Found", MessageBoxButtons.OK, MessageBoxIcon.Hand);
                    break;
                case DLLInjection.DllInjectionResult.GameProcessNotFound:
                    int num2 = (int)MessageBox.Show("You need to launch roblox first!", "[ERROR] Roblox Not Launched", MessageBoxButtons.OK, MessageBoxIcon.Hand);
                    break;
                case DLLInjection.DllInjectionResult.InjectionFailed:
                    int num3 = (int)MessageBox.Show("Injection failed!", "Injection Failed", MessageBoxButtons.OK, MessageBoxIcon.Hand);
                    break;
            }
        }

        public void LaunchExploit()
        {
            if (Module.namedPipeExist("ocybedam"))
            {
                int num1 = (int)MessageBox.Show("Script has been already Injected!", "[ERROR] Already Injected", MessageBoxButtons.OK, MessageBoxIcon.Hand);
            }
            else if (!this.CheckDllUpdate() && System.IO.File.Exists("EasyExploitsDLL.dll"))
                this.InjectDLL();
            else if (this.DownloadDLL())
            {
                this.InjectDLL();
            }
            else
            {
                int num2 = (int)MessageBox.Show("Can't download the lastest version!", "[ERROR] Couldn't Update", MessageBoxButtons.OK, MessageBoxIcon.Hand);
            }
        }

        private static bool namedPipeExist(string pipeName)
        {
            try
            {
                if (!Module.WaitNamedPipe(Path.GetFullPath(string.Format("\\\\.\\pipe\\{0}", (object)pipeName)), 0))
                {
                    switch (Marshal.GetLastWin32Error())
                    {
                        case 0:
                            return false;
                        case 2:
                            return false;
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern bool WaitNamedPipe(string name, int timeout);
    }
}
