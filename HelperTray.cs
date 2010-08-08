using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.IO;
using System.Windows.Forms;
using System.Drawing;
using Microsoft.Win32;




namespace TheRenamerHelper
{
    class HelperTray
    {
        
        private String pathToMonitor;
        private String pathToRenamer;
        private FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog();
        private FileSystemWatcher folderMonitorWatcher;
        private ContextMenu contextMenu;
        private NotifyIcon notifyIcon;
        private RegistryKey rkApp;
        private Queue fileQueue;

        public HelperTray()
        {
            rkApp = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);
            folderMonitorWatcher = new FileSystemWatcher();
            folderMonitorWatcher.NotifyFilter = NotifyFilters.LastAccess | NotifyFilters.LastWrite | NotifyFilters.FileName | NotifyFilters.DirectoryName;
            folderMonitorWatcher.IncludeSubdirectories = true;
            // Add event handlers.
            folderMonitorWatcher.Created += new FileSystemEventHandler(OnCreated);
           

            pathToMonitor = Properties.Settings.Default.monitorPath;
            pathToRenamer = Properties.Settings.Default.renamerPath;

            if (!pathToMonitor.Equals(String.Empty) && Directory.Exists(pathToMonitor))
            {
                folderMonitorWatcher.Path = pathToMonitor;
                folderMonitorWatcher.EnableRaisingEvents = true;
            }


            contextMenu = new ContextMenu();

            if(rkApp.GetValue("The Renamer Helper")==null)
                contextMenu.MenuItems.Add("Open on Startup", startupToggle);
            else
                contextMenu.MenuItems.Add('\u221a'+ "Open on Startup", startupToggle);
            if(TheRenamerHelper.Properties.Settings.Default.fetchMoviesSet)
                contextMenu.MenuItems.Add('\u221a' + " Fetch Movies", movieToggle);
            else
                contextMenu.MenuItems.Add("Fetch Movies", movieToggle);
            if (TheRenamerHelper.Properties.Settings.Default.fetchTVSet)
                contextMenu.MenuItems.Add('\u221a' + " Fetch TV", movieToggle);
            else
                contextMenu.MenuItems.Add("Fetch TV", tvToggle);
            
            contextMenu.MenuItems.Add("Path To theRenamer...", browseRenamer);
            contextMenu.MenuItems.Add("Folder To Monitor...", browseFolder);
            contextMenu.MenuItems.Add("About...", aboutContext);
            contextMenu.MenuItems.Add("Exit", OnExit);

            notifyIcon = new NotifyIcon();
            notifyIcon.Text = "The Renamer Helper";
            notifyIcon.Icon = TheRenamerHelper.Properties.Resources.app;
            notifyIcon.ContextMenu = contextMenu;
            notifyIcon.Visible = true;

            fileQueue = new Queue();

            checkSetup();
          
        }
        private void aboutContext(object sender, EventArgs e)
        {
            AboutBox a = new AboutBox();
            a.Show();

        }
        private void movieToggle(object sender, EventArgs e)
        {

            if (contextMenu.MenuItems[1].Text.Equals("Fetch Movies"))
            {
                contextMenu.MenuItems[1].Text = '\u221a' + " Fetch Movies";
                TheRenamerHelper.Properties.Settings.Default.fetchMoviesSet = true;


            }
            else if (contextMenu.MenuItems[1].Text.Equals('\u221a' + " Fetch Movies"))
            {
                contextMenu.MenuItems[1].Text = "Fetch Movies";
                TheRenamerHelper.Properties.Settings.Default.fetchMoviesSet = false;
            }


        }
        private void tvToggle(object sender, EventArgs e)
        {

            if (contextMenu.MenuItems[2].Text.Equals("Fetch TV"))
            {
                contextMenu.MenuItems[2].Text = '\u221a' + " Fetch TV";
                TheRenamerHelper.Properties.Settings.Default.fetchTVSet = true;


            }
            else if (contextMenu.MenuItems[2].Text.Equals('\u221a' + " Fetch TV"))
            {
                contextMenu.MenuItems[2].Text = "Fetch TV";
                TheRenamerHelper.Properties.Settings.Default.fetchTVSet = false;
            }


        }
        private void checkSetup()
        {
            int errorCount = 0;
            notifyIcon.BalloonTipText = "";
            if (pathToMonitor.Equals(String.Empty))
            {
                errorCount++;
                notifyIcon.BalloonTipText += "Folder to Monitor is not configured.\n";
            }
            if (pathToRenamer.Equals(String.Empty))
            {
                errorCount++;
                notifyIcon.BalloonTipText += "Path to theRenamer.exe is not configured.\n";
            }
            else if (Directory.GetFiles(pathToRenamer, "theRenamer.exe").Length == 0)
            {
                errorCount++;
                notifyIcon.BalloonTipText += "Path to theRenamer does not contain theRenamer.exe.\n";
            }


            if(errorCount == 0){
                notifyIcon.BalloonTipText = "The Renamer Helper is configured correctly";
                notifyIcon.Icon = TheRenamerHelper.Properties.Resources.app;
            }
            else{
                notifyIcon.Icon = TheRenamerHelper.Properties.Resources.warning;
            }
            notifyIcon.ShowBalloonTip(5000);
        }
        private void startupToggle(object sender, EventArgs e)
        {
            
            if (contextMenu.MenuItems[0].Text.Equals("Open on Startup"))
            {
                contextMenu.MenuItems[0].Text = '\u221a' + " Open on Startup";
                rkApp.SetValue("The Renamer Helper", Application.ExecutablePath.ToString());


            }
            else if (contextMenu.MenuItems[0].Text.Equals('\u221a' + " Open on Startup"))
            {
                contextMenu.MenuItems[0].Text = "Open on Startup";
                rkApp.DeleteValue("The Renamer Helper", false);
            }
              
           
        }
        private void browseRenamer(object sender, EventArgs e)
        {
            if (folderBrowserDialog.ShowDialog() == DialogResult.OK)
            {
                pathToRenamer = folderBrowserDialog.SelectedPath;
                Properties.Settings.Default.renamerPath = pathToRenamer;
                Properties.Settings.Default.Save();
                checkSetup();
            }
        }
        private void browseFolder(object sender, EventArgs e)
        {
            if (folderBrowserDialog.ShowDialog() == DialogResult.OK)
            {
                pathToMonitor = folderBrowserDialog.SelectedPath;
                folderMonitorWatcher.Path = pathToMonitor;
                Properties.Settings.Default.monitorPath = pathToMonitor;
                Properties.Settings.Default.Save();
                folderMonitorWatcher.EnableRaisingEvents = true;
                checkSetup();
            }
        }
        private void OnCreated(object source, FileSystemEventArgs e)
        {
            try
            {
                if (pathToRenamer.Equals(String.Empty))
                {
                    MessageBox.Show("You must provide a path to theRenamer.exe");
                }
                else if (Directory.GetFiles(pathToRenamer, "theRenamer.exe").Length == 0)
                {
                    MessageBox.Show("Selected path to theRenamer does not contain theRenamer.exe");
                }
                else
                {
                   // MessageBox.Show(pathToRenamer + @"\theRenamer.exe");
                    //if its a directory don't go in.. wait for file updates
                   
                    if (File.Exists(e.FullPath))
                    {
                        while (!fileAccessible(e.FullPath)) ;
                        if (TheRenamerHelper.Properties.Settings.Default.fetchMoviesSet)
                            System.Diagnostics.Process.Start(pathToRenamer + @"\theRenamer.exe", "-fetchmovie");
                        if (TheRenamerHelper.Properties.Settings.Default.fetchTVSet)
                            System.Diagnostics.Process.Start(pathToRenamer + @"\theRenamer.exe", "-fetch");
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }
        private bool fileAccessible(string fileName)
        {
            try
            {
                FileStream fs = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.None);
                fs.Close();
                return true;
            }
            catch (Exception e)
            {
                System.Threading.Thread.Sleep(5000);
                return false;

            }

        }
        private void OnExit(object sender, EventArgs e)
        {
            Application.ExitThread();
            Application.Exit();
        }

    }
}
