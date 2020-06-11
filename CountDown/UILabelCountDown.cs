using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using CoreAnimation;
using CoreGraphics;
using Foundation;
using UIKit;

namespace CountDown
{
    [Register("UILabelCountDown"), DesignTimeVisible(true)]
    public class UILabelCountDown : UILabel
    {
        private int start { get; set; }
        private int stop { get; set; }
        private int interval { get; set; }
        private int future { get; set; }
        private int progression { get; set; }
        private bool repeat { get; set; }
        private string startText { get; set; }
        private string stopText { get; set; }
        private bool CounterEnd { get; set; }

        private int fixedStop = -11111111;
        private int lastnumber;
        private bool isViewVisible = true, futureDone;
        public event FinishEvent Finish;
        CADisplayLink displayLink;

        [Export("Repeat"), Browsable(true)]
        public bool Repeat
        {
            get { return repeat; }
            set
            {
                repeat = value;
                SetText();
            }
        }

        [Export("Start"), Browsable(true)]
        public int Start
        {
            get { return start; }
            set
            {
                start = value;
                SetText();
            }
        }

        [Export("Stop"), Browsable(true)]
        public int Stop
        {
            get { return stop; }
            set
            {
                stop = value;
                SetText();
            }
        }

        [Export("Progression"), Browsable(true)]
        public int Progression
        {
            get { return progression; }
            set
            {
                progression = value;
                SetText();
            }
        }

        [Export("Interval"), Browsable(true)]
        public int Interval
        {
            get { return interval; }
            set
            {
                interval = value;
                SetText();
            }
        }

        [Export("Future"), Browsable(true)]
        public int Future
        {
            get { return future; }
            set
            {
                future = value;
                futureDone = false;
                if (future > 0)
                    futureDone = true;
                SetText();
            }
        }

        [Export("StartText"), Browsable(true)]
        public string StartText
        {
            get { return stopText; }
            set
            {
                startText = value;
                SetText();
            }
        }

        [Export("StopText"), Browsable(true)]
        public string StopText
        {
            get { return stopText; }
            set
            {
                stopText = value;
            }
        }

        [Export("init")]
        public UILabelCountDown()
        {
            init();
        }

        [Export("initWithFrame:")]
        public UILabelCountDown(CGRect frame) : base(frame)
        {
        }

        [Export("initWithCoder:")]
        public UILabelCountDown(NSCoder coder) : base(coder)
        {
        }

        public UILabelCountDown(IntPtr handle) : base(handle)
        {
        }

        public bool IsViewVisible
        {
            get { return isViewVisible; }
            set
            {
                isViewVisible = value;
            }
        }

        private void init()
        {
            stop = fixedStop;
            SetText();
        }

        private void SetText()
        {
            CloseTimer();
            try
            {
                Text = string.IsNullOrEmpty(startText) ? start.ToString() : startText;
                lastnumber = start;
                SetFuture();
                SetTextSub();
            }
            catch (Exception ex)
            {
                Console.WriteLine("CountDown:UILabelCountDown:SetText:" + ex.Message);
            }
        }

        static int lastelapsed = 0;
        private void SetTextSub()
        {
            if (Hidden == false && Alpha > 0 && future > 0)
            {
                CloseTimer();
                CounterEnd = false;
                var animationStartDate = DateTime.Now;
                displayLink = CADisplayLink.Create(() => {
                    var dateNow = DateTime.Now;
                    TimeSpan ts = DateTime.Now - animationStartDate;
                    var elapsedTime = (int)ts.TotalMilliseconds;
                    if (elapsedTime > future)
                    {
                        try
                        {
                            SendEvent();
                            if (!repeat)
                            {
                                Text = !string.IsNullOrEmpty(stopText) ? stopText : stop == fixedStop ? lastnumber.ToString() : stop.ToString();
                                CloseTimer();
                            }
                            else
                            {
                                SetText();
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine("CountDown:UILabelCountDown:SetText:" + ex.Message);
                        }
                    }
                    else
                    {
                        if (elapsedTime % interval<20 && (elapsedTime - lastelapsed)>(interval-100))
                        {
                            lastelapsed = elapsedTime;
                            try
                            {
                                lastnumber = lastnumber + progression;
                                if (isViewVisible && Alpha > 0 && !Hidden)
                                    Text = lastnumber.ToString();
                                if (lastnumber == stop)
                                {
                                    SendEvent();
                                    CloseTimer();
                                    Text = !string.IsNullOrEmpty(stopText) ? stopText : stop == fixedStop ? lastnumber.ToString() : stop.ToString();
                                }
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine("CountDown:TextViewCounter:SetText:" + ex.Message);
                            }
                        }
                    }
                });
                displayLink.AddToRunLoop(NSRunLoop.Current, NSRunLoopMode.Default);
            }
        }

        public override bool Hidden
        {
            get => base.Hidden; set
            {
                base.Hidden = value;
                CloseTimer();
            }
        }

        public override nfloat Alpha
        {
            get => base.Alpha; set
            {
                base.Alpha = value;
                CloseTimer();
            }
        }

        private void CloseTimer()
        {
            try
            {
                if (displayLink != null)
                {
                    displayLink.Paused = true;
                    displayLink.Invalidate();
                    displayLink.Dispose();
                    displayLink = null;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("CountDown:UILabelCountDown:CloseTimer:" + ex.Message);
            }
        }

        private void SetFuture()
        {
            if (future < 0 || !futureDone)
            {
                int diff = Math.Abs(start - (stop == fixedStop ? 0 : stop));
                future = diff * 1000;
            }
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            CloseTimer();
        }

        private void SendEvent()
        {
            CounterEnd = true;
            Finish?.Invoke();
        }
    }

    public delegate void FinishEvent();
}
