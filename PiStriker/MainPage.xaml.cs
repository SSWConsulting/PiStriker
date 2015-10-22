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
        private Object thisLock = new Object();

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
        private CancellationTokenSource _partyModeCancellationTokenSource = new CancellationTokenSource();

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

            InitGPIO();
            _playTimer.Interval = TimeSpan.FromMilliseconds(2000);
            _playTimer.Tick += GameEnded;
            StartParty();
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
            _1stSenorPin.ValueChanged += _SenorPinValueChanged;

            //_2ndSenorPin = gpio.OpenPin(SIG2);
            //_2ndSenorPin.SetDriveMode(GpioPinDriveMode.Input);
            //_2ndSenorPin.DebounceTimeout = TimeSpan.FromTicks(10);
            //_2ndSenorPin.ValueChanged += _2ndSenorPinValueChanged;

            _3rdSenorPin = gpio.OpenPin(SIG3);
            _3rdSenorPin.SetDriveMode(GpioPinDriveMode.Input);
            _3rdSenorPin.DebounceTimeout = TimeSpan.FromTicks(10);
            _3rdSenorPin.ValueChanged += _SenorPinValueChanged;

            _4thSenorPin = gpio.OpenPin(SIG4);
            _4thSenorPin.SetDriveMode(GpioPinDriveMode.Input);
            _4thSenorPin.DebounceTimeout = TimeSpan.FromTicks(10);
            _4thSenorPin.ValueChanged += _SenorPinValueChanged;

            _5thSenorPin = gpio.OpenPin(SIG5);
            _5thSenorPin.SetDriveMode(GpioPinDriveMode.Input);
            _5thSenorPin.DebounceTimeout = TimeSpan.FromTicks(10);
            _5thSenorPin.ValueChanged += _SenorPinValueChanged;
        
            _6thSenorPin = gpio.OpenPin(SIG6);
            _6thSenorPin.SetDriveMode(GpioPinDriveMode.Input);
            _6thSenorPin.DebounceTimeout = TimeSpan.FromTicks(10);
            _6thSenorPin.ValueChanged += _SenorPinValueChanged;
            
            _7thSenorPin = gpio.OpenPin(SIG7);
            _7thSenorPin.SetDriveMode(GpioPinDriveMode.Input);
            _7thSenorPin.DebounceTimeout = TimeSpan.FromTicks(10);
            _7thSenorPin.ValueChanged += _SenorPinValueChanged;

            _8thSenorPin = gpio.OpenPin(SIG8);
            _8thSenorPin.SetDriveMode(GpioPinDriveMode.Input);
            _8thSenorPin.DebounceTimeout = TimeSpan.FromTicks(10);
            _8thSenorPin.ValueChanged += _SenorPinValueChanged;

            _9thSenorPin = gpio.OpenPin(SIG9);
            _9thSenorPin.SetDriveMode(GpioPinDriveMode.Input);
            _9thSenorPin.DebounceTimeout = TimeSpan.FromTicks(10);
            _9thSenorPin.ValueChanged += _SenorPinValueChanged;

            _10thSenorPin = gpio.OpenPin(SIG10);
            _10thSenorPin.SetDriveMode(GpioPinDriveMode.Input);
            _10thSenorPin.DebounceTimeout = TimeSpan.FromTicks(10);
            _10thSenorPin.ValueChanged += _SenorPinValueChanged;

            _11thSenorPin = gpio.OpenPin(SIG11);
            _11thSenorPin.SetDriveMode(GpioPinDriveMode.Input);
            _11thSenorPin.DebounceTimeout = TimeSpan.FromTicks(10);
            _11thSenorPin.ValueChanged += _SenorPinValueChanged;

            _12thSenorPin = gpio.OpenPin(SIG12);
            _12thSenorPin.SetDriveMode(GpioPinDriveMode.Input);
            _12thSenorPin.DebounceTimeout = TimeSpan.FromTicks(10);
            _12thSenorPin.ValueChanged += _SenorPinValueChanged;


            //_13thSenorPin = gpio.OpenPin(SIG13);
            //_13thSenorPin.SetDriveMode(GpioPinDriveMode.Input);
            //_13thSenorPin.DebounceTimeout = TimeSpan.FromTicks(10);
            //_13thSenorPin.ValueChanged += _13thSenorPinValueChanged;

            _14thSenorPin = gpio.OpenPin(SIG14);
            _14thSenorPin.SetDriveMode(GpioPinDriveMode.Input);
            _14thSenorPin.DebounceTimeout = TimeSpan.FromTicks(10);
            _14thSenorPin.ValueChanged += _SenorPinValueChanged;

            _ardI2C = await SetUpI2C();
            SetToBlack();
            GpioStatus.Text = "GPIO pins initialized correctly.";
        }

        private void StartPlay()
        {
            if (!_playing)
            {
                _playing = true;
                StopParty();
                Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                {
                    _playTimer.Start();
                });
            }
        }

        private void StartParty()
        {
            if (!_playing)
            {
                _partyModeCancellationTokenSource = new CancellationTokenSource();
                PartyMode(_partyModeCancellationTokenSource.Token);
            }
        }

        private void StopParty()
        {
            if (_playing)
            {
                _partyModeCancellationTokenSource.Cancel();
            }
        }
                
        private void _SenorPinValueChanged(GpioPin sender, GpioPinValueChangedEventArgs args)
        {
                var pinNumber = sender.PinNumber;
                switch (pinNumber)
        {
                    case 22:
                        _results[0] = true; //Hack to compensate for dead sensor
            _results[1] = true;
                        break;

                    case 24:
                        _results[2] = true; //Hack to compensate for dead sensor
                        _results[3] = true; //Hack to compensate for dead sensor
            _results[4] = true;
                        break;

                    case 25:
            _results[5] = true;
                        break;

                    case 16:
            _results[6] = true;
                        break;

                    case 6:
            _results[7] = true;
                        break;

                    case 4:
                        _results[8] = true; //Hack to compensate for dead sensor
            _results[9] = true;
                        break;

                    case 12:
            _results[10] = true;
                        break;

                    case 18:
                        _results[11] = true;
                        break;

                    case 13:
            _results[12] = true;
                        break;

                    case 27:
                        _results[13] = true;
                        break;

                    default:
                        break;
        }


            lock (thisLock)
            {
                    StartPlay();
            }
        }

        public async void PartyMode(CancellationToken partyModeCancellationToken)
        {
            try
            {
                while (_playing != true)
                {
                    if (!_playing)
                    {
                        await SlowPinkRise();
                    }

                    if (!_playing)
                    {
                        await SlowYellowRise();
                    }

                    if (!_playing)
                    {
                        await SlowLightBlueRise();
                    }
                }

                SetToBlack();
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

            if (offset == 46)
            {
                offset = 50;                
            }

            StrengthIndex.Text = offset.ToString();

            var NextLightAddress = Convert.ToByte(lightAddress + offset);
            byte[] lightExampleBytes = {0, 255, 0, lightAddress, NextLightAddress, 0x1};
            byte[] lightExampleBytes2 = {0, 255, 0, lightAddress, NextLightAddress, 0x2};

            SendLightingCommand(lightExampleBytes);
            SendLightingCommand(lightExampleBytes2);

            await Task.Delay(TimeSpan.FromSeconds(1));
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
            await Task.Delay(TimeSpan.FromSeconds(0.5));

            _playing = false;
            StrengthIndex.Text = "0";
            StartParty();
        }

        public async Task SlowYellowRise()
        {
            for (byte lightAddress = 0x00; lightAddress <= 0x50; lightAddress++)
            {
                var NextLightAddress = Convert.ToByte(lightAddress + 1);
                byte[] lightExampleBytes = {255, 255, 0, lightAddress, NextLightAddress, 0x1};
                byte[] lightExampleBytes2 = { 255, 255, 0, lightAddress, NextLightAddress, 0x2 };
                SendLightingCommand(lightExampleBytes);
                SendLightingCommand(lightExampleBytes2);
                await Task.Delay(TimeSpan.FromMilliseconds(10));
            }
        }

        public async Task SlowPinkRise()
        {
            for (byte lightAddress = 0x00; lightAddress <= 0x50; lightAddress++)
            {
                var NextLightAddress = Convert.ToByte(lightAddress + 1);
                byte[] lightExampleBytes = {255, 0, 255, lightAddress, NextLightAddress, 0x1};
                byte[] lightExampleBytes2 = { 255, 0, 255, lightAddress, NextLightAddress, 0x2 };
                SendLightingCommand(lightExampleBytes);
                SendLightingCommand(lightExampleBytes2);
                await Task.Delay(TimeSpan.FromMilliseconds(10));
            }
        }

        public async Task SlowLightBlueRise()
        {
            for (byte lightAddress = 0x00; lightAddress <= 0x50; lightAddress++)
            {
                var endlightAddress = Convert.ToByte(lightAddress + 1);
                byte[] lightExampleBytes = {0, 255, 255, lightAddress, endlightAddress, 0x1};
                byte[] lightExampleBytes2 = { 0, 255, 255, lightAddress, endlightAddress, 0x2 };
                SendLightingCommand(lightExampleBytes);
                SendLightingCommand(lightExampleBytes2);
                await Task.Delay(TimeSpan.FromMilliseconds(10));
            }
        }
    }
}