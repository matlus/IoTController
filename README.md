# IoTController
This Application listens on HTTP Port 8000 and alows controller hardware attached to the IoT Device via a Web Browser.
This application has a small (limited) Web Server that only does what I need it do here. It is not a full fledged WebServer. So you will need to extend it in order to support more of the HTTP protocol if needed.

This application essentially allows for remotely (via a Wifi/Ethernet) controlling hardware (lights etc.). Atthis time it only controls a single LED, but this can be extended (at the hardware level) to control electric lights and appliances.
At this time, the Windows Iot does not support PWM. 
It should be easy to extend the project to support controlling stepper motors
