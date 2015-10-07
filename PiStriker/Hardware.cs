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
        public I2cDevice _ardI2C { get; private set; }
        public GpioPin FirstSenorPin { get; private set; }
        private readonly ILogger _logger;
        public GpioPin ThirdSenorPin { get; private set; }

        public Hardware(ILogger logger)
        {
            _logger = logger;

        }

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

                _ardI2C = await SetUpI2C();

                _logger.Information("GPIO & I2C successfully initialized.");

                return true;
            }
            catch (Exception exception)
            {
                _logger.Error(exception, "There is no GPIO controller on this device.");
                return false;
            }
        }

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
                return null;
            }
        }

        public void SendLightingCommand(byte[] bytesToSend)
        {
            try
            {
                var writeResults = _ardI2C.WritePartial(bytesToSend);

                if (writeResults.Status != I2cTransferStatus.FullTransfer)
                {
                    _logger.Error("Failed to write to arcI2C.");
                    SendLightingCommand(bytesToSend);
                }
            }
            catch (Exception exception)
            {
                _logger.Error(exception, "Exception: {0}", exception.Message);
            }
        }
    }
}