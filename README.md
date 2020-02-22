# TAP WINDOWS SDK

## What Is This ?

TAP WINDOWS SDK allows you to build a a native Windows application that can receive input from TAP devices,
In a way that each tap is being interpreted as an array or fingers that are tapped, or a binary combination integer (explanation follows), Thus allowing the TAP device to act as a controller for your app!

## Integration

Download the SDK open-source code from github, and compile it to create TAPWin.dll file.
The SDK is written in C#.

Add TAPWIn.dll as a reference to your project.

Importing TAPWin into your source code:
```c#
    using TAPWin
```

## Implementing TAP Manager Events:

The SDK uses events to call the neccessary functions;

Events:

### OnTapConnected

```c#
void OnTapConnected(string identifier, string name, int fw)
```

Called when a TAP device is connected to the iOS device, sending the TAP identifier, it's display name and the firmware version of the TAP.
Each TAP device has an identifier (a unique string) to allow you to keep track of all the taps that are connected
(if for example you're developing a multiplayer game, you need to keep track of the players).
This identifier is used in the rest of the functions.

* The SDK does NOT scan for TAP devices, therefor the use must pair the devices to the computer first.

```c#
TAPManager.Instance.OnTapConnected += OnTapConnected;
```

### OnTapDisconnected

```c#
void OnTapDisconnected(string identifier)
```

Called when a TAP device is disconnected from the computer device, sending the TAP identifier.

### OnTapped

```c#
void OnTapped(string identifier, int tapcode)
```

This is where magic will happen.
This function will tell you which TAP was being tapped (identifier), and which fingers are tapped (tapcode)
tapcode is a number between 1 and 31.
It's binary form represents the fingers that are tapped.
The LSB is thumb finger, the MSB (bit number 5) is the pinky finger.
For example: if tapcode equls 3 - it's binary form is 00101,
Which means that the thumb and the middle fingers are tapped.

```c#
TAPManager.Instance.OnTapped += OnTapped;
```

### OnMoused

```c#
void OnMoused(string identifier, int vx, int vy, bool isMouse)
```

This function will be called when the user is using the TAP as a mouse.
velocityX and velocityY are the velocities of the mouse movement. These values can be multiplied by a constant to simulate "mouse sensitivity" option.
isMouse is a boolean that determine if the movement is real (true) or falsely detected by the TAP (false).

```c#
TAPManager.Instance.OnMoused += OnMoused;
```

```c#
void OnAirGestured(string identifier, TAPAirGesture airGesture)
```

This function will be called when the user is performing Air Gesture with the TAP.

```c#
enum TAPAirGesture : int
    {
        Undefined = -1000,
        OneFingerUp = 2,
        TwoFingersUp = 3,
        OnefingerDown = 4,
        TwoFingersDown = 5,
        OneFingerLeft = 6,
        TwoFingersLeft = 7,
        OneFingerRight = 8,
        TwoFingersRight = 9,
        IndexToThumbTouch = 10,
        MiddleToThumbTouch = 11
    }
```

```c#
void OnChangedAirGestureState(string identifier, bool isInAirGestureState)
```
This function will be called when the user's TAP is entering/leaving Air Gesture State.


```c#
void OnRawSensorDataReceieved(string identifier, RawSensorData rsData)
```
In raw sensors mode, the TAP continuously sends raw data from the following sensors:
    1. Five 3-axis accelerometers on each finger ring.
    2. IMU (3-axis accelerometer + gyro) located on the thumb (**for TAP Strap 2 only**).

```c#
void OnRawSensorDataReceieved(string identifier, RawSensorData rsData)
        {
            // RawSensorData has a timestamp, type and an array of points(x,y,z)
            if (rsData.type == RawSensorDataType.Device) {
                Point3 thumb = rsData.GetPoint(RawSensorData.indexof_DEV_THUMB);
                if (thumb != null)
                {
                    // thumb.x, thumb.y, thumb.z ...
                }
                // Etc.. use indexes: RawSensorData.indexof_DEV_THUMB, RawSensorData.indexof_DEV_INDEX, RawSensorData.indexof_DEV_MIDDLE, RawSensorData.indexof_DEV_RING, RawSensorData.indexof_DEV_PINKY
            }
            else if (rsData.type == RawSensorDataType.IMU)
            {
                Point3 gyro = rsData.GetPoint(RawSensorData.indexof_IMU_GYRO);
                if (gyro != null)
                {
                    // gyro.x, gyro.y, gyro.z ...
                }
                // Etc.. use indexes: RawSensorData.indexof_IMU_GYRO, RawSensorData.indexof_IMU_ACCELEROMETER
            }
        }
```

[For more information about raw sensor mode click here](https://tapwithus.atlassian.net/wiki/spaces/TD/pages/792002574/Tap+Strap+Raw+Sensors+Mode)

## Start TAP Manager

After you've done registering to the desired events, you need to call the "Start" function.
The "Start" function should be called once, usually in the main screen where you need the OnTapConnected callback.

```c#
    TAPManager.Instance.Start();
```


## TAPInputMode

```c#
void SetTapInputMode(TAPInputMode newInputMode, string identifier = "");
```

Each TAP has a mode in which it works as.
Four modes available: 
CONTROLLER MODE (Default) 
    allows receiving the "tapped" and "moused" func callbacks in TAPKitDelegate with the fingers combination without any post-processing.
    
TEXT MODE 
    the TAP device will behave as a plain bluetooth keyboard, "tapped" and "moused" funcs in TAPKitDelegate will not be called.
CONTROLLER MODE WITH MOUSE HiD 
    Same as controller mode but allows the user to use the mouse also as a regular mouse input.
    Starting iOS 13, Apple added Assitive Touch feature. (Can be toggled within accessibility settings on iPhone).
    This adds a cursor to the screen that can be navigated using TAP device. 

RAW SENSOR DATA MODE
    This will stream the sensors (Gyro and Accelerometer) values. More or that later ...

When a TAP device is connected it is by default set to controller mode.

if "identifier" is empty string, the mode will be applied to all connected taps.

## Setting TAP Modes:

```c#
TAPManager.Instance.SetTapInputMode(TAPInputMode.Controller());
TAPManager.Instance.SetTapInputMode(TAPInputMode.Text());
TAPManager.Instance.SetTapInputMode(TAPInputMode.ControllerWithMouseHID());
```
### Setting Default Input Mode:

If you wish - You can change the default TAPInputMode so new connected devices will be set to this mode, with an option to apply this mode to current connected devices;

```c#
void SetDefaultInputMode(TAPInputMode newDefaultInputMode, bool applyToCurrentTaps)
```

# Raw Sensor Mode

In raw sensors mode, the TAP continuously sends raw data from the following sensors:
    1. Five 3-axis accelerometers on each finger ring.
    2. IMU (3-axis accelerometer + gyro) located on the thumb (**for TAP Strap 2 only**).
        
### To put a TAP into Raw Sensor Mode:
```c#
byte deviceAccelerometerSensitivity = 1;
byte imuGyroSensitivity = 2;
byte imuAccelerometerSensitivity = 1;
TAPManager.Instance.SetTapInputMode(TAPInputMode.RawSensor(new RawSensorSensitivity(deviceAccelerometerSensitivity, imuGyroSensitivity, imuAccelerometerSensitivity)));
```

When puting TAP in Raw Sensor Mode, the sensitivities of the values can be defined by the developer.
    deviceAccelerometerSensitivity refers to the sensitivities of the fingers' accelerometers. Range: 1 to 4.
    imuGyroSensitivity refers to the gyro sensitivity on the thumb's sensor. Range: 1 to 4.
    imuAccelerometerSensitivity refers to the accelerometer sensitivity on the thumb's sensor. Range: 1 to 5.

Using the default sensitivities:
```c#
TAPManager.Instance.SetTapInputMode(TAPInputMode.RawSensor(new RawSensorSensitivity()));
```

## Vibrations/Haptic

Send Haptic/Vibration to TAP devices.
```c#
public void Vibrate(int[] durations, string identifier = "")
```
durations: An array of durations in the format of haptic, pause, haptic, pause ... You can specify up to 18 elements in this array. The rest will be ignored.
Each array element is defined in milliseconds.
When identifier is an empty string - all TAPS will vibrate.
Example:
```c#
TAPManager.Instance.Vibrate(new int[] { 500, 100, 500 });
```
Will send two 500 milliseconds haptics with a 100 milliseconds pause in the middle.

## Example APP

The Visual Studio Solution contains an example app where you can see how to use the features of TAP SDK.

## Support

Please refer to the issues tab! :)

# Have fun!





