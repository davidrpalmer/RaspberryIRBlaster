using System;
using RaspberryIRDotNet;

namespace RaspberryIRBlaster.Common.ConfigObjects
{
    public class Button
    {
        public string Name { get; set; }

        public string Data { get; set; }

        public Button()
        {

        }

        public Button(string name, IReadOnlyPulseSpaceUnitList data = null)
        {
            Name = name;
            Data = data?.SaveToString();
        }

        public PulseSpaceUnitList ReadData()
        {
            return PulseSpaceUnitList.LoadFromString(Data);
        }

        public void WriteData(IReadOnlyPulseSpaceUnitList data)
        {
            Data = data.SaveToString();
        }
    }
}
