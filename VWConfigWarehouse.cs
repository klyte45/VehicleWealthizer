using ColossalFramework.Globalization;
using Klyte.Commons.Interfaces;
using Klyte.VehicleWealthizer.Extensors;
using Klyte.VehicleWealthizer.Utils;
using System;
using System.IO;
using System.Linq;
using UnityEngine;
using static Citizen;

namespace Klyte.VehicleWealthizer
{
    internal class VWConfigWarehouse : ConfigWarehouseBase<VWConfigWarehouse.ConfigIndex, VWConfigWarehouse>
    {
        public const string CONFIG_FILENAME = "CityConfigV0";
        public static readonly string CONFIG_PATH = VWUtils.BASE_FOLDER_PATH + VWSingleton.FOLDER_NAME + Path.DirectorySeparatorChar + "CityConfigs" + Path.DirectorySeparatorChar;
        public override string ConfigFilename => CONFIG_FILENAME;
        public override string ConfigPath => CONFIG_PATH;
        public const string TRUE_VALUE = "1";
        public const string FALSE_VALUE = "0";

        public bool unsafeMode = false;
        public VWConfigWarehouse() { }

        public enum ConfigIndex
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

        public override bool getDefaultBoolValueForProperty(ConfigIndex i)
        {
            return false;
        }

        public override int getDefaultIntValueForProperty(ConfigIndex i)
        {
            return 0;
        }

        internal static ConfigIndex getConfigAssetsForDef(ref CitizenWealthDefinition definition)
        {
            return getConfigServiceSystemForDefinition(ref definition) | ConfigIndex.VEHICLE_ASSETS_DATA;
        }

        internal static ConfigIndex getConfigServiceSystemForDefinition(ref CitizenWealthDefinition serviceSystemDefinition)
        {
            if (serviceSystemDefinition == CitizenWealthDefinition.LOW) return ConfigIndex.WEALTH_LOW;
            if (serviceSystemDefinition == CitizenWealthDefinition.MEDIUM) return ConfigIndex.WEALTH_MED;
            if (serviceSystemDefinition == CitizenWealthDefinition.HIGH) return ConfigIndex.WEALTH_HGH;
            return ConfigIndex.NIL;
        }
    }
}
