using System;
using System.Threading.Tasks;
using Windows.Devices.Bluetooth;
using Windows.Devices.Bluetooth.GenericAttributeProfile;
using System.Diagnostics;
using Windows.Storage.Streams;


namespace TAPWin
{
    sealed class TAPDevice : IEquatable<TAPDevice>
    {
        internal struct TAPProperties
        {
            internal GattCharacteristic tapData;
            internal GattCharacteristic mouseData;
            internal GattCharacteristic nusRx;
            internal int fwVersion;
            public TAPProperties(GattCharacteristic tData, GattCharacteristic mData, GattCharacteristic nRx, int fw)
            {
                tapData = tData;
                mouseData = mData;
                nusRx = nRx;
                fwVersion = fw;
            }
        }
        
        private BluetoothLEDevice device;
        private GattCharacteristic rx;
        private GattCharacteristic tapData;
        private GattCharacteristic mouseData;

        bool tapDataValueChangedAssigned;
        bool mouseDataValueChangedAssigned;

        private bool isReady;
        private int fw;

        private TAPInputMode _inputMode;

        public TAPInputMode InputMode
        {
            get
            {
                return _inputMode;
            }
            set
            {
                _inputMode = value;

            }
        }

        internal event Action<TAPDevice, bool> OnTapReady;
        internal event Action<string,int> OnTapped;
        internal event Action<string,int,int, bool> OnMoused;
        public bool IsConnected
        {
            get
            {
                return this.device.ConnectionStatus == BluetoothConnectionStatus.Connected;
            }
        }

        private bool supportsMouse;

        public bool SupportsMouse
        {
            get
            {
                return this.supportsMouse;
            }
        }


        public string Identifier
        {
            get
            {
                return this.device.DeviceId;
            }
        }

        public string Name
        {
            get
            {
                return this.device.Name;
            }
        }

        public int FW
        {
            get
            {
                return this.fw;
            }
        }

        public bool IsReady
        {
            get
            {
                return this.isReady;
            }
        }

        //internal void Finalize()
        //{
        //    Debug.WriteLine("DESTRUCATOR");
        //    device.Dispose();
        //    device = null;
        //    tapData = null;
        //    rx = null;
        //    mouseData = null;
        //}
        private TAPDevice(BluetoothLEDevice d, TAPInputMode mode)
        {
            this.fw = 0;
            this.device = d;
            this.isReady = false;
            this.InputMode = mode;
            this.mouseData = null;
            this.supportsMouse = false;
            this.tapDataValueChangedAssigned = false;
            this.mouseDataValueChangedAssigned = false;
        }

        public bool Equals(TAPDevice other)
        {
            return this.Identifier == other.Identifier;
        }

        public override int GetHashCode()
        {
            return this.Identifier.GetHashCode();
        }

        public static string IdentifierFromBluetoothLEDevice(BluetoothLEDevice bleDevice)
        {
            return bleDevice.DeviceId;
        }

        internal void SetEventActions(Action<string, int> tapDataAction, Action<string,int,int,bool> mouseDataAction)
        {
            if (this.OnTapped == null)
            {
                this.OnTapped += tapDataAction;
            }
            if (this.OnMoused == null)
            {
                this.OnMoused += mouseDataAction;
            }
        }

        internal void Unready()
        {
            this.isReady = false;
        }

        public async void MakeReady()
        {

            if (tapData != null && rx != null && this.IsConnected)
            {
                
                GattCommunicationStatus status = await tapData.WriteClientCharacteristicConfigurationDescriptorAsync(GattClientCharacteristicConfigurationDescriptorValue.Notify);

                if (status == GattCommunicationStatus.Success)
                {
                    if (!this.tapDataValueChangedAssigned)
                    {
                        tapData.ValueChanged += OnTapDataValueChanged;
                        this.tapDataValueChangedAssigned = true;
                    }
                    
                    if (this.mouseData != null)
                    {
                        GattCommunicationStatus statusMouse = await mouseData.WriteClientCharacteristicConfigurationDescriptorAsync(GattClientCharacteristicConfigurationDescriptorValue.Notify);
                        if (statusMouse == GattCommunicationStatus.Success)
                        {
                            this.supportsMouse = true;
                            if (!this.mouseDataValueChangedAssigned)
                            {
                                mouseData.ValueChanged += onTapMouseValueChanged;
                                this.mouseDataValueChangedAssigned = true;
                            }
                            
                        }
                    }

                    if (OnTapReady != null)
                    {
                        this.isReady = true;
                        OnTapReady(this, true);
                        return;
                    }
                }
            }
            OnTapReady(this, false);
        }

        public async void sendMode(TAPInputMode overrideMode = TAPInputMode.Null)
        {
            
            TAPInputMode mode = overrideMode == TAPInputMode.Null ? this.InputMode : overrideMode;
            if (mode != TAPInputMode.Null)
            {
                if (this.isReady && this.IsConnected)
                {
                    if (mode != TAPInputMode.Null)
                    {

                        if (mode == TAPInputMode.Controller_With_MouseHID && this.fw < 010615) {
                            Debug.WriteLine("NOT SUPPORTED");
                            mode = TAPInputMode.Controller;
                        }

                        DataWriter writer = new DataWriter();

                        byte[] data = { 0x3, 0xc, 0x0, (byte)mode };
                        writer.WriteBytes(data);
                        TAPManagerLog.Instance.Log(TAPManagerLogEvent.Info, String.Format("TAP {0} ({1}) Sent mode ({2})", this.Name, this.Identifier, mode.ToString()));
                        GattCommunicationStatus result = await rx.WriteValueAsync(writer.DetachBuffer(), GattWriteOption.WriteWithoutResponse);
                    }
                }
            }


        }

        void onTapMouseValueChanged(GattCharacteristic sender, GattValueChangedEventArgs args)
        {
            if (this.OnMoused != null)
            {

                byte[] value = new byte[args.CharacteristicValue.Length];
                DataReader reader = DataReader.FromBuffer(args.CharacteristicValue);
                reader.ReadBytes(value);
                if (value.Length >= 10 && value[0] == 0)
                {
                    Int16 vx = (Int16)((Int16)(value[2] << 8) | (Int16)(value[1]));
                    Int16 vy = (Int16)((Int16)(value[4] << 8) | (Int16)(value[3]));
                    this.OnMoused(this.Identifier, vx, vy, value[9] == 1);
                }
            }

        }

        void OnTapDataValueChanged(GattCharacteristic sender, GattValueChangedEventArgs args)
        {
            if (this.OnTapped != null)
            {
                byte[] value = new byte[args.CharacteristicValue.Length];
                DataReader reader = DataReader.FromBuffer(args.CharacteristicValue);
                reader.ReadBytes(value);
                Debug.WriteLine("TAPPED " + value[0]);
                this.OnTapped(this.Identifier, value[0]);
                
            }

        }

        public String GetStringDescription()
        {
            return String.Format("{0} ({1})", this.Name, this.Identifier);
        }

        internal async Task Reconnect(TAPInputMode inputMode)
        {
            this._inputMode = inputMode;
            if (tapData == null && rx == null && !this.IsConnected && !this.IsReady)
            {
                TAPProperties properties = await GetTAPPropertiesAsync(this.device);
                if (properties.tapData != null && properties.nusRx != null)
                {
                    tapData = properties.tapData;
                    rx = properties.nusRx;
                    mouseData = properties.mouseData;
                    fw = properties.fwVersion;
                }
            }
            this.MakeReady();
        }

        internal static async Task<GattDeviceServicesResult> GetServicesAsync(BluetoothLEDevice d, int tryAgainCount, int tryAgainDelay)
        {
            GattDeviceServicesResult serviceResult = await d.GetGattServicesAsync(BluetoothCacheMode.Uncached);
            if (tryAgainCount <= 0)
            {
                return serviceResult;
            } else
            {
                if (serviceResult.Status == GattCommunicationStatus.AccessDenied)
                {
                    await Task.Delay(tryAgainDelay < 0 ? 0 : tryAgainDelay);
                    return await GetServicesAsync(d, tryAgainCount - 1, tryAgainDelay);
                } else
                {
                    return serviceResult;
                }
                
            }
        }

        internal static async Task<GattCharacteristicsResult> GetCharacteristicsAsync(GattDeviceService ser, int tryAgainCount, int tryAgainDelay)
        {
            GattCharacteristicsResult charResult = await ser.GetCharacteristicsAsync(BluetoothCacheMode.Uncached);
            if (tryAgainCount <= 0)
            {
                return charResult;
            } else
            {
                if (charResult.Status == GattCommunicationStatus.AccessDenied)
                {
                    await Task.Delay(tryAgainDelay < 0 ? 0 : tryAgainDelay);
                    return await GetCharacteristicsAsync(ser, tryAgainCount - 1, tryAgainDelay);
                } else
                {
                    return charResult;
                }
                
            }
        }

        internal static async Task<TAPProperties> GetTAPPropertiesAsync(BluetoothLEDevice d)
        {
            GattCharacteristic tapData = null;
            GattCharacteristic tapMouse = null;
            GattCharacteristic nusRx = null;
            int fwVersion = 0;
            
            GattDeviceServicesResult tapServicesResult = await GetServicesAsync(d, 10, 800);
            foreach (GattDeviceService ser in tapServicesResult.Services)
            {
                
                if (ser.Uuid == TAPGuids.service_tap)
                {

            
                    GattCharacteristicsResult tapCharacteristicsResult = await GetCharacteristicsAsync(ser, 10, 800);
                    foreach (GattCharacteristic ch in tapCharacteristicsResult.Characteristics)
                    {
                
                        if (ch.Uuid == TAPGuids.characteristic_tapdata)
                        {
                            tapData = ch;
                        }
                        else if (ch.Uuid == TAPGuids.characteristic_mousedata)
                        {
                            tapMouse = ch;
                        }

                    }
                }

                if (ser.Uuid == TAPGuids.service_nus)
                {

                    GattCharacteristicsResult nusCharacteristicsResult = await GetCharacteristicsAsync(ser, 10, 800);
                
                    foreach (GattCharacteristic ch in nusCharacteristicsResult.Characteristics)
                    {
                
                        if (ch.Uuid == TAPGuids.characteristic_rx)
                        {
                            nusRx = ch;
                        }
                    }

                }

                if (ser.Uuid == TAPGuids.service_device_information)
                {
                
                    GattCharacteristicsResult fwCharacteristicsResult = await GetCharacteristicsAsync(ser, 10, 800);
                    foreach (GattCharacteristic ch in fwCharacteristicsResult.Characteristics)
                    {
                
                        if (ch.Uuid == TAPGuids.characteristic_fw_version)
                        {
                            GattReadResult fwRead = await ch.ReadValueAsync();
                            if (fwRead.Status == GattCommunicationStatus.Success)
                            {
                                DataReader reader = DataReader.FromBuffer(fwRead.Value);
                                string str = reader.ReadString(fwRead.Value.Length);
                                fwVersion = VersionNumber.FromString(str);
                            }
                            
                        }
                    }

                }
            }

            return new TAPProperties(tapData, tapMouse, nusRx, fwVersion);
        }

        public static async Task<TAPDevice> FromBluetoothLEDeviceAsync(BluetoothLEDevice d, TAPInputMode inputMode)
        {

            TAPProperties properties = await GetTAPPropertiesAsync(d);
            
            if (properties.tapData != null && properties.nusRx != null)
            {
                TAPDevice t = new TAPDevice(d, inputMode);
                t.tapData = properties.tapData;
                t.rx = properties.nusRx;
                t.mouseData = properties.mouseData;
                t.fw = properties.fwVersion;
                return t;
            }

            return null;
        }
    }
}
