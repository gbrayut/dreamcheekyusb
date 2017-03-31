**Project Description**
DreamCheekyUSB provides a Console App and .NET drivers for the Dream Cheeky Webmail Notifier and the Dream Cheeky Iron Man USB Stress Button.

MOVED TO https://github.com/gbrayut/dreamcheekyusb

DreamCheekyUSB was created using the [https://github.com/mikeobrien/HidLibrary/](https://github.com/mikeobrien/HidLibrary/) and is released under the Apache License V2.0

You can control the LED using either DreamCheekyLED.exe with command line arguments or via C#/VB/Powershell using the DreamCheekyUSB.DreamCheekyLED object. The code supports multiple devices and has options for blinking and fading. See [DreamCheekyLED](DreamCheekyLED) for command line examples or the DreamCheekyLED*.ps1 files for powershell examples.

The DreamCheekyBTN.exe has command line arguments that will let you run a program whenever the button is pressed or convert the button press events into a keyboard macro that can perform an action or be picked up by other programs like AutoHotKey. The code should also support multiple devices, and can be run multiple times for additional triggers. See [DreamCheekyBTN](DreamCheekyBTN) for command line examples or the AutoHotKey.ahk file for a sample AutoHotKey script.


