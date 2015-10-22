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

            _1stSenorPin = SetUpPin(gpio, SIG1);
            //_2ndSenorPin = SetUpPin(gpio, SIG2);
            _3rdSenorPin = SetUpPin(gpio, SIG3);
            _4thSenorPin = SetUpPin(gpio, SIG4);
            _5thSenorPin = SetUpPin(gpio, SIG5);
            _6thSenorPin = SetUpPin(gpio, SIG6);
            _7thSenorPin = SetUpPin(gpio, SIG7);
            _8thSenorPin = SetUpPin(gpio, SIG8);
            _9thSenorPin = SetUpPin(gpio, SIG9);
            _10thSenorPin = SetUpPin(gpio, SIG10);
            _11thSenorPin = SetUpPin(gpio, SIG11);
            _12thSenorPin = SetUpPin(gpio, SIG12);            
            //_13thSenorPin = SetUpPin(gpio, SIG13);
            _14thSenorPin = SetUpPin(gpio, SIG14);

            _ardI2C = await SetUpI2C();
            SetToBlack();
            GpioStatus.Text = "GPIO pins initialized correctly.";
        }

        private GpioPin SetUpPin(GpioController gpio, int pinNumber)
        { 
            var senorPin = gpio.OpenPin(pinNumber);
            senorPin.SetDriveMode(GpioPinDriveMode.Input);
            senorPin.DebounceTimeout = TimeSpan.FromTicks(10);
            senorPin.ValueChanged += _SenorPinValueChanged;

            return senorPin;
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