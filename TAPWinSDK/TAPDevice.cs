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
            internal GattCharacteristic airGestures;
            internal GattCharacteristic nusRx;
            internal GattCharacteristic nusTx;
            internal GattCharacteristic uiCommands;
            internal int fwVersion;
            public TAPProperties(GattCharacteristic tData, GattCharacteristic mData, GattCharacteristic nRx, GattCharacteristic nTx, GattCharacteristic uiCmds, GattCharacteristic airGest, int fw)
            {
                tapData = tData;
                mouseData = mData;
                nusRx = nRx;
                fwVersion = fw;
                uiCommands = uiCmds;
                airGestures = airGest;
                nusTx = nTx;
            }
        }
        
        private BluetoothLEDevice device;
        private GattCharacteristic rx;
        private GattCharacteristic tx;
        private GattCharacteristic tapData;
        private GattCharacteristic mouseData;
        private GattCharacteristic airGesturesData;
        private GattCharacteristic uiCommands;

        bool tapDataValueChangedAssigned;
        bool mouseDataValueChangedAssigned;
        bool nusTxValueChangedAssigned;
        bool airGestureDataValueChangedAssigned;

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
        internal event Action<string, bool> OnTapChangedAirGesturesState;
        internal event Action<string, TAPAirGesture> OnAirGestured;
        internal event Action<string, RawSensorData> OnRawSensorDataReceieved;

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

        private bool isInAirGestureState;

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
            this.nusTxValueChangedAssigned = false;
            this.airGestureDataValueChangedAssigned = false;
            this.isInAirGestureState = false;
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

        internal void SetEventActions(Action<string, int> tapDataAction, 
            Action<string,int,int,bool> mouseDataAction, 
            Action<String,bool> airGesturesStateAction, 
            Action<string,TAPAirGesture> airGestureDataAction,
            Action<string,RawSensorData> onRawSensorDataReceievedAction)
        {
            if (this.OnTapped == null)
            {
                this.OnTapped += tapDataAction;
            }
            if (this.OnMoused == null)
            {
                this.OnMoused += mouseDataAction;
            }
            if (this.OnTapChangedAirGesturesState == null)
            {
                this.OnTapChangedAirGesturesState += airGesturesStateAction;
            }
            if (this.OnAirGestured == null)
            {
                this.OnAirGestured += airGestureDataAction;
            }
            if (this.OnRawSensorDataReceieved == null)
            {
                this.OnRawSensorDataReceieved += onRawSensorDataReceievedAction;
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

                    if (this.tx != null)
                    {
                        GattCommunicationStatus statusTX = await this.tx.WriteClientCharacteristicConfigurationDescriptorAsync(GattClientCharacteristicConfigurationDescriptorValue.Notify);
                        if (statusTX == GattCommunicationStatus.Success)
                        {
                            if (!this.nusTxValueChangedAssigned)
                            {
                                this.tx.ValueChanged += OnTXValueChanged;
                                this.nusTxValueChangedAssigned = true;
                            }
                        }
                    }

                    if (this.airGesturesData != null)
                    {
                        GattCommunicationStatus statusAirGesture = await this.airGesturesData.WriteClientCharacteristicConfigurationDescriptorAsync(GattClientCharacteristicConfigurationDescriptorValue.Notify);
                        if (statusAirGesture == GattCommunicationStatus.Success)
                        {
                            if (!this.airGestureDataValueChangedAssigned)
                            {
                                this.airGesturesData.ValueChanged += OnAirGesturesDataValueChanged;
                                this.airGestureDataValueChangedAssigned = true;
                                this.RequestReadAirGesturesMode();
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

        public async void sendMode(TAPInputMode overrideMode = null)
        {
            if (this.isReady && this.IsConnected)
            {
                TAPInputMode mode = overrideMode == null ? this.InputMode : overrideMode;
                mode = mode.makeCompatibleWithFWVersion(this.fw);
                if (mode.isValid)
                {
                    byte[] data = mode.getBytes();
                    Debug.WriteLine("[{0}]", string.Join(", ", data));
                    DataWriter writer = new DataWriter();
                    writer.WriteBytes(data);
                    GattCommunicationStatus result = await rx.WriteValueAsync(writer.DetachBuffer(), GattWriteOption.WriteWithoutResponse);

                        

                        //if (mode != TAPInputMode.Null)
                        //{

                        //    if (mode == TAPInputMode.Controller_With_MouseHID && this.fw < 010615) {
                        //        Debug.WriteLine("NOT SUPPORTED");
                        //        mode = TAPInputMode.Controller;
                        //    }

                        //    DataWriter writer = new DataWriter();

                        //    byte[] data = { 0x3, 0xc, 0x0, (byte)mode };
                        //    writer.WriteBytes(data);
                        //    TAPManagerLog.Instance.Log(TAPManagerLogEvent.Info, String.Format("TAP {0} ({1}) Sent mode ({2})", this.Name, this.Identifier, mode.ToString()));
                        //    GattCommunicationStatus result = await rx.WriteValueAsync(writer.DetachBuffer(), GattWriteOption.WriteWithoutResponse);
                        //}
                    
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
            if (this.isInAirGestureState)
            {
                if (this.OnAirGestured != null)
                {
                    byte[] value = new byte[args.CharacteristicValue.Length];
                    DataReader reader = DataReader.FromBuffer(args.CharacteristicValue);
                    reader.ReadBytes(value);
                    TAPAirGesture airGesture = TAPAirGestureHelper.tapToAirGesture(value[0]);
                    if (airGesture != TAPAirGesture.Undefined)
                    {
                        this.OnAirGestured(this.Identifier, airGesture);
                    }
                }
            }
            else
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
            

        }

        void OnAirGesturesDataValueChanged(GattCharacteristic sender, GattValueChangedEventArgs args)
        {

            byte[] value = new byte[args.CharacteristicValue.Length];
            DataReader reader = DataReader.FromBuffer(args.CharacteristicValue);
            reader.ReadBytes(value);
            if (value.Length > 0)
            {
                byte first = value[0];
                if (first != 20)
                {
                    // Air Gestured
                    TAPAirGesture airGesture = TAPAirGestureHelper.intToAirGesture(first);
                    if (this.OnAirGestured != null && airGesture != TAPAirGesture.Undefined)
                    {
                        this.OnAirGestured(this.Identifier, airGesture);
                    } 
                } else if (value.Length > 1)
                {
                    // State
                    this.isInAirGestureState = value[1] == 1;
                    if (this.OnTapChangedAirGesturesState != null)
                    {
                        this.OnTapChangedAirGesturesState(this.Identifier, value[1] == 1);
                    }
                }
            }
        }

        void OnTXValueChanged(GattCharacteristic sender, GattValueChangedEventArgs args)
        {
            if (this.InputMode.isModeEquals(TAPInputMode.kRawSensor)  && this.InputMode.isValid)
            {
                byte[] value = new byte[args.CharacteristicValue.Length];
                DataReader reader = DataReader.FromBuffer(args.CharacteristicValue);
                reader.ReadBytes(value);
                RawSensorDataParser.ParseWhole(value, this.InputMode.sensitivity, ((rsData) =>
                {
                    if (this.OnRawSensorDataReceieved != null)
                    {
                        this.OnRawSensorDataReceieved(this.Identifier, rsData);
                    }
                }));
            }
        }

        internal async void RequestReadAirGesturesMode()
        {
            if (this.airGesturesData == null || !this.IsConnected)
            {
                return;
            }
            byte[] data = new byte[20];
            data[0] = 13;
            for (int i=1; i<data.Length; i++)
            {
                data[i] = 0;
            }
            DataWriter writer = new DataWriter();
            writer.WriteBytes(data);
            GattCommunicationStatus result = await this.airGesturesData.WriteValueAsync(writer.DetachBuffer(), GattWriteOption.WriteWithoutResponse);
        }

        internal async void Vibrate(int[] durations)
        {
            if (durations == null || this.uiCommands == null || !this.isReady || !this.IsConnected)
            {
                return;
            }
            byte[] data = new byte[20];
            data[0] = 0;
            data[1] = 2;
            for (int i = 0; i < data.Length-2; i++)
            {
                if (i < durations.Length)
                {
                    data[i+2] = (byte)((double)durations[i] / (double)10.0);
                } else
                {
                    data[i+2] = 0;
                }

            }
            DataWriter writer = new DataWriter();
            writer.WriteBytes(data);
            
            GattCommunicationStatus result = await this.uiCommands.WriteValueAsync(writer.DetachBuffer(), GattWriteOption.WriteWithoutResponse);
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
            GattCharacteristic nusTx = null;
            GattCharacteristic airGestures = null;
            GattCharacteristic uiCommands = null;

            int fwVersion = 0;
            
            GattDeviceServicesResult tapServicesResult = await GetServicesAsync(d, 10, 800);
            foreach (GattDeviceService ser in tapServicesResult.Services)
            {
                
                if (ser.Uuid == TAPGuids.service_tap)
                {
                    GattCharacteristicsResult tapCharacteristicsResult = await GetCharacteristicsAsync(ser, 10, 800);
                    foreach (GattCharacteristic ch in tapCharacteristicsResult.Characteristics)
                    {
                        TAPManagerLog.Instance.Log(TAPManagerLogEvent.Info, String.Format("Found Characteristic {0} for Service {1}", TAPGuids.GetCharacteristicNameByGUID(ch.Uuid), TAPGuids.GetServiceNameByGUID(ser.Uuid)));
                        if (ch.Uuid == TAPGuids.characteristic_tapdata)
                        {
                            tapData = ch;
                        }
                        else if (ch.Uuid == TAPGuids.characteristic_mousedata)
                        {
                            tapMouse = ch;
                        }
                        else if (ch.Uuid == TAPGuids.characteristic_airgesturesdata)
                        {
                            airGestures = ch;
                        } else if (ch.Uuid == TAPGuids.characteristic_uicommands)
                        {
                            uiCommands = ch;
                        }

                    }
                }

                if (ser.Uuid == TAPGuids.service_nus)
                {
                    
                    GattCharacteristicsResult nusCharacteristicsResult = await GetCharacteristicsAsync(ser, 10, 800);
                
                    foreach (GattCharacteristic ch in nusCharacteristicsResult.Characteristics)
                    {
                        TAPManagerLog.Instance.Log(TAPManagerLogEvent.Info, String.Format("Found Characteristic {0} for Service {1}", TAPGuids.GetCharacteristicNameByGUID(ch.Uuid), TAPGuids.GetServiceNameByGUID(ser.Uuid)));
                        if (ch.Uuid == TAPGuids.characteristic_rx)
                        {
                            nusRx = ch;
                        }
                        else if (ch.Uuid == TAPGuids.characteristic_tx)
                        {
                            nusTx = ch;
                        }
                    }

                }

                if (ser.Uuid == TAPGuids.service_device_information)
                {
                    
                    GattCharacteristicsResult fwCharacteristicsResult = await GetCharacteristicsAsync(ser, 10, 800);
                    foreach (GattCharacteristic ch in fwCharacteristicsResult.Characteristics)
                    {
                        TAPManagerLog.Instance.Log(TAPManagerLogEvent.Info, String.Format("Found Characteristic {0} for Service {1}", TAPGuids.GetCharacteristicNameByGUID(ch.Uuid), TAPGuids.GetServiceNameByGUID(ser.Uuid)));
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

            return new TAPProperties(tapData, tapMouse, nusRx, nusTx, uiCommands, airGestures, fwVersion);
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
                t.airGesturesData = properties.airGestures;
                t.uiCommands = properties.uiCommands;
                t.tx = properties.nusTx;
                return t;
            }

            return null;
        }
    }
}
