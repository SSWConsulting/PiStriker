using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Windows.Devices.Enumeration;
using Windows.Devices.Gpio;
using Windows.Devices.I2c;
using Windows.UI.Xaml.Controls;
using Stateless;

namespace PiStriker
{
    public sealed partial class MainPage : Page
    {
        private const int FIRSTSENOR_PIN = 27;
        private const int THIRDSENOR_PIN = 22;

        private readonly byte[] _endLeds = new byte[10]
        {
            6, 11, 16, 21, 26, 31, 36, 41, 46, 51
        };

        private readonly byte[] _startLeds = new byte[10]
        {
            1, 6, 11, 16, 21, 26, 31, 36, 41, 46
        };

        private readonly StateMachine<Modes, Modes> _stateMachine = new StateMachine<Modes, Modes>(Modes.InitMode);
        private I2cDevice _ardI2C;
        private GpioPin _firstSenorPin;
        public bool _isInUse = false;
        private GpioPinValue _ledPinValue = GpioPinValue.High;
        private bool[] _results = new bool[14];
        private CancellationTokenSource _source = new CancellationTokenSource();
        private GpioPin _thirdSenorPin;

        public MainPage()
        {
            InitializeComponent();

            _stateMachine.Configure(Modes.InitMode).Permit(Modes.Next, Modes.QuiteMode);
            //_stateMachine.Configure(Modes.PartyMode).Permit(Modes.Next, Modes.PlayMode);
            _stateMachine.Configure(Modes.PlayMode).Permit(Modes.Next, Modes.QuiteMode);
            _stateMachine.Configure(Modes.QuiteMode).Permit(Modes.Next, Modes.PlayMode);

            //_stateMachine.Configure(Modes.PartyMode)
            //    .OnEntry(SetUpPartyMode)
            //    .OnExit(() => _source.Cancel());

            _stateMachine.Configure(Modes.PlayMode)
                .OnEntry(CountEvents);

            //_stateMachine.Configure(Modes.QuiteMode)
            //    .OnEntry(Cooldown);

            InitGPIO();
        }

        private async void Cooldown()
        {
            await Task.Delay(TimeSpan.FromSeconds(15));

            _stateMachine.Fire(Modes.Next);
        }

        private void SetUpPartyMode()
        {
            _source = new CancellationTokenSource();
            PartyMode(_source.Token);
        }

        private async void InitGPIO()
        {
            var gpio = GpioController.GetDefault();

            if (gpio == null)
            {
                GpioStatus.Text = "There is no GPIO controller on this device.";
                return;
            }

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

        private void ThirdSenorPinValueChanged(GpioPin sender, GpioPinValueChangedEventArgs args)
        {
            _results[2] = true;

            if (_stateMachine.State != Modes.PlayMode)
            {
                _stateMachine.Fire(Modes.Next);
            }
        }

        public async void PartyMode(CancellationToken cancellationToken)
        {
            try
            {
                cancellationToken.ThrowIfCancellationRequested();

                while (true)
                {
                    SlowPinkRise();

                    cancellationToken.ThrowIfCancellationRequested();

                    await Task.Delay(TimeSpan.FromSeconds(1));

                    QuickOrange();

                    cancellationToken.ThrowIfCancellationRequested();

                    await Task.Delay(TimeSpan.FromSeconds(1));

                    slowYellowRise();

                    cancellationToken.ThrowIfCancellationRequested();

                    await Task.Delay(TimeSpan.FromSeconds(1));

                    QuickPurple();

                    cancellationToken.ThrowIfCancellationRequested();

                    await Task.Delay(TimeSpan.FromSeconds(1));

                    SlowLightBlueRise();

                    cancellationToken.ThrowIfCancellationRequested();

                    await Task.Delay(TimeSpan.FromSeconds(1));


                    SetToBlack();

                    cancellationToken.ThrowIfCancellationRequested();

                    await Task.Delay(TimeSpan.FromSeconds(30));
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine("Exception: {0}", e.Message);
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
            catch (Exception e)
            {
                Debug.WriteLine("Exception: {0}", e.Message);
                return null;
            }
        }

        public async void SetToBlack()
        {
            byte[] blackbytes1 = {0, 0, 0, 0, 50, 0x1};
            byte[] blackbytes2 = {0, 0, 0, 0, 50, 0x2};
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
                    Debug.WriteLine("Failed to write to arcI2C.");
                    SendLightingCommand(bytesToSend);
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine("Exception: {0}", e.Message);
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

            var NextLightAddress = Convert.ToByte(lightAddress + offset);
            byte[] lightExampleBytes = {0, 255, 0, lightAddress, NextLightAddress, 0x1};
            byte[] lightExampleBytes2 = {0, 255, 0, lightAddress, NextLightAddress, 0x2};

            SendLightingCommand(lightExampleBytes);
            SendLightingCommand(lightExampleBytes2);
            NextLightAddress = lightAddress;

            await Task.Delay(TimeSpan.FromSeconds(5));
            SetToBlack();
            _stateMachine.Fire(Modes.Next);
        }

        private async void FirstSenorPinValueChanged(GpioPin sender, GpioPinValueChangedEventArgs e)
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

        public async void slowYellowRise()
        {
            for (byte lightAddress = 0x00; lightAddress <= 0x32; lightAddress++)
            {
                var NextLightAddress = Convert.ToByte(lightAddress + 1);
                byte[] lightExampleBytes = {255, 255, 0, lightAddress, NextLightAddress, 0x1};
                SendLightingCommand(lightExampleBytes);
                await Task.Delay(TimeSpan.FromMilliseconds(10));
            }
        }

        public async void SlowPinkRise()
        {
            for (byte lightAddress = 0x00; lightAddress <= 0x32; lightAddress++)
            {
                var NextLightAddress = Convert.ToByte(lightAddress + 1);
                byte[] lightExampleBytes = {255, 0, 255, lightAddress, NextLightAddress, 0x1};
                SendLightingCommand(lightExampleBytes);
                await Task.Delay(TimeSpan.FromMilliseconds(10));
            }
        }

        public async void SlowLightBlueRise()
        {
            for (byte lightAddress = 0x00; lightAddress <= 0x32; lightAddress++)
            {
                var endlightAddress = Convert.ToByte(lightAddress + 1);
                byte[] lightExampleBytes = {0, 255, 255, lightAddress, endlightAddress, 0x1};
                SendLightingCommand(lightExampleBytes);
                await Task.Delay(TimeSpan.FromMilliseconds(10));
            }
        }

        public async void QuickOrange()
        {
            for (var i = 0; i < 10; i++)
            {
                byte[] bytes = {255, 128, 0, _startLeds[i], _endLeds[i], 0x2};
                SendLightingCommand(bytes);
                await Task.Delay(TimeSpan.FromMilliseconds(10));
            }
        }

        public async void QuickYellow()
        {
            for (var i = 0; i < 10; i++)
            {
                byte[] bytes = {255, 255, 0, _startLeds[i], _endLeds[i], 0x2};
                SendLightingCommand(bytes);
                await Task.Delay(TimeSpan.FromMilliseconds(10));
            }
        }

        public async void QuickPurple()
        {
            for (var i = 0; i < 10; i++)
            {
                byte[] bytes = {128, 0, 255, _startLeds[i], _endLeds[i], 0x2};
                SendLightingCommand(bytes);
                await Task.Delay(TimeSpan.FromMilliseconds(10));
            }
        }

        private enum Modes
        {
            PartyMode,
            InitMode,
            PlayMode,
            Next,
            QuiteMode
        };
    }
}