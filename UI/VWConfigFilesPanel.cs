using ColossalFramework.Globalization;
using ColossalFramework.UI;
using Klyte.Commons.Extensors;
using Klyte.Commons.Utils;
using Klyte.VehicleWealthizer.Extensors;
using Klyte.VehicleWealthizer.Listing;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Klyte.VehicleWealthizer.UI
{

    internal class VWConfigFilesPanel : UICustomControl
    {
        private const int NUM_SERVICES = 0;
        private static VWConfigFilesPanel m_instance;
        private UIPanel controlContainer;

        public static VWConfigFilesPanel instance => m_instance;
        public UIPanel m_mainPanel { get; private set; }


        public const string EXT_CONF = ".vwwcnf";
        private UIDropDown m_ddImport;
        private UIButton m_btnImport;
        private Dictionary<string, string> m_importFiles;

        #region Awake
        private void Awake()
        {
            m_instance = this;
            controlContainer = GetComponent<UIPanel>();
            controlContainer.name = "VWConfigFilesPanel";
            controlContainer.autoLayout = true;
            controlContainer.autoLayoutDirection = LayoutDirection.Vertical;
            controlContainer.clipChildren = true;
            var group1 = new UIHelperExtension(controlContainer);

            UILabel lblTitle = group1.AddLabel(Locale.Get("K45_VW_IMPORT_EXPORT_TITLE"));
            lblTitle.autoSize = true;
            lblTitle.minimumSize = Vector2.zero;
            lblTitle.maximumSize = Vector2.zero;
            lblTitle.wordWrap = false;
            KlyteMonoUtils.LimitWidth(lblTitle, (uint) (controlContainer.width - 10));
            KlyteMonoUtils.LimitWidth((UIButton) group1.AddButton(Locale.Get("K45_VW_OPEN_FOLDER_IMPORT_EXPORT"), () => { ColossalFramework.Utils.OpenInFileBrowser(FileUtils.EnsureFolderCreation(VehicleWealthizerMod.ImportExportWealthFolder).FullName); }), (uint) (controlContainer.width - 10));

            KlyteMonoUtils.LimitWidth((UIButton) group1.AddButton(Locale.Get("K45_VW_RELOAD_IMPORT_FILES"), ReloadImportFiles), (uint) (controlContainer.width - 10));
            m_ddImport = group1.AddDropdown(Locale.Get("K45_VW_SELECT_FILE_IMPORT"), new string[0], "", (x) => { m_btnImport.isEnabled = (x >= 0); });
            ConfigComponentPanel(m_ddImport);
            m_btnImport = (UIButton) group1.AddButton(Locale.Get("K45_VW_IMPORT_SELECTED"), Import);
            KlyteMonoUtils.LimitWidth(m_btnImport, (uint) (controlContainer.width - 10));
            KlyteMonoUtils.LimitWidth((UIButton) group1.AddButton(Locale.Get("K45_VW_EXPORT_CURRENT"), Export), (uint) (controlContainer.width - 10));

            ReloadImportFiles();
            m_btnImport.isEnabled = (m_ddImport.items.Length > 0);
        }
        #endregion

        private void ConfigComponentPanel(UIComponent reference)
        {
            reference.GetComponentInParent<UIPanel>().autoFitChildrenVertically = true;
            KlyteMonoUtils.CreateElement(out UIPanel labelContainer, reference.parent.transform);
            labelContainer.size = new Vector2(240, reference.height);
            labelContainer.zOrder = 0;
            UILabel lbl = reference.parent.GetComponentInChildren<UILabel>();
            lbl.transform.SetParent(labelContainer.transform);
            lbl.textAlignment = UIHorizontalAlignment.Center;
            lbl.minimumSize = new Vector2(240, reference.height);
            KlyteMonoUtils.LimitWidth(lbl, 240);
            lbl.verticalAlignment = UIVerticalAlignment.Middle;
            lbl.pivot = UIPivotPoint.TopCenter;
            lbl.relativePosition = new Vector3(0, lbl.relativePosition.y);
            reference.width = controlContainer.width - 10;
        }

        private void ReloadImportFiles()
        {
            m_importFiles = new Dictionary<string, string>();
            foreach (string filename in Directory.GetFiles(VehicleWealthizerMod.ImportExportWealthFolder, "*" + EXT_CONF).Select(x => x.Split(Path.DirectorySeparatorChar).Last()))
            {
                string name = filename.Substring(0, filename.Length - EXT_CONF.Length);
                m_importFiles[name] = VehicleWealthizerMod.ImportExportWealthFolder + Path.DirectorySeparatorChar + filename;
            }
            m_ddImport.items = m_importFiles.Keys.ToArray();
        }

        private void Import()
        {
            string fileContents = File.ReadAllText(m_importFiles[m_ddImport.selectedValue], Encoding.UTF8);
            VWVehicleExtensionUtils.DeserializeGeneratedFile(fileContents);
            VWVehicleList.instance.Invalidate();
        }

        private void Export()
        {
            string contents = VWVehicleExtensionUtils.GenerateSerializedFile();
            string filename = $"{SimulationManager.instance.m_metaData.m_CityName} - {DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss")}";
            File.WriteAllText(VehicleWealthizerMod.ImportExportWealthFolder + Path.DirectorySeparatorChar + filename + EXT_CONF, contents);
            ReloadImportFiles();
            m_ddImport.selectedValue = filename;
            VWVehicleList.instance.Invalidate();
        }
    }
}
