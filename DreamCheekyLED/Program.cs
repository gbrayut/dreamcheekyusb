using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DreamCheekyUSB
{
    class Program
    {
        static void Main(string[] args)
        {
            int actions = 0; //Track if any actions are executed
            DreamCheekyLED led = null;
            try
            {
                if (args.ContainsInsensitive("debug"))
                {
                    System.Diagnostics.Trace.Listeners.Add(new System.Diagnostics.TextWriterTraceListener(Console.Out));
                }

                string devicearg = args.StartsWith("device=").FirstOrDefault();
                if (string.IsNullOrEmpty(devicearg))
                {
                    Console.WriteLine("\r\nConnecting to DreamCheekyLED using default values...");
                    led = new DreamCheekyLED();
                }
                else
                {
                    Console.WriteLine("\r\nConnecting to DreamCheekyLED using specified device...");
                    string[] deviceSplit = devicearg.Substring(7).Split(',');
                    if (deviceSplit.Length == 1)
                    {
                        led = new DreamCheekyLED(deviceSplit[0]); //One argument = device path
                    }
                    else
                    {
                        //Two or Three arguments = VID,PID,Count=0
                        int devicecount = 0;
                        if (deviceSplit.Length > 2)
                        {
                            devicecount = int.Parse(deviceSplit[2]);
                        }

                        int VID = int.Parse(deviceSplit[0].Substring(2), System.Globalization.NumberStyles.HexNumber);
                        int PID = int.Parse(deviceSplit[1].Substring(2), System.Globalization.NumberStyles.HexNumber);

                        led = new DreamCheekyLED(VID, PID, devicecount);
                    }
                }

                if (args.ContainsInsensitive("test"))
                {
                    actions++;
                    Console.WriteLine("Testing USBLED...");
                    bool result = led.Test();
                    Console.WriteLine("Testing USBLED Result: " + result);
                    System.Threading.Thread.Sleep(1000);
                }


                if (args.ContainsInsensitive("testblink"))
                {
                    actions++;
                    Console.WriteLine("Blinking test...");
                    bool result = led.TestBlink();
                    Console.WriteLine("Blinking test result: " + result);
                }

                string rgbarg = args.StartsWith("rgb=").FirstOrDefault();
                if (!string.IsNullOrEmpty(rgbarg))
                {
                    actions++;
                    string rgb = rgbarg.Substring(4);
                    Console.WriteLine("Setting RGB Color: {0}", rgb);
                    bool result = led.SetColor(rgb);
                    Console.WriteLine("Result: " + result);
                }

                string colorarg = args.StartsWith("color=").FirstOrDefault();
                string fadearg = args.StartsWith("fade=").FirstOrDefault();
                string blinkarg = args.StartsWith("blink=").FirstOrDefault();
                System.Drawing.Color color = System.Drawing.Color.Empty;
                if (!string.IsNullOrEmpty(colorarg))
                {
                    string strcolor = colorarg.Substring(6);
                    Console.WriteLine("Setting System Color: {0}", strcolor);

                    if (led.TryParseNametoColor(strcolor, out color))
                    {
                        if (string.IsNullOrEmpty(fadearg) && string.IsNullOrEmpty(blinkarg)) //Only set color if fade and blink options are not set
                        {
                            actions++;
                            bool result = led.SetColor(color);
                            Console.WriteLine("Result: " + result);
                        }
                    }
                    else
                    {
                        Console.WriteLine("Error: Unknown color '{0}'. See http://www.flounder.com/csharp_color_table.htm.", color);
                    }
                }

                if (!string.IsNullOrEmpty(fadearg))
                {
                    if (string.IsNullOrEmpty(colorarg))
                    {
                        Console.WriteLine("Error... Must set color when using fade options\r\n");
                        return;
                    }
                    else
                    {
                        actions++;
                        string[] fadesplit = fadearg.Substring(5).Split(',');
                        int totalms = int.Parse(fadesplit[0]);
                        int inout = 0;
                        if (fadesplit.ContainsInsensitive("in"))
                        {
                            inout = 1;
                        }
                        else if (fadesplit.ContainsInsensitive("out"))
                        {
                            inout = 2;
                        }

                        switch (inout)
                        {
                            case 1:
                                System.Console.WriteLine("Fading color in. TotalMS=" + totalms);
                                led.FadeIn(color, totalms);
                                break;
                            case 2:
                                System.Console.WriteLine("Fading color out. TotalMS=" + totalms);
                                led.FadeOut(color, totalms);
                                break;
                            default:
                                System.Console.WriteLine("Fading color in and then out. TotalMS=" + totalms);
                                led.FadeInOut(color, totalms);
                                break;
                        }
                    }
                }

                if (!string.IsNullOrEmpty(blinkarg))
                {
                    if (string.IsNullOrEmpty(colorarg))
                    {
                        Console.WriteLine("Error... Must set color when using blink option\r\n");
                        return;
                    }
                    else
                    {
                        actions++;
                        string[] blinksplit = blinkarg.Substring(6).Split(',');
                        int count = int.Parse(blinksplit[0]);
                        int blinkms = 500;
                        if (blinksplit.Length > 1)
                        {
                            blinkms = int.Parse(blinksplit[1]);
                        }
                        System.Console.WriteLine("Blinking color. Count={0} Blinkms={1}", count, blinkms);
                        led.Blink(color, count, blinkms);
                    }
                }

                string delayarg = args.StartsWith("delay=").FirstOrDefault();
                if (!string.IsNullOrEmpty(delayarg))
                {
                    actions++;
                    int delay = int.Parse(delayarg.Substring(6));
                    Console.WriteLine("Delay for {0}ms", delay);
                    System.Threading.Thread.Sleep(delay);
                }

                if (args.ContainsInsensitive("off"))
                {
                    actions++;
                    Console.WriteLine("Turning led off");
                    led.Off();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("\r\n\r\nError: " + ex.Message + "\r\n\r\n");
            }
            finally
            {
                if (led != null)
                {
                    led.Dispose();
                }
            }

            //Pause on exit or display usage syntax
            if (actions > 0)
            {
                Console.WriteLine("Finished\r\n");
            }
            else //No actions specified, show help
            {
                Console.WriteLine("\r\nUsage:");
                Console.WriteLine("  DreamCheekyLED.exe [device=...] [options] [rgb=xxx,xxx,xxx] [color=....]");
                Console.WriteLine("                     [fade=...] [blink=...] [delay=xxxx] [off]");

                Console.WriteLine("\r\nExamples:");
                Console.WriteLine("  DreamCheekyLED.exe debug nopause color=red");
                Console.WriteLine("  DreamCheekyLED.exe debug nopause color=green fade=3000");
                Console.WriteLine("  DreamCheekyLED.exe debug nopause color=green fade=\"3000,in\"");
                Console.WriteLine("  DreamCheekyLED.exe debug nopause color=blue blink=2");
                Console.WriteLine("  DreamCheekyLED.exe debug nopause color=blue blink=\"5,250\"");
                Console.WriteLine("  DreamCheekyLED.exe debug nopause rgb=\"255,255,0\" delay=5000 off");
                Console.WriteLine("  DreamCheekyLED.exe debug nopause color=yellow fade=\"3000,in\" delay=5000 off");

                Console.WriteLine("\r\n\r\nDevice Path:");
                Console.WriteLine("  Optional, Defaults to first USB device with VID=0x1D34 and PID=0x0004");
                Console.WriteLine("  Example (VID,PID,Index): device=\"0x1D34,0x0004,0\"");
                Console.WriteLine("  Example (Path): device=" + @"""\\?\hid#vid_1d34&pid_0004#6&1067c3dc&0&0000#{4d1e55b2-f16f-11cf-88cb-001111000030}""");

                Console.WriteLine("\r\nOptions:");
                Console.WriteLine("  debug = Print trace statements to Console.Out");
                Console.WriteLine("  test = Cycle Red/Green/Blue then fade Red/Green/Blue");
                Console.WriteLine("  testblink = test a few blink cycles");
                Console.WriteLine("  nopause = omit the 'Press enter to exit...' message at the end");

                Console.WriteLine("\r\nColors: (See http://www.flounder.com/csharp_color_table.htm )");
                Console.WriteLine("  Use rgb=xxx,xxx,xxx or one of the .NET System Colors");
                Console.WriteLine("  Example (yellow): rgb=\"255,255,0\"");
                Console.WriteLine("  Example (System.Drawing.Color.Pink): color=DeepPink");
                Console.WriteLine();

                Console.WriteLine("\r\nFade: set to fade in, out, or both");
                Console.WriteLine("  Makes the colors fade in or out instead of instantly on or off.");
                Console.WriteLine("  Example (Fade in and out in 2 seconds): color=Indigo fade=2000");
                Console.WriteLine("  Example (Fade in 1 second): color=Indigo fade=\"1000,in\"");
                Console.WriteLine("  Example (Fade out 3 seconds): color=Indigo fade=\"3000,out\"");

                Console.WriteLine("\r\nBlink: will blink specified color");
                Console.WriteLine("  Example (Blink twice at default 500ms): color=LimeGreen blink=5");
                Console.WriteLine("  Example (Blink 5 times at 200ms each): color=LimeGreen blink=\"5,250\"");

                Console.WriteLine("\r\nDelay: Add a delay in milliseconds.");
                Console.WriteLine();
            }

            if (!args.ContainsInsensitive("nopause"))
            {
                Console.WriteLine("\r\nPress enter to exit...");
                Console.ReadLine();
            }
        }
    }

    /// <summary>
    /// Extenstions for working with string arrays
    /// </summary>
    public static class StringArrayExtenstions
    {
        public static bool ContainsInsensitive(this string[] args, string Name)
        {
            return args.Contains(Name, StringComparer.CurrentCultureIgnoreCase);
        }

        public static IEnumerable<string> StartsWith(this string[] args, string Value, StringComparison options = StringComparison.CurrentCultureIgnoreCase)
        {
            var q = from a in args
                    where a.StartsWith(Value, options)
                    select a;
            return q;
        }
    }
}
