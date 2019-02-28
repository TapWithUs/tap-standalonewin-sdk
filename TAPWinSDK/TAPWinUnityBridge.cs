
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;



namespace TAPWin
{
#if BRIDGE_TO_UNITY

    public class TAPWinUnityBridge
    {
    
        private TAPWinUnityBridge()
        {

        }

        public static bool started = false;

        [DllExport(CallingConvention.StdCall)]
        public static void TAPWinUnityBridgeStart()
        {
            if (started)
            {
                return;
            }
            TAPDataManager.Instance.RegisterWithTAPPManager(TAPManager.Instance);
            TAPManager.Instance.Start();
            started = true;
    }

        [DllExport(CallingConvention.StdCall)]
        public static void TAPWinUnityBridgeSetActivated(bool enabled)
        {
            if (enabled)
            {
                TAPManager.Instance.Activate();
            } else
            {
                TAPManager.Instance.Deactivate();
            }
        }

        [DllExport(CallingConvention.StdCall)]
        public static bool TAPWinUnityBridgeGetTapConnected(out string identifier, out string name, out int fw)
        {
            return TAPDataManager.Instance.GetTapConnected(out identifier, out name, out fw);
        }

        [DllExport(CallingConvention.StdCall)]
        public static bool TAPWinUnityBridgeGetTapDisconnected(out string identifier)
        {
            return TAPDataManager.Instance.GetTapDisconnected(out identifier);
        }

        [DllExport(CallingConvention.StdCall)]
        public static bool TAPWinUnityBridgeGetTapped(out string identifier, out int tapcode)
        {
            return TAPDataManager.Instance.GetTapped(out identifier, out tapcode);
        }

        [DllExport(CallingConvention.StdCall)]
        public static bool TAPWinUnityBridgeGetMoused(out string identifier, out int vx, out int vy)
        {
            return TAPDataManager.Instance.GetMoused(out identifier, out vx, out vy);
        }

        [DllExport(CallingConvention.StdCall)]
        public static bool TAPWinUnityBridgeGetLog(out string line)
        {
            return TAPDataManager.Instance.GetLog(out line);
        }

    }
#endif
}
