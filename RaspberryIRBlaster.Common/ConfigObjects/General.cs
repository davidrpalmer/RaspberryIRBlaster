﻿using System;

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
    }
}