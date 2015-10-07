using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Gpio;

namespace PiStriker
{
    public interface IHardware
    {
        GpioPin FirstSenorPin { get; }
        GpioPin ThirdSenorPin { get; }
        Task<bool> InitializeHardware();
        void SendLightingCommand(byte[] bytesToSend);
    }
}
