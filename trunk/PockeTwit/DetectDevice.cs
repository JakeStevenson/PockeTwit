using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;

namespace PockeTwit
{
    class DetectDevice
    {

		#region Fields (5) 

        const int _bufferSize = 32;
        private static DeviceType _DeviceType = DeviceType.Undefined;
        const string _pocketPcTypeString = "PocketPC";
        const string _smartphoneTypeString = "SmartPhone";
        const uint SPI_GETPLATFORMTYPE=257;

		#endregion Fields 

		#region Properties (1) 

        public static DeviceType DeviceType
        {
            get
            {
                if (_DeviceType == DeviceType.Undefined)
                {
                    string platformType = GetPlatformType();

                    switch (platformType)
                    {
                        case _smartphoneTypeString:
                            _DeviceType = DeviceType.Standard;
                            break;
                        case _pocketPcTypeString:
                            _DeviceType = DeviceType.Professional;
                            break;
                    }
                }
                return _DeviceType;
            }
        }

		#endregion Properties 

		#region Methods (2) 


		// Private Methods (2) 

        private static string GetPlatformType()
        {
            StringBuilder platformType = new StringBuilder(_bufferSize);
            SystemParametersInfo(SPI_GETPLATFORMTYPE, _bufferSize, platformType, 0);
            return platformType.ToString();
        }

        [DllImport("CoreDLL.dll")]
        static extern bool SystemParametersInfo(uint uiAction, uint uiParam, StringBuilder pvParam, uint unused);


		#endregion Methods 

    }

    [Flags]
    enum DeviceType
    {
        Undefined = 0x00,
        Professional = 0x01,
        Standard = 0x02
    }
}
