using ColossalFramework.Globalization;
using ColossalFramework.UI;
using Klyte.Commons.Interfaces;
using Klyte.Commons.UI;
using Klyte.Commons.Utils;
using Klyte.VehicleWealthizer.Extensors;
using Klyte.VehicleWealthizer.Listing;
using System;
using UnityEngine;

namespace Klyte.VehicleWealthizer.UI
{

    public class VWPanel : BasicKPanel<VehicleWealthizerMod, VWController, VWPanel>
    {
        public ExtensorContainer extensorContainer => ExtensorContainer.instance;



        #region Awake
        protected override void AwakeActions()
        {
            KlyteMonoUtils.CreateScrollPanel(MainPanel, out _, out _, 450, base.MainPanel.height - 120, new Vector3(10, 110));

            MainPanel.gameObject.AddComponent<VWVehicleList>();
            CreateTitleRow(MainPanel);

            SetPreviewWindow();
            CreateRemoveUnwantedButton();

            KlyteMonoUtils.CreateUIElement(out UIPanel exportPanel, MainPanel.transform, "ImportExportPanel", new Vector4(480, 275, 380, 275));
            exportPanel.gameObject.AddComponent<VWConfigFilesPanel>();

        }

        private void CreateTitleRow(UIComponent parent)
        {
            KlyteMonoUtils.CreateUIElement(out UIPanel titleLine, parent.transform, "VWtitleline", new Vector4(5, 60, 500, 40));

            KlyteMonoUtils.CreateUIElement(out UILabel modelNameLabel, titleLine.transform, "districtNameLabel");
            modelNameLabel.autoSize = false;
            modelNameLabel.area = new Vector4(0, 10, 175, 18);
            modelNameLabel.textAlignment = UIHorizontalAlignment.Center;
            modelNameLabel.text = Locale.Get("K45_VW_MODEL_NAME");
            modelNameLabel.eventClick += (x, y) => { VWVehicleList.instance.SetSorting(VWVehicleList.SortCriterion.NAME); };

            KlyteMonoUtils.CreateUIElement(out UILabel lowWealth, titleLine.transform, "lowWealth");
            lowWealth.autoSize = false;
            lowWealth.area = new Vector4(330, 10, 40, 18);
            lowWealth.textAlignment = UIHorizontalAlignment.Center;
            lowWealth.text = "§";
            lowWealth.eventClick += (x, y) => { VWVehicleList.instance.SetSorting(VWVehicleList.SortCriterion.LOWWTH); };

            KlyteMonoUtils.CreateUIElement(out UILabel medWealth, titleLine.transform, "medWealth");
            medWealth.autoSize = false;
            medWealth.area = new Vector4(370, 10, 40, 18);
            medWealth.textAlignment = UIHorizontalAlignment.Center;
            medWealth.text = "§§";
            medWealth.eventClick += (x, y) => { VWVehicleList.instance.SetSorting(VWVehicleList.SortCriterion.MEDWTH); };

            KlyteMonoUtils.CreateUIElement(out UILabel highWealth, titleLine.transform, "highWealth");
            highWealth.autoSize = false;
            highWealth.area = new Vector4(410, 10, 40, 18);
            highWealth.textAlignment = UIHorizontalAlignment.Center;
            highWealth.text = "§§§";
            highWealth.eventClick += (x, y) => { VWVehicleList.instance.SetSorting(VWVehicleList.SortCriterion.HGHWTH); };
        }

        #endregion

        public void Update() => RotateCamera();

        private void CreateRemoveUnwantedButton()
        {
            KlyteMonoUtils.CreateUIElement<UIButton>(out UIButton removeUndesired, MainPanel.transform);
            removeUndesired.relativePosition = new Vector3(470f, 65f);
            removeUndesired.textScale = 0.6f;
            removeUndesired.width = 20;
            removeUndesired.height = 20;
            removeUndesired.tooltip = Locale.Get("K45_VW_REMOVE_UNWANTED_TOOLTIP");
            KlyteMonoUtils.InitButton(removeUndesired, true, "ButtonMenu");
            removeUndesired.name = "DeleteLineButton";
            removeUndesired.isVisible = true;
            removeUndesired.eventClick += (component, eventParam) =>
            {
                VWVehicleExtensionUtils.RemoveAllUnwantedVehicles();
            };

            UISprite icon = removeUndesired.AddUIComponent<UISprite>();
            icon.relativePosition = new Vector3(2, 2);
            icon.width = 18;
            icon.height = 18;
            icon.spriteName = "K45_VW_RemoveUnwantedIcon";
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

        public override float PanelWidth => 885;

        public override float PanelHeight => GetComponentInParent<UIComponent>().height;

        private void SetPreviewWindow()
        {
            KlyteMonoUtils.CreateUIElement(out m_previewPanel, MainPanel.transform);
            m_previewPanel.backgroundSprite = "GenericPanel";
            m_previewPanel.width = MainPanel.width - 520f;
            m_previewPanel.height = m_previewPanel.width / 2;
            m_previewPanel.relativePosition = new Vector3(510, 80);
            KlyteMonoUtils.CreateUIElement(out m_preview, m_previewPanel.transform);
            m_preview.size = m_previewPanel.size;
            m_preview.relativePosition = Vector3.zero;
            KlyteMonoUtils.CreateElement(out m_previewRenderer, MainPanel.transform);
            m_previewRenderer.Size = m_preview.size * 2f;
            m_preview.texture = m_previewRenderer.Texture;
            m_previewRenderer.Zoom = 3;
            m_previewRenderer.CameraRotation = 40;

            KlyteMonoUtils.CreateUIElement(out m_previewTitle, MainPanel.transform, "previewTitle", new Vector4(510, 50, m_previewPanel.width, 30));
            m_previewTitle.textAlignment = UIHorizontalAlignment.Center;
        }

        public void RotateCamera()
        {
            if (m_lastInfo != default(VehicleInfo) && m_previewPanel.isVisible)
            {
                m_previewRenderer.CameraRotation -= 2;
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
            m_previewRenderer.RenderVehicle(m_lastInfo, Color.HSVToRGB(Math.Abs(m_previewRenderer.CameraRotation) / 360f, .5f, .5f), true);
        }

    }
}
