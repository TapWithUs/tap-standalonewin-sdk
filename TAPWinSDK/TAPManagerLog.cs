using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;
using System.Diagnostics;

namespace TAPWin
{
    public enum TAPManagerLogEvent : int
    {
        Warning = 0,
        Error = 1,
        Info = 2,
        Fatal = 3
    };

    public class TAPManagerLog
    {
        private static readonly TAPManagerLog instance = new TAPManagerLog();

        private Dictionary<TAPManagerLogEvent, bool> enabledEvents;

        private List<string> _logs;

        public event Action<string> OnLineLogged;

        static TAPManagerLog()
        {

        }

        private TAPManagerLog()
        {
            this._logs = new List<string>();
            enabledEvents = new Dictionary<TAPManagerLogEvent, bool>();
            enabledEvents.Add(TAPManagerLogEvent.Error, true);
            enabledEvents.Add(TAPManagerLogEvent.Fatal, true);
            enabledEvents.Add(TAPManagerLogEvent.Info, true);
            enabledEvents.Add(TAPManagerLogEvent.Warning, true);
        }

        public static TAPManagerLog Instance
        {
            get
            {
                return instance;
            }
        }

        String DateString()
        {
            return DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss");
        }

        public void Log(TAPManagerLogEvent e, String message, [CallerFilePath] string file = "", [CallerMemberName] string member = "", [CallerLineNumber] int line = 0)
        {

            if (this.IsEventEnabled(e))
            {
                string str = "[" + this.DateString() + "] TAPLib " + this.GetEventString(e) + " # " + message + " @ " + file + ":" + member + "(" + line + ")";
                Debug.WriteLine(str);
                this._logs.Add(str);
                if (this.OnLineLogged != null)
                {
                    this.OnLineLogged(str);
                }
            }
        }

        public string GetLog(bool clear)
        {
            if (this._logs.Count > 0)
            {
                string aggr = this._logs.Aggregate((i, j) => i + "\n" + j);
                if (clear)
                {
                    this._logs.Clear();
                }
                return aggr;
            }

            return "";
        }

        private String GetEventString(TAPManagerLogEvent e)
        {
            switch (e)
            {
                case TAPManagerLogEvent.Error: return "ERROR";
                case TAPManagerLogEvent.Fatal: return "FATAL";
                case TAPManagerLogEvent.Info: return "info";
                case TAPManagerLogEvent.Warning: return "warning";
            }
            return "";
        }

        private bool IsEventEnabled(TAPManagerLogEvent e)
        {
            if (this.enabledEvents.ContainsKey(e))
            {
                return this.enabledEvents[e];
            }
            else
            {
                this.enabledEvents.Add(e, false);
                return false;
            }
        }

        public void EnableEvent(TAPManagerLogEvent e)
        {
            this.enabledEvents[e] = true;
        }

        public void DisableEvent(TAPManagerLogEvent e)
        {
            this.enabledEvents[e] = false;
        }

        public void EnableAllEvents()
        {
            var values = Enum.GetValues(typeof(TAPManagerLogEvent));
            foreach (TAPManagerLogEvent e in values)
            {
                this.enabledEvents[e] = true;
            }
        }

        public void DisableAllEvents()
        {
            var values = Enum.GetValues(typeof(TAPManagerLogEvent));
            foreach (TAPManagerLogEvent e in values)
            {
                this.enabledEvents[e] = false;
            }
        }
    }
}
