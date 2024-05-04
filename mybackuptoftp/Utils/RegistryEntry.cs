using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Win32;

namespace mybackuptoftp.Utils
{
    public class RegistryEntry
    {
        /// <summary>
        /// Resgistrar� en Inicio del registro la aplicaci�n indicada
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
                // a�adirlo al registro
                // Si el path contiene espacios se deber�a incluir entre comillas dobles
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
        /// Quitar� de Inicio la aplicaci�n indicada
        /// Devuelve True si todo fue bien, False en caso contrario
        /// Si la aplicaci�n no estaba en Inicio, devuelve True salvo que se produzca un error
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
        /// Comprobar� si la clave indicada est� asignada en Inicio
        /// en caso de ser as� devolver� el contenido,
        /// en caso contrario devolver� una cadena vacia
        /// Si se produce un error, se devolver� la cadena de error
        /// </summary>
        /// <param name="keyName"></param>
        /// <returns></returns>
        public static string CheckInStart(string keyName)
        {
            try
            {
                RegistryKey runK = Registry.LocalMachine.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Run", false);
                // comprobar si est�
                return runK.GetValue(keyName, "").ToString();
            }
            catch
            {
                return "";
            }
        }
    }
}
