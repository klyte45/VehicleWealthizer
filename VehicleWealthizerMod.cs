using Klyte.Commons.Interfaces;
using Klyte.Commons.Utils;
using Klyte.VehicleWealthizer.UI;
using System.IO;
using System.Reflection;

[assembly: AssemblyVersion("3.0.0.0")]
namespace Klyte.VehicleWealthizer
{
    public class VehicleWealthizerMod : BasicIUserMod<VehicleWealthizerMod, VWController, VWPanel>
    {

        public static readonly string FOLDER_NAME = "VehicleWealthizer";
        public static readonly string FOLDER_PATH = FileUtils.BASE_FOLDER_PATH + FOLDER_NAME;
        public const string IMPORT_EXPORT_SUBFOLDER_NAME = "ImportExportVehicleWealth";
        public static string ImportExportWealthFolder { get; } = FOLDER_PATH + Path.DirectorySeparatorChar + IMPORT_EXPORT_SUBFOLDER_NAME;

        public override string SimpleName => "Vehicle Wealthizer";

        public override string Description => "Allow categorize vehicles by citizen wealth.";

        public override string IconName => "K45_VW_Icon";

        protected void Awake()
        {
            FileUtils.EnsureFolderCreation(Commons.CommonProperties.ModRootFolder);
            FileUtils.EnsureFolderCreation(ImportExportWealthFolder);
        }


    }
}
