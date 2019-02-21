using ColossalFramework;
using ColossalFramework.DataBinding;
using ColossalFramework.Globalization;
using ColossalFramework.IO;
using ColossalFramework.UI;
using ICities;
using Klyte.Commons.Extensors;
using Klyte.Commons.Interfaces;
using Klyte.Commons.UI;
using Klyte.VehicleWealthizer.i18n;
using Klyte.VehicleWealthizer.TextureAtlas;
using Klyte.VehicleWealthizer.UI;
using Klyte.VehicleWealthizer.Utils;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;

[assembly: AssemblyVersion("1.0.0.9999")]
namespace Klyte.VehicleWealthizer
{
    public class VehicleWealthizerMod : BasicIUserMod<VehicleWealthizerMod, VWLocaleUtils, VWResourceLoader, MonoBehaviour, VWCommonTextureAtlas, VWPanel>
    {

        public static readonly string FOLDER_NAME = "VehicleWealthizer";
        public static readonly string FOLDER_PATH = VWUtils.BASE_FOLDER_PATH + FOLDER_NAME;
        public const string IMPORT_EXPORT_SUBFOLDER_NAME = "ImportExportVehicleWealth";
        public static string importExportWealthFolder => FOLDER_PATH + Path.DirectorySeparatorChar + IMPORT_EXPORT_SUBFOLDER_NAME;
        public static string configsFolder => VWConfigWarehouse.CONFIG_PATH;

        protected override ModTab? Tab => ModTab.VehicleWealthizer;
        protected override float? TabWidth => 500;


        public override string SimpleName => "Vehicle Wealthizer";

        public override string Description => "Allow categorize vehicles by citizen wealth. Requires Klyte Commons.";

        public override void doErrorLog(string fmt, params object[] args)
        {
            VWUtils.doErrorLog(fmt, args);
        }

        public override void doLog(string fmt, params object[] args)
        {
            VWUtils.doLog(fmt, args);
        }

        public override void LoadSettings()
        {
            VWUtils.EnsureFolderCreation(configsFolder);
            VWUtils.EnsureFolderCreation(importExportWealthFolder);
        }

        public override void TopSettingsUI(UIHelperExtension helper)
        {
        }


        public VehicleWealthizerMod()
        {
            Construct();
        }

    }

    public class UIButtonLineInfo : UIButton
    {
        public ushort lineID;
    }
}
