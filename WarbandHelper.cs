using Microsoft.Win32;
using System;
using System.IO;
using System.Windows.Forms;

namespace TGpatch_Installer
{
    class WarbandHelper
    {
        public const string STEAM32_REGISTRY = "SOFTWARE\\Valve\\Steam";
        public const string STEAM64_REGISTRY = "SOFTWARE\\Wow6432Node\\Valve\\Steam";

        public const string WB_STEAMID = "48700";
        public const string WB_EXE_NAME = "mb_warband.exe";
        public const string WB_MODULES_DIR = "/Modules";
        public const string WB_NATIVE_DIR = WB_MODULES_DIR + "/Native";

        public static bool ValidateWarbandPath(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                MessageBox.Show("Please specify Warband installation directory.",
                    "Error Invalid Directory",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning
                );
                return false;
            }

            if (!Directory.Exists(path))
            {
                MessageBox.Show("Could not find specified directory. Please try again.",
                    "Error Invalid Directory",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning
                );
                return false;
            }

            if (!File.Exists(path + "/" + WB_EXE_NAME))
            {
                MessageBox.Show("Could not find warband executable in selected location. Please try again.",
                    "Error Locating Warband",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning
                );
                return false;
            }

            if (!Directory.Exists(path + WB_NATIVE_DIR))
            {
                MessageBox.Show("Native module is missing from your Warband directory. Stopping installation.",
                    "Error Locating Native Module",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
                Application.Exit();
            }

            return true;
        }

        public static string FindWarbandInstallPath()
        {
            string wbLibraryPath = GetSteamGameLibraryPath(WB_STEAMID);

            if (string.IsNullOrEmpty(wbLibraryPath))
                return null;
            else
                wbLibraryPath = wbLibraryPath.Replace("\\\\", "/");

            if (string.IsNullOrEmpty(wbLibraryPath))
                return null;

            string wbAppManifestFile = wbLibraryPath + "/steamapps/appmanifest_" + WB_STEAMID + ".acf";
            if (!File.Exists(wbAppManifestFile))
                return null;

            string[] appManifestLines = File.ReadAllLines(wbAppManifestFile);
            foreach (var item in appManifestLines)
            {
                string[] parameters = item.Split(new char[] { '\t', '"' }, StringSplitOptions.RemoveEmptyEntries);
                if (parameters.Length > 1)
                {
                    if (parameters[0] == "installdir")
                    {
                        return wbLibraryPath + "/steamapps/common/" + parameters[1];
                    }
                }
            }

            return null;
        }

        private static string GetSteamGameLibraryPath(string gameSteamID)
        {
            string steamInstallPath = GetSteamInstallPath();

            if (string.IsNullOrEmpty(steamInstallPath))
                return null;

            string configFile = steamInstallPath + "/steamapps/libraryfolders.vdf";
            if (File.Exists(configFile))
            {
                string[] configLines = File.ReadAllLines(configFile);

                string currentLibraryPath = "";
                foreach (var item in configLines)
                {
                    string[] parameters = item.Split(new char[] { '\t', '"' }, StringSplitOptions.RemoveEmptyEntries);
                    if (parameters.Length > 1)
                    {
                        if (parameters[0] == "path")
                        {
                            currentLibraryPath = parameters[1];
                        }
                        else if (parameters[0] == gameSteamID)
                        {
                            return currentLibraryPath;
                        }
                    }
                }
            }

            return null;
        }

        private static string GetSteamInstallPath()
        {
            // x86
            string steamInstallPath = getProgramInstallPath(STEAM32_REGISTRY);

            // x64
            if (string.IsNullOrEmpty(steamInstallPath))
                steamInstallPath = getProgramInstallPath(STEAM64_REGISTRY);

            if (string.IsNullOrEmpty(steamInstallPath))
                return null;

            return steamInstallPath;
        }

        private static string getProgramInstallPath(string key)
        {
            try
            {
                using (RegistryKey subKey = Registry.LocalMachine.OpenSubKey(key, false))
                {
                    return subKey.GetValue("InstallPath").ToString();
                }
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}
