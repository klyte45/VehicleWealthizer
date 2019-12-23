using static Citizen;

namespace Klyte.VehicleWealthizer
{
    public enum VWConfigIndex
    {
        NIL = -1,
        ADC_DESC_PART = 0x7F000000,
        WEALTH_PART = 0xFF0000,
        TYPE_PART = 0x00FF00,
        DESC_DATA = 0xFF,

        GLOBAL_CONFIG = 0x1000000,

        TYPE_STRING = 0x0100,
        TYPE_INT = 0x0200,
        TYPE_BOOL = 0x0300,
        TYPE_LIST = 0x0400,
        TYPE_DICTIONARY = 0x0500,

        WEALTH_LOW = (Wealth.Low + 1) << 16,
        WEALTH_MED = (Wealth.Medium + 1) << 16,
        WEALTH_HGH = (Wealth.High + 1) << 16,

        VEHICLE_ASSETS_DATA = 0x1 | TYPE_STRING | GLOBAL_CONFIG,

    }
}
