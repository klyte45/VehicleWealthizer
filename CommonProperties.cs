using Klyte.VehicleWealthizer;

namespace Klyte.Commons
{
    public static class CommonProperties
    {
        public static bool DebugMode => VehicleWealthizerMod.DebugMode;
        public static string Version => VehicleWealthizerMod.Version;
        public static string ModName => VehicleWealthizerMod.Instance.SimpleName;
        public static string Acronym => "VW";
        public static string ModRootFolder => VehicleWealthizerMod.FOLDER_PATH;
        public static string ModIcon => VehicleWealthizerMod.Instance.IconName;
        public static string ModDllRootFolder => VehicleWealthizerMod.RootFolder;
    }
}