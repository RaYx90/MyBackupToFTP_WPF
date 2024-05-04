using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace mybackuptoftp.Utils
{
    public class ConfigOptions
    {
        public static string PathFile { get; set; }
        public static string NameClient { get; set; }
        public static string HourBackup { get; set; }
        public static string TypeBackup { get; set; }
        public static string EmailClient { get; set; }
        public static string HostDB { get; set; }
        public static string PortDB { get; set; }
        public static string UserDB { get; set; }
        public static string PassDB { get; set; }
        public static List<string> ListNamesDB { get; set; } = new List<string>();
        public static string UserFTP { get; set; }
        public static string PassFTP { get; set; }
        public static string AutoBackup { get; set; }
        //
        public static string HelpWindows { get; set; }
        public static string PathExe { get; set; }
        public static string ServerFTP { get; set; }

        public static string DirectoryTypeFTP
        {
            get
            {
                if (TypeBackup.Equals("0"))
                {
                    return ServerFTP + "s/";
                }
                else
                {
                    return ServerFTP + "m/";
                }
            }
        }

        public static string DirectoryDayFTP
        {
            get
            {
                if (TypeBackup.Equals("0"))
                {
                    DateTime today = DateTime.Today;
                    return DirectoryTypeFTP + today.DayOfWeek.ToString();
                }
                else
                {
                    DateTime today = DateTime.Today;
                    return DirectoryTypeFTP + today.Day.ToString();
                }
            }
        }

        public static string DirectoryPathAseembly()
        {
            string codeBase = Assembly.GetExecutingAssembly().CodeBase;
            UriBuilder uri = new UriBuilder(codeBase);
            string path = Uri.UnescapeDataString(uri.Path);
            return Path.GetDirectoryName(path);
        }

        public static void CreateConfigFile()
        {
            try
            {
                using (StreamWriter sw = File.CreateText(ConfigOptions.PathFile))
                {
                    sw.WriteLine("Client:::" + Encoder.Base64Encode(ConfigOptions.NameClient));
                    sw.WriteLine("Hour:::" + Encoder.Base64Encode(ConfigOptions.HourBackup));
                    sw.WriteLine("Type:::" + Encoder.Base64Encode(ConfigOptions.TypeBackup));
                    sw.WriteLine("Mail:::" + Encoder.Base64Encode(ConfigOptions.EmailClient));
                    sw.WriteLine("Auto:::" + Encoder.Base64Encode(ConfigOptions.AutoBackup));
                    //
                    sw.WriteLine("HelpWin:::" + Encoder.Base64Encode(ConfigOptions.HelpWindows));
                    sw.WriteLine("Host:::" + Encoder.Base64Encode(ConfigOptions.HostDB));
                    sw.WriteLine("Port:::" + Encoder.Base64Encode(ConfigOptions.PortDB));
                    sw.WriteLine("UserDB:::" + Encoder.Base64Encode(ConfigOptions.UserDB));
                    sw.WriteLine("PassDB:::" + Encoder.Base64Encode(ConfigOptions.PassDB));
                    var DBS = "";
                    foreach (var db in ConfigOptions.ListNamesDB)
                    {
                        DBS += db + ";";
                    }
                    sw.WriteLine("DB:::" + Encoder.Base64Encode(DBS));
                    sw.WriteLine("ServerFTP:::" + Encoder.Base64Encode(ConfigOptions.ServerFTP));
                    sw.WriteLine("UserFTP:::" + Encoder.Base64Encode(ConfigOptions.UserFTP));
                    sw.WriteLine("PassFTP:::" + Encoder.Base64Encode(ConfigOptions.PassFTP));
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                throw;
            }
        }

        // bool withResponse = false
        public static void ReadConfigFile()
        {
            try
            {

                if (File.Exists(ConfigOptions.PathFile))
                {
                    // Open the file to read from.
                    using (StreamReader sr = File.OpenText(ConfigOptions.PathFile))
                    {
                        string s = "";
                        string[] separatingStrings = { ":::" };
                        while ((s = sr.ReadLine()) != null)
                        {
                            string[] values = s.Split(separatingStrings, StringSplitOptions.RemoveEmptyEntries);

                            //
                            if (values.Length > 0)
                            {
                                switch (values[0])
                                {
                                    case "Client":
                                        NameClient = Encoder.Base64Decode(values[1]);
                                        break;
                                    case "Hour":
                                        HourBackup = Encoder.Base64Decode(values[1]);
                                        break;
                                    case "Type":
                                        TypeBackup = Encoder.Base64Decode(values[1]);
                                        break;
                                    case "Auto":
                                        AutoBackup = Encoder.Base64Decode(values[1]);
                                        break;
                                    case "HelpWin":
                                        HelpWindows = Encoder.Base64Decode(values[1]);
                                        break;
                                    case "Mail":
                                        EmailClient = Encoder.Base64Decode(values[1]);
                                        break;
                                    case "Host":
                                        HostDB = Encoder.Base64Decode(values[1]);
                                        break;
                                    case "Port":
                                        PortDB = Encoder.Base64Decode(values[1]);
                                        break;
                                    case "UserDB":
                                        UserDB = Encoder.Base64Decode(values[1]);
                                        break;
                                    case "PassDB":
                                        PassDB = Encoder.Base64Decode(values[1]);
                                        break;
                                    case "DB":
                                        ListNamesDB.Clear();
                                        var splitDBS = Encoder.Base64Decode(values[1]).Split(';');
                                        foreach (var db in splitDBS)
                                        {
                                            if (!string.IsNullOrWhiteSpace(db))
                                            {
                                                ListNamesDB.Add(db);
                                            }
                                        }
                                        break;
                                    case "ServerFTP":
                                        ServerFTP = Encoder.Base64Decode(values[1]);
                                        break;
                                    case "UserFTP":
                                        UserFTP = Encoder.Base64Decode(values[1]);
                                        break;
                                    case "PassFTP":
                                        PassFTP = Encoder.Base64Decode(values[1]);
                                        break;
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                throw;
            }
        }

        /* private static void AddShortcut() 
         {
             string desktop = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);

             using (StreamWriter w = new StreamWriter(desktop + "\\PGA Backup 2.url"))
             {
                 string app = ConfigOptions.DirectoryPathAseembly() + "\\pgabackup2.exe";
                 w.WriteLine("[InternetShortcut]");
                 w.WriteLine("URL=file:///" + app);
                 w.WriteLine("IconIndex=0");
                 string icon = app.Replace('\\', '/');
                 w.WriteLine("IconFile=" + icon);
                 w.Flush();
             }
         }*/
    }
}
