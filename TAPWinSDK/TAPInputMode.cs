using System.Collections.Generic;
using System.Linq;
namespace TAPWin
{

    public class TAPInputMode
    {
        public static readonly string kController = "Controller";
        public static readonly string kControllerWithMouseHID = "ControllWithMouseHID";
        public static readonly string kText = "Text";
        public static readonly string kRawSensor = "RawSensor";

        private static Dictionary<string, byte> modesByte = new Dictionary<string, byte>()
        {
            {TAPInputMode.kController, 0x1},
            {TAPInputMode.kText, 0x0 },
            {TAPInputMode.kRawSensor, 0xa},
            {TAPInputMode.kControllerWithMouseHID, 0x3}
        };

        public RawSensorSensitivity sensitivity;
        public string mode;

        internal bool isValid
        {
            get
            {
                if (this.mode.Equals(TAPInputMode.kRawSensor)) {
                    return this.sensitivity != null;
                } else
                {
                    return TAPInputMode.modesByte.ContainsKey(this.mode);
                }
            }
        }

        private TAPInputMode(string _mode, RawSensorSensitivity _sensitivity = null)
        {
            this.mode = validateModeString(_mode);
            this.sensitivity = _sensitivity;
        }

        private string validateModeString(string mode)
        {
            if (TAPInputMode.modesByte.ContainsKey(mode)) {
                return mode;
            } else
            {
                return "";
            }
        }

        internal byte[] getBytes()
        {
            byte modeByte = 0;
            if (TAPInputMode.modesByte.ContainsKey(this.mode))
            {
                modeByte = TAPInputMode.modesByte[this.mode];
            } else
            {
                return null;
            }
            byte[] sensitivityArray = new byte[0];
            
            if (this.mode.Equals(TAPInputMode.kRawSensor))
            {
                if (this.sensitivity != null)
                {
                    sensitivityArray = this.sensitivity.getBytes();
                } else
                {
                    return null;
                }
            }

            byte[] bytes = new byte[] { 0x3, 0xc, 0x0, modeByte };
            return bytes.Concat(sensitivityArray).ToArray();
        }

        internal TAPInputMode makeCompatibleWithFWVersion(int fwVersion)
        {
            if (this.mode.Equals(TAPInputMode.kControllerWithMouseHID) && fwVersion < 010615) {
                return TAPInputMode.Controller();
            } else
            {
                return this;
            }
        }

        // Public functions

        public static TAPInputMode Controller()
        {
            return new TAPInputMode(TAPInputMode.kController);
        }

        public static TAPInputMode Text()
        {
            return new TAPInputMode(TAPInputMode.kText);
        }

        public static TAPInputMode RawSensor(RawSensorSensitivity sensitivity)
        {
            return new TAPInputMode(TAPInputMode.kRawSensor, sensitivity);
        }

        public static TAPInputMode ControllerWithMouseHID()
        {
            return new TAPInputMode(TAPInputMode.kControllerWithMouseHID);
        }

        public bool isModeEquals(string _mode)
        {
            return this.mode.Equals(_mode);
        }
    }

    //public enum TAPInputMode : int
    //{
    //    Controller = 0x1,
    //    Text = 0x0,
    //    Controller_With_MouseHID = 0x03,
    //    Null = 0xff
    //};
}
