using ColossalFramework;
using ColossalFramework.DataBinding;
using ColossalFramework.Globalization;
using ColossalFramework.IO;
using ColossalFramework.UI;
using ICities;
using Klyte.Commons.Extensors;
using Klyte.VehicleWealthizer.i18n;
using Klyte.VehicleWealthizer.Utils;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;

[assembly: AssemblyVersion("1.0.0.0")]
namespace Klyte.VehicleWealthizer
{
    public class VehicleWealthizerMod : IUserMod, ILoadingExtension
    {

        public string Name => "Vehicle Wealthizer " + VWSingleton.version;
        public string Description => "Allow categorize vehicles by citizen wealth. Requires Klyte Commons.";

        private static bool m_isKlyteCommonsLoaded = false;
        public static bool IsKlyteCommonsEnabled()
        {
            if (!m_isKlyteCommonsLoaded)
            {
                try
                {
                    var assemblies = AppDomain.CurrentDomain.GetAssemblies();
                    var assembly = (from a in assemblies
                                    where a.GetType("Klyte.Commons.KlyteCommonsMod") != null
                                    select a).SingleOrDefault();
                    if (assembly != null)
                    {
                        m_isKlyteCommonsLoaded = true;
                    }
                }
                catch { }
            }
            return m_isKlyteCommonsLoaded;
        }


        public void OnSettingsUI(UIHelperBase helperDefault)
        {
            if (!IsKlyteCommonsEnabled())
            {
                return;
            }
            UIHelperExtension lastUIHelper = new UIHelperExtension((UIHelper)helperDefault);
            VWSingleton.instance.LoadSettingsUI(lastUIHelper);
        }

        public void OnCreated(ILoading loading) { }

        public void OnLevelLoaded(LoadMode mode)
        {
            if (!IsKlyteCommonsEnabled())
            {
                throw new Exception("Vehicle Wealthizer requires Klyte Commons active!");
            }
            VWSingleton.instance.doOnLevelLoad(mode);
        }

        public void OnLevelUnloading()
        {
            VWSingleton.instance.doOnLevelUnload();
            try
            {
                GameObject.Destroy(VWSingleton.instance?.gameObject);
            }
            catch { }
        }

        public void OnReleased()
        {
            try
            {
                GameObject.Destroy(VWSingleton.instance?.gameObject);
            }
            catch { }
        }
    }

    internal class VWSingleton : Singleton<VWSingleton>
    {
        public static readonly string FOLDER_NAME = "VehicleWealthizer";
        public static readonly string FOLDER_PATH = VWUtils.BASE_FOLDER_PATH + FOLDER_NAME;
        public const string IMPORT_EXPORT_SUBFOLDER_NAME = "ImportExportVehicleWealth";

        public static string importExportWealthFolder => FOLDER_PATH + Path.DirectorySeparatorChar + IMPORT_EXPORT_SUBFOLDER_NAME;
        public static string configsFolder => VWConfigWarehouse.CONFIG_PATH;

        public static string minorVersion
        {
            get {
                return majorVersion + "." + typeof(VWSingleton).Assembly.GetName().Version.Build;
            }
        }
        public static string majorVersion
        {
            get {
                return typeof(VWSingleton).Assembly.GetName().Version.Major + "." + typeof(VWSingleton).Assembly.GetName().Version.Minor;
            }
        }
        public static string fullVersion
        {
            get {
                return minorVersion + " r" + typeof(VWSingleton).Assembly.GetName().Version.Revision;
            }
        }
        public static string version
        {
            get {
                if (typeof(VWSingleton).Assembly.GetName().Version.Minor == 0 && typeof(VWSingleton).Assembly.GetName().Version.Build == 0)
                {
                    return typeof(VWSingleton).Assembly.GetName().Version.Major.ToString();
                }
                if (typeof(VWSingleton).Assembly.GetName().Version.Build > 0)
                {
                    return minorVersion;
                }
                else
                {
                    return majorVersion;
                }
            }
        }



        private SavedBool m_debugMode;


        public bool needShowPopup;
        private bool isLocaleLoaded = false;

        public static SavedBool debugMode => VWSingleton.instance.m_debugMode;

        private SavedString currentSaveVersion = new SavedString("VWSaveVersion", Settings.gameSettingsFile, "null", true);

        private SavedInt currentLanguageId = new SavedInt("VWLanguage", Settings.gameSettingsFile, 0, true);

        public int currentLanguageIdx => currentLanguageId.value;




        internal void doOnLevelLoad(LoadMode mode)
        {

            VWUtils.doLog("LEVEL LOAD");
            if (mode != LoadMode.LoadGame && mode != LoadMode.NewGame && mode != LoadMode.NewGameFromScenario)
            {
                VWUtils.doLog("NOT GAME ({0})", mode);
                return;
            }

            Assembly asm = Assembly.GetAssembly(typeof(VWSingleton));
            Type[] types = asm.GetTypes();

            VWController.instance.Awake();
        }

        public void Awake()
        {
            Debug.LogWarningFormat("VWv" + VWSingleton.majorVersion + " LOADING TLM ");
            VWUtils.EnsureFolderCreation(configsFolder);
            VWUtils.EnsureFolderCreation(importExportWealthFolder);
            string currentConfigPath = PathUtils.AddExtension(VWConfigWarehouse.CONFIG_PATH + VWConfigWarehouse.CONFIG_FILENAME + "_" + VWConfigWarehouse.GLOBAL_CONFIG_INDEX, GameSettings.extension);
            if (!File.Exists(currentConfigPath))
            {
                var legacyFilename = Path.Combine(DataLocation.localApplicationData, PathUtils.AddExtension("TransportsLinesManager5_DEFAULT", GameSettings.extension));
                if (File.Exists(legacyFilename))
                {
                    File.Copy(legacyFilename, currentConfigPath);
                }
            }
            Debug.LogWarningFormat("VWv" + VWSingleton.majorVersion + " LOADING VARS ");

            m_debugMode = new SavedBool("KVWdebugMode", Settings.gameSettingsFile, false, true);

            if (m_debugMode.value)
                VWUtils.doLog("currentSaveVersion.value = {0}, fullVersion = {1}", currentSaveVersion.value, fullVersion);
            if (currentSaveVersion.value != fullVersion)
            {
                needShowPopup = true;
            }
            LocaleManager.eventLocaleChanged += new LocaleManager.LocaleChangedHandler(this.autoLoadVWLocale);
            if (instance != null) GameObject.Destroy(instance);
            loadVWLocale(false);

            onAwake?.Invoke();
        }

        public bool showVersionInfoPopup(bool force = false)
        {
            if (needShowPopup || force)
            {
                try
                {
                    UIComponent uIComponent = UIView.library.ShowModal("ExceptionPanel");
                    if (uIComponent != null)
                    {
                        Cursor.lockState = CursorLockMode.None;
                        Cursor.visible = true;
                        BindPropertyByKey component = uIComponent.GetComponent<BindPropertyByKey>();
                        if (component != null)
                        {
                            string title = "Vehicle Wealthizer v" + version;
                            string notes = VWResourceLoader.instance.loadResourceString("UI.VersionNotes.txt");
                            string text = "Vehicle Wealthizer was updated! Release notes:\r\n\r\n" + notes;
                            string img = "IconMessage";
                            component.SetProperties(TooltipHelper.Format(new string[]
                            {
                            "title",
                            title,
                            "message",
                            text,
                            "img",
                            img
                            }));
                            needShowPopup = false;
                            currentSaveVersion.value = fullVersion;
                            return true;
                        }
                        return false;
                    }
                    else
                    {
                        if (VWSingleton.instance != null && VWSingleton.debugMode)
                            VWUtils.doLog("PANEL NOT FOUND!!!!");
                        return false;
                    }
                }
                catch (Exception e)
                {
                    if (VWSingleton.instance != null && VWSingleton.debugMode)
                        VWUtils.doLog("showVersionInfoPopup ERROR {0} {1}", e.GetType(), e.Message);
                }
            }
            return false;
        }

        internal delegate void OnLocaleLoaded();
        internal static event OnLocaleLoaded onAwake;

        internal void LoadSettingsUI(UIHelperExtension helper)
        {
            try
            {
                foreach (Transform child in helper.self.transform)
                {
                    GameObject.Destroy(child.gameObject);
                }
            }
            catch
            {

            }

            foreach (Transform child in helper.self.transform)
            {
                GameObject.Destroy(child?.gameObject);
            }

            helper.self.eventVisibilityChanged += delegate (UIComponent component, bool b)
            {
                if (b)
                {
                    showVersionInfoPopup();
                }
            };

            UIHelperExtension group9 = helper.AddGroupExtended($"VW v{VWSingleton.version}");
            group9.AddDropdownLocalized("VW_MOD_LANG", VWLocaleUtils.instance.getLanguageIndex(), currentLanguageId.value, delegate (int idx)
            {
                currentLanguageId.value = idx;
                loadVWLocale(true);
            });
            group9.AddCheckbox(Locale.Get("VW_DEBUG_MODE"), m_debugMode.value, delegate (bool val) { m_debugMode.value = val; });
            group9.AddLabel("Version: " + fullVersion);
            group9.AddLabel(Locale.Get("VW_ORIGINAL_TLM_VERSION") + " " + string.Join(".", VWResourceLoader.instance.loadResourceString("TLMVersion.txt").Split(".".ToCharArray()).Take(3).ToArray()));
            group9.AddButton(Locale.Get("VW_RELEASE_NOTES"), delegate ()
            {
                showVersionInfoPopup(true);
            });
            VWUtils.doLog("End Loading Options");

        }

        public void autoLoadVWLocale()
        {
            if (currentLanguageId.value == 0)
            {
                loadVWLocale(false);
            }
        }
        public void loadVWLocale(bool force, int? idx = null)
        {
            if (idx != null)
            {
                currentLanguageId.value = (int)idx;
            }
            if (SingletonLite<LocaleManager>.exists)
            {
                VWLocaleUtils.instance.loadLocale(currentLanguageId.value == 0 ? SingletonLite<LocaleManager>.instance.language : VWLocaleUtils.instance.getSelectedLocaleByIndex(currentLanguageId.value), force);
                if (!isLocaleLoaded)
                {
                    isLocaleLoaded = true;
                }
            }
        }

        internal void doOnLevelUnload()
        {
            if (VWController.instance != null)
            {
                GameObject.Destroy(VWController.instance);
            }
        }
    }

    public class UIButtonLineInfo : UIButton
    {
        public ushort lineID;
    }



}
