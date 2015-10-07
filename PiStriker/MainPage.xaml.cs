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

        /// <summary>
        /// Initialize page, autofac, hardware & configures the State Machine
        /// </summary>
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

            Unloaded += MainPage_Unloaded;

        }

        /// <summary>
        /// Unloaded event so we can clean up upon exit
        /// </summary>
        private void MainPage_Unloaded(object sender, object args)
        {
            // Cleanup

            _hardware.FirstSenorPin.ValueChanged -= FirstSenorPinValueChanged;
            _hardware.ThirdSenorPin.ValueChanged -= ThirdSenorPinValueChanged;

            _hardware.FirstSenorPin.Dispose();
            _hardware.ThirdSenorPin.Dispose();
            _hardware.ArdI2C.Dispose();
        }

        /// <summary>
        /// Event handler for the 1st break beam sensor
        /// </summary>
        private void FirstSenorPinValueChanged(GpioPin sender, GpioPinValueChangedEventArgs e)
        {
            int SensorPosition = 0;
            GpioPinEdge gpioPinEdge = e.Edge;

            RecordBrokenBeamEvent(gpioPinEdge, SensorPosition);
        }

        /// <summary>
        /// Event handler for the 3rd break beam sensor
        /// </summary>
        private void ThirdSenorPinValueChanged(GpioPin sender, GpioPinValueChangedEventArgs e)
        {
            int SensorPosition = 2;
            GpioPinEdge gpioPinEdge = e.Edge;

            RecordBrokenBeamEvent(gpioPinEdge, SensorPosition);
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

            _lights.TurnOnLedToDisplayScore(height);

            await Task.Delay(TimeSpan.FromSeconds(5));
            _lights.SetToBlack();
            _stateMachine.Fire(Modes.Next);
        }





        private void RecordBrokenBeamEvent(GpioPinEdge gpioPinEdge, int SensorPosition)
        {
            if (gpioPinEdge == GpioPinEdge.RisingEdge)
            {
                _results[SensorPosition] = true;
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