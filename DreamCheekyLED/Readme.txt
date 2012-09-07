DreamCheekyLED is a Console / .NET driver for the Dream Cheeky Webmail Notifier (http://www.dreamcheeky.com/webmail-notifier)
It was created using the https://github.com/mikeobrien/HidLibrary/ and is released under the Apache License V2.0

You can control the LED using either DreamCheekyLED.exe with command line arguments or via C#/VB/Powershell using 
the DreamCheekyUSB.DreamCheekyLED object. The code supports multiple devices and has options for blinking and fading.
See below for command line examples or the DreamCheekyLED*.ps1 files for powershell examples.

If you are interested in how this works at a high level the general process is:
1. Create HidDevice object using HidLibrary.HidDevices.Enumerate (default VendorID=0x1D34 and ProductID=0x0004)

2. Send the following USB commands to initialize the device: 
NOTE: These were found by using USBTrace http://www.sysnucleus.com/ to sniff the Dreamcheeky software and may vary
        public static readonly byte[] init01 = new byte[9] { 0, 0x1F, 0x02, 0x00, 0x5F, 0x00, 0x00, 0x1F, 0x03 };
        public static readonly byte[] init02 = new byte[9] { 0, 0x00, 0x02, 0x00, 0x5F, 0x00, 0x00, 0x1F, 0x04 };
        public static readonly byte[] init03 = new byte[9] { 0, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x1F, 0x05 };
        public static readonly byte[] init04 = new byte[9] { 0, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x01 };

3. Set desired color using following USB commands (or combinations of RGB for full color spectrum)
		public static readonly byte[] cmd_Red =   new byte[9] { 0, 0xFF, 0x00, 0x00, 0x00, 0x00, 0x00, 0x1F, 0x05 };
        public static readonly byte[] cmd_Green = new byte[9] { 0, 0x00, 0xFF, 0x00, 0x00, 0x00, 0x00, 0x1F, 0x05 };
        public static readonly byte[] cmd_Blue =  new byte[9] { 0, 0x00, 0x00, 0xFF, 0x00, 0x00, 0x00, 0x1F, 0x05 };
        public static readonly byte[] cmd_Off =   new byte[9] { 0, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x1F, 0x05 }; 
NOTE: The DreamCheekyLED intensity maxes out at 64, so 0xFF is the same as 0x40. This driver will scale the input 
values of each color componet (range 0-255) to the correct output (range 0-64).

4. The DreamCheekyUSB.LEDBase class has additional methods for Blinking, Fade In/Out/InOut, and converting system
colors to RGB values.


Command Line Usage:
  DreamCheekyLED.exe [device=...] [options] [rgb=xxx,xxx,xxx] [color=....]
                     [fade=...] [blink=...] [delay=xxxx] [off]

Examples:
  DreamCheekyLED.exe debug nopause color=red
  DreamCheekyLED.exe debug nopause color=green fade=3000
  DreamCheekyLED.exe debug nopause color=green fade="3000,in"
  DreamCheekyLED.exe debug nopause color=blue blink=2
  DreamCheekyLED.exe debug nopause color=blue blink="5,250"
  DreamCheekyLED.exe debug nopause rgb="255,255,0" delay=5000 off
  DreamCheekyLED.exe debug nopause color=yellow fade="3000,in" delay=5000 off

Device Path:
  Optional, Defaults to first USB device with VID=0x1D34 and PID=0x0004
  Example (VID,PID,Index): device="0x1D34,0x0004,0"
  Example (Path): device="\\?\hid#vid_1d34&pid_0004#6&1067c3dc&0&0000#{4d1e55b2-f16f-11cf-88cb-001111000030}"

Options:
  debug = Print trace statements to Console.Out
  test = Cycle Red/Green/Blue then fade Red/Green/Blue
  testblink = test a few blink cycles
  nopause = omit the 'Press enter to exit...' message at the end

Colors: (See http://www.flounder.com/csharp_color_table.htm )
  Use rgb=xxx,xxx,xxx or one of the .NET System Colors
  Example (yellow): rgb="255,255,0"
  Example (System.Drawing.Color.Pink): color=DeepPink


Fade: set to fade in, out, or both
  Makes the colors fade in or out instead of instantly on or off.
  Example (Fade in and out in 2 seconds): color=Indigo fade=2000
  Example (Fade in 1 second): color=Indigo fade="1000,in"
  Example (Fade out 3 seconds): color=Indigo fade="3000,out"

Blink: will blink specified color
  Example (Blink twice at default 500ms): color=LimeGreen blink=5
  Example (Blink 5 times at 200ms each): color=LimeGreen blink="5,250"

Delay: Add a delay in milliseconds. Program will sleep before returning.