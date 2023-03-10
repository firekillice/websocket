using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BeetleX;
using BeetleX.EventArgs;
using BeetleX.FastHttpApi;

namespace Carbon.Match.Utility
{
    public static class LogUtility
    {
        private static readonly object mLockConsole = new();

        private static LogType logLevel = LogType.Debug;

        public static IServer? Server { get; set; }

        /// <summary>
        /// 普通日志
        /// </summary>
        /// <param name="type"></param>
        /// <param name="message"></param>
        /// <param name="parameters"></param>
        public static void Log(LogType type, string message, params object[] parameters)
        {
            //if (Server != null && Server.EnableLog(type))
            //{
            //    Server?.Log(type, null, string.Format(message, parameters));
            //}

            Log(type, string.Format(message, parameters));
        }

        /// <summary>
        /// 会话日志
        /// </summary>
        /// <param name="type"></param>
        /// <param name="session"></param>
        /// <param name="message"></param>
        /// <param name="parameters"></param>
        public static void SessionLog(LogType type, ISession? session, string message, params object[] parameters)
        {
            //if (Server != null && Server.EnableLog(type))
            //{
            //    Server?.Log(type, session, string.Format(message, parameters));
            //}

            Log(type, string.Format(message, parameters));
        }

        /// <summary>
        /// 写入日志
        /// </summary>
        /// <param name="type"></param>
        /// <param name="message"></param>
        private static void Log(LogType type, string message)
        {
            if (type < logLevel)
                return;

            lock (mLockConsole)
            {
                Console.Write($"[{ DateTime.Now.ToString("HH:mmm:ss")}] ");
                switch (type)
                {
                    case LogType.Error:
                        Console.ForegroundColor = ConsoleColor.DarkRed;
                        break;
                    case LogType.Warring:
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        break;
                    case LogType.Fatal:
                        Console.ForegroundColor = ConsoleColor.Red;
                        break;
                    case LogType.Info:
                        Console.ForegroundColor = ConsoleColor.Green;
                        break;
                    default:
                        Console.ForegroundColor = ConsoleColor.White;
                        break;
                }
                Console.Write($"[{type.ToString()}] ");
                Console.ForegroundColor = ConsoleColor.Gray;
                Console.WriteLine(message);
            }
        }
    }
}
