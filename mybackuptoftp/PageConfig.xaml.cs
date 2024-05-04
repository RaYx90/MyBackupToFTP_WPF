using mybackuptoftp.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;

namespace mybackuptoftp
{
    public partial class PageConfig : Page
    {
        /// <summary>
        /// declaración de variables
        /// </summary>
        //private string pathConfig;
        private List<string> listItems { get; set; }
        private NotifyIcon existNotifyIcon;
        public static bool showWindowPassword;
        public static bool continuePassword;
        public static PageConfig pageConfig;

        /// <summary>
        /// Constructor de la clase
        /// </summary>
        public PageConfig(NotifyIcon notifyIcon = null)
        {
            InitializeComponent();
            this.lbType.SelectedIndex = 0;
            this.btInit.Click += BtInit_Click;
            this.btCancel.Click += BtCancel_Click;
            //this.btUnlock.Click += BtUnlock_Click;
            this.CheckIfExistFileConfig();
            this.existNotifyIcon = notifyIcon;
            showWindowPassword = true;
            continuePassword = false;
            pageConfig = this;
        }

        /// <summary>
        /// funcion para desbloquear los valores
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /*private void BtUnlock_Click(object sender, RoutedEventArgs e)
        {
            this.tbClient.IsEnabled = true;
            this.tbHour.IsEnabled = true;
            this.lbType.IsEnabled = true;
            this.tbEmail.IsEnabled = true;
            this.tbHost.IsEnabled = true;
            this.tbPort.IsEnabled = true;
            this.tbUserDB.IsEnabled = true;
            this.pbPasswordDB.IsEnabled = true;
            this.tbDB.IsEnabled = true;
            this.tbUserFTP.IsEnabled = true;
            this.pbPasswordFTP.IsEnabled = true;
            this.cbCopyAuto.IsEnabled = true;
        }*/

        /// <summary>
        /// Funcion para verificar que existe el archivo de configuración
        /// </summary>
        private void CheckIfExistFileConfig()
        {
            try
            {
                if (File.Exists(ConfigOptions.PathFile))
                {
                    ConfigOptions.ReadConfigFile();

                    if (ConfigOptions.NameClient != null)
                    {
                        this.tbClient.Text = ConfigOptions.NameClient;
                    }

                    
                    if (ConfigOptions.HourBackup != null)
                    {
                        this.tbHour.Text = ConfigOptions.HourBackup;
                    }

                    
                    if (ConfigOptions.TypeBackup != null)
                    {
                        if (ConfigOptions.TypeBackup.Equals("0"))
                        {
                            this.lbType.SelectedIndex = 0;
                        }
                        else
                        {
                            this.lbType.SelectedIndex = 1;
                        }
                    }

                     
                    if (ConfigOptions.AutoBackup != null)
                    {
                        if (ConfigOptions.AutoBackup.Equals("0"))
                        {
                            this.cbCopyAuto.IsChecked = false;
                        }
                        else
                        {
                            this.cbCopyAuto.IsChecked = true;
                        }
                    }

                    
                    if (ConfigOptions.HelpWindows != null)
                    {
                        
                        if (ConfigOptions.HelpWindows.Equals("0"))
                        {
                            this.cbHelpWindows.IsChecked = false;
                        }
                        else
                        {
                            this.cbHelpWindows.IsChecked = true;
                        }
                    }

                    if (ConfigOptions.EmailClient != null)
                    {
                        this.tbEmail.Text = ConfigOptions.EmailClient;
                    }

                     
                    if (ConfigOptions.HostDB != null)
                    {
                        this.tbHost.Text = ConfigOptions.HostDB;
                    }

                    //this.tbHost.Text =ConfigOptions.HostDB;

                    
                    if (ConfigOptions.PortDB != null)
                    {
                        this.tbPort.Text = ConfigOptions.PortDB;
                    }

                    
                    if (ConfigOptions.PassDB != null)
                    {
                        this.tbUserDB.Text = ConfigOptions.UserDB;
                    }

                    
                    if (ConfigOptions.PassDB != null)
                    {
                        this.pbPasswordDB.Password = ConfigOptions.PassDB;
                    }

                    if (ConfigOptions.ServerFTP != null)
                    {
                        this.tbServerFTP.Text = ConfigOptions.ServerFTP;
                    }

                    if (ConfigOptions.UserFTP != null)
                    {
                        this.tbUserFTP.Text = ConfigOptions.UserFTP;
                    }

                    
                    if (ConfigOptions.PassFTP != null)
                    {
                        this.pbPasswordFTP.Password = ConfigOptions.PassFTP;
                    }

                    //this.tbPort.Text = ConfigOptions.PortDB;
                    //this.tbUserDB.Text = ConfigOptions.UserDB;
                    //this.pbPasswordDB.Password = ConfigOptions.PassDB;
                    //this.tbUserFTP.Text = ConfigOptions.UserFTP;
                    //this.pbPasswordFTP.Password = ConfigOptions.PassFTP;

                    
                    if (ConfigOptions.ListNamesDB.Count > 0)
                    {
                        this.tbDB.Text = "";

                        foreach (var db in ConfigOptions.ListNamesDB)
                        {
                            this.tbDB.Text += db + ";";
                        }
                    }


                    //this.btUnlock.Visibility = Visibility.Visible;
                }

            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                throw;
            }
        }

        /// <summary>
        /// Funcion para crear el listado de items
        /// </summary>
        private void CreateListItems()
        {
            try
            {
                ConfigOptions.NameClient = this.tbClient.Text;

                if (ValidateData.verifyHour(this.tbHour.Text))
                {
                    ConfigOptions.HourBackup = this.tbHour.Text;
                }
                else
                {
                    this.tbHour.Text = "00:00";
                    throw new Exception("La hora del backup no tiene un formato válido.");
                }

                if (ValidateData.verifyEmail(this.tbEmail.Text))
                {
                    ConfigOptions.EmailClient = this.tbEmail.Text;
                }
                else
                {
                    this.tbEmail.Text = "";
                    throw new Exception("La cuenta de email no tiene un formato válido.");
                }

                ConfigOptions.HostDB = this.tbHost.Text;
                ConfigOptions.PortDB = this.tbPort.Text;
                ConfigOptions.UserDB = this.tbUserDB.Text;
                if (string.IsNullOrWhiteSpace(this.pbPasswordDB.Password))
                {
                    ConfigOptions.PassDB = "NOPASSWORD";
                }
                else
                {
                    ConfigOptions.PassDB = this.pbPasswordDB.Password;
                }
                ConfigOptions.ListNamesDB.Clear();
                var splitDBS = this.tbDB.Text.Split(';');
                foreach (var db in splitDBS)
                {
                    if (!string.IsNullOrWhiteSpace(db))
                    {
                        
                        //Si el nombre de la base de datos tiene un espacio en blanco este, se elimina
                        if (db.Contains(" "))
                        {
                            ConfigOptions.ListNamesDB.Add(db.Replace(" ", ""));
                        }
                        else
                        {
                            ConfigOptions.ListNamesDB.Add(db);
                        }
                        //ConfigOptions.ListNamesDB.Add(db);
                    }
                }
                ConfigOptions.ServerFTP = this.tbServerFTP.Text;
                ConfigOptions.UserFTP = this.tbUserFTP.Text;
                ConfigOptions.PassFTP = this.pbPasswordFTP.Password;

                if (this.lbType.SelectedIndex == 0)
                {
                    ConfigOptions.TypeBackup = "0";
                }
                else
                {
                    ConfigOptions.TypeBackup = "1";
                }

                if (this.cbCopyAuto.IsChecked == true)
                {
                    ConfigOptions.AutoBackup = "1";
                }
                else
                {
                    ConfigOptions.AutoBackup = "0";
                }

                
                if (this.cbHelpWindows.IsChecked == true)
                {
                    ConfigOptions.HelpWindows = "1";
                }
                else
                {
                    ConfigOptions.HelpWindows = "0";
                }

                this.listItems = new List<string>
                {
                    ConfigOptions.NameClient,
                    ConfigOptions.HourBackup,
                    ConfigOptions.TypeBackup,
                    ConfigOptions.EmailClient,
                    ConfigOptions.HostDB,
                    ConfigOptions.PortDB,
                    ConfigOptions.UserDB,
                    ConfigOptions.PassDB,
                    //ConfigOptions.NameDB,
                    this.tbDB.Text,
                    ConfigOptions.ServerFTP,
                    ConfigOptions.UserFTP,
                    ConfigOptions.PassFTP,
                    ConfigOptions.AutoBackup,
                    
                    ConfigOptions.HelpWindows
                };
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        /// <summary>
        /// Funcion para comprobar que el listado tiene todos los campos
        /// </summary>
        /// <returns></returns>
        private bool CheckListContent()
        {
            try
            {
                bool continueProcess = true;
                foreach (string item in this.listItems)
                {
                    if (string.IsNullOrWhiteSpace(item))
                    {
                        continueProcess = false;
                    }
                }
                return continueProcess;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                throw;
            }
        }

        /// <summary>
        /// Cancela la ejecucion de la app
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtCancel_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Application.Current.Shutdown();
        }

        /// <summary>
        /// evento para ir al siguiente page
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void BtInit_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                this.CreateListItems();

                if (CheckListContent())
                {
                    MainWindow.mw.WindowStyle = WindowStyle.None;
                    this.IsEnabled = false;
                    if (showWindowPassword)
                    {
                        showWindowPassword = false;
                        WindowPassword wp = new WindowPassword(this);
                        wp.Show();
                    }
                    else
                    {
                        if (continuePassword)
                        {
                            ConfigOptions.CreateConfigFile();

                            if (this.existNotifyIcon != null)
                            {
                                this.existNotifyIcon.Dispose();
                            }
                            this.IsEnabled = true;
                            MainWindow.mw.WindowStyle = WindowStyle.SingleBorderWindow;
                            showWindowPassword = true;
                            MainWindow.nframe.Navigate(new PageStart());
                        }
                    }
                }
                else
                {
                    System.Windows.MessageBox.Show("Debe rellenar todos los campos para continuar", "Mensaje de ayuda");
                    this.IsEnabled = true;
                    MainWindow.mw.WindowStyle = WindowStyle.SingleBorderWindow;
                    showWindowPassword = true;
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show("Ha ocurrido el siguiente error: " + ex.Message, "Mensaje de error");
                this.IsEnabled = true;
                MainWindow.mw.WindowStyle = WindowStyle.SingleBorderWindow;
                showWindowPassword = true;
            }
        }
    }
}