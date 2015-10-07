using System;
using System.Threading.Tasks;
using Windows.Devices.Enumeration;
using Windows.Devices.Gpio;
using Windows.Devices.I2c;
using Serilog;

namespace PiStriker
{
    public class Hardware : IHardware
    {
        private const int FIRSTSENOR_PIN = 27;
        private const int THIRDSENOR_PIN = 22;
        private readonly ILogger _logger;

        public Hardware(ILogger logger)
        {
            _logger = logger;
        }

        public I2cDevice ArdI2C { get; private set; }
        public GpioPin FirstSenorPin { get; private set; }
        public GpioPin ThirdSenorPin { get; private set; }

        /// <summary>
        ///     Sets up GpioController, all GPIO Pins and calls SetUpI2C to create I2C connection.
        /// </summary>
        /// <returns>True if all hardware was initialized correctly.</returns>
        public async Task<bool> InitializeHardware()
        {
            try
            {
                var gpio = GpioController.GetDefault();

                FirstSenorPin = gpio.OpenPin(FIRSTSENOR_PIN);

                FirstSenorPin.SetDriveMode(GpioPinDriveMode.Input);
                FirstSenorPin.DebounceTimeout = TimeSpan.FromTicks(10);

                ThirdSenorPin = gpio.OpenPin(THIRDSENOR_PIN);

                ThirdSenorPin.SetDriveMode(GpioPinDriveMode.Input);
                ThirdSenorPin.DebounceTimeout = TimeSpan.FromTicks(10);

                ArdI2C = await SetUpI2C();

                _logger.Information("GPIO & I2C successfully initialized.");

                return true;
            }
            catch (Exception exception)
            {
                _logger.Error(exception, "There is no GPIO controller on this device.");
                return false;
            }
        }

        /// <summary>
        ///     Handles sending the bytes to the arduino via the previously set up I2C connection.
        ///     Also this method will resend any bytes that are not acknowledged as been sent successfully.
        /// </summary>
        /// <param name="bytesToSend">Must be in the following format: R,G,B,StartingLed,EndingLed,StrandID</param>
        public void SendBytesToArduino(byte[] bytesToSend)
        {
            if (bytesToSend.Length != 6)
            {
                try
                {
                    var writeResults = ArdI2C.WritePartial(bytesToSend);

                    if (writeResults.Status != I2cTransferStatus.FullTransfer)
                    {
                        _logger.Error("Failed to write to arcI2C.");
                        SendBytesToArduino(bytesToSend);
                    }
                }
                catch (Exception exception)
                {
                    _logger.Error(exception, "Exception: {0}", exception.Message);
                }
            }
            throw new InvalidOperationException(
                "Must be 6 bytes - In the following format: R, G, B, StartingLed, EndingLed, StrandID");
        }

        /// <summary>
        ///     Set up the I2C connection to the arduino.
        /// </summary>
        /// <returns></returns>
        private async Task<I2cDevice> SetUpI2C()
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
                _logger.Error(exception, "Exception: {0}", exception.Message);
                throw;
            }
        }
    }
}