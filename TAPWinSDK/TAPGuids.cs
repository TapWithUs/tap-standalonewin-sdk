using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TAPWin
{
    public sealed class TAPGuids
    {
        public static Guid service_tap = new Guid("c3ff0001-1d8b-40fd-a56f-c7bd5d0f3370");
        public static Guid service_nus = new Guid("6e400001-b5a3-f393-e0a9-e50e24dcca9e");
        public static Guid service_device_information = new Guid("0000180A-0000-1000-8000-00805F9B34FB");
        public static Guid characteristic_tapdata = new Guid("c3ff0005-1d8b-40fd-a56f-c7bd5d0f3370");
        public static Guid characteristic_mousedata = new Guid("c3ff0006-1d8b-40fd-a56f-c7bd5d0f3370");
        public static Guid characteristic_airgesturesdata = new Guid("c3ff000A-1d8b-40fd-a56f-c7bd5d0f3370");
        public static Guid characteristic_uicommands = new Guid("c3ff0009-1d8b-40fd-a56f-c7bd5d0f3370");
        public static Guid characteristic_rx = new Guid("6e400002-b5a3-f393-e0a9-e50e24dcca9e");
        public static Guid characteristic_tx = new Guid("6e400003-b5a3-f393-e0a9-e50e24dcca9e");
        
        public static Guid characteristic_fw_version = new Guid("00002A26-0000-1000-8000-00805F9B34FB");
        
        private TAPGuids()
        {

        }

        public static string GetServiceNameByGUID(Guid service)
        {
            if (service == service_tap)
            {
                return "TAP";
            } else if (service == service_nus)
            {
                return "NUS";
            } else if (service == service_device_information)
            {
                return "DEVICE INFORMATION";
            } else
            {
                return service.ToString();
            }
            
        }

        public static string GetCharacteristicNameByGUID(Guid characteristic)
        {
            if (characteristic == characteristic_fw_version)
            {
                return "FW VERSION";
            }
            else if (characteristic == characteristic_mousedata)
            {
                return "MOUSE DATA";
            }
            else if (characteristic == characteristic_rx)
            {
                return "NUS_RX";
            }
            else if (characteristic == characteristic_tapdata)
            {
                return "TAP DATA";
            }
            else if (characteristic == characteristic_tx)
            {
                return "NUS_TX";
            }
            else if (characteristic == characteristic_airgesturesdata)
            {
                return "AIR GESTURES DATA";
            } 
            else if (characteristic == characteristic_uicommands)
            {
                return "TAP UI COMMANDS";
            }
            else
            {
                return characteristic.ToString();
            }
        }


    }
}
