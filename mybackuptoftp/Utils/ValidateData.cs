using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace mybackuptoftp.Utils
{
    public static class ValidateData
    {
        /// <summary>
        /// Funcion para validar que se introdujo un email válido
        /// </summary>
        /// <returns></returns>
        static public bool verifyEmail(string mail)
        {
            String expresion;
            expresion = "\\w+([-+.']\\w+)*@\\w+([-.]\\w+)*\\.\\w+([-.]\\w+)*";
            if (Regex.IsMatch(mail, expresion))
            {
                if (Regex.Replace(mail, expresion, String.Empty).Length == 0)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Funcion para verificar el formato de la hora
        /// </summary>
        /// <param name="hour"></param>
        /// <returns></returns>
        public static bool verifyHour(string hour)
        {
            var patternHour = "^([01]?[0-9]|2[0-3]):[0-5][0-9]$";
            Regex rgxHour = new Regex(patternHour);
            if (rgxHour.IsMatch(hour))
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
