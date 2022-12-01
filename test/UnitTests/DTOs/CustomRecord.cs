/* Copyright 2018 by Nomadeon Software LLC. Licensed uinder MIT: https://opensource.org/licenses/MIT */
using System;
using System.Collections.Generic;
using System.Text;

namespace NovaQueueTests
{
    /// <summary>
    /// Contrived complex object for testing the T in NovaQueue<T>
    /// </summary>
    public class CustomRecord
    {
        public DeviceLocation Device { get; set; }

        public double SensorReading { get; set; }
        public string LogValue { get; set; }
    }
}
