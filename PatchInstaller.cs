using System.IO;
using System.Windows.Forms;

namespace TGpatch_Installer
{
    class PatchInstaller
    {
        public const string TG_PATCH_FOLDER = "TG-NeoGK";
        public const string TG_PATCH_EXTRAS_FOLDER = "TG-NeoGK/Extra Files";
        public const string NATIVE_BACKUP_FOLDER = "Native (Original)";

        private string wbPath;
        private string wbNativePath;

        public PatchInstaller(string wbPath)
        {
            this.wbPath = wbPath;
            wbNativePath = wbPath + WarbandHelper.WB_NATIVE_DIR;
        }

        public static bool VerifyPatchFiles()
        {
            return Directory.Exists(TG_PATCH_FOLDER);
        }

        public void InstallMainPatch()
        {
            string corePatchFiles = TG_PATCH_FOLDER + "/Native";

            if (!Directory.Exists(corePatchFiles))
                throw new System.Exception("Could not find Native folder inside Patch files.");

            if (!Directory.Exists(wbNativePath))
                throw new System.Exception("Error finding Native module.");

            CopyDirectory(corePatchFiles, wbNativePath, true, true);
        }

        public void BackupNativeFolder()
        {
            string backupFolderPath = wbPath + WarbandHelper.WB_MODULES_DIR + "/" + NATIVE_BACKUP_FOLDER;

            if (Directory.Exists(backupFolderPath))
                return;

            try
            {
                CopyDirectory(wbPath + WarbandHelper.WB_NATIVE_DIR, backupFolderPath, true);
            }
            catch (System.Exception e)
            {
                MessageBox.Show(e.Message,
                    "Could Not Create Backup Folder",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning
                );
            }
        }

        public void InstallFemaleVoices()
        {
            string femaleVoiceFiles = TG_PATCH_EXTRAS_FOLDER + "/extra_voice";
            string soundsFolder = wbNativePath + "/Sounds";

            CopyDirectory(femaleVoiceFiles, soundsFolder, true, true);
        }

        public void InstallBannerpack()
        {
            string bannerpackFiles = TG_PATCH_EXTRAS_FOLDER + "/Neo-GK Bannerpack";
            string texturesFolder = wbNativePath + "/Textures";

            CopyDirectory(bannerpackFiles, texturesFolder, true, true);
        }

        public void InstallCoreShaders()
        {
            string coreShaderFile = TG_PATCH_EXTRAS_FOLDER + "/core_shaders.brf";
            string commonResFolder = wbPath + "/CommonRes";
            string coreShaderFileOriginal = commonResFolder + "/core_shaders.brf";
            string coreShaderBackupFile = commonResFolder + "/core_shaders (Original).brf";

            // Create backup if it doesn't exist already
            if (!File.Exists(coreShaderBackupFile))
            {
                File.Copy(coreShaderFileOriginal, coreShaderBackupFile);
            }

            File.Copy(coreShaderFile, coreShaderFileOriginal, true);
        }

        // Taken from MSDN docs: https://docs.microsoft.com/en-us/dotnet/standard/io/how-to-copy-directories#example
        private static void CopyDirectory(string sourceDir, string destinationDir, bool recursive, bool overwrite = false)
        {
            // Get information about the source directory
            var dir = new DirectoryInfo(sourceDir);

            // Check if the source directory exists
            if (!dir.Exists)
                throw new DirectoryNotFoundException($"Source directory not found: {dir.FullName}");

            // Cache directories before we start copying
            DirectoryInfo[] dirs = dir.GetDirectories();

            // Create the destination directory
            Directory.CreateDirectory(destinationDir);

            // Get the files in the source directory and copy to the destination directory
            foreach (FileInfo file in dir.GetFiles())
            {
                string targetFilePath = Path.Combine(destinationDir, file.Name);
                file.CopyTo(targetFilePath, overwrite);
            }

            // If recursive and copying subdirectories, recursively call this method
            if (recursive)
            {
                foreach (DirectoryInfo subDir in dirs)
                {
                    string newDestinationDir = Path.Combine(destinationDir, subDir.Name);
                    CopyDirectory(subDir.FullName, newDestinationDir, recursive, overwrite);
                }
            }
        }
    }
}
