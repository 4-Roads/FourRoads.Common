using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FourRoads.Common;


namespace TestDependencyInjectionAndSettings
{
    public class SettingsTest : Settings<SettingsTest>
    {
        public SettingsTest()
        {
            FileName = "test.config";
        }

    }
}
