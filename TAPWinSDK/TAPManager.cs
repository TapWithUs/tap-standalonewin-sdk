using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Enumeration;
using Windows.Devices.Bluetooth;
using System.Diagnostics;

namespace TAPWin
{
    public class TappedEventArgs : EventArgs
    {
        public string id;
        public int tapCode;
    }

    public sealed class TAPManager
    {
        private static readonly TAPManager instance = new TAPManager();

        private TAPInputMode defaultInputMode;
        private TAPInputMode inputModeWhenDeactivated;

        
        private bool activated;

        System.Threading.Timer __modeTimer;
        
        private DeviceWatcher deviceWatcher;

        
        private string property_IsConnected = "System.Devices.Aep.IsConnected";
        private string property_IsPaired = "System.Devices.Aep.IsPaired";
        
        public event Action<string, string, int> OnTapConnected;
        public event Action<string> OnTapDisconnected;
        public event Action<string, int> OnTapped;
        public event Action<string, int, int,bool> OnMoused;
        public event Action<string, bool> OnChangedAirGestureState;
        public event Action<string, TAPAirGesture> OnAirGestured;
        public event Action<string, RawSensorData> OnRawSensorDataReceieved;

        private Dictionary<string, TAPDevice> taps;
        private HashSet<string> pending;

        private bool started;
        private bool restartWatcher;

        

        static TAPManager()
        {
        }

        private TAPManager()
        {
            
            this.activated = false;
            this.defaultInputMode = TAPInputMode.Controller();
            this.inputModeWhenDeactivated = TAPInputMode.Text();
            this.started = false;
            this.taps = new Dictionary<string, TAPDevice>();
            this.pending = new HashSet<string>();
            this.restartWatcher = false;
        }

        
        public static TAPManager Instance
        {
            get
            {
                return instance;
            }
        }

        public void DoNothing()
        {

        }
        
        public void Start()
        {
            if (!started)
            {
                
                string aqsConnectedFilter = BluetoothLEDevice.GetDeviceSelectorFromConnectionStatus(BluetoothConnectionStatus.Connected);
                string[] requestedProperties = { "System.Devices.Aep.IsPaired", "System.Devices.Aep.IsConnected", "System.Devices.Aep.DeviceAddress" };
                deviceWatcher = DeviceInformation.CreateWatcher(aqsConnectedFilter, requestedProperties, DeviceInformationKind.AssociationEndpoint);
                deviceWatcher.Added += dw_added;
                deviceWatcher.Removed += dw_removed;
                deviceWatcher.Stopped += dw_stopped;
                deviceWatcher.EnumerationCompleted += dw_enum_completed;
                this.started = true;
                this.activated = true;
                
                this.__modeTimer = new System.Threading.Timer(__ModeTimerTick, null, 0, 5000);
                
                TAPManagerLog.Instance.Log(TAPManagerLogEvent.Info, "TAP Manager Started.");
                this.RestartDeviceWatcher();
                
            }

        }

        private void SendModeToAllTaps()
        {
            foreach (KeyValuePair<string, TAPDevice> kv in this.taps)
            {

                if (this.activated)
                {
                    
                    kv.Value.sendMode();
                }
                else
                {
                    
                    kv.Value.sendMode(this.inputModeWhenDeactivated);
                }

            }
        }

        private async void __ModeTimerTick(object state)
        {
            if (this.activated)
            {
                this.SendModeToAllTaps();
            }
        }

        private void ModeTimerTick(object sender, object e)
        {
            
            this.SendModeToAllTaps();
        }

        private async void dw_stopped(DeviceWatcher sender, Object o)
        {
            TAPManagerLog.Instance.Log(TAPManagerLogEvent.Info, "Device Watcher Stopped.");
            if (this.restartWatcher)
            {
                this.restartWatcher = false;
                
                this.deviceWatcher.Start();

            }
        }

        private async void dw_enum_completed(DeviceWatcher sender, Object o)
        {
            TAPManagerLog.Instance.Log(TAPManagerLogEvent.Info, "Device Watcher Enumeration Completed.");
            
        }


        private async void dw_removed(DeviceWatcher sender, DeviceInformationUpdate deviceInfoUpdate)
        {
            //if (deviceInfoUpdate.Id == this.sampleID)
            //{
            //    Debug.WriteLine("REMOVED " + deviceInfoUpdate.Id);
            //}

            pending.Remove(deviceInfoUpdate.Id);
            TAPDevice tap;
            this.taps.TryGetValue(deviceInfoUpdate.Id, out tap);
            if (tap != null)
            {
                
                TAPManagerLog.Instance.Log(TAPManagerLogEvent.Info, "TAP disconnected: " + tap.GetStringDescription());

                //TAPDevice d = taps[deviceInfoUpdate.Id];
                //d.Finalize();
                //d = null;
                //taps.Remove(deviceInfoUpdate.Id);
                tap.Unready();
                
                if (OnTapDisconnected != null)
                {
                    OnTapDisconnected(deviceInfoUpdate.Id);
                }
                
            }
            //tap = null;
            //taps.Remove(deviceInfoUpdate.Id);
            

        }

        private void RestartDeviceWatcher()
        {
            if (started)
            {
                this.restartWatcher = true;
                if (this.deviceWatcher.Status == DeviceWatcherStatus.Created || this.deviceWatcher.Status == DeviceWatcherStatus.Stopped || this.deviceWatcher.Status == DeviceWatcherStatus.Aborted)
                {
                    this.restartWatcher = false;
                    TAPManagerLog.Instance.Log(TAPManagerLogEvent.Info, "Device Watcher Starting");
                    this.deviceWatcher.Start();

                }
                else if (this.deviceWatcher.Status != DeviceWatcherStatus.Stopping || this.deviceWatcher.Status == DeviceWatcherStatus.EnumerationCompleted)
                {
                    this.restartWatcher = true;
                    TAPManagerLog.Instance.Log(TAPManagerLogEvent.Info, "Device Watcher Stopping");
                    this.deviceWatcher.Stop();
                }
            }

        }

        public void Activate()
        {
            TAPManagerLog.Instance.Log(TAPManagerLogEvent.Info, "TAPManager received WindowActivated event.");
            this.activated = true;
            this.SendModeToAllTaps();
            this.RestartDeviceWatcher();
        }

        public void Deactivate()
        {
            TAPManagerLog.Instance.Log(TAPManagerLogEvent.Info, "TAPManager received WindowDeactivated event. ");
            this.activated = false;
            this.SendModeToAllTaps();
        }
        
        internal void AppTerminated()
        {

        }

        private void OnTapReady(TAPDevice tap, bool success)
        {
            this.pending.Remove(tap.Identifier);
            if (success)
            {
                TAPManagerLog.Instance.Log(TAPManagerLogEvent.Info, "TAP is Ready:" + tap.GetStringDescription());
                tap.sendMode();
                tap.SetEventActions(this.OnTapTapped, this.OnTapMoused, this.OnTapChangedAirGestureState, this.OnTapAirGestured, this.OnTapRawSensorDataReceived);
                
                
                if (this.OnTapConnected != null)
                {
                    this.OnTapConnected(tap.Identifier, tap.Name, tap.FW);
                }

            }
            else
            {
                TAPManagerLog.Instance.Log(TAPManagerLogEvent.Error, "Couldn't make TAP Ready: " + tap.GetStringDescription());
                this.taps.Remove(tap.Identifier);
            }
        }

        private void OnTapMoused(string identifier, int vx, int vy, bool isMouse)
        {
            if (!this.activated)
            {
                return;
            }
            
            if (this.OnMoused != null)
            {

                this.OnMoused(identifier, vx, vy, isMouse);
            }
        }

        private void OnTapTapped(string identifier, int tapCode)
        {
        
            if (!this.activated)
            {
                return;
            }
            
            if (this.OnTapped != null)
            {

                this.OnTapped(identifier, tapCode);
            }
        }

        private void OnTapChangedAirGestureState(string identifier, bool isInAirGestureState)
        {
            if (!this.activated)
            {
                return;
            }
            if (this.OnChangedAirGestureState != null)
            {
                this.OnChangedAirGestureState(identifier, isInAirGestureState);
            }
        }

        private void OnTapAirGestured(string identifier, TAPAirGesture airGesture)
        {
            if (!this.activated)
            {
                return;
            }
            if (this.OnAirGestured != null)
            {
                this.OnAirGestured(identifier, airGesture);
            }
        }

        private void OnTapRawSensorDataReceived(string identifier, RawSensorData data)
        {
            if (!this.activated)
            {
                return;
            }
            if (this.OnRawSensorDataReceieved != null)
            {
                this.OnRawSensorDataReceieved(identifier, data);
            }

        }

        public int GetPendingCount()
        {
            return this.pending.Count;
        }

        public int GetTapsCount()
        {
            return this.taps.Where(kvp => kvp.Value.IsReady && kvp.Value.IsConnected).Count();
            
        }

        

        private async void dw_added(DeviceWatcher sender, DeviceInformation deviceInfo)
        {

            TAPManagerLog.Instance.Log(TAPManagerLogEvent.Info, "Device Watcher added proc.");
            if ((bool)deviceInfo.Properties[this.property_IsConnected] == true && (bool)deviceInfo.Properties[this.property_IsPaired] == true)
            {
                
                if (taps.ContainsKey(deviceInfo.Id) && !pending.Contains(deviceInfo.Id))
                {
                    if (taps[deviceInfo.Id].IsConnected)
                    {
                        TAPManagerLog.Instance.Log(TAPManagerLogEvent.Info, string.Format("TAP {0} Reconnected. Making it ready...", taps[deviceInfo.Id].GetStringDescription()));
                        await taps[deviceInfo.Id].Reconnect(this.defaultInputMode);
                        //taps[deviceInfo.Id].MakeReady();
                        //this.OnTapReady(taps[deviceInfo.Id], true);
                    }
                    return;
                }

                TAPManagerLog.Instance.Log(TAPManagerLogEvent.Info, string.Format("Found Connected Device {0}", deviceInfo.Name));
                if (!taps.ContainsKey(deviceInfo.Id) && !pending.Contains(deviceInfo.Id))
                {
                    TAPManagerLog.Instance.Log(TAPManagerLogEvent.Info, string.Format("{0} Is a new device. Checking if it's a TAP", deviceInfo.Name));
                    pending.Add(deviceInfo.Id);
                    BluetoothLEDevice device = await BluetoothLEDevice.FromIdAsync(deviceInfo.Id);
                    
                    
                    TAPDevice t = await TAPDevice.FromBluetoothLEDeviceAsync(device, this.defaultInputMode);
                    if (t != null)
                    {
                        TAPManagerLog.Instance.Log(TAPManagerLogEvent.Info, "Found a connected TAP: " + t.GetStringDescription() + ". Making it ready...");
                        t.OnTapReady += OnTapReady;
                        taps.Add(deviceInfo.Id, t);
                        t.MakeReady();

                    }
                    else
                    {
                        TAPManagerLog.Instance.Log(TAPManagerLogEvent.Info, string.Format("{0} Is Not a TAP", deviceInfo.Name));
                        pending.Remove(deviceInfo.Id);
                    }
                 
                    

                }

            }
            
        }

        private void PerformTapAction(Action<TAPDevice> action, string identifier = "")
        {
            if (!identifier.Equals(""))
            {
                TAPDevice tap;
                if (this.taps.TryGetValue(identifier, out tap))
                {
                    action(tap);

                }
            }
            else
            {
                foreach (KeyValuePair<string, TAPDevice> kv in this.taps)
                {
                    TAPDevice tap = kv.Value;
                    action(tap);
                }
            }
        }

        public void Vibrate(int[] durations, string identifier = "")
        {
            this.PerformTapAction((tap) =>
            {
                tap.Vibrate(durations);
            });
        }

        public void SetDefaultInputMode(TAPInputMode newDefaultInputMode, bool applyToCurrentTaps)
        {
            this.defaultInputMode = newDefaultInputMode;
            if (applyToCurrentTaps)
            {
                this.PerformTapAction((tap) =>
                {
                    tap.InputMode = newDefaultInputMode;
                    if (this.activated)
                    {
                        tap.sendMode();
                    }
                    else
                    {
                        tap.sendMode(this.inputModeWhenDeactivated);
                    }
                });
            }
        }

        public void SetTapInputMode(TAPInputMode newInputMode, string identifier = "")
        {
            if (!newInputMode.isValid)
            {
                return;
            }
            this.PerformTapAction((tap) =>
            {
                tap.InputMode = newInputMode;
                if (this.activated)
                {
                    tap.sendMode();
                } else
                {
                    tap.sendMode(this.inputModeWhenDeactivated);
                }
            });

            

        }


    }
}
