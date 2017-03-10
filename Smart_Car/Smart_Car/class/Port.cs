using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.IO;
using System.IO.Ports;

namespace Smart_Car {
    /// <summary>
    /// class Port, the super class for all serial port drivers used in autocar
    /// </summary>
    class Port {
        /// <summary>
        /// Name and baudrate are necessary to create an insatnce of serial port
        /// </summary>
        protected string portName;
        protected string portBaudrate;
        protected SerialPort port;

        /// <summary>
        /// default constructor
        /// </summary>
        public Port() {
        }

        /// <summary>
        /// default destructor
        /// </summary>
        ~Port() { }

        /// <summary>
        /// get the status if port is open
        /// </summary>
        public bool IsOpen {
            get {
                return ((port != null) && (port.IsOpen));
            }
        }
        /// <summary>
        /// openPort method used to open specific port (also create instance port)
        /// </summary>
        /// <param name="portName">name of port</param>
        /// <param name="portBaudrate">baudrate of port</param>
        /// <returns>true if open port successfully</returns>
        public virtual bool OpenPort(string portName, string portBaudrate) {
            int baudrate = int.Parse(portBaudrate);
            if (port == null) {
                try {
                    port = new SerialPort(portName, baudrate);
                }
                catch (Exception) {
                    return false;
                }
            }
            if (!port.IsOpen) {
                try {
                    port.Open();
                }
                catch (Exception) {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// closePort method used to close port
        /// </summary>
        /// <returns>true if close successfully</returns>
        public virtual bool ClosePort() {
            if (port != null && port.IsOpen) {
                try {
                    port.Close();
                }
                catch (Exception) {
                    return false;
                }
            }
            return true;
        }
        // bottom of class Port
    }
    // end of class Port
}
