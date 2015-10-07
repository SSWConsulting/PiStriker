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
        private readonly ILights _lights;

        private bool[] _results = new bool[14];

        public MainPage()
        {
            InitializeComponent();
            
            var containerBuilder = new ContainerBuilder();

            containerBuilder.RegisterModule(new SerilogModule());

            containerBuilder.RegisterType<Hardware>().As<IHardware>().SingleInstance();
            containerBuilder.RegisterType<Lights>().As<ILights>().SingleInstance();
            var container = containerBuilder.Build();

            using (var lifetimeScope = container.BeginLifetimeScope())
            {
                _log = lifetimeScope.Resolve<ILogger>();
                _hardware = lifetimeScope.Resolve<IHardware>();
                _lights = lifetimeScope.Resolve<ILights>();
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

            _hardware.FirstSenorPin.ValueChanged -= FirstSenorPinValueChanged;
            _hardware.ThirdSenorPin.ValueChanged -= ThirdSenorPinValueChanged;

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

        private async void CountEvents()
        {
            _lights.SetToBlack();
            await Task.Delay(TimeSpan.FromSeconds(1));

            DisplayMode(_results);
        }

        private async void DisplayMode(bool[] results)
        {
            var height = 0;
            for (var i = 0; i < results.Length; i++)
            {
                if (results[i])
                {
                    height = height + 7;
                }
            }

            _lights.TurnOnLedToDsplayScore(height);

            await Task.Delay(TimeSpan.FromSeconds(5));
            _lights.SetToBlack();
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