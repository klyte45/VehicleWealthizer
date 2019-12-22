using ColossalFramework;
using ColossalFramework.UI;
using ICities;
using Klyte.Commons.Extensors;
using Klyte.Commons.Interfaces;
using Klyte.VehicleWealthizer.TextureAtlas;
using Klyte.VehicleWealthizer.UI;
using Klyte.VehicleWealthizer.Utils;
using System.IO;
using System.Reflection;
using UnityEngine;

[assembly: AssemblyVersion("1.1.0.0")]
namespace Klyte.VehicleWealthizer
{
    public class VehicleWealthizerMod : BasicIUserMod<VehicleWealthizerMod, VWResourceLoader, MonoBehaviour, VWCommonTextureAtlas, VWPanel>
    {

        public static readonly string FOLDER_NAME = "VehicleWealthizer";
        public static readonly string FOLDER_PATH = VWUtils.BASE_FOLDER_PATH + FOLDER_NAME;
        public const string IMPORT_EXPORT_SUBFOLDER_NAME = "ImportExportVehicleWealth";
        public static string importExportWealthFolder => FOLDER_PATH + Path.DirectorySeparatorChar + IMPORT_EXPORT_SUBFOLDER_NAME;
        public static string configsFolder => VWConfigWarehouse.CONFIG_PATH;

        protected override float? TabWidth => 885;


        public override string SimpleName => "Vehicle Wealthizer";

        public override string Description => "Allow categorize vehicles by citizen wealth.";

        public override void doErrorLog(string fmt, params object[] args) => VWUtils.doErrorLog(fmt, args);

        public override void doLog(string fmt, params object[] args) => VWUtils.doLog(fmt, args);

        public override void LoadSettings()
        {
            VWUtils.EnsureFolderCreation(configsFolder);
            VWUtils.EnsureFolderCreation(importExportWealthFolder);
        }

        public override void TopSettingsUI(UIHelperExtension helper)
        {
        }

        public static SavedFloat ButtonPosX { get; } = new SavedFloat("K45_ButtonPosX", Settings.gameSettingsFile, 300, true);
        public static SavedFloat ButtonPosY { get; } = new SavedFloat("K45_ButtonPosY", Settings.gameSettingsFile, 20, true);

        private UIButton m_modPanelButton;
        private UITabstrip m_modsTabstrip;
        private UIPanel m_modsPanel;

        public override void OnLevelLoaded(LoadMode mode)
        {
            base.OnLevelLoaded(mode);
            m_modsPanel = UIView.Find<UIPanel>("K45_ModsPanel");
            if (m_modsPanel == null)
            {
                UIComponent uicomponent = UIView.Find("TSBar");
                UIPanel bg = uicomponent.AddUIComponent<UIPanel>();
                bg.name = "K45_MB";
                bg.absolutePosition = new Vector2(ButtonPosX.value, ButtonPosY.value);
                bg.width = 60f;
                bg.height = 60f;
                bg.zOrder = 1;
                UIButton doneButton = bg.AddUIComponent<UIButton>();
                doneButton.normalBgSprite = "GenericPanel";
                doneButton.width = 100f;
                doneButton.height = 50f;
                doneButton.relativePosition = new Vector2(-40f, 70f);
                doneButton.text = "Done";
                doneButton.hoveredTextColor = new Color32(0, byte.MaxValue, byte.MaxValue, 1);
                doneButton.Hide();
                doneButton.zOrder = 99;
                UIDragHandle handle = bg.AddUIComponent<UIDragHandle>();
                handle.name = "K45_DragHandle";
                handle.relativePosition = Vector2.zero;
                handle.width = bg.width - 5f;
                handle.height = bg.height - 5f;
                handle.zOrder = 0;
                handle.target = bg;
                handle.Start();
                handle.enabled = false;
                bg.zOrder = 9;

                bg.isInteractive = false;
                handle.zOrder = 10;
                doneButton.eventClick += (component, ms) =>
                {
                    doneButton.Hide();
                    handle.zOrder = 10;
                    handle.enabled = false;
                    ButtonPosX.value = (int) bg.absolutePosition.x;
                    ButtonPosY.value = (int) bg.absolutePosition.y;
                };
                bg.color = new Color32(96, 96, 96, byte.MaxValue);
                m_modPanelButton = bg.AddUIComponent<UIButton>();
                m_modPanelButton.disabledTextColor = new Color32(128, 128, 128, byte.MaxValue);
                VWUtils.initButton(m_modPanelButton, false, CommonTextureAtlas.instance.SpriteNames[0], false);
                m_modPanelButton.atlas = CommonTextureAtlas.instance.atlas;
                m_modPanelButton.relativePosition = new Vector3(5f, 0f);
                m_modPanelButton.size = new Vector2(64, 64);
                m_modPanelButton.name = "K45_ModsButton";
                m_modPanelButton.zOrder = 11;
                m_modPanelButton.textScale = 1.3f;
                m_modPanelButton.textVerticalAlignment = UIVerticalAlignment.Middle;
                m_modPanelButton.textHorizontalAlignment = UIHorizontalAlignment.Center;
                m_modPanelButton.eventDoubleClick += (component, ms) =>
                {
                    handle.zOrder = 13;
                    doneButton.Show();
                    handle.enabled = true;
                };

                m_modsPanel = bg.AddUIComponent<UIPanel>();
                m_modsPanel.name = "K45_ModsPanel";
                m_modsPanel.size = new Vector2(875, 550);
                m_modsPanel.relativePosition = new Vector3(0f, 7f);
                m_modsPanel.isInteractive = false;
                m_modsPanel.Hide();

                m_modPanelButton.eventClicked += TogglePanel;

                CreateTabsComponent(out m_modsTabstrip, out _, m_modsPanel.transform, "K45", new Vector4(74, 0, m_modsPanel.width - 84, 40), new Vector4(64, 40, m_modsPanel.width - 64, m_modsPanel.height));
            }
            else
            {
                m_modPanelButton = UIView.Find<UIButton>("K45_ModsButton");
                m_modsTabstrip = UIView.Find<UITabstrip>("K45_Tabstrip");
            }

            AddTab();
        }
        public static void CreateTabsComponent(out UITabstrip tabstrip, out UITabContainer tabContainer, Transform parent, string namePrefix, Vector4 areaTabstrip, Vector4 areaContainer)
        {
            VWUtils.createUIElement(out tabstrip, parent, $"{namePrefix}_Tabstrip", areaTabstrip);

            VWUtils.createUIElement(out tabContainer, parent, $"{namePrefix}_TabContainer", areaContainer);
            tabstrip.tabPages = tabContainer;
            tabstrip.selectedIndex = 0;
            tabstrip.selectedIndex = -1;
        }
        private void TogglePanel(UIComponent component, UIMouseEventParameter eventParam)
        {
            if (m_modsPanel == null)
            {
                return;
            }

            m_modsPanel.isVisible = !m_modsPanel.isVisible;
            if (m_modsPanel.isVisible)
            {
                m_modPanelButton?.Focus();
            }
            else
            {
                m_modPanelButton?.Unfocus();
            }
        }


        internal void AddTab()
        {
            if (m_modsTabstrip.Find<UIComponent>("VW") != null)
            {
                return;
            }

            UIButton superTab = CreateTabTemplate();
            superTab.normalFgSprite = VWCommonTextureAtlas.instance.SpriteNames[0];
            superTab.atlas = VWCommonTextureAtlas.instance.atlas;
            superTab.color = Color.gray;
            superTab.focusedColor = Color.white;
            superTab.hoveredColor = Color.white;
            superTab.disabledColor = Color.black;
            superTab.playAudioEvents = true;
            superTab.tooltip = instance.Name;
            superTab.foregroundSpriteMode = UIForegroundSpriteMode.Stretch;

            VWUtils.createUIElement(out UIPanel content, null);
            content.name = "Container";
            content.area = new Vector4(0, 0, m_modsPanel.width + 70, m_modsPanel.height);

            UIComponent component = m_modsTabstrip.AddTab("VW", superTab.gameObject, content.gameObject, typeof(VWPanel));

            content.eventVisibilityChanged += (x, y) => { if (y) { instance.showVersionInfoPopup(); } };


        }
        private static UIButton CreateTabTemplate()
        {
            VWUtils.createUIElement(out UIButton tabTemplate, null, "KCTabTemplate");
            VWUtils.initButton(tabTemplate, false, "GenericTab");
            tabTemplate.autoSize = false;
            tabTemplate.width = 40;
            tabTemplate.height = 40;
            tabTemplate.foregroundSpriteMode = UIForegroundSpriteMode.Scale;
            return tabTemplate;
        }
        public void ClosePanel()
        {
            if (m_modsPanel == null)
            {
                return;
            }

            m_modsPanel.isVisible = false;
            m_modPanelButton?.Unfocus();

        }

        public void OpenPanel()
        {
            if (m_modsPanel == null)
            {
                return;
            }

            m_modsPanel.isVisible = true;
            m_modPanelButton?.Focus();
        }

        public void OpenPanelAtModTab()
        {
            OpenPanel();
            m_modsTabstrip.ShowTab("VW");
        }

        public VehicleWealthizerMod() => Construct();

    }

    public class UIButtonLineInfo : UIButton
    {
        public ushort lineID;
    }
}
