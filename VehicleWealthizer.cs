using ColossalFramework.UI;
using Klyte.Commons.Extensors;
using Klyte.Commons.Interfaces;
using Klyte.Commons.Utils;
using Klyte.VehicleWealthizer.UI;
using System.IO;
using System.Reflection;
using UnityEngine;

[assembly: AssemblyVersion("2.0.0.0")]
namespace Klyte.VehicleWealthizer
{
    public class VehicleWealthizerMod : BasicIUserMod<VehicleWealthizerMod, MonoBehaviour, VWPanel>
    {

        public static readonly string FOLDER_NAME = "VehicleWealthizer";
        public static readonly string FOLDER_PATH = FileUtils.BASE_FOLDER_PATH + FOLDER_NAME;
        public const string IMPORT_EXPORT_SUBFOLDER_NAME = "ImportExportVehicleWealth";
        public static string ImportExportWealthFolder { get; } = FOLDER_PATH + Path.DirectorySeparatorChar + IMPORT_EXPORT_SUBFOLDER_NAME;

        protected override float? TabWidth => 885;


        public override string SimpleName => "Vehicle Wealthizer";

        public override string Description => "Allow categorize vehicles by citizen wealth.";

        public override string IconName => "K45_VW_Icon";

        public override void DoErrorLog(string fmt, params object[] args) => LogUtils.DoErrorLog(fmt, args);

        public override void DoLog(string fmt, params object[] args) => LogUtils.DoLog(fmt, args);

        public override void LoadSettings()
        {
            FileUtils.EnsureFolderCreation(Commons.CommonProperties.ModRootFolder);
            FileUtils.EnsureFolderCreation(ImportExportWealthFolder);
        }

        public override void TopSettingsUI(UIHelperExtension helper)
        {
        }

        public VehicleWealthizerMod() => Construct();

    }

    public class UIButtonLineInfo : UIButton
    {
        public ushort lineID;
    }
}
