using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace TGpatch_Installer
{
    public partial class Setup : Form
    {
        private enum Step
        {
            Intro,
            SelectLocation,
            ConfigureOptions,
            Disclaimer,
            Installation,
            Final
        }
        private Step currentStep;

        private int installationSteps = 1;

        public Setup()
        {
            InitializeComponent();
            installationWorker.WorkerReportsProgress = true;
            installationWorker.WorkerSupportsCancellation = true;
        }

        private void Intro_Load(object sender, EventArgs e)
        {
            currentStep = Step.Intro;
            UpdateStep();

            if (!PatchInstaller.VerifyPatchFiles())
            {
                MessageBox.Show("Patch folder could not be found. Make sure to extract all the files from archive or run the installer from archive itself.",
                    "Patch Files Not Found",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
                Application.Exit();
            }

            txtBoxDirWB.Text = WarbandHelper.FindWarbandInstallPath();
        }

        private void btnNext_Click(object sender, EventArgs e)
        {
            bool shouldContinue = BeforeStepUpdate();

            if (shouldContinue)
            {
                currentStep++;
                UpdateStep();

                AfterStepUpdate();
            }
        }

        private void btnBack_Click(object sender, EventArgs e)
        {
            currentStep--;

            UpdateStep();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void btnBrowseWB_Click(object sender, EventArgs e)
        {
            using (var fbd = new FolderBrowserDialog())
            {
                DialogResult result = fbd.ShowDialog();

                if (result == DialogResult.OK && !string.IsNullOrWhiteSpace(fbd.SelectedPath))
                {
                    txtBoxDirWB.Text = fbd.SelectedPath;
                }
            }
        }

        private bool BeforeStepUpdate()
        {
            if (currentStep == Step.SelectLocation)
            {
                if (!WarbandHelper.ValidateWarbandPath(txtBoxDirWB.Text))
                    return false;
            }

            return true;
        }

        private void UpdateStep()
        {
            ShowButtons();
            HidePanels();

            switch (currentStep)
            {
                case Step.Intro:
                    introPanel.Show();
                    btnBack.Hide();
                    btnNext.Focus();
                    break;
                case Step.SelectLocation:
                    selectLocationPanel.Show();
                    btnNext.Focus();
                    break;
                case Step.ConfigureOptions:
                    configureOptionsPanel.Show();
                    btnNext.Focus();
                    break;
                case Step.Disclaimer:
                    disclaimerPanel.Show();
                    btnNext.Text = "Install";
                    break;
                case Step.Installation:
                    installationPanel.Show();
                    btnNext.Text = "Continue";
                    btnNext.Enabled = false;
                    btnBack.Hide();
                    break;
                case Step.Final:
                    finishPanel.Show();
                    btnCancel.Text = "Finish";
                    btnBack.Hide();
                    btnNext.Hide();
                    break;
                default:
                    btnNext.Hide();
                    break;
            }
        }

        private void AfterStepUpdate()
        {
            if (currentStep == Step.Installation)
            {
                // Count installation steps
                installationSteps += createBackupCheckBox.Checked ? 1 : 0;
                installationSteps += extraFemaleVoicesCheckbox.Checked ? 1 : 0;
                installationSteps += extraBannersCheckbox.Checked ? 1 : 0;
                installationSteps += extraCoreShadersCheckbox.Checked ? 1 : 0;

                // Run installation
                installationWorker.RunWorkerAsync();
            }
        }

        private void ShowButtons()
        {
            btnNext.Show();
            btnBack.Show();
            btnNext.Text = "Next >";
        }

        private void HidePanels()
        {
            introPanel.Hide();
            selectLocationPanel.Hide();
            configureOptionsPanel.Hide();
            disclaimerPanel.Hide();
            installationPanel.Hide();
            finishPanel.Hide();
        }

        private void wbBanner_Paint(object sender, PaintEventArgs e)
        {
            ControlPaint.DrawBorder(e.Graphics, wbBanner.ClientRectangle, Color.Black, ButtonBorderStyle.Solid);
        }

        private void installationWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker worker = sender as BackgroundWorker;
            PatchInstaller patchInstaller = new PatchInstaller(txtBoxDirWB.Text);

            int currentStep = 0;

            // Extra - Backup option
            if (createBackupCheckBox.Checked)
            {
                worker.ReportProgress(currentStep++, new string("Backing up Native folder..."));
                patchInstaller.BackupNativeFolder();
            }

            // Main patch installation
            try
            {
                worker.ReportProgress(currentStep++, new string("Installing patch..."));
                patchInstaller.InstallMainPatch();
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.Message,
                    "Error Installing Patch Files",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
                Application.Exit();
            }

            // Extra - Female option
            if (extraFemaleVoicesCheckbox.Checked)
            {
                worker.ReportProgress(currentStep++, new string("Installing female voices..."));
                patchInstaller.InstallFemaleVoices();
            }

            // Extra - Bannerpack option
            if (extraBannersCheckbox.Checked)
            {
                worker.ReportProgress(currentStep++, new string("Installing bannerpack..."));
                patchInstaller.InstallBannerpack();
            }

            // Extra - core_shaders.brf option
            if (extraCoreShadersCheckbox.Checked)
            {
                worker.ReportProgress(currentStep++, new string("Installing core_shaders.brf..."));
                patchInstaller.InstallCoreShaders();
            }

            // Finished
            worker.ReportProgress(currentStep, new string("Installation finished."));
        }

        private void installationWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            installationCurrentStepLabel.Text = (string)e.UserState;
            installationProgressBar.Value = (int)((float)e.ProgressPercentage / installationSteps * 100);

            if (e.ProgressPercentage == installationSteps)
            {
                btnNext.Enabled = true;
            }
        }

        public void OpenUrl(string url)
        {
            Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
        }

        private void trollGame_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            OpenUrl("https://troll-game.org/");
        }

        private void author_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            OpenUrl("https://community.troll-game.org/user/10836-lamb/");
        }

        private void patch_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            OpenUrl("https://community.troll-game.org/files/file/7-trollgame-neogk/");
        }

        private void tgForumsLink_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            OpenUrl("https://community.troll-game.org/");
        }

        private void tgDiscordLink_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            OpenUrl("https://discord.com/invite/qaFUWAU");
        }

        private void tgSupportLink_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            OpenUrl("https://www.buymeacoffee.com/trollgame");
        }
    }
}
