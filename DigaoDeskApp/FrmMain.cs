﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DigaoDeskApp
{
    public partial class FrmMain : Form
    {

        private int? _lastTrayIndex = null;
        private WindowsPowerCtrl _powerCtrl = new();

        public FrmMain()
        {
            InitializeComponent();

            LoadLang();

            this.ShowInTaskbar = false;
            this.Opacity = 0;
        }

        private void FrmMain_Shown(object sender, EventArgs e)
        {
            this.Hide();
        }

        private void FrmMain_Load(object sender, EventArgs e)
        {
            ApplicationsStore.LoadApplications();
            UpdateTrayIcon();

            _powerCtrl.Init();
            GitHubUpdater.RunTask();
            Survey.Check();
        }

        private void FrmMain_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (Vars.AppList.Any(x => x.Running))
            {
                e.Cancel = true;
                Messages.Error(Vars.Lang.DenyExitByRunningApp);
            }

            if (Application.OpenForms.Cast<Form>().Any(x => x.Modal))
            {
                e.Cancel = true;
                Messages.Error(Vars.Lang.DenyExitByModal);
            }
        }

        private void FrmMain_FormClosed(object sender, FormClosedEventArgs e)
        {
            //force forms close before application terminate, otherwise close event of forms isn't triggered, so customizations are not saved.
            if (Vars.FrmAppsObj != null) Vars.FrmAppsObj.Close();
            if (Vars.FrmReposObj != null) Vars.FrmReposObj.Close();

            GitHubUpdater.CheckForRunTmpUpdateExeOnClosing();

            _powerCtrl.Release();

            EventAudit.Do("Program finished");
        }

        private void LoadLang()
        {
            miVersion.Text = string.Format(Vars.Lang.MenuVersion, Vars.APP_VERSION);
            miDonate.Text = Vars.Lang.MenuDonate;
            miApplications.Text = Vars.Lang.MenuApplications;
            miRepos.Text = Vars.Lang.MenuGitRepositories;
            miConfig.Text = Vars.Lang.MenuSettings;
            miExit.Text = Vars.Lang.MenuExit;
        }

        private void ShowForm<T>(ref T f) where T : Form
        {
            if (f == null) f = (T)Activator.CreateInstance(typeof(T));
            f.Show();
            f.Restore();
            f.BringToFront();
        }

        private void miConfig_Click(object sender, EventArgs e)
        {
            ShowForm(ref Vars.FrmConfigObj);
        }

        private void miApplications_Click(object sender, EventArgs e)
        {
            ShowForm(ref Vars.FrmAppsObj);
        }

        private void miRepos_Click(object sender, EventArgs e)
        {
            ShowForm(ref Vars.FrmReposObj);
        }

        private void miExit_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        public void UpdateTrayIcon()
        {
            var appsRunning = Vars.AppList.Where(x => x.Running);

            int idx = 0;
            if (appsRunning.Any())
            {
                idx = 1;

                var appsRunningWithTcpPort = appsRunning.Where(x => x.TcpPort.HasValue);
                if (appsRunningWithTcpPort.Any() && !appsRunningWithTcpPort.All(x => x.TcpOnline))
                {
                    idx = 2;
                }
            }

            if (idx != _lastTrayIndex)
            {
                EventAudit.Do("Program icon changed to " + idx);

                this.BeginInvoke(new MethodInvoker(() => //using invoke because could be called by thread
                {
                    tray.Icon = Icon.FromHandle((images.Images[idx] as Bitmap).GetHicon());
                    _lastTrayIndex = idx;
                }));
            }
        }

        private void miDigaoDesk_Click(object sender, EventArgs e)
        {
            Utils.Navigate(Vars.GITHUB_LINK);
        }

        private void miVersion_Click(object sender, EventArgs e)
        {
            Utils.Navigate(Vars.GITHUB_LINK + "/releases");
        }

        private void miDonate_Click(object sender, EventArgs e)
        {
            Utils.Navigate(Vars.DIGAODALPIAZ_DONATE_LINK);
        }

        private void timerApps_Tick(object sender, EventArgs e)
        {
            if (Vars.IsSuspended) return;
            timerApps.Enabled = false;

            new Task(() =>
            {
                List<Task> tasks = new();

                foreach (var app in Vars.AppList.Where(x => x.Running && x.TcpPort.HasValue))
                {
                    Task t = new(() =>
                    {
                        if (app.CheckWebPort()) UpdateTrayIcon();
                    });
                    tasks.Add(t);
                    t.Start();
                }

                Task.WaitAll(tasks.ToArray());

                this.BeginInvoke(new MethodInvoker(() =>
                {
                    timerApps.Enabled = true;
                }));

            }).Start();
        }

    }
}
