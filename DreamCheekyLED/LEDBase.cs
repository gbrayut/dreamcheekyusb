using System;
namespace DreamCheekyUSB
{
    public abstract class LEDBase : IDisposable
    {
        /// <summary>
        /// HidLibray.HidDevice for this USBLED instance
        /// </summary>
        public HidLibrary.HidDevice hidLED;

        internal bool initialized = false;
        internal virtual bool Initialize() { //Should be extended by base classes
            if (initialized) { return true; }
            if (hidLED == null) { throw new NullReferenceException("hidLED not initialized"); }
            initialized = true;
            return true;
        }

        public void Dispose()
        {
            if (hidLED != null)
            {
                hidLED.Dispose();
            }
        }

        public abstract bool Write(byte[] data);
        public abstract bool SetColor(System.Drawing.Color Color);
        public abstract bool SetColor(byte Red, byte Green, byte Blue);
        /// <summary>
        /// Accepts rgb value as a string in xxx,xxx,xxx format. Will convert to byte values.
        /// </summary>
        /// <param name="rgb">Example: 255,255,0 for Yellow</param>
        /// <returns>True if success, False otherwise.</returns>
        public bool SetColor(string rgb)
        {
            try
            {
                var rxRGB = System.Text.RegularExpressions.Regex.Match(rgb, "^([0-9]|[1-9][0-9]|1[0-9][0-9]|2[0-4][0-9]|25[0-5]),([0-9]|[1-9][0-9]|1[0-9][0-9]|2[0-4][0-9]|25[0-5]),([0-9]|[1-9][0-9]|1[0-9][0-9]|2[0-4][0-9]|25[0-5])$");
                if (rxRGB.Success)
                {
                    byte red = byte.Parse(rxRGB.Groups[1].Value);
                    byte green = byte.Parse(rxRGB.Groups[2].Value);
                    byte blue = byte.Parse(rxRGB.Groups[3].Value);
                    return SetColor(red, green, blue);
                }
                else
                {
                    System.Diagnostics.Trace.WriteLine("Invalid RGB string: " + rgb);
                    return false;   
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.WriteLine("Error in SetColor: " + ex.Message);
                return false;
            }
        }

        public bool Off() { return SetColor(0, 0, 0); }
        /// <summary>
        /// Cycle Red, Green, Blue, then fade Red, Green Blue
        /// </summary>
        /// <returns></returns>
        public virtual bool Test()
        {
            bool bReturn = true;
            bReturn &= SetColor(255,0,0);
            System.Threading.Thread.Sleep(250);
            bReturn &= SetColor(0,255,0);
            System.Threading.Thread.Sleep(250);
            bReturn &= SetColor(0,0,255);
            System.Threading.Thread.Sleep(250);
            bReturn &= Off();
            System.Threading.Thread.Sleep(250);
            bReturn &= FadeInOut(System.Drawing.Color.Red,1000);
            System.Threading.Thread.Sleep(100);
            bReturn &= FadeInOut(System.Drawing.Color.Green,1000);
            System.Threading.Thread.Sleep(100);
            bReturn &= FadeInOut(System.Drawing.Color.Blue,1000);
            return bReturn;
        }

        public virtual bool TestBlink()
        {
            bool bReturn = true;
            bReturn &= Blink(System.Drawing.Color.Red, 1, 500);
            bReturn &= Blink(System.Drawing.Color.Orange, 1, 500);
            bReturn &= Blink(System.Drawing.Color.Yellow, 1, 500);
            bReturn &= Blink(System.Drawing.Color.Green, 1, 500);
            bReturn &= Blink(System.Drawing.Color.Blue, 1, 500);
            bReturn &= Blink(System.Drawing.Color.Indigo, 1, 500);
            bReturn &= Blink(System.Drawing.Color.Violet, 1, 500);
            return Off() && bReturn;
        }
        

        public bool FadeInOut(System.Drawing.Color ToColor, int Totalms = 2000)
        {
            if (Totalms <= 0) { throw new ArgumentOutOfRangeException("Totalms must be greater than zero"); }
            FadeIn(ToColor, Totalms / 2);
            return FadeOut(ToColor, Totalms / 2);
        }

        public void FadeIn(System.Drawing.Color ToColor, int Totalms = 1000)
        {
            if (Totalms <= 0) { throw new ArgumentOutOfRangeException("Totalms must be greater than zero"); }
            int t = 0;
            int step = 35;
            float ratio = 0;
            while (t < Totalms)
            {
                System.Threading.Thread.Sleep(step);
                ratio = (float)t / (float)Totalms;
                byte red = (byte)(ToColor.R * ratio);
                byte green = (byte)(ToColor.G * ratio);
                byte blue = (byte)(ToColor.B * ratio);
                SetColor(red, green, blue);
                t += step;
            }
        }
        public bool FadeOut(System.Drawing.Color FromColor, int Totalms = 1000)
        {
            if (Totalms <= 0) { throw new ArgumentOutOfRangeException("Totalms must be greater than zero"); }
            var t = 0;
            int step = 35;
            float ratio = 0;
            while (t < Totalms)
            {
                System.Threading.Thread.Sleep(step);
                ratio = (float)t / (float)Totalms;
                byte red = (byte)(FromColor.R - (FromColor.R * ratio));
                byte green = (byte)(FromColor.G - (FromColor.G * ratio));
                byte blue = (byte)(FromColor.B - (FromColor.B * ratio));
                SetColor(red, green, blue);
                t += step;
            }
            return Off();
        }

        public bool Blink(System.Drawing.Color Color, int Count = 1, int Blinkms = 500)
        {
            if (Count <= 0 || Blinkms <= 0) { throw new ArgumentOutOfRangeException("Count and TotalMS cannot be less than zero"); };
            int i = 0;
            bool bReturn = true;
            while (i < Count)
            {
                bReturn &= FadeInOut(Color, Blinkms);
                i++;
            }
            return bReturn;
        }

        public bool TryParseNametoColor(string Name, out System.Drawing.Color Result)
        {
            Result = System.Drawing.Color.FromName(Name);
            if (Result.IsKnownColor || Result.IsSystemColor)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}

