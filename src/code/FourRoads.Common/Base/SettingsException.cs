using System;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;

namespace FourRoads.Common
{
    [ClassInterface(ClassInterfaceType.None)]
    [ComDefaultInterface(typeof (_Exception))]
    [ComVisible(true)]
    [Serializable]
    public class SettingsException : Exception
    {
        public SettingsException()
        {
        }

        public SettingsException(string message) : base(message)
        {
        }

        public SettingsException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected SettingsException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}