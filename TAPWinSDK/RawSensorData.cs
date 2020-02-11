using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

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
            return new Point3((double)i_x * sensitivityFactor, (double)i_y * sensitivityFactor, (double)i_z * sensitivityFactor);
        }

        public override string ToString()
        {
            return String.Format("( {0:0.00}, {1:0.00}, {2:0.00} )", this.x, this.y, this.z);
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
        public readonly RawSensorDataType type;
        public readonly Point3[] points;


        private RawSensorData(RawSensorDataType _type, UInt32 _timestamp, Point3[] _points)
        {
            this.timestamp = _timestamp;
            this.type = _type;
            this.points = _points;
        }

        public Point3 GetPoint(int index)
        {
            if (this.points == null)
            {
                return null;
            }
            if (index >= 0 && index < this.points.Length)
            {
                return this.points[index];
            } else
            {
                return null;
            }
        }

        public override string ToString()
        {
            if (this.type == RawSensorDataType.None) {
                return "Unknown Type";
            }
            string typeString = this.type == RawSensorDataType.Device ? "Device" : "IMU";
            string pointsString = "";
            for (int i=0; i<this.points.Length; i++)
            {
                pointsString = pointsString + " " + this.points[i].ToString();
            }
            return String.Format("Timestamp: {0}, Type: {1}, Points: {2}", this.timestamp.ToString(), typeString, pointsString);
        }

        public static RawSensorData Create(RawSensorDataType _type, UInt32 _timestamp, byte[] dataArray, RawSensorSensitivity sensitivitiy)
        {
            if (_type == RawSensorDataType.None)
            {
                return null;
            }
            List<Point3> pointsInProcess = new List<Point3>();
            string pointSensitivity = _type == RawSensorDataType.Device ? RawSensorSensitivity.kDeviceAccelerometer : RawSensorSensitivity.kIMUGyro;
            int pointsDataOffset = 0;
            int pointsDataLength = 6;
            
            while (pointsDataOffset  < dataArray.Length)
            {
                if (pointsDataOffset + pointsDataLength-1 < dataArray.Length)
                {
                    double sens = sensitivitiy.getSensitivity(pointSensitivity);
                    Point3 point = Point3.Create(dataArray.Skip(pointsDataOffset).Take(pointsDataLength).ToArray(), sens);
                    if (point != null)
                    {
                        pointsInProcess.Add(point);
                    } else
                    {
                        return null;
                    }
                    pointsDataOffset += pointsDataLength;
                    if (_type == RawSensorDataType.IMU)
                    {
                        pointSensitivity = RawSensorSensitivity.kIMUAccelerometer;
                    }
                } else
                {
                    return null;
                }
            }

            if (_type == RawSensorDataType.IMU)
            {
                if (pointsInProcess.Count != 2)
                {
                    return null;
                }
            } else if (_type == RawSensorDataType.Device)
            {
                if (pointsInProcess.Count != 5)
                {
                    return null;
                }
            }

            return new RawSensorData(_type, _timestamp, pointsInProcess.ToArray());
        }

    }
}
