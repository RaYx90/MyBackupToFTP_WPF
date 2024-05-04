using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Win32;

namespace mybackuptoftp.Utils
{
    public class RegistryEntry
    {
        /// <summary>
        /// Resgistrará en Inicio del registro la aplicación indicada
        /// Devuelve True si todo fue bien, False en caso contrario
        /// Guardar la clave en el registro
        /// HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\CurrentVersion\Run
        /// </summary>
        /// <param name="keyName"></param>
        /// <param name="appName"></param>
        /// <returns></returns>
        public static bool AddInStartup(string keyName, string appName)
        {
            try
            {
                RegistryKey runK = Registry.LocalMachine.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Run", true);
                // añadirlo al registro
                // Si el path contiene espacios se debería incluir entre comillas dobles
                if (appName.StartsWith("\"") == false && appName.IndexOf(" ") > -1)
                {
                    appName = "\"" + appName + "\"";
                }
                runK.SetValue(keyName, appName);
                return true;
            }
            catch(Exception ex)
            {
                string exception = ex.Message;
                return false;
            }
        }

        /// <summary>
        /// Quitará de Inicio la aplicación indicada
        /// Devuelve True si todo fue bien, False en caso contrario
        /// Si la aplicación no estaba en Inicio, devuelve True salvo que se produzca un error
        /// </summary>
        /// <param name="nombreClave"></param>
        /// <returns></returns>
        public static bool RemoveFromStartup(string keyName)
        {
            
            try
            {
                RegistryKey runK = Registry.LocalMachine.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Run", true);
                // quitar la clave indicada del registo
                runK.DeleteValue(keyName, false);
                return true;
            }
            catch (Exception ex)
            {
                string exception = ex.Message;
                return false;
            }
        }

        /// <summary>
        /// Comprobará si la clave indicada está asignada en Inicio
        /// en caso de ser así devolverá el contenido,
        /// en caso contrario devolverá una cadena vacia
        /// Si se produce un error, se devolverá la cadena de error
        /// </summary>
        /// <param name="keyName"></param>
        /// <returns></returns>
        public static string CheckInStart(string keyName)
        {
            try
            {
                RegistryKey runK = Registry.LocalMachine.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Run", false);
                // comprobar si está
                return runK.GetValue(keyName, "").ToString();
            }
            catch
            {
                return "";
            }
        }
    }
}
