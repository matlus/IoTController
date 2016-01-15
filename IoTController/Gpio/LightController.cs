using IoTController.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Gpio;

namespace IoTController.Gpio
{
    internal static class LightController
    {
        private static int RED_LED_PIN = 5;
        private static GpioPin redLedPin;

        static LightController()
        {
            InitGpio();
        }

        private static void InitGpio()
        {
            var gpioController = GpioController.GetDefault();
            if (gpioController == null)
            {
                return;
            }

            redLedPin = gpioController.OpenPin(RED_LED_PIN);
            redLedPin.SetDriveMode(GpioPinDriveMode.Output);
        }

        public static void SetLightState(LightColor lighColor, LightState lightState)
        {
            GpioPinValue pinValue = (GpioPinValue)lightState;
            GpioPin pin = null;

            switch (lighColor)
            {
                case LightColor.Red:
                    pin = redLedPin;
                    break;
                case LightColor.Green:
                    break;
                default:
                    return;
            }

            pin.Write(pinValue);
        }
    }
}
