using mybackuptoftp.Utils;
using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;

namespace mybackuptoftp
{
    /// <summary>
    /// Lógica de interacción para MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public static Frame nframe;
        public static MainWindow mw;
        public static bool isFirtsRun;

        public MainWindow()
        {
            InitializeComponent();
            this.Initialice();
        }

        private void Initialice()
        {
            try
            {
                isFirtsRun = true;
                Process[] listprocess = Process.GetProcessesByName("mybackuptoftp");

                if (listprocess.Length > 1)
                {
                    var count = 1;
                    foreach (Process proceso in listprocess)
                    {
                        if (count < listprocess.Length)
                        {
                            proceso.Kill();
                            count++;
                        }

                    }
                    //MessageBox.Show("MyBackupToFTP ya esta en ejecución, revise la barra de notificaciones.", "Mensaje de ayuda");
                    //Application.Current.Shutdown();
                }

                mw = this;
                nframe = new Frame();
                nframe = NavigationFrame;

                ConfigOptions.PathExe = ConfigOptions.DirectoryPathAseembly() + "\\mybackuptoftp.exe";
                ConfigOptions.PathFile = ConfigOptions.DirectoryPathAseembly() + "\\mybackuptoftp.conf";

                if (File.Exists(ConfigOptions.PathFile))
                {
                    nframe.Navigate(new PageStart(true));
                }
                else
                {
                    nframe.Navigate(new PageConfig());
                }
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message+". Contacte con soporte técnico.", "Mensaje de error");
                Application.Current.Shutdown();
            }
        }
    }
}
