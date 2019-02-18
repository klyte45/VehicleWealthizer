using ColossalFramework;
using ColossalFramework.DataBinding;
using ColossalFramework.Globalization;
using ColossalFramework.IO;
using ColossalFramework.UI;
using ICities;
using Klyte.Commons.Extensors;
using Klyte.Commons.Interfaces;
using Klyte.VehicleWealthizer.i18n;
using Klyte.VehicleWealthizer.Utils;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;

[assembly: AssemblyVersion("1.0.0.9999")]
namespace Klyte.VehicleWealthizer
{
    public class VehicleWealthizerMod : BasicIUserMod<VehicleWealthizerMod, VWLocaleUtils, VWResourceLoader>
    {

        public static readonly string FOLDER_NAME = "VehicleWealthizer";
        public static readonly string FOLDER_PATH = VWUtils.BASE_FOLDER_PATH + FOLDER_NAME;
        public const string IMPORT_EXPORT_SUBFOLDER_NAME = "ImportExportVehicleWealth";
        public static string importExportWealthFolder => FOLDER_PATH + Path.DirectorySeparatorChar + IMPORT_EXPORT_SUBFOLDER_NAME;
        public static string configsFolder => VWConfigWarehouse.CONFIG_PATH;


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

        public override void OnCreated(ILoading loading)
        {
        }

        public override void OnLevelLoaded(LoadMode mode)
        {
            VWUtils.doLog("LEVEL LOAD");
            if (mode != LoadMode.LoadGame && mode != LoadMode.NewGame && mode != LoadMode.NewGameFromScenario)
            {
                VWUtils.doLog("NOT GAME ({0})", mode);
                return;
            }

            Assembly asm = Assembly.GetAssembly(typeof(VehicleWealthizerMod));
            Type[] types = asm.GetTypes();

            VWController.instance.Awake();
        }

        public override void OnLevelUnloading()
        {
        }

        public override void OnReleased()
        {
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
