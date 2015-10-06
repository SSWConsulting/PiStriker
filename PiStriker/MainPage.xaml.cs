using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Windows.Devices.Enumeration;
using Windows.Devices.Gpio;
using Windows.Devices.I2c;
using Windows.UI.Xaml.Controls;
using Autofac;
using Polly;
using Serilog;
using Stateless;

namespace PiStriker
{
    public sealed partial class MainPage : Page
    {

        private readonly StateMachine<Modes, Modes> _stateMachine = new StateMachine<Modes, Modes>(Modes.InitMode);

        private I2cDevice _ardI2C;

        private const int FIRSTSENOR_PIN = 27;
        private const int THIRDSENOR_PIN = 22;

        private GpioPin _firstSenorPin;
        private GpioPin _thirdSenorPin;

        private IContainer _container;
        private ILifetimeScope _lifetimeScope;
        private ILogger _log;

        private bool[] _results = new bool[14];

        public MainPage()
        {
            InitializeComponent();

            var containerBuilder = new ContainerBuilder();

            containerBuilder.RegisterModule(new SerilogModule());

            _container = containerBuilder.Build();
            _lifetimeScope = _container.BeginLifetimeScope();
             _log = _lifetimeScope.Resolve<ILogger>();

            _stateMachine.Configure(Modes.InitMode).Permit(Modes.Next, Modes.QuiteMode);
            _stateMachine.Configure(Modes.PlayMode).Permit(Modes.Next, Modes.QuiteMode);
            _stateMachine.Configure(Modes.QuiteMode).Permit(Modes.Next, Modes.PlayMode);

            _stateMachine.Configure(Modes.PlayMode).OnEntry(CountEvents);

            _log.Information("StateMachine is Configured");
            
            InitGPIO();
        }

        private async void InitGPIO()
        {
            try
            {
            var gpio = GpioController.GetDefault();

            _firstSenorPin = gpio.OpenPin(FIRSTSENOR_PIN);

            _firstSenorPin.SetDriveMode(GpioPinDriveMode.Input);
            _firstSenorPin.DebounceTimeout = TimeSpan.FromTicks(10);
            _firstSenorPin.ValueChanged += FirstSenorPinValueChanged;

            _thirdSenorPin = gpio.OpenPin(THIRDSENOR_PIN);

            _thirdSenorPin.SetDriveMode(GpioPinDriveMode.Input);
            _thirdSenorPin.DebounceTimeout = TimeSpan.FromTicks(10);
            _thirdSenorPin.ValueChanged += ThirdSenorPinValueChanged;

            _ardI2C = await SetUpI2C();
            SetToBlack();
            GpioStatus.Text = "GPIO pins initialized correctly.";

            _stateMachine.Fire(Modes.Next);
            }
            catch (Exception exception)
            {
                _log.Error(exception,"There is no GPIO controller on this device.");
            }
        }

        private void ThirdSenorPinValueChanged(GpioPin sender, GpioPinValueChangedEventArgs args)
        {
            _results[2] = true;

            if (_stateMachine.State != Modes.PlayMode)
            {
                _stateMachine.Fire(Modes.Next);
            }
        }

        public async Task<I2cDevice> SetUpI2C()
        {
            try
            {
                var i2CSettings = new I2cConnectionSettings(0x04);
                i2CSettings.BusSpeed = I2cBusSpeed.FastMode;
                var deviceSelector = I2cDevice.GetDeviceSelector("I2C1");
                var i2CDeviceControllers = await DeviceInformation.FindAllAsync(deviceSelector);
                var ardi2C = await I2cDevice.FromIdAsync(i2CDeviceControllers[0].Id, i2CSettings);
                return ardi2C;
            }
            catch (Exception exception)
            {
                _log.Error(exception, "Exception: {0}", exception.Message);
                return null;
            }
        }

        public async void SetToBlack()
        {
            byte[] blackbytes1 = { 0, 0, 0, 0, 50, 0x1 };
            byte[] blackbytes2 = { 0, 0, 0, 0, 50, 0x2 };
            SendLightingCommand(blackbytes1);
            SendLightingCommand(blackbytes2);
            await Task.Delay(TimeSpan.FromMilliseconds(1));
            SendLightingCommand(blackbytes1);
            SendLightingCommand(blackbytes2);
            await Task.Delay(TimeSpan.FromMilliseconds(1));

        }

        public void SendLightingCommand(byte[] bytesToSend)
        {
            try
            {
                var writeResults = _ardI2C.WritePartial(bytesToSend);

                if (writeResults.Status != I2cTransferStatus.FullTransfer)
                {
                    _log.Error("Failed to write to arcI2C.");
                    SendLightingCommand(bytesToSend);
                }
            }
            catch (Exception exception)
            {
                _log.Error(exception,"Exception: {0}", exception.Message);
            }
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

            byte nextLightAddress = Convert.ToByte(lightAddress + offset);
            byte[] lightExampleBytes = { 0, 255, 0, lightAddress, nextLightAddress, 0x1 };
            byte[] lightExampleBytes2 = { 0, 255, 0, lightAddress, nextLightAddress, 0x2 };

            SendLightingCommand(lightExampleBytes);
            SendLightingCommand(lightExampleBytes2);

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