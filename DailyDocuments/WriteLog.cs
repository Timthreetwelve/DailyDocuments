// Copyright (c) TIm Kennedy. All Rights Reserved. Licensed under the MIT License.

using System;
using System.IO;
using System.Reflection;

namespace TKUtils
{
    /// <summary>
    /// Class for writing log files
    /// </summary>
    public class WriteLog
    {
        protected WriteLog()
        {
        }

        /// <Summary>
        /// This overload is used when a time stamp is desired.
        /// </Summary>
        /// <param name="path">Path to log file</param>
        /// <param name="message">String to be written to log file</param>
        /// <param name="timeStampType">Valid time stamp chars are U, S, L, E, M, N, or X</param>
        /// <param name="useBrackets">Enclose time stamp with square brackets. Y or N</param>
        public static bool WriteLogFile(string path, string message, char timeStampType, char useBrackets)
        {
            if (CheckLogFile(path, timeStampType, useBrackets))
            {
                string tsMessage = GetTimeStamp(timeStampType, useBrackets) + message.TrimEnd() + Environment.NewLine;
                try
                {
                    File.AppendAllText(path, tsMessage);
                    return true;
                }
                catch (Exception)
                {
                    return false;
                }
            }
            return false;
        }

        /// <Summary>
        /// This overload is used when a time stamp is not desired
        /// </Summary>
        /// <param name="path">Path to log file</param>
        /// <param name="message">String to be written to log file</param>

        public static bool WriteLogFile(string path, string message)
        {
            if (CheckLogFile(path))
            {
                string tsMessage = message + Environment.NewLine;
                try
                {
                    File.AppendAllText(path, tsMessage);
                    return true;
                }
                catch (Exception)
                {
                    return false;
                }
            }
            return false;
        }

        private static bool CheckLogFile(string path, char tstype, char brackets)
        {
            if (!File.Exists(path))
            {
                string ThisExeName = Assembly.GetEntryAssembly().Location.ToString();

                try
                {
                    string CreatedMsg = String.Format("{0}This log file created by {1}" + Environment.NewLine,
                            GetTimeStamp(tstype, brackets), System.IO.Path.GetFileName(ThisExeName));
                    File.WriteAllText(path, CreatedMsg.ToString());
                }
                catch (Exception)
                {
                    return false;
                }
                return true;
            }
            return true;
        }

        private static bool CheckLogFile(string path)
        {
            if (!File.Exists(path))
            {
                string ThisExeName = Assembly.GetEntryAssembly().Location.ToString();

                try
                {
                    string CreatedMsg = String.Format("This log file created by {0}" + Environment.NewLine,
                            System.IO.Path.GetFileName(ThisExeName));
                    File.WriteAllText(path, CreatedMsg.ToString());
                }
                catch (Exception)
                {
                    return false;
                }
                return true;
            }
            return true;
        }

        private static string GetTimeStamp(char tstype, char brackets)
        {
            DateTime Now = DateTime.Now;
            char ts = char.ToUpper(tstype);
            char br = char.ToUpper(brackets);
            string TimeStamp;

            switch (ts)
            {
                case 'U': // US
                    TimeStamp = Now.ToString("[MM/dd/yyyy HH:mm:ss]  ");
                    break;

                case 'S': // Short
                    TimeStamp = Now.ToString("[MM/dd/yy HH:mm]  ");
                    break;

                case 'L': // Long
                    TimeStamp = Now.ToString("[MM/dd/yyyy HH:mm:ss.ffff]  ");
                    break;

                case 'E': // European
                    TimeStamp = Now.ToString("[yyyy/MM/dd HH:mm:ss]  ");
                    break;

                case 'M': // Month
                    TimeStamp = Now.ToString("[dd MMM yyyy HH:mm:ss]  ");
                    break;

                default:
                    TimeStamp = "";
                    break;
            }

            if (br != 'Y')
            {
                TimeStamp = TimeStamp.Replace("[", "").Replace("]", "");
            }
            return TimeStamp;
        }

        #region Temp file
        public static string GetTempFile()
        {
            string myExe = Assembly.GetExecutingAssembly().GetName().Name;
            string tStamp = string.Format("{0:yyyyMMdd}", DateTime.Now);
            string path = Path.GetTempPath();
#if DEBUG
            string filename = myExe + ".debug." + tStamp + ".log";
#else
            string filename = myExe + ".temp." + tStamp + ".log";
#endif
            return Path.Combine(path, filename);
        }

        public static void WriteTempFile(string msg)
        {
            WriteLog.WriteLogFile(GetTempFile(), msg, 'U', 'N');
        }

        #endregion Temp file
    }
}
