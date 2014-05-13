using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace DreamCheekyUSB {
	class Program {
		static void Main(string[] args) {
			int actions = 0; //Track if any actions are executed
			DreamCheekyLED led = null;
			try {
				if (args.ContainsInsensitive("debug")) {
					Trace.Listeners.Add(new TextWriterTraceListener(Console.Out));
				}

				string devicearg = args.StartsWith("device=").FirstOrDefault();
				if (string.IsNullOrEmpty(devicearg)) {
					Trace.WriteLine("\r\nConnecting to DreamCheekyLED using default values...");
					led = new DreamCheekyLED();
				} else {
					Trace.WriteLine("\r\nConnecting to DreamCheekyLED using specified device...");
					string[] deviceSplit = devicearg.Substring(7).Split(',');
					if (deviceSplit.Length == 1) {
						led = new DreamCheekyLED(deviceSplit[0]); //One argument = device path
					} else {
						//Two or Three arguments = VID,PID,Count=0
						int devicecount = 0;
						if (deviceSplit.Length > 2) {
							devicecount = int.Parse(deviceSplit[2]);
						}

						int VID = int.Parse(deviceSplit[0].Substring(2), System.Globalization.NumberStyles.HexNumber);
						int PID = int.Parse(deviceSplit[1].Substring(2), System.Globalization.NumberStyles.HexNumber);

						led = new DreamCheekyLED(VID, PID, devicecount);
					}
				}

				if (args.ContainsInsensitive("test")) {
					actions++;
					Trace.WriteLine("Testing USBLED...");
					bool result = led.Test();
					Trace.WriteLine("Testing USBLED Result: " + result);
					System.Threading.Thread.Sleep(1000);
				}


				if (args.ContainsInsensitive("testblink")) {
					actions++;
					Trace.WriteLine("Blinking test...");
					bool result = led.TestBlink();
					Trace.WriteLine("Blinking test result: " + result);
				}

				string rgbarg = args.StartsWith("rgb=").FirstOrDefault();
				if (!string.IsNullOrEmpty(rgbarg)) {
					actions++;
					string rgb = rgbarg.Substring(4);
					Trace.WriteLine("Setting RGB Color: {0}", rgb);
					bool result = led.SetColor(rgb);
					Trace.WriteLine("Result: " + result);
				}

				string colorarg = args.StartsWith("color=").FirstOrDefault();
				string fadearg = args.StartsWith("fade=").FirstOrDefault();
				string blinkarg = args.StartsWith("blink=").FirstOrDefault();
				System.Drawing.Color color = System.Drawing.Color.Empty;
				if (!string.IsNullOrEmpty(colorarg)) {
					string strcolor = colorarg.Substring(6);
					Trace.WriteLine("Setting System Color: {0}", strcolor);

					if (led.TryParseNametoColor(strcolor, out color)) {
						if (string.IsNullOrEmpty(fadearg) && string.IsNullOrEmpty(blinkarg)) { //Only set color if fade and blink options are not set
							actions++;
							bool result = led.SetColor(color);
							Trace.WriteLine("Result: " + result);
						}
					} else {
						Trace.WriteLine("Error: Unknown color '{0}'. See http://www.flounder.com/csharp_color_table.htm.", strcolor);
					}
				}

				if (!string.IsNullOrEmpty(fadearg)) {
					if (string.IsNullOrEmpty(colorarg)) {
						Trace.WriteLine("Error... Must set color when using fade options\r\n");
						return;
					} else {
						actions++;
						string[] fadesplit = fadearg.Substring(5).Split(',');
						int totalms = int.Parse(fadesplit[0]);
						int inout = 0;
						if (fadesplit.ContainsInsensitive("in")) {
							inout = 1;
						} else if (fadesplit.ContainsInsensitive("out")) {
							inout = 2;
						}

						switch (inout) {
						case 1:
							Trace.WriteLine("Fading color in. TotalMS=" + totalms);
							led.FadeIn(color, totalms);
							break;
						case 2:
							Trace.WriteLine("Fading color out. TotalMS=" + totalms);
							led.FadeOut(color, totalms);
							break;
						default:
							Trace.WriteLine("Fading color in and then out. TotalMS=" + totalms);
							led.FadeInOut(color, totalms);
							break;
						}
					}
				}

				if (!string.IsNullOrEmpty(blinkarg)) {
					if (string.IsNullOrEmpty(colorarg)) {
						Trace.WriteLine("Error... Must set color when using blink option\r\n");
						return;
					} else {
						actions++;
						string[] blinksplit = blinkarg.Substring(6).Split(',');
						int count = int.Parse(blinksplit[0]);
						int blinkms = 500;
						if (blinksplit.Length > 1) {
							blinkms = int.Parse(blinksplit[1]);
						}
						Trace.WriteLine(String.Format("Blinking color. Count={0} Blinkms={1}", count, blinkms));
						led.Blink(color, count, blinkms);
					}
				}

				string delayarg = args.StartsWith("delay=").FirstOrDefault();
				if (!string.IsNullOrEmpty(delayarg)) {
					actions++;
					int delay = int.Parse(delayarg.Substring(6));
					Trace.WriteLine("Delay for {0}ms", Convert.ToString(delay));
					System.Threading.Thread.Sleep(delay);
				}

				if (args.ContainsInsensitive("off")) {
					actions++;
					Trace.WriteLine("Turning led off");
					led.Off();
				}
			} catch (Exception ex) {
				Trace.WriteLine("\r\n\r\nError: " + ex.Message + "\r\n\r\n");
			} finally {
				if (led != null) {
					led.Dispose();
				}
			}

			//Pause on exit or display usage syntax
			if (actions > 0) {
				Trace.WriteLine("Finished\r\n");
			} else { //No actions specified, show help
				Trace.WriteLine("\r\nUsage:");
				Trace.WriteLine("  DreamCheekyLED.exe [device=...] [options] [rgb=xxx,xxx,xxx] [color=....]");
				Trace.WriteLine("                     [fade=...] [blink=...] [delay=xxxx] [off]");

				Trace.WriteLine("\r\nExamples:");
				Trace.WriteLine("  DreamCheekyLED.exe debug nopause color=red");
				Trace.WriteLine("  DreamCheekyLED.exe debug nopause color=green fade=3000");
				Trace.WriteLine("  DreamCheekyLED.exe debug nopause color=green fade=\"3000,in\"");
				Trace.WriteLine("  DreamCheekyLED.exe debug nopause color=blue blink=2");
				Trace.WriteLine("  DreamCheekyLED.exe debug nopause color=blue blink=\"5,250\"");
				Trace.WriteLine("  DreamCheekyLED.exe debug nopause rgb=\"255,255,0\" delay=5000 off");
				Trace.WriteLine("  DreamCheekyLED.exe debug nopause color=yellow fade=\"3000,in\" delay=5000 off");

				Trace.WriteLine("\r\n\r\nDevice Path:");
				Trace.WriteLine("  Optional, Defaults to first USB device with VID=0x1D34 and PID=0x0004");
				Trace.WriteLine("  Example (VID,PID,Index): device=\"0x1D34,0x0004,0\"");
				Trace.WriteLine("  Example (Path): device=" + @"""\\?\hid#vid_1d34&pid_0004#6&1067c3dc&0&0000#{4d1e55b2-f16f-11cf-88cb-001111000030}""");

				Trace.WriteLine("\r\nOptions:");
				Trace.WriteLine("  debug = Print trace statements to Console.Out");
				Trace.WriteLine("  test = Cycle Red/Green/Blue then fade Red/Green/Blue");
				Trace.WriteLine("  testblink = test a few blink cycles");
				Trace.WriteLine("  nopause = omit the 'Press enter to exit...' message at the end");

				Trace.WriteLine("\r\nColors: (See http://www.flounder.com/csharp_color_table.htm )");
				Trace.WriteLine("  Use rgb=xxx,xxx,xxx or one of the .NET System Colors");
				Trace.WriteLine("  Example (yellow): rgb=\"255,255,0\"");
				Trace.WriteLine("  Example (System.Drawing.Color.Pink): color=DeepPink");
				Trace.WriteLine("\r\n");

				Trace.WriteLine("\r\nFade: set to fade in, out, or both");
				Trace.WriteLine("  Makes the colors fade in or out instead of instantly on or off.");
				Trace.WriteLine("  Example (Fade in and out in 2 seconds): color=Indigo fade=2000");
				Trace.WriteLine("  Example (Fade in 1 second): color=Indigo fade=\"1000,in\"");
				Trace.WriteLine("  Example (Fade out 3 seconds): color=Indigo fade=\"3000,out\"");

				Trace.WriteLine("\r\nBlink: will blink specified color");
				Trace.WriteLine("  Example (Blink twice at default 500ms): color=LimeGreen blink=5");
				Trace.WriteLine("  Example (Blink 5 times at 250ms each): color=LimeGreen blink=\"5,250\"");

				Trace.WriteLine("\r\nDelay: Add a delay in milliseconds.");
				Trace.WriteLine("\r\n");
			}

			if (!args.ContainsInsensitive("nopause")) {
				Trace.WriteLine("\r\nPress enter to exit...");
				Console.ReadLine();
			}
		}
	}

	/// <summary>
	/// Extenstions for working with string arrays
	/// </summary>
	public static class StringArrayExtenstions {
		public static bool ContainsInsensitive(this string[] args, string name) {
			return args.Contains(name, StringComparer.CurrentCultureIgnoreCase);
		}

		public static IEnumerable<string> StartsWith(this string[] args, string value, StringComparison options = StringComparison.CurrentCultureIgnoreCase) {
			var q = from a in args
			        where a.StartsWith(value, options)
			        select a;
			return q;
		}
	}
}
