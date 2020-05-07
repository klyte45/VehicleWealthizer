namespace Klyte.VehicleWealthizer.Listing
{
    using ColossalFramework.Globalization;
    using ColossalFramework.UI;
    using Klyte.Commons.Utils;
    using Klyte.VehicleWealthizer.Extensors;
    using Klyte.VehicleWealthizer.UI;
    using UnityEngine;
    internal class VWVehicleInfoItem : ToolsModifierControl
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
        private bool m_isUpdated = false;

        public string prefabName
        {
            get => m_prefabName;
            set => SetPrefab(value);
        }

        public string vehicleName => m_vehicleModelName.text;

        public bool lowWealth => m_lowWealth.isChecked;

        public bool mediumWealth => m_mediumWealth.isChecked;

        public bool highWealth => m_highWealth.isChecked;

        public VehicleInfo info => m_prefabName != null ? PrefabCollection<VehicleInfo>.FindLoaded(m_prefabName) : null;

        private void SetPrefab(string id)
        {
            m_prefabName = id;

            m_vehicleModelName.text = Locale.Get("VEHICLE_TITLE", id);
            VehicleInfo vInfo = info;
            m_vehicleImage.atlas = vInfo.m_Atlas;
            m_vehicleImage.spriteName = vInfo.m_Thumbnail;
        }



        public void RefreshData(bool force = false)
        {
            if (force || (transform.parent.gameObject.GetComponent<UIComponent>().isVisible && !m_isUpdated))
            {
                GetComponent<UIComponent>().isVisible = true;

                m_lowWealth.isChecked = VWVehiclesWealthExtensionLow.Instance.IsModelSelected(m_prefabName);
                m_mediumWealth.isChecked = VWVehiclesWealthExtensionMed.Instance.IsModelSelected(m_prefabName);
                m_highWealth.isChecked = VWVehiclesWealthExtensionHgh.Instance.IsModelSelected(m_prefabName);

                m_isUpdated = transform.parent.gameObject.GetComponent<UIComponent>().isVisible;
            }
        }

        public void SetBackgroundColor()
        {
            Color32 backgroundColor = m_BackgroundColor;
            backgroundColor.a = (byte) ((base.component.zOrder % 2 != 0) ? 127 : 255);
            if (m_mouseIsOver)
            {
                backgroundColor.r = (byte) Mathf.Min(255, (backgroundColor.r * 3) >> 1);
                backgroundColor.g = (byte) Mathf.Min(255, (backgroundColor.g * 3) >> 1);
                backgroundColor.b = (byte) Mathf.Min(255, (backgroundColor.b * 3) >> 1);
            }
            m_Background.color = backgroundColor;
        }

        private void LateUpdate()
        {
            if (base.component.parent.isVisible)
            {
                RefreshData();
            }
        }

        private void Awake()
        {
            KlyteMonoUtils.ClearAllVisibilityEvents(GetComponent<UIPanel>());
            UIPanel panel = GetComponent<UIPanel>();
            panel.width = 450;
            panel.eventClick += (x, y) =>
            {
                VWPanel.Instance.previewInfo = info;
            };

            base.component.eventZOrderChanged += delegate (UIComponent c, int r)
            {
                SetBackgroundColor();
            };
            GameObject.Destroy(base.Find<UICheckBox>("LineVisible").gameObject);
            GameObject.Destroy(base.Find<UIColorField>("LineColor").gameObject);
            GameObject.Destroy(base.Find<UIPanel>("WarningIncomplete"));
            GameObject.Destroy(base.Find<UILabel>("LineName"));
            GameObject.Destroy(base.Find<UILabel>("LinePassengers"));
            GameObject.Destroy(base.Find<UILabel>("LineVehicles"));
            GameObject.Destroy(base.Find<UIButton>("ViewLine"));
            GameObject.Destroy(base.Find<UIButton>("DeleteLine"));
            GameObject.Destroy(base.Find<UIPanel>("LineModelSelectorContainer"));

            m_lowWealth = base.Find<UICheckBox>("DayLine");
            m_mediumWealth = base.Find<UICheckBox>("NightLine");
            m_highWealth = base.Find<UICheckBox>("DayNightLine");

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
            KlyteMonoUtils.CreateUIElement(out m_vehicleImage, transform, "VehicleImage", new Vector4(250, 0, 40, 40));

            m_Background = base.Find("Background");
            m_BackgroundColor = m_Background.color;
            m_mouseIsOver = false;
            base.component.eventMouseEnter += new MouseEventHandler(OnMouseEnter);
            base.component.eventMouseLeave += new MouseEventHandler(OnMouseLeave);
            base.component.eventVisibilityChanged += delegate (UIComponent c, bool v)
            {
                if (v)
                {
                    RefreshData();
                }
            };


        }

        private void OnMouseEnter(UIComponent comp, UIMouseEventParameter param)
        {
            if (!m_mouseIsOver)
            {
                m_mouseIsOver = true;
                SetBackgroundColor();
            }
        }

        private void OnMouseLeave(UIComponent comp, UIMouseEventParameter param)
        {
            if (m_mouseIsOver)
            {
                m_mouseIsOver = false;
                SetBackgroundColor();
            }
        }

        private void OnEnable()
        {
        }

        private void OnDisable()
        {
        }

        private PropertyChangedEventHandler<bool> assetChange<W, SG>() where W : VWWthDef<W>, new() where SG : VWVehiclesWealthExtension<W, SG>, new()
        {
            return (x, val) =>
            {
                if (val)
                {
                    VWVehiclesWealthExtension<W, SG>.Instance.AddAsset(m_prefabName);
                }
                else
                {
                    VWVehiclesWealthExtension<W, SG>.Instance.RemoveAsset(m_prefabName);
                }
            };
        }

    }

}
