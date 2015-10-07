using System;
using System.Threading.Tasks;
using Windows.Devices.Gpio;
using Windows.UI.Xaml.Controls;
using Autofac;
using Serilog;
using Stateless;

namespace PiStriker
{
    public sealed partial class MainPage : Page
    {
        private readonly StateMachine<Modes, Modes> _stateMachine = new StateMachine<Modes, Modes>(Modes.InitMode);

        private readonly IHardware _hardware;
        private readonly ILogger _log;

        private bool[] _results = new bool[14];

        public MainPage()
        {
            InitializeComponent();
            
            var containerBuilder = new ContainerBuilder();

            containerBuilder.RegisterModule(new SerilogModule());

            containerBuilder.RegisterType<Hardware>().As<IHardware>();
            
            var container = containerBuilder.Build();

            using (var lifetimeScope = container.BeginLifetimeScope())
            {
                _log = lifetimeScope.Resolve<ILogger>();
                _hardware = lifetimeScope.Resolve<IHardware>();
            }
            
            _stateMachine.Configure(Modes.InitMode).Permit(Modes.Next, Modes.QuiteMode);
            _stateMachine.Configure(Modes.PlayMode).Permit(Modes.Next, Modes.QuiteMode);
            _stateMachine.Configure(Modes.QuiteMode).Permit(Modes.Next, Modes.PlayMode);

            _stateMachine.Configure(Modes.PlayMode).OnEntry(CountEvents);
            

            if (_hardware.InitializeHardware().Result)
            {
                _hardware.FirstSenorPin.ValueChanged += FirstSenorPinValueChanged;
                _hardware.ThirdSenorPin.ValueChanged += ThirdSenorPinValueChanged;

                _stateMachine.Fire(Modes.Next);
            }

            // Register for the unloaded event so we can clean up upon exit
            Unloaded += MainPage_Unloaded;

        }

        private void MainPage_Unloaded(object sender, object args)
        {
            // Cleanup
            _hardware.FirstSenorPin.Dispose();
            _hardware.ThirdSenorPin.Dispose();
            _hardware.ArdI2C.Dispose();
        }

        private void ThirdSenorPinValueChanged(GpioPin sender, GpioPinValueChangedEventArgs args)
        {
            _results[2] = true;

            if (_stateMachine.State != Modes.PlayMode)
            {
                _stateMachine.Fire(Modes.Next);
            }
        }

        public async void SetToBlack()
        {
            byte[] blackbytes1 = {0, 0, 0, 0, 50, 0x1};
            byte[] blackbytes2 = {0, 0, 0, 0, 50, 0x2};
            _hardware.SendLightingCommand(blackbytes1);
            _hardware.SendLightingCommand(blackbytes2);
            await Task.Delay(TimeSpan.FromMilliseconds(1));
            _hardware.SendLightingCommand(blackbytes1);
            _hardware.SendLightingCommand(blackbytes2);
            await Task.Delay(TimeSpan.FromMilliseconds(1));
        }

        private async void CountEvents()
        {
            SetToBlack();
            await Task.Delay(TimeSpan.FromSeconds(1));

            DisplayMode(_results);
        }

        private async void DisplayMode(bool[] results)
        {
            byte lightAddress = 0x00;
            var offset = 0;
            for (var i = 0; i < results.Length; i++)
            {
                if (results[i])
                {
                    offset = offset + 7;
                }
            }

            var nextLightAddress = Convert.ToByte(lightAddress + offset);
            byte[] lightExampleBytes = {0, 255, 0, lightAddress, nextLightAddress, 0x1};
            byte[] lightExampleBytes2 = {0, 255, 0, lightAddress, nextLightAddress, 0x2};

            _hardware.SendLightingCommand(lightExampleBytes);
            _hardware.SendLightingCommand(lightExampleBytes2);

            await Task.Delay(TimeSpan.FromSeconds(5));
            SetToBlack();
            _stateMachine.Fire(Modes.Next);
        }

        private void FirstSenorPinValueChanged(GpioPin sender, GpioPinValueChangedEventArgs e)
        {
            if (e.Edge == GpioPinEdge.FallingEdge)
            {
            }
            else if (e.Edge == GpioPinEdge.RisingEdge)
            {
                _results[0] = true;
                if (_stateMachine.State != Modes.PlayMode)
                {
                    _results = new bool[14];
                    _stateMachine.Fire(Modes.Next);
                }
            }
        }


        private enum Modes
        {
            InitMode,
            PlayMode,
            Next,
            QuiteMode
        };
    }
}