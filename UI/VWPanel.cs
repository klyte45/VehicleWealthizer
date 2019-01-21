using ColossalFramework.Globalization;
using ColossalFramework.UI;
using Klyte.Commons.UI;
using Klyte.Commons.Utils;
using Klyte.VehicleWealthizer.Extensors;
using Klyte.VehicleWealthizer.Listing;
using Klyte.VehicleWealthizer.Utils;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Klyte.VehicleWealthizer.UI
{

    internal class VWPanel : UICustomControl
    {
        private const int NUM_SERVICES = 0;
        private static VWPanel m_instance;
        private UIPanel controlContainer;

        public static VWPanel instance => m_instance;
        public UIPanel m_mainPanel { get; private set; }



        #region Awake
        private void Awake()
        {
            m_instance = this;
            controlContainer = GetComponent<UIPanel>();
            controlContainer.area = new Vector4(0, 0, 0, 0);
            controlContainer.isVisible = false;
            controlContainer.name = "VWPanel";


            VWUtils.createUIElement(out UIPanel _mainPanel, GetComponent<UIPanel>().transform, "VWListPanel", new Vector4(0, 0, controlContainer.parent.width, controlContainer.parent.height));
            m_mainPanel = _mainPanel;
            m_mainPanel.backgroundSprite = "MenuPanel2";



            CreateTitleBar();
            VWUtils.CreateScrollPanel(_mainPanel, out UIScrollablePanel scrollablePanel, out UIScrollbar scrollbar, 450, controlContainer.height - 120, new Vector3(10, 110));

            _mainPanel.gameObject.AddComponent<VWVehicleList>();
            CreateTitleRow(out UIPanel title, _mainPanel);

            SetPreviewWindow();
            CreateRemoveUnwantedButton();

            VWUtils.createUIElement(out UIPanel exportPanel, m_mainPanel.transform, "ImportExportPanel", new Vector4(480,275,380,275));
            exportPanel.gameObject.AddComponent<VWConfigFilesPanel>();

        }

        private void OnOpenClosePanel(UIComponent component, bool value)
        {
            if (value)
            {
                VWSingleton.instance.showVersionInfoPopup();
            }
        }

        private void CreateTitleRow(out UIPanel titleLine, UIComponent parent)
        {
            VWUtils.createUIElement(out titleLine, parent.transform, "VWtitleline", new Vector4(5, 60, 500, 40));

            VWUtils.createUIElement(out UILabel modelNameLabel, titleLine.transform, "districtNameLabel");
            modelNameLabel.autoSize = false;
            modelNameLabel.area = new Vector4(0, 10, 175, 18);
            modelNameLabel.textAlignment = UIHorizontalAlignment.Center;
            modelNameLabel.text = Locale.Get("VW_MODEL_NAME");
            modelNameLabel.eventClick += (x, y) => { VWVehicleList.instance.SetSorting(VWVehicleList.SortCriterion.NAME); };

            VWUtils.createUIElement(out UILabel lowWealth, titleLine.transform, "lowWealth");
            lowWealth.autoSize = false;
            lowWealth.area = new Vector4(330, 10, 40, 18);
            lowWealth.textAlignment = UIHorizontalAlignment.Center;
            lowWealth.text = "§";
            lowWealth.eventClick += (x, y) => { VWVehicleList.instance.SetSorting(VWVehicleList.SortCriterion.LOWWTH); };

            VWUtils.createUIElement(out UILabel medWealth, titleLine.transform, "medWealth");
            medWealth.autoSize = false;
            medWealth.area = new Vector4(370, 10, 40, 18);
            medWealth.textAlignment = UIHorizontalAlignment.Center;
            medWealth.text = "§§";
            medWealth.eventClick += (x, y) => { VWVehicleList.instance.SetSorting(VWVehicleList.SortCriterion.MEDWTH); };

            VWUtils.createUIElement(out UILabel highWealth, titleLine.transform, "highWealth");
            highWealth.autoSize = false;
            highWealth.area = new Vector4(410, 10, 40, 18);
            highWealth.textAlignment = UIHorizontalAlignment.Center;
            highWealth.text = "§§§";
            highWealth.eventClick += (x, y) => { VWVehicleList.instance.SetSorting(VWVehicleList.SortCriterion.HGHWTH); };
        }

        private void CreateTitleBar()
        {
            VWUtils.createUIElement(out UILabel titlebar, m_mainPanel.transform, "VWPanel", new Vector4(75, 10, m_mainPanel.width - 150, 20));
            titlebar.autoSize = false;
            titlebar.text = "Vehicle Wealthizer v" + VWSingleton.version;
            titlebar.textAlignment = UIHorizontalAlignment.Center;
            VWUtils.createDragHandle(titlebar, KlyteModsPanel.instance.mainPanel);

            VWUtils.createUIElement(out UISprite logo, m_mainPanel.transform, "VWLogo", new Vector4(22, 5f, 32, 32));
            logo.atlas = VWController.taVW;
            logo.spriteName = "VWIcon";
            VWUtils.createDragHandle(logo, KlyteModsPanel.instance.mainPanel);
        }
        #endregion

        private void Update()
        {
            RotateCamera();
        }

        private void CreateRemoveUnwantedButton()
        {
            VWUtils.createUIElement<UIButton>(out UIButton removeUndesired, m_mainPanel.transform);
            removeUndesired.relativePosition = new Vector3(470f, 65f);
            removeUndesired.textScale = 0.6f;
            removeUndesired.width = 20;
            removeUndesired.height = 20;
            removeUndesired.tooltip = Locale.Get("VW_REMOVE_UNWANTED_TOOLTIP");
            VWUtils.initButton(removeUndesired, true, "ButtonMenu");
            removeUndesired.name = "DeleteLineButton";
            removeUndesired.isVisible = true;
            removeUndesired.eventClick += (component, eventParam) =>
            {
                VWVehicleExtensionUtils.RemoveAllUnwantedVehicles();
            };

            var icon = removeUndesired.AddUIComponent<UISprite>();
            icon.relativePosition = new Vector3(2, 2);
            icon.atlas = VWController.taVW;
            icon.width = 18;
            icon.height = 18;
            icon.spriteName = "RemoveUnwantedIcon";
        }


        private AVOPreviewRenderer m_previewRenderer;
        private UITextureSprite m_preview;
        private UIPanel m_previewPanel;
        private VehicleInfo m_lastInfo;
        private UILabel m_previewTitle;

        public VehicleInfo previewInfo
        {
            get => m_lastInfo;
            set {
                m_lastInfo = value;
                m_previewTitle.text = Locale.Get("VEHICLE_TITLE", value.name);
            }
        }

        private void SetPreviewWindow()
        {
            VWUtils.createUIElement(out m_previewPanel, m_mainPanel.transform);
            m_previewPanel.backgroundSprite = "GenericPanel";
            m_previewPanel.width = m_mainPanel.width - 520f;
            m_previewPanel.height = m_previewPanel.width / 2;
            m_previewPanel.relativePosition = new Vector3(510, 80);
            VWUtils.createUIElement(out m_preview, m_previewPanel.transform);
            this.m_preview.size = m_previewPanel.size;
            this.m_preview.relativePosition = Vector3.zero;
            VWUtils.createElement(out m_previewRenderer, m_mainPanel.transform);
            this.m_previewRenderer.size = this.m_preview.size * 2f;
            this.m_preview.texture = this.m_previewRenderer.texture;
            m_previewRenderer.zoom = 3;
            m_previewRenderer.cameraRotation = 40;

            VWUtils.createUIElement(out m_previewTitle, m_mainPanel.transform, "previewTitle", new Vector4(510, 50, m_previewPanel.width, 30));
            m_previewTitle.textAlignment = UIHorizontalAlignment.Center;
        }

        public void RotateCamera()
        {
            if (m_lastInfo != default(VehicleInfo) && m_previewPanel.isVisible)
            {
                this.m_previewRenderer.cameraRotation -= 2;
                redrawModel();
            }
        }

        private void redrawModel()
        {
            if (m_lastInfo == default(VehicleInfo))
            {
                m_previewPanel.isVisible = false;
                return;
            }
            m_previewPanel.isVisible = true;
            m_previewRenderer.RenderVehicle(m_lastInfo, Color.HSVToRGB(Math.Abs(m_previewRenderer.cameraRotation) / 360f, .5f, .5f), true);
        }
    }
}
