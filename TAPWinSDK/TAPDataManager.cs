using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TAPWin
{
#if BRIDGE_TO_UNITY
    class TAPDataManager
    {
 
        private List<TapConnectedStruct> _connected;
        private List<string> _disconnected;
        private List<TappedStruct> _tapped;
        private List<MousedStruct> _moused;
        private List<string> _log;

        private bool registeredWithTAPManager;

        struct TapConnectedStruct
        {
            public string identifier;
            public string name;
            public int fw;
            public TapConnectedStruct(string id, string tapName, int fwVersion)
            {
                identifier = id;
                name = tapName;
                fw = fwVersion;
            }
        }

        struct TappedStruct
        {
            public string identifier;
            public int tapcode;
            public TappedStruct(string id, int tc)
            {
                identifier = id;
                tapcode = tc;
            }
        }

        struct MousedStruct
        {
            public string identifier;
            public int vx;
            public int vy;
            public MousedStruct(string id, int x, int y)
            {
                identifier = id;
                vx = x;
                vy = y;
            }
        }

        private static readonly TAPDataManager instance = new TAPDataManager();

        static TAPDataManager()
        {
        }

        private TAPDataManager()
        {
            registeredWithTAPManager = false;
            _tapped = new List<TappedStruct>();
            _moused = new List<MousedStruct>();
            _connected = new List<TapConnectedStruct>();
            _disconnected = new List<string>();
            _log = new List<string>();
        }


        public static TAPDataManager Instance
        {
            get
            {
                return instance;
            }
        }

        internal void RegisterWithTAPPManager(TAPManager instance)
        {
            if (registeredWithTAPManager)
            {
                return;
            }
            instance.OnTapConnected += TapConnected;
            instance.OnTapDisconnected += TapDisconnected;
            instance.OnTapped += TapTapped;
            instance.OnMoused += TapMoused;
            TAPManagerLog.Instance.EnableAllEvents();
            TAPManagerLog.Instance.OnLineLogged += OnLog;
            registeredWithTAPManager = true;
        }

        internal void OnLog(string line)
        {
            _log.Add(line);
        }

        internal void TapConnected(string identifier, string name, int fw)
        {
            _connected.Add(new TapConnectedStruct(identifier, name, fw));
        }

        internal void TapDisconnected(string identifier)
        {
            _disconnected.Add(identifier);
        }

        internal void TapTapped(string identifier, int tapcode)
        {
            _tapped.Add(new TappedStruct(identifier, tapcode));
        }

        internal void TapMoused(string identifier, int vx, int vy)
        {
            _moused.Add(new MousedStruct(identifier, vx, vy));
        }

        internal bool GetTapDisconnected(out string identifier)
        {
            if (_disconnected.Count > 0)
            {
                identifier = _disconnected.First();
                _disconnected.RemoveAt(0);
                return true;
            } else
            {
                identifier = "";
                return false;
            }
        }

        internal bool GetTapConnected(out string identifier, out string name, out int fw)
        {
            if (_connected.Count > 0)
            {
                TapConnectedStruct t = _connected.First();
                identifier = t.identifier;
                name = t.name;
                fw = t.fw;
                _connected.RemoveAt(0);
                return true;
            } else
            {
                identifier = "";
                name = "";
                fw = 0;
                return false;
            }
            
        }

        internal bool GetLog(out string line)
        {
            if (_log.Count > 0)
            {
                line = _log.First();
                _log.RemoveAt(0);
                return true;
            } else
            {
                line = "";
                return false;
            }
        }

        internal bool GetTapped(out string identifier, out int tapcode)
        {
            if (_tapped.Count > 0)
            {
                TappedStruct t = _tapped.First();
                identifier = t.identifier;
                tapcode = t.tapcode;
                _tapped.RemoveAt(0);
                return true;
            } else
            {
                identifier = "";
                tapcode = 0;
                return false;
            }
        }

        internal bool GetMoused(out string identifier, out int vx, out int vy)
        {
            if (_moused.Count > 0)
            {
                MousedStruct t = _moused.First();
                identifier = t.identifier;
                vx = t.vx;
                vy = t.vy;
                _moused.RemoveAt(0);
                return true;
            } else
            {
                identifier = "";
                vx = 0;
                vy = 0;
                return false;
            }
        }

         
    }
#endif
}
