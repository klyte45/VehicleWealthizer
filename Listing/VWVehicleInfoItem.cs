namespace Klyte.VehicleWealthizer.Listing
{
    using ColossalFramework;
    using ColossalFramework.Globalization;
    using ColossalFramework.Math;
    using ColossalFramework.UI;
    using Klyte.Commons.Utils;
    using Klyte.VehicleWealthizer.Extensors;
    using Klyte.VehicleWealthizer.Overrides;
    using Klyte.VehicleWealthizer.UI;
    using System;
    using UnityEngine;
    using Utils;
    internal class VWBuildingInfoItem : ToolsModifierControl
    {
        private string m_prefabName;

        private UILabel m_vehicleModelName;

        private UICheckBox m_lowWealth;

        private UICheckBox m_mediumWealth;

        private UICheckBox m_highWealth;

        private UISprite m_vehicleImage;

        private UIComponent m_Background;

        private Color32 m_BackgroundColor;

        private bool m_mouseIsOver;

        public string prefabName
        {
            get {
                return this.m_prefabName;
            }
            set {
                this.SetPrefab(value);
            }
        }

        public string vehicleName => this.m_vehicleModelName.text;

        public bool lowWealth => this.m_lowWealth.isChecked;

        public bool mediumWealth => this.m_mediumWealth.isChecked;

        public bool highWealth => this.m_highWealth.isChecked;

        public VehicleInfo info => m_prefabName != null ? PrefabCollection<VehicleInfo>.FindLoaded(m_prefabName) : null;

        private void SetPrefab(string id)
        {
            this.m_prefabName = id;

            m_vehicleModelName.text = Locale.Get("VEHICLE_TITLE", id);
            var vInfo = info;
            m_vehicleImage.atlas = vInfo.m_Atlas;
            m_vehicleImage.spriteName = vInfo.m_Thumbnail;
        }



        public void RefreshData()
        {
            if (transform.parent.gameObject.GetComponent<UIComponent>().isVisible)
            {
                GetComponent<UIComponent>().isVisible = true;

                var extL = VWVehiclesWealthExtensionLow.instance;
                var extM = VWVehiclesWealthExtensionMed.instance;
                var extH = VWVehiclesWealthExtensionHgh.instance;

                m_lowWealth.isChecked = extL.IsModelSelected(m_prefabName);
                m_mediumWealth.isChecked = extM.IsModelSelected(m_prefabName);
                m_highWealth.isChecked = extH.IsModelSelected(m_prefabName);

            }
        }

        public void SetBackgroundColor()
        {
            Color32 backgroundColor = this.m_BackgroundColor;
            backgroundColor.a = (byte)((base.component.zOrder % 2 != 0) ? 127 : 255);
            if (this.m_mouseIsOver)
            {
                backgroundColor.r = (byte)Mathf.Min(255, backgroundColor.r * 3 >> 1);
                backgroundColor.g = (byte)Mathf.Min(255, backgroundColor.g * 3 >> 1);
                backgroundColor.b = (byte)Mathf.Min(255, backgroundColor.b * 3 >> 1);
            }
            this.m_Background.color = backgroundColor;
        }

        private void LateUpdate()
        {
            if (base.component.parent.isVisible)
            {
                this.RefreshData();
            }
        }

        private void Awake()
        {
            VWUtils.clearAllVisibilityEvents(this.GetComponent<UIPanel>());
            UIPanel panel = GetComponent<UIPanel>();
            panel.width = 450;
            panel.eventClick += (x, y) =>
            {
                VWPanel.instance.previewInfo = info;
            };

            base.component.eventZOrderChanged += delegate (UIComponent c, int r)
            {
                this.SetBackgroundColor();
            };
            GameObject.Destroy(base.Find<UICheckBox>("LineVisible").gameObject);
            GameObject.Destroy(base.Find<UIColorField>("LineColor").gameObject);
            GameObject.Destroy(base.Find<UIPanel>("WarningIncomplete"));
            GameObject.Destroy(base.Find<UILabel>("LineName"));
            GameObject.Destroy(base.Find<UILabel>("LinePassengers"));
            GameObject.Destroy(base.Find<UILabel>("LineVehicles"));
            GameObject.Destroy(base.Find<UIButton>("ViewLine"));
            GameObject.Destroy(base.Find<UIButton>("DeleteLine"));

            this.m_lowWealth = base.Find<UICheckBox>("DayLine");
            this.m_mediumWealth = base.Find<UICheckBox>("NightLine");
            this.m_highWealth = base.Find<UICheckBox>("DayNightLine");

            m_lowWealth.relativePosition = new Vector3(330, 8);
            m_mediumWealth.relativePosition = new Vector3(370, 8);
            m_highWealth.relativePosition = new Vector3(410, 8);

            m_lowWealth.eventCheckChanged += assetChange<VWWthDefLow, VWVehiclesWealthExtensionLow>();
            m_mediumWealth.eventCheckChanged += assetChange<VWWthDefMed, VWVehiclesWealthExtensionMed>();
            m_highWealth.eventCheckChanged += assetChange<VWWthDefHgh, VWVehiclesWealthExtensionHgh>();

            m_vehicleModelName = base.Find<UILabel>("LineStops");
            m_vehicleModelName.size = new Vector2(175, 18);
            m_vehicleModelName.relativePosition = new Vector3(0, 10);
            m_vehicleModelName.pivot = UIPivotPoint.MiddleCenter;
            m_vehicleModelName.wordWrap = true;
            m_vehicleModelName.autoHeight = true;
            VWUtils.createUIElement(out m_vehicleImage, transform, "VehicleImage", new Vector4(250, 0, 40, 40));

            this.m_Background = base.Find("Background");
            this.m_BackgroundColor = this.m_Background.color;
            this.m_mouseIsOver = false;
            base.component.eventMouseEnter += new MouseEventHandler(this.OnMouseEnter);
            base.component.eventMouseLeave += new MouseEventHandler(this.OnMouseLeave);
            base.component.eventVisibilityChanged += delegate (UIComponent c, bool v)
            {
                if (v)
                {
                    this.RefreshData();
                }
            };


        }

        private void OnMouseEnter(UIComponent comp, UIMouseEventParameter param)
        {
            if (!this.m_mouseIsOver)
            {
                this.m_mouseIsOver = true;
                this.SetBackgroundColor();
            }
        }

        private void OnMouseLeave(UIComponent comp, UIMouseEventParameter param)
        {
            if (this.m_mouseIsOver)
            {
                this.m_mouseIsOver = false;
                this.SetBackgroundColor();
            }
        }

        private void OnEnable()
        {
        }

        private void OnDisable()
        {
        }


        private void OnLineChanged(string id)
        {
            if (id == this.m_prefabName)
            {
                this.RefreshData();
            }
        }

        private PropertyChangedEventHandler<bool> assetChange<W, SG>() where W : VWWthDef<W>, new() where SG : VWVehiclesWealthExtension<W, SG>
        {
            return (x, val) =>
            {
                if (val)
                {
                    Singleton<SG>.instance.AddAsset(m_prefabName);
                }
                else
                {
                    Singleton<SG>.instance.RemoveAsset(m_prefabName);
                }
            };
        }

    }

}
