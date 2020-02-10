using System;
using System.Collections;
using System.Collections.Generic;

namespace TAPWin
{

    public class Point3
    {
        public double x;
        public double y;
        public double z;

        private Point3(double _x, double _y, double _z)
        {
            this.x = _x;
            this.y = _y;
            this.z = _z;
        }

        public static Point3 Create(byte[] bytes, double sensitivityFactor)
        {
            if (bytes.Length != 6)
            {
                return null;
            }
            Int16 i_x = (Int16)((int)bytes[1] << 8 | (int)bytes[0]);
            Int16 i_y = (Int16)((int)bytes[3] << 8 | (int)bytes[2]);
            Int16 i_z = (Int16)((int)bytes[5] << 8 | (int)bytes[4]);
            return new Point3((double)i_x, (double)i_y, (double)i_z);
        }
    }

    public enum RawSensorDataType
    {
        None,
        IMU,
        Device
    }

    public class RawSensorData
    {
        public static int indexof_IMU_GYRO = 0;
        public static int indexof_IMU_ACCELEROMETER = 1;
        public static int indexof_DEV_THUMB = 0;
        public static int indexof_DEV_INDEX = 1;
        public static int indexof_DEV_MIDDLE = 2;
        public static int indexof_DEV_RING = 3;
        public static int indexof_DEV_PINKY = 4;

        public readonly UInt32 timestamp;
        public readonly RawSensorData type;
        public readonly Point3[] points;

        public RawSensorData Create(RawSensorDataType _type, UInt32 _timestamp, byte[] dataArray, RawSensorSensitivity sensitivitiy)
        {
            List<Point3> pointsInProcess = new List<Point3>();
            string pointSensitivity = _type == RawSensorDataType.Device ? RawSensorSensitivity.kDeviceAccelerometer : RawSensorSensitivity.kIMUGyro;
            int pointsDataOffset = 0;
            int pointsDataLength = 6;
            while (pointsDataOffset  < dataArray.Length)
            {
                if (pointsDataOffset + pointsDataLength < dataArray.Length)
                {
                    double sens = sensitivitiy.getSensitivity(pointSensitivity);
                    Point3 point = new Point3.cre
                } else
                {
                    return null;
                }
            }

            

        }

    }
}
