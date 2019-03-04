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
Two modes available:
CONTROLLER MODE (Default) - allows receiving the "tapped" event callback.
TEXT MODE - the TAP device will behave as a plain bluetooth keyboard, "tapped" event will NOT be called..

When a TAP device is connected it is by default set to controller mode.

* In this SDK, (.Windows), The controller mode allows the TAP to also behave as a mouse, So the user will be able to navigate through the UI buttons with TAP.

If you wish for a TAP to act as a bluetooth keyboard and allow the user to enter text input in your app, you can set the mode:

```c#
    TAPManager.Instance.SetTapInputMode(TAPInputMode.Text);
```

The first parameter is the mode. You can use any of these modes: TAPInputMode.Controller, TAPInputMode.Text
the second parameter is the identifier of the TAP that will change it's mode. If it's empty string, ALL taps connected will change their mode.

Just don't forget to switch back to controller mode after the user has entered the text :

```c#
TAPManager.Instance.SetTapInputMode(TAPInputMode.Controller);
```

## Example APP

The Visual Studio Solution contains an example app where you can see how to use the features of TAP SDK.

## Support

Please refer to the issues tab! :)

# Have fun!





