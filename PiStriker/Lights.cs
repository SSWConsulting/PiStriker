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
        /// <summary>
        ///  Turns the neo pixel strip off
        /// </summary>
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

        /// <summary>
        /// Displays the score to the user by turning on the neo pixel strips on
        /// </summary>
        /// <param name="height">The highest beam sensor broken by the wood block.</param>
        public void TurnOnLedToDisplayScore(int height)
        {
            byte startingLightAddress = 0x00;
            var endingLightAddress = Convert.ToByte(startingLightAddress + height);
            byte[] lightExampleBytes = { 0, 255, 0, startingLightAddress, endingLightAddress, 0x1 };
            byte[] lightExampleBytes2 = { 0, 255, 0, startingLightAddress, endingLightAddress, 0x2 };

            _hardware.SendBytesToArduino(lightExampleBytes);
            _hardware.SendBytesToArduino(lightExampleBytes2);
        }

        /// <summary>
        ///  Predefined light byte command sequence - Which turns the entire selected neo pixel strip a predefined color one led at a time
        /// </summary>
        public async void Crawling(byte r, byte g, byte b, byte stripId, CancellationToken cancellationToken)
        {
            for (byte lightAddress = 0x00; lightAddress <= 0x32; lightAddress++)
            {
                var NextLightAddress = Convert.ToByte(lightAddress + 1);
                byte[] lightExampleBytes = {r, g, b, lightAddress, NextLightAddress, stripId};
                _hardware.SendBytesToArduino(lightExampleBytes);
                await Task.Delay(TimeSpan.FromMilliseconds(10), cancellationToken);
            }

            await Task.Delay(TimeSpan.FromSeconds(1), cancellationToken);
        }

        /// <summary>
        ///  Predefined light byte command sequence - Which turns the entire selected neo pixel strip a predefined color in batches of 5 at a time
        /// </summary>
        public async void Batched(byte r, byte g, byte b, byte stripId, CancellationToken cancellationToken)
        {
            for (var i = 0; i < 10; i++)
            {
                byte[] bytes = { r, g, b, _startLeds[i], _endLeds[i], stripId};
                _hardware.SendBytesToArduino(bytes);
                await Task.Delay(TimeSpan.FromMilliseconds(10), cancellationToken);
            }

            await Task.Delay(TimeSpan.FromSeconds(1), cancellationToken);
        }

        public async void PartyMode(CancellationToken cancellationToken)
        {
            try
            {
                cancellationToken.ThrowIfCancellationRequested();

                while (true)
                {
                    //Purple
                    Crawling(255, 0, 255, 1, cancellationToken);
                    //Orange
                    Batched(255, 128, 0,2, cancellationToken);
                    //Yellow
                    Crawling(255,255,0,1, cancellationToken);
                    //Blue
                    Batched(0, 128, 255, 2, cancellationToken);
                    //Cyan
                    Crawling(0, 255, 255, 1, cancellationToken);
                    //Pink
                    Batched(255, 51, 255, 2, cancellationToken);

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