﻿using System;
using System.IO;

namespace DigaoDeskApp
{
    internal class EventAudit
    {

        private static object _lockCtrl = new();

        public static void Do(string info)
        {
            lock (_lockCtrl) {
                File.AppendAllText(
                    Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "audit.txt"),
                    DateTime.Now.ToString(Vars.DATETIME_FMT) + " - " + info +
                    Environment.NewLine
                );
            }
        }

    }
}
