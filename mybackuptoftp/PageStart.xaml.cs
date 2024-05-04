using Ionic.Zip;
using Ionic.Zlib;
using MySql.Data.MySqlClient;
using mybackuptoftp.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Threading;

namespace mybackuptoftp
{
    public partial class PageStart : Page
    {
        /// <summary>
        /// declaración de variables
        /// </summary>
        /// 
        private bool shutdownForce;
        public static Thread th1;
        private System.Threading.Timer timer;
        private bool isCancelCopy;
        private string bodyEmail;
        private NotifyIcon notifyIcon;
        private bool showPage;
        private bool copyInProcess;
        string[] hourAndMinutes;

        public PageStart(bool showPage = false)
        {
            InitializeComponent();
            this.shutdownForce = false;
            this.isCancelCopy = false;
            this.copyInProcess = false;
            this.bodyEmail = "";
            this.notifyIcon = new NotifyIcon();
            this.InitNotifyIcon();
            MainWindow.mw.Closing += Mw_Closing;
            this.btInit.Click += BtInit_Click;
            this.btModifyConf.Click += BtModifyConf_Click;
            this.btCancel.Click += BtCancel_Click;
            this.showPage = showPage;
            this.CheckIfExistFileConfig();
        }

        /// <summary>
        /// Funcion para inicializar el icono en la barra de notificaciones
        /// </summary>
        private void InitNotifyIcon()
        {
            try
            {
                this.notifyIcon.Icon = new System.Drawing.Icon(System.Windows.Application.GetResourceStream(new Uri("Images/Icon.ico", UriKind.Relative)).Stream);
                this.notifyIcon.Visible = true;
                this.notifyIcon.BalloonTipIcon = System.Windows.Forms.ToolTipIcon.Info;
                this.notifyIcon.Text = "MyBackupToFTP";
                this.notifyIcon.DoubleClick += NotifyIcon_DoubleClick;
                this.notifyIcon.Text = "Presione doble click para Mostrar";
                System.Windows.Forms.ContextMenu menu = new System.Windows.Forms.ContextMenu();
                System.Windows.Forms.MenuItem item1 = new System.Windows.Forms.MenuItem("Mostrar");
                item1.Click += new EventHandler(item1_Click);
                System.Windows.Forms.MenuItem item2 = new System.Windows.Forms.MenuItem("Cerrar");
                item2.Click += new EventHandler(item2_Click);
                menu.MenuItems.Add(item1);
                menu.MenuItems.Add(item2);
                this.notifyIcon.ContextMenu = menu;
            }
            catch(Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }

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

                    this.tbNameClient.Text = ConfigOptions.NameClient;
                    this.tbEmail.Text = ConfigOptions.EmailClient;
                    this.tbHour.Text = ConfigOptions.HourBackup;

                    
                    if(ConfigOptions.TypeBackup != null)
                    {
                        if (ConfigOptions.TypeBackup.Equals("0"))
                        {
                            this.tbType.Text = "Semanal";
                        }
                        else
                        {
                            this.tbType.Text = "Mensual";
                        }
                    }

                    if(this.showPage)
                    {
                        MainWindow.mw.Hide();
                    }

                    //no habrá que hacerlo si esta checkeada la ventana de ocultar avisos
                    string valueKey = RegistryEntry.CheckInStart("MyBackupToFTP");

                    if (string.IsNullOrWhiteSpace(valueKey))
                    {
                        if (!RegistryEntry.AddInStartup("MyBackupToFTP", ConfigOptions.PathExe) && MainWindow.isFirtsRun)
                        {
                            MainWindow.isFirtsRun = false;
                            System.Windows.MessageBox.Show("Debe ejecutar el MyBackupToFTP como administrador para que se agregue al inicio de Windows", "Mensaje de ayuda");
                        }
                    }
                    else
                    {
                        if (!ConfigOptions.PathExe.Equals(valueKey))
                        {
                            if (RegistryEntry.RemoveFromStartup("MyBackupToFTP"))
                            {
                                RegistryEntry.AddInStartup("MyBackupToFTP", ConfigOptions.PathExe);
                            }
                            else 
                            {
                                bool showMessage = false;
                                
                                if(ConfigOptions.HelpWindows != null)
                                {
                                    
                                    if (ConfigOptions.HelpWindows.Equals("1"))
                                    {
                                        showMessage = true;                                        
                                    }
                                }
                                else
                                {
                                    showMessage = true;
                                }

                                
                                if(showMessage)
                                {
                                    MainWindow.isFirtsRun = false;
                                    System.Windows.MessageBox.Show("Debe ejecutar el MyBackupToFTP como administrador para que se agregue al inicio de Windows", "Mensaje de ayuda");
                                }
                            }
                        }
                    }

                    
                    if(ConfigOptions.AutoBackup != null)
                    {
                        if (ConfigOptions.AutoBackup.Equals("1"))
                        {
                            this.hourAndMinutes = ConfigOptions.HourBackup.Split(':');
                            DateTime dateExec = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, Convert.ToInt32(this.hourAndMinutes[0]), Convert.ToInt32(this.hourAndMinutes[1]), 00);
                            this.SetUpTimer(dateExec, OpenProcess);
                        }
                    }
                }
                else
                {
                    System.Windows.MessageBox.Show("No se ha encontrado el fichero de configuración.", "Mensaje de ayuda");
                    MainWindow.nframe.NavigationUIVisibility = System.Windows.Navigation.NavigationUIVisibility.Hidden;
                    MainWindow.nframe.Navigate(new PageConfig());
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show("Ha ocurrido el siguiente error:" + ex.Message, "Mensaje de error");
                System.Windows.Application.Current.Shutdown();
            }
        }

        /// <summary>
        /// Configura la ejecucion del timer
        /// </summary>
        /// <param name="alertTime"></param>
        /// <param name="delg"></param>
        private void SetUpTimer(DateTime alertTime, Action<TimeSpan> delg)
        {

            TimeSpan timeToGo = alertTime - DateTime.Now;

            if (timeToGo < TimeSpan.Zero)
            {
                return;//time already passed
            }
            this.timer = new System.Threading.Timer(x =>
            {
                delg(alertTime.TimeOfDay);
            }, null, timeToGo, Timeout.InfiniteTimeSpan);
        }

        /// <summary>
        /// Lanza el hilo y reprograma el timer
        /// </summary>
        /// <param name="alertTime"></param>
        public void OpenProcess(TimeSpan alertTime)
        {
            this.ExecuteThread();
            DateTime dateExec = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day + 1, Convert.ToInt32(this.hourAndMinutes[0]), Convert.ToInt32(this.hourAndMinutes[1]), 00);
            this.SetUpTimer(dateExec, OpenProcess);
        }

        /// <summary>
        /// Evento del boton que inicia el proceso de copia manualmente
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtInit_Click(object sender, RoutedEventArgs e)
        {
            this.ExecuteThread();
        }   

        /// <summary>
        /// Ejecuta automaticamente la copia
        /// </summary>
        private void ExecuteThread()
        {
            this.bodyEmail = "";
            th1 = new Thread(this.BackupAndUploadToFtp);
            th1.Start();
        }

        /// <summary>
        /// funcion intermediaria entre el hilo y los procesos que iremos ejecutando hasta subir el backup
        /// </summary>
        private async void BackupAndUploadToFtp()
        {
            try
            {
                this.copyInProcess = true;
                await this.BackupBD();
                this.CompressBackup();
                this.CreateFTPDirectory();
                this.UploadToFtp();
                this.DeleteBackup();
                await ModifyComponents();
            }
            catch(Exception ex)
            {
                if (!this.isCancelCopy)
                {
                    await System.Windows.Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() =>
                    {
                        this.pbProgress.Value = 0;
                        this.notifyIcon.Text = "Error durante proceso de la copia. Doble click para Mostrar";
                        this.tbProgress.Text = "Error durante proceso de copia de seguridad";
                        this.bodyEmail += DateTime.Now.ToLongTimeString()+" - Error durante proceso de copia de seguridad: "+ex.Message;
                        this.btInit.IsEnabled = true;
                        this.btCancel.Visibility = Visibility.Hidden;
                        this.btModifyConf.IsEnabled = true;
                        System.Windows.MessageBox.Show(ex.Message, "Mensaje de error");
                    }));

                    if (th1.IsAlive && th1.ThreadState == System.Threading.ThreadState.Running)
                    {
                        th1.Interrupt();
                        th1.Abort();
                    }
                }
            }
            finally
            {
                try
                {
                    ClientSmtp.SendEmail(this.bodyEmail, ConfigOptions.EmailClient, ConfigOptions.NameClient);
                    this.copyInProcess = false;
                }
                catch{ }
            }
        }

        /// <summary>
        /// Funcion para realizar un backup por mysqldump de la base de datos
        /// </summary>
        private async Task BackupBD()
        {
            try
            {
                await System.Windows.Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() =>
                {
                    this.btModifyConf.IsEnabled = false;
                    this.btCancel.Visibility = Visibility.Visible;
                    this.btInit.IsEnabled = false;
                    this.notifyIcon.Text = "Realizando copia de seguridad (25%). Doble click para mostrar";
                    this.tbProgress.Text = "Realizando copia de seguridad de la base de datos (25%)";
                    this.bodyEmail += DateTime.Now.ToLongTimeString() + " - Realizando copia de seguridad de la base de datos. \n";
                    this.pbProgress.Value = 25;
                }));

                string constring = "";
                string mysqldump_directory = AppDomain.CurrentDomain.BaseDirectory;

                foreach (var db in ConfigOptions.ListNamesDB)
                {
                    string file = db + ".sql";

                    if (ConfigOptions.PassDB.Equals("NOPASSWORD"))
                    {
                        constring = "mysqldump.exe -u " + ConfigOptions.UserDB + " -h " + ConfigOptions.HostDB + " -P " + ConfigOptions.PortDB + " " + db + "--max_allowed_packet=512M > " + file;
                    }
                    else
                    {
                        constring = "mysqldump.exe -u " + ConfigOptions.UserDB + " -p" + ConfigOptions.PassDB + " -h " + ConfigOptions.HostDB + " -P " + ConfigOptions.PortDB + " " + db +"--max_allowed_packet=512M > " + file;
                    }

                     await this.ExecuteCommand(constring, mysqldump_directory);
                }

                


            }
            catch(Exception ex)
            {
                Debug.WriteLine(ex.Message);
                throw;
            }
        }

        /// <summary>
        /// Funcion para ejecutar comandos de consola
        /// </summary>
        /// <param name="_Command"></param>
        /// <param name="working_dir"></param>
        private async Task ExecuteCommand(string command, string working_dir = null)
        {
            try
            {
                //Indicamos que deseamos inicializar el proceso cmd.exe junto a un comando de arranque. 
                //(/C, le indicamos al proceso cmd que deseamos que cuando termine la tarea asignada se cierre el proceso).
                //Para mas informacion consulte la ayuda de la consola con cmd.exe /? 
                ProcessStartInfo procStartInfo = new ProcessStartInfo("cmd", "/c " + command);
                // Indicamos que la salida del proceso se redireccione en un Stream
                procStartInfo.RedirectStandardOutput = true;
                procStartInfo.UseShellExecute = false;
                //Indica que el proceso no despliegue una pantalla negra (El proceso se ejecuta en background)
                procStartInfo.CreateNoWindow = true;

                //Inicializa el proceso
                System.Diagnostics.Process proc = new System.Diagnostics.Process();

                if (!string.IsNullOrEmpty(working_dir))
                {
                    procStartInfo.WorkingDirectory = working_dir;
                }

                proc.StartInfo = procStartInfo;
                proc.Start();
                proc.WaitForExit();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                throw;
            }
        }

        /// <summary>
        /// Funcion para comprimir la copia de seguridad
        /// </summary>
        private void CompressBackup()
        {
            try
            {
                System.Windows.Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() =>
                {
                    this.btModifyConf.IsEnabled = false;
                    this.btCancel.Visibility = Visibility.Visible;
                    this.btInit.IsEnabled = false;
                    this.notifyIcon.Text = "Comprimiendo copia de seguridad (50%). Doble click para mostrar";
                    this.tbProgress.Text = "Comprimiendo copia de seguridad de la base de datos (50%)";
                    this.bodyEmail += DateTime.Now.ToLongTimeString() + " - Comprimiendo copia de seguridad de la base de datos. \n";
                    this.pbProgress.Value = 50;
                }));

                foreach (var db in ConfigOptions.ListNamesDB)
                {
                    using (ZipFile zip = new ZipFile())
                    {
                        zip.CompressionLevel = CompressionLevel.BestCompression;
                        zip.UseZip64WhenSaving = Zip64Option.Always;
                        zip.Password = "MyPassword";
                        zip.BufferSize = 65536 * 8; // Set the buffersize to 512k for better efficiency with large files
                        zip.AddFile(ConfigOptions.DirectoryPathAseembly() + "\\" + db + ".sql", "");
                        zip.Save(ConfigOptions.DirectoryPathAseembly() + "\\" + db + ".zip");
                    }
                }
            }
            catch(Exception ex)
            {
                Debug.WriteLine(ex);
                throw;
            }
        }

        /// <summary>
        /// Crea un directorio ftp en caso de no existir
        /// </summary>
        private void CreateFTPDirectory()
        {
            try
            {
                System.Windows.Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() =>
                {
                    this.btModifyConf.IsEnabled = false;
                    this.btCancel.Visibility = Visibility.Visible;
                    this.btInit.IsEnabled = false;
                    this.tbProgress.Text = "Comprobando directorios en la nube (75%)";
                    this.bodyEmail += DateTime.Now.ToLongTimeString() + " - Comprobando directorios en la nube. \n";
                    this.pbProgress.Value = 75;
                }));


                try
                {

                    //create the directory
                    FtpWebRequest requestDir = (FtpWebRequest)WebRequest.Create(new Uri(ConfigOptions.ServerFTP));
                    requestDir.Method = WebRequestMethods.Ftp.MakeDirectory;
                    requestDir.Credentials = new NetworkCredential(ConfigOptions.UserFTP, ConfigOptions.PassFTP);
                    requestDir.UsePassive = true;
                    requestDir.UseBinary = true;
                    requestDir.KeepAlive = false;
                    FtpWebResponse response = (FtpWebResponse)requestDir.GetResponse();
                    Stream ftpStream = response.GetResponseStream();
                    ftpStream.Close();
                    response.Close();
                }
                catch {}

                try
                {
                    //create the directory
                    FtpWebRequest requestDir = (FtpWebRequest)FtpWebRequest.Create(new Uri(ConfigOptions.ServerFTP));
                    requestDir.Method = WebRequestMethods.Ftp.MakeDirectory;
                    requestDir.Credentials = new NetworkCredential(ConfigOptions.UserFTP, ConfigOptions.PassFTP);
                    requestDir.UsePassive = true;
                    requestDir.UseBinary = true;
                    requestDir.KeepAlive = false;
                    FtpWebResponse response = (FtpWebResponse)requestDir.GetResponse();
                    Stream ftpStream = response.GetResponseStream();

                    ftpStream.Close();
                    response.Close();
                }
                catch { }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                throw;
            }
        }

        /// <summary>
        /// Subimos el archivo por FTP
        /// </summary>
        /// <param name="strServer"></param>
        private void UploadToFtp()
        {
            try
            {
                System.Windows.Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() =>
                {
                    this.btModifyConf.IsEnabled = false;
                    this.btCancel.Visibility = Visibility.Visible;
                    this.btInit.IsEnabled = false;
                    this.notifyIcon.Text = "Subiendo copia a la nube (80%). Doble click para Mostrar";
                    this.tbProgress.Text = "Subiendo copia de seguridad a la nube (80%)";
                    this.bodyEmail += DateTime.Now.ToLongTimeString() + " - Subiendo copia de seguridad a la nube. \n";
                    this.pbProgress.Value = 80;
                }));

                foreach (var db in ConfigOptions.ListNamesDB)
                {
                    FtpWebRequest ftpRequest;
                    // Crea el objeto de conexión del servidor FTP
                    ftpRequest = (FtpWebRequest)WebRequest.Create(ConfigOptions.ServerFTP + "/" + db + ".zip");
                    // Asigna las credenciales
                    ftpRequest.Credentials = new NetworkCredential(ConfigOptions.UserFTP, ConfigOptions.PassFTP);
                    // Asigna las propiedades
                    ftpRequest.Method = WebRequestMethods.Ftp.UploadFile;
                    ftpRequest.UsePassive = true;
                    ftpRequest.UseBinary = true;
                    ftpRequest.KeepAlive = false;

                    // Lee el archivo y lo envía
                    using (FileStream stmFile = File.OpenRead(ConfigOptions.DirectoryPathAseembly() + "\\" + db + ".zip"))
                    { // Obtiene el stream sobre la comunicación FTP
                        using (Stream stmFTP = ftpRequest.GetRequestStream())
                        {
                            byte[] arrBytBuffer = new byte[2048];
                            int intRead;

                            // Lee y escribe el archivo en el stream de comunicaciones
                            while ((intRead = stmFile.Read(arrBytBuffer, 0, 2048)) != 0)
                                stmFTP.Write(arrBytBuffer, 0, intRead);
                            // Cierra el stream FTP
                            stmFTP.Close();
                        }
                        // Cierra el stream del archivo
                        stmFile.Close();
                    }
                }

            }
            catch(Exception ex)
            {
                Debug.WriteLine(ex);
                throw;
            }
        }

        /// <summary>
        /// Funcion para eliminar el sql y el zip al terminar el proceso
        /// </summary>
        private void DeleteBackup()
        {
            try
            {
                System.Windows.Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() =>
                {
                    this.btModifyConf.IsEnabled = false;
                    this.btCancel.Visibility = Visibility.Visible;
                    this.btInit.IsEnabled = false;
                    this.notifyIcon.Text = "Eliminando copias temporales (95%). Doble click para Mostrar";
                    this.tbProgress.Text = "Eliminando copias temporales (95%)";
                    this.bodyEmail += DateTime.Now.ToLongTimeString() + " - Eliminando copias temporales. \n";
                    this.pbProgress.Value = 95;
                }));

                foreach (var db in ConfigOptions.ListNamesDB)
                {
                    File.Delete(ConfigOptions.DirectoryPathAseembly() + "\\" + db + ".sql");
                    File.Delete(ConfigOptions.DirectoryPathAseembly() + "\\" + db + ".zip");
                }
            }
            catch(Exception ex)
            {
                Debug.WriteLine(ex);
                throw;
            }
        }

        /// <summary>
        /// Funcion para modificar los componentes y dejarlos de manera original
        /// </summary>
        private async Task ModifyComponents()
        {
            await System.Windows.Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() =>
            {
                this.btModifyConf.IsEnabled = true;
                this.btCancel.Visibility = Visibility.Hidden;
                this.btInit.IsEnabled = true;
                this.notifyIcon.Text = "Copia de seguridad completada (100%). Doble click para Mostrar";
                this.tbProgress.Text = "Copia de seguridad completada (100%)";
                this.bodyEmail += DateTime.Now.ToLongTimeString() + " - Copia de seguridad completada. \n";
                this.pbProgress.Value = 100;
            }));
        }

        /// <summary>
        /// Cancela el proceso de copia de seguridad
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtCancel_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (System.Windows.MessageBox.Show("¿Desea cancelar el proceso de copia de seguridad?", "Mensaje de Confirmación", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                {
                    if (th1.IsAlive && th1.ThreadState == System.Threading.ThreadState.Running)
                    {
                        th1.Interrupt();
                        th1.Abort();
                    }
                    this.isCancelCopy = true;
                }
                //this.isCancelCopy = true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
            finally
            {
                System.Windows.Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() =>
                {
                    this.pbProgress.Value = 0;
                    this.notifyIcon.Text = "Cancelada la copia de seguridad. Doble click para Mostrar";
                    this.tbProgress.Text = "Cancelado proceso de copia de seguridad";
                    this.bodyEmail += DateTime.Now.ToLongTimeString() + " - Cancelado proceso de copia de seguridad. \n";
                    this.btInit.IsEnabled = true;
                    this.btCancel.Visibility = Visibility.Hidden;
                    this.btModifyConf.IsEnabled = true;
                }));
            }
        }

        /// <summary>
        /// Funcion para ir al page de la configuracion
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtModifyConf_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.nframe.Navigate(new PageConfig(this.notifyIcon));
        }

        /// <summary>
        /// Se encarga de ocultar la ventana principal si el cierre no viene desde el icono de la barra de notificaciones
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Mw_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (!this.shutdownForce)
            {
                e.Cancel = true;
                MainWindow.mw.Hide();
            }
        }

        /// <summary>
        /// Fuerza el cierre del programa
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void item2_Click(object sender, EventArgs e)
        {
            try
            {
                if (this.copyInProcess)
                {
                    System.Windows.Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() =>
                    {
                        System.Windows.MessageBox.Show("No es posible finalizar la app, ya que hay una copia en progreso.", "Mensaje de ayuda");
                    }));
                }
                else
                {
                    this.shutdownForce = true;
                    System.Windows.Application.Current.Shutdown();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }            
        }

        /// <summary>
        /// Muestra la ventana principal
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void item1_Click(object sender, EventArgs e)
        {
            MainWindow.mw.Show();
        }
    
        /// <summary>
        /// Funcion para abrir el programa al primer plano
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void NotifyIcon_DoubleClick(object sender, EventArgs e)
        {
            MainWindow.mw.Show();
        }

    }
}