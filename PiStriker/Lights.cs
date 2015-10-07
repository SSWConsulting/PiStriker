using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Serilog;

namespace PiStriker
{
    public class Lights : ILights
    {
        private readonly byte[] _endLeds = new byte[10]
        {
            6, 11, 16, 21, 26, 31, 36, 41, 46, 51
        };

        private readonly ILogger _logger;

        private readonly byte[] _startLeds = new byte[10]
        {
            1, 6, 11, 16, 21, 26, 31, 36, 41, 46
        };

        private readonly IHardware _hardware;

        public Lights(IHardware hardware, ILogger logger)
        {
            _logger = logger;
            _hardware = hardware;
        }

        public async void SetToBlack()
        {
            byte[] blackbytes1 = {0, 0, 0, 0, 50, 0x1};
            byte[] blackbytes2 = {0, 0, 0, 0, 50, 0x2};
            _hardware.SendBytesToArduino(blackbytes1);
            _hardware.SendBytesToArduino(blackbytes2);
            await Task.Delay(TimeSpan.FromMilliseconds(1));
            _hardware.SendBytesToArduino(blackbytes1);
            _hardware.SendBytesToArduino(blackbytes2);
            await Task.Delay(TimeSpan.FromMilliseconds(1));
        }

        public void TurnOnLedToDsplayScore(int height)
        {
            byte startingLightAddress = 0x00;
            var endingLightAddress = Convert.ToByte(startingLightAddress + height);
            byte[] lightExampleBytes = { 0, 255, 0, startingLightAddress, endingLightAddress, 0x1 };
            byte[] lightExampleBytes2 = { 0, 255, 0, startingLightAddress, endingLightAddress, 0x2 };

            _hardware.SendBytesToArduino(lightExampleBytes);
            _hardware.SendBytesToArduino(lightExampleBytes2);
        }

        public async void SlowYellowRise()
        {
            for (byte lightAddress = 0x00; lightAddress <= 0x32; lightAddress++)
            {
                var NextLightAddress = Convert.ToByte(lightAddress + 1);
                byte[] lightExampleBytes = {255, 255, 0, lightAddress, NextLightAddress, 0x1};
                _hardware.SendBytesToArduino(lightExampleBytes);
                await Task.Delay(TimeSpan.FromMilliseconds(10));
            }
        }

        public async void SlowPinkRise()
        {
            for (byte lightAddress = 0x00; lightAddress <= 0x32; lightAddress++)
            {
                var NextLightAddress = Convert.ToByte(lightAddress + 1);
                byte[] lightExampleBytes = {255, 0, 255, lightAddress, NextLightAddress, 0x1};
                _hardware.SendBytesToArduino(lightExampleBytes);
                await Task.Delay(TimeSpan.FromMilliseconds(10));
            }
        }

        public async void SlowLightBlueRise()
        {
            for (byte lightAddress = 0x00; lightAddress <= 0x32; lightAddress++)
            {
                var endlightAddress = Convert.ToByte(lightAddress + 1);
                byte[] lightExampleBytes = {0, 255, 255, lightAddress, endlightAddress, 0x1};
                _hardware.SendBytesToArduino(lightExampleBytes);
                await Task.Delay(TimeSpan.FromMilliseconds(10));
            }
        }

        public async void QuickOrange()
        {
            for (var i = 0; i < 10; i++)
            {
                byte[] bytes = {255, 128, 0, _startLeds[i], _endLeds[i], 0x2};
                _hardware.SendBytesToArduino(bytes);
                await Task.Delay(TimeSpan.FromMilliseconds(10));
            }
        }

        public async void QuickYellow()
        {
            for (var i = 0; i < 10; i++)
            {
                byte[] bytes = {255, 255, 0, _startLeds[i], _endLeds[i], 0x2};
                _hardware.SendBytesToArduino(bytes);
                await Task.Delay(TimeSpan.FromMilliseconds(10));
            }
        }

        public async void QuickPurple()
        {
            for (var i = 0; i < 10; i++)
            {
                byte[] bytes = {128, 0, 255, _startLeds[i], _endLeds[i], 0x2};
                _hardware.SendBytesToArduino(bytes);
                await Task.Delay(TimeSpan.FromMilliseconds(10));
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

                    await Task.Delay(TimeSpan.FromSeconds(1), cancellationToken);

                    QuickOrange();

                    await Task.Delay(TimeSpan.FromSeconds(1), cancellationToken);

                    SlowYellowRise();

                    await Task.Delay(TimeSpan.FromSeconds(1), cancellationToken);

                    QuickPurple();

                    await Task.Delay(TimeSpan.FromSeconds(1), cancellationToken);

                    SlowLightBlueRise();

                    await Task.Delay(TimeSpan.FromSeconds(1), cancellationToken);

                    SetToBlack();

                    await Task.Delay(TimeSpan.FromSeconds(30), cancellationToken);
                }
            }
            catch (Exception exception)
            {
                _logger.Error(exception, "Exception: {0}", exception.Message);
            }
        }
    }
}