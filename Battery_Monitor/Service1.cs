using System;
using System.Management;
using System.Speech.Synthesis;
using System.ServiceProcess;
using System.Timers;

namespace Battery_Monitor
{
    public partial class BatteryMonitorService : ServiceBase
    {
        private Timer timer;
        private SpeechSynthesizer synthesizer;
        private bool isBatteryFullNotified = false;
        private bool isBatteryLowNotified = false;

        public BatteryMonitorService()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            timer = new Timer();
            timer.Interval = 60000; // Check every minute
            timer.Elapsed += Timer_Elapsed;
            timer.Start();

            synthesizer = new SpeechSynthesizer();
        }

        protected override void OnStop()
        {
            timer.Stop();
            timer.Dispose();
        }

        private void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            CheckBatteryStatus();
        }

        private void CheckBatteryStatus()
        {
            var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_Battery");
            var battery = searcher.Get().OfType<ManagementObject>().FirstOrDefault();

            if (battery != null)
            {
                int batteryLevel = Convert.ToInt32(battery["EstimatedChargeRemaining"]);

                if (batteryLevel > 90)
                {
                    if (!isBatteryFullNotified)
                    {
                        synthesizer.Speak("Battery is full, please unplug the charger.");
                        isBatteryFullNotified = true;
                        isBatteryLowNotified = false;
                    }
                }
                else if (batteryLevel <= 10)
                {
                    if (!isBatteryLowNotified)
                    {
                        synthesizer.Speak("Battery is low, please plug in the charger.");
                        isBatteryLowNotified = true;
                        isBatteryFullNotified = false;
                    }
                }
                else
                {
                    isBatteryLowNotified = false;
                    isBatteryFullNotified = false;
                }
            }
        }
    }
}
