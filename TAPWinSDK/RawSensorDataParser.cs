using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TAPWin
{
    class RawSensorDataParser
    {

        public static void ParseWhole(byte[] data, RawSensorSensitivity sensitivity, Action<RawSensorData> onCompleted)
        {
            int metaLength = 4;
            int metaOffset = 0;
            UInt32 timestamp = 1;
            while ((metaOffset + metaLength < data.Length) && (timestamp > 0))
            {
                UInt32 meta = 0;
                int add = 0;
                meta = BitConverter.ToUInt32(data, metaOffset);
                int messageOffset = 0;
                int messageLength = 0;
                if (meta > 0)
                {
                    UInt32 packetType = (meta & (UInt32)(0x80000000)) >> 31;
                    timestamp = (UInt32)(meta & (UInt32)(0x7fffffff));
                    RawSensorDataType type = RawSensorDataType.None;
                    if (timestamp == 0)
                    {
                        return;
                    }
                    if (packetType == 0)
                    {
                        messageOffset = metaOffset + metaLength;
                        messageLength = 12;
                        type = RawSensorDataType.IMU;
                        add = 12;
                    } else if (packetType == 1)
                    {
                        messageOffset = metaOffset + metaLength;
                        messageLength = 30;
                        add = 30;
                        type = RawSensorDataType.Device;
                    } else
                    {
                        return;
                    }
                    if (type != RawSensorDataType.None)
                    {
                        if (messageOffset + messageLength < data.Length)
                        {
                            RawSensorDataParser.ParseSingle(type, timestamp, data.Skip(messageOffset).Take(messageLength).ToArray(), sensitivity, onCompleted);
                        }
                    }
                    if (add == 0)
                    {
                        return;
                    }
                } else
                {
                    return;
                }
                metaOffset = metaOffset + metaLength + add;
            }
        }

        private static void ParseSingle(RawSensorDataType type, UInt32 timestamp, byte[] data, RawSensorSensitivity sensitivity, Action<RawSensorData> onCompleted)
        {
            RawSensorData rsData = RawSensorData.Create(type, timestamp, data, sensitivity);
            if (rsData != null)
            {
                onCompleted(rsData);
            }
        }
    }
}
