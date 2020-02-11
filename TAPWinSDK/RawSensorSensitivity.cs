using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TAPWin
{
    public class RawSensorSensitivity
    {
        private struct Range
        {
            byte low;
            byte high;
            byte defaultValue;
            
            Range(byte _low, byte _high, byte _defaultValue)
            {
                this.low = _low;
                this.high = _high;
                this.defaultValue = _defaultValue;
            }
        }

        public static readonly string kDeviceAccelerometer = "DeviceAccelerometer";
        public static readonly string kIMUGyro = "IMUGyro";
        public static readonly string kIMUAccelerometer = "IMUAccelerometer";

        private Dictionary<string,byte> parameters= new Dictionary<string, byte>();

        private static readonly Dictionary<string, double[]> factors = new Dictionary<string, double[]>()
        {
            { RawSensorSensitivity.kDeviceAccelerometer, new double[] { 31.25, 3.90625, 7.8125, 15.625, 31.25} },
            { RawSensorSensitivity.kIMUAccelerometer, new double[] { 0.122, 0.061, 0.122, 0.244, 0.488} },
            { RawSensorSensitivity.kIMUGyro, new double[] { 17.5, 4.375, 8.75, 17.5, 35, 70} }
        };

        private static readonly string[] order = new string[] { RawSensorSensitivity.kDeviceAccelerometer, RawSensorSensitivity.kIMUGyro, RawSensorSensitivity.kIMUAccelerometer };

        public byte deviceAccelerometer
        {
            get
            {
                if (this.parameters.ContainsKey(RawSensorSensitivity.kDeviceAccelerometer))
                {
                    return this.parameters[RawSensorSensitivity.kDeviceAccelerometer];
                } else
                {
                    return 0;
                }
            }
            set
            {
                this.parameters[RawSensorSensitivity.kDeviceAccelerometer] = normalize(value, RawSensorSensitivity.kDeviceAccelerometer);
            }
        }

        public byte imuGyro
        {
            get
            {
                if (this.parameters.ContainsKey(RawSensorSensitivity.kIMUGyro))
                {
                    return this.parameters[RawSensorSensitivity.kIMUGyro];
                }
                else
                {
                    return 0;
                }
            }
            set
            {
                this.parameters[RawSensorSensitivity.kIMUGyro] = normalize(value, RawSensorSensitivity.kIMUGyro);
            }
        }

        public byte imuAccelerometer
        {
            get
            {
                if (this.parameters.ContainsKey(RawSensorSensitivity.kIMUAccelerometer))
                {
                    return this.parameters[RawSensorSensitivity.kIMUAccelerometer];
                }
                else
                {
                    return 0;
                }
            }
            set
            {
                this.parameters[RawSensorSensitivity.kIMUAccelerometer] = normalize(value, RawSensorSensitivity.kIMUAccelerometer);
            }
        }

        public RawSensorSensitivity()
        {
            this.deviceAccelerometer = 0;
            this.imuGyro = 0;
            this.imuAccelerometer = 0;
        }

        public RawSensorSensitivity(byte _deviceAccelerometer, byte _imuGyro, byte _imuAccelerometer)
        {
            this.deviceAccelerometer = _deviceAccelerometer;
            this.imuGyro = _imuGyro;
            this.imuAccelerometer = _imuAccelerometer;
        }

        private byte normalize(byte value, string type)
        {
            if (RawSensorSensitivity.factors.ContainsKey(type))
            {
                double[] factor = RawSensorSensitivity.factors[type];
                if (value >= factor.Length)
                {
                    return 0;
                } else
                {
                    return value;
                }
            } else
            {
                return 0;
            }
        }

        private byte getValue(string type)
        {
            if (this.parameters.ContainsKey(type))
            {
                return this.parameters[type];
            } else
            {
                return 0;
            }
        }

        internal double getSensitivity(string type)
        {
            if (RawSensorSensitivity.factors.ContainsKey(type) && this.parameters.ContainsKey(type))
            {
                byte index = this.parameters[type];
                if (index >= 0 && index < RawSensorSensitivity.factors[type].Length)
                {
                    return RawSensorSensitivity.factors[type][index];
                }
                else
                {
                    return 1.0;
                }
            }
            else
            {
                return 1.0;
            }
        }

        internal byte[] getBytes()
        {
            byte[] result = new byte[RawSensorSensitivity.order.Length];
            for (int i=0; i<RawSensorSensitivity.order.Length; i++)
            {
                string key = RawSensorSensitivity.order[i];
                result[i] = this.getValue(key);
            }
            return result;
        }
    }
}
