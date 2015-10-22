using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Windows.Devices.Enumeration;
using Windows.Devices.Gpio;
using Windows.Devices.I2c;
using Windows.UI.Xaml.Controls;
using Stateless;
using Windows.UI.Xaml;

namespace PiStriker
{
    public sealed partial class MainPage : Page
    {
        private static DispatcherTimer _playTimer = new DispatcherTimer();
        private static bool _playing = false;

        private const int SIG1 = 4;
        private const int SIG2 = 17;
        private const int SIG3 = 18;
        private const int SIG4 = 27;
        private const int SIG5 = 22;
        private const int SIG6 = 23;
        private const int SIG7 = 24;
        private const int SIG8 = 25;
        private const int SIG9 = 5;
        private const int SIG10 = 6;
        private const int SIG11 = 12;
        private const int SIG12 = 13;
        private const int SIG13 = 19;
        private const int SIG14 = 16;


        private readonly byte[] _endLeds = new byte[10]
        {
            6, 11, 16, 21, 26, 31, 36, 41, 46, 51
        };

        private readonly byte[] _startLeds = new byte[10]
        {
            1, 6, 11, 16, 21, 26, 31, 36, 41, 46
        };

        //private readonly StateMachine<Modes, Modes> _stateMachine = new StateMachine<Modes, Modes>(Modes.InitMode);
        private I2cDevice _ardI2C;
        public bool _isInUse = false;
        private GpioPinValue _ledPinValue = GpioPinValue.High;
        private bool[] _results = new bool[14];
        private CancellationTokenSource _source = new CancellationTokenSource();


        private GpioPin _1stSenorPin;
        private GpioPin _2ndSenorPin;
        private GpioPin _3rdSenorPin;
        private GpioPin _4thSenorPin;
        private GpioPin _5thSenorPin;
        private GpioPin _6thSenorPin;
        private GpioPin _7thSenorPin;
        private GpioPin _8thSenorPin;
        private GpioPin _9thSenorPin;
        private GpioPin _10thSenorPin;
        private GpioPin _11thSenorPin;
        private GpioPin _12thSenorPin;
        private GpioPin _13thSenorPin;
        private GpioPin _14thSenorPin;

        public MainPage()
        {
            InitializeComponent();

            //_stateMachine.Configure(Modes.InitMode).Permit(Modes.Next, Modes.QuiteMode);
            //_stateMachine.Configure(Modes.PartyMode).Permit(Modes.Next, Modes.PlayMode);
            //_stateMachine.Configure(Modes.PlayMode).Permit(Modes.Next, Modes.QuiteMode);
            //_stateMachine.Configure(Modes.QuiteMode).Permit(Modes.Next, Modes.PlayMode);

            //_stateMachine.Configure(Modes.PartyMode)
            //    .OnEntry(SetUpPartyMode)
            //    .OnExit(() => _source.Cancel());

            //_stateMachine.Configure(Modes.PlayMode)
            //    .OnEntry(CountEvents);

            //_stateMachine.Configure(Modes.QuiteMode)
            //    .OnEntry(Cooldown);

            InitGPIO();
            _playTimer.Interval = TimeSpan.FromMilliseconds(2000);
            _playTimer.Tick += GameEnded;
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

            _1stSenorPin = gpio.OpenPin(SIG1);

            _1stSenorPin.SetDriveMode(GpioPinDriveMode.Input);
            _1stSenorPin.DebounceTimeout = TimeSpan.FromTicks(10);
            _1stSenorPin.ValueChanged += _1StSenorPinValueChanged;

            //_2ndSenorPin = gpio.OpenPin(SIG2);

            //_2ndSenorPin.SetDriveMode(GpioPinDriveMode.Input);
            //_2ndSenorPin.DebounceTimeout = TimeSpan.FromTicks(10);
            //_2ndSenorPin.ValueChanged += _2ndSenorPinValueChanged;

            _3rdSenorPin = gpio.OpenPin(SIG3);

            _3rdSenorPin.SetDriveMode(GpioPinDriveMode.Input);
            _3rdSenorPin.DebounceTimeout = TimeSpan.FromTicks(10);
            _3rdSenorPin.ValueChanged += _3RdSenorPinValueChanged;

            _4thSenorPin = gpio.OpenPin(SIG4);

            _4thSenorPin.SetDriveMode(GpioPinDriveMode.Input);
            _4thSenorPin.DebounceTimeout = TimeSpan.FromTicks(10);
            _4thSenorPin.ValueChanged += _4thSenorPinValueChanged;

            _5thSenorPin = gpio.OpenPin(SIG5);

            _5thSenorPin.SetDriveMode(GpioPinDriveMode.Input);
            _5thSenorPin.DebounceTimeout = TimeSpan.FromTicks(10);
            _5thSenorPin.ValueChanged += _5thSenorPinValueChanged;
        
            _6thSenorPin = gpio.OpenPin(SIG6);

            _6thSenorPin.SetDriveMode(GpioPinDriveMode.Input);
            _6thSenorPin.DebounceTimeout = TimeSpan.FromTicks(10);
            _6thSenorPin.ValueChanged += _6thSenorPinValueChanged;
            
            _7thSenorPin = gpio.OpenPin(SIG7);

            _7thSenorPin.SetDriveMode(GpioPinDriveMode.Input);
            _7thSenorPin.DebounceTimeout = TimeSpan.FromTicks(10);
            _7thSenorPin.ValueChanged += _7thSenorPinValueChanged;

            _8thSenorPin = gpio.OpenPin(SIG8);

            _8thSenorPin.SetDriveMode(GpioPinDriveMode.Input);
            _8thSenorPin.DebounceTimeout = TimeSpan.FromTicks(10);
            _8thSenorPin.ValueChanged += _8thSenorPinValueChanged;

            _9thSenorPin = gpio.OpenPin(SIG9);

            _9thSenorPin.SetDriveMode(GpioPinDriveMode.Input);
            _9thSenorPin.DebounceTimeout = TimeSpan.FromTicks(10);
            _9thSenorPin.ValueChanged += _9thSenorPinValueChanged;

            _10thSenorPin = gpio.OpenPin(SIG10);

            _10thSenorPin.SetDriveMode(GpioPinDriveMode.Input);
            _10thSenorPin.DebounceTimeout = TimeSpan.FromTicks(10);
            _10thSenorPin.ValueChanged += _10thSenorPinValueChanged;

            _11thSenorPin = gpio.OpenPin(SIG11);

            _11thSenorPin.SetDriveMode(GpioPinDriveMode.Input);
            _11thSenorPin.DebounceTimeout = TimeSpan.FromTicks(10);
            _11thSenorPin.ValueChanged += _11thSenorPinValueChanged;


            _12thSenorPin = gpio.OpenPin(SIG12);

            _12thSenorPin.SetDriveMode(GpioPinDriveMode.Input);
            _12thSenorPin.DebounceTimeout = TimeSpan.FromTicks(10);
            _12thSenorPin.ValueChanged += _12thSenorPinValueChanged;


            //_13thSenorPin = gpio.OpenPin(SIG13);

            //_13thSenorPin.SetDriveMode(GpioPinDriveMode.Input);
            //_13thSenorPin.DebounceTimeout = TimeSpan.FromTicks(10);
            //_13thSenorPin.ValueChanged += _13thSenorPinValueChanged;

            _14thSenorPin = gpio.OpenPin(SIG14);

            _14thSenorPin.SetDriveMode(GpioPinDriveMode.Input);
            _14thSenorPin.DebounceTimeout = TimeSpan.FromTicks(10);
            _14thSenorPin.ValueChanged += _14thSenorPinValueChanged;


            _ardI2C = await SetUpI2C();
            SetToBlack();
            GpioStatus.Text = "GPIO pins initialized correctly.";
        }

        private async Task StartPlay()
        {
            if (!_playing)
            {
                _playing = true;
                await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                {
                    _playTimer.Start();
                });
            }
        }

        private async void _1StSenorPinValueChanged(GpioPin sender, GpioPinValueChangedEventArgs e)
        {
            _results[0] = true;
            await StartPlay();
        }

        private async void _2ndSenorPinValueChanged(GpioPin sender, GpioPinValueChangedEventArgs args)
        {
            _results[1] = true;
            await StartPlay();
        }

        private async void _3RdSenorPinValueChanged(GpioPin sender, GpioPinValueChangedEventArgs args)
        {
            _results[2] = true;
            await StartPlay();
        }

        private async void _4thSenorPinValueChanged(GpioPin sender, GpioPinValueChangedEventArgs args)
        {
            _results[3] = true;
            await StartPlay();
        }

        private async void _5thSenorPinValueChanged(GpioPin sender, GpioPinValueChangedEventArgs args)
        {
            _results[4] = true;
            await StartPlay();
        }

        private async void _6thSenorPinValueChanged(GpioPin sender, GpioPinValueChangedEventArgs args)
        {
            _results[5] = true;
            await StartPlay();
        }

        private async void _7thSenorPinValueChanged(GpioPin sender, GpioPinValueChangedEventArgs args)
        {
            _results[6] = true;
            await StartPlay();
        }

        private async void _8thSenorPinValueChanged(GpioPin sender, GpioPinValueChangedEventArgs args)
        {
            _results[7] = true;
            await StartPlay();
        }

        private async void _9thSenorPinValueChanged(GpioPin sender, GpioPinValueChangedEventArgs args)
        {
            _results[8] = true;
            await StartPlay();
        }

        private async void _10thSenorPinValueChanged(GpioPin sender, GpioPinValueChangedEventArgs args)
        {
            _results[9] = true;
            await StartPlay();
        }

        private async void _11thSenorPinValueChanged(GpioPin sender, GpioPinValueChangedEventArgs args)
        {
            _results[10] = true;
            await StartPlay();
        }

        private async void _12thSenorPinValueChanged(GpioPin sender, GpioPinValueChangedEventArgs args)
        {
            _results[12] = true;
            await StartPlay();
        }

        private async void _13thSenorPinValueChanged(GpioPin sender, GpioPinValueChangedEventArgs args)
        {
            _results[12] = true;
            await StartPlay();
        }

        private async void _14thSenorPinValueChanged(GpioPin sender, GpioPinValueChangedEventArgs args)
        {
            _results[13] = true;
            await StartPlay();
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

        private void GameEnded(object sender, object e)
        {
            _playTimer.Stop();
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
                    if (offset == 0)
                    {
                        offset = offset + 7;
                    }
                    else
                    {
                        offset = offset + 3;
                    }

                    results[i] = false;
                }
            }

            var NextLightAddress = Convert.ToByte(lightAddress + offset);
            byte[] lightExampleBytes = {0, 255, 0, lightAddress, NextLightAddress, 0x1};
            byte[] lightExampleBytes2 = {0, 255, 0, lightAddress, NextLightAddress, 0x2};

            SendLightingCommand(lightExampleBytes);
            SendLightingCommand(lightExampleBytes2);

            await Task.Delay(TimeSpan.FromSeconds(2));
            SetToBlack();
            await Task.Delay(TimeSpan.FromSeconds(0.5));
            SendLightingCommand(lightExampleBytes);
            SendLightingCommand(lightExampleBytes2);
            await Task.Delay(TimeSpan.FromSeconds(0.5));
            SetToBlack();
            await Task.Delay(TimeSpan.FromSeconds(0.5));
            SendLightingCommand(lightExampleBytes);
            SendLightingCommand(lightExampleBytes2);
            await Task.Delay(TimeSpan.FromSeconds(0.5));
            SetToBlack();
            await Task.Delay(TimeSpan.FromSeconds(0.5));
            SendLightingCommand(lightExampleBytes);
            SendLightingCommand(lightExampleBytes2);
            await Task.Delay(TimeSpan.FromSeconds(0.5));
            SetToBlack();
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