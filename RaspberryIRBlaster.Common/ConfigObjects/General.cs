using System;

namespace RaspberryIRBlaster.Common.ConfigObjects
{
    public class General
    {
        /// <summary>
        /// Path to the IR sending device. If null/empty then the only IR device that can transmit will be used. If there are multiple then it will fail.
        /// </summary>
        public string IRTXDevice { get; set; }

        /// <summary>
        /// Path to the IR receiving device. If null/empty then the only IR device that can transmit will be used. If there are multiple then it will fail.
        /// </summary>
        public string IRRXDevice { get; set; }

        /// <summary>
        /// The URL to listen at. Example: http://*:1234/
        /// </summary>
        public string ListenAtUrl { get; set; }

        /// <summary>
        /// List of origins (e.g. http://192.168.0.3 / http://192.168.0.3:2233) that can access the REST API.
        /// CORS will be disabled if no domains are specified.
        /// </summary>
        public string[] CorsOrigins { get; set; }

        /// <summary>
        /// After how many minutes of inactivity should the server shutdown. Zero to never shutdown.
        /// </summary>
        public int IdleShutdownMins { get; set; }
    }
}
