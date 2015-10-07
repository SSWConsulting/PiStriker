using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Gpio;
using Windows.Devices.I2c;

namespace PiStriker
{
    public interface IHardware
    {
        GpioPin FirstSenorPin { get; }
        GpioPin ThirdSenorPin { get; }
        I2cDevice ArdI2C { get; }
        Task<bool> InitializeHardware();
        void SendLightingCommand(byte[] bytesToSend);
    }
}
