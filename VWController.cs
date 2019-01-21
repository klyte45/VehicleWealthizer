using ColossalFramework;
using ColossalFramework.Globalization;
using ColossalFramework.UI;
using Klyte.Commons;
using Klyte.Commons.Extensors;
using Klyte.Commons.UI;
using Klyte.VehicleWealthizer.UI;
using Klyte.VehicleWealthizer.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
//using TLMCW = Klyte.TransportLinesManager.TLMConfigWarehouse;

namespace Klyte.VehicleWealthizer
{
    internal class VWController : Singleton<VWController>
    {

        public static UITextureAtlas taVW
        {
            get {
                if (_taVW == null)
                {
                    VWResourceLoader.Ensure();
                    _taVW = VWResourceLoader.instance.CreateTextureAtlas("UI.Images.sprites.png", "TransportLinesManagerSprites", GameObject.FindObjectOfType<UIPanel>().atlas.material, 64, 64, new string[] {
                    "VWIcon","AutoNameIcon","AutoColorIcon","RemoveUnwantedIcon","ConfigIcon","24hLineIcon", "PerHourIcon","AbsoluteMode","RelativeMode"
                });
                }
                return _taVW;
            }
        }

        private static UITextureAtlas _taVW = null;


        public UIView uiView;
        private UIComponent mainRef;
        public bool initialized = false;
        public bool initializedWIP = false;
        private int lastLineCount = 0;

        public Transform TargetTransform => mainRef?.transform;
        public Transform TransformLinearMap => uiView?.transform;

        private ushort m_currentSelectedId;

        public ushort CurrentSelectedId => m_currentSelectedId;
        public void setCurrentSelectedId(ushort line) => m_currentSelectedId = line;

        public bool CanSwitchView => false;

        public bool ForceShowStopsDistances
        {
            get {
                return true;
            }
        }

        public TransportInfo CurrentTransportInfo
        {
            get {
                return Singleton<TransportTool>.instance.m_prefab;
            }
        }
        public void Update()
        {
            if (!GameObject.FindGameObjectWithTag("GameController") || ((GameObject.FindGameObjectWithTag("GameController")?.GetComponent<ToolController>())?.m_mode & ItemClass.Availability.Game) == ItemClass.Availability.None)
            {
                VWUtils.doErrorLog("GameController NOT FOUND!");
                return;
            }
            if (!initialized)
            {
                Awake();
            }

            lastLineCount = TransportManager.instance.m_lineCount;
        }

        public void Awake()
        {
            if (!initialized && gameObject != null)
            {
                VWSingleton.instance.loadVWLocale(false);

                uiView = GameObject.FindObjectOfType<UIView>();
                if (!uiView)
                    return;
                mainRef = uiView.FindUIComponent<UIPanel>("InfoPanel").Find<UITabContainer>("InfoViewsContainer").Find<UIPanel>("InfoViewsPanel");
                if (!mainRef)
                    return;
                mainRef.clipChildren = false;

                var typeTarg = typeof(Redirector<>);
                var instances = from t in Assembly.GetAssembly(typeof(VWController)).GetTypes()
                                let y = t.BaseType
                                where t.IsClass && !t.IsAbstract && y != null && y.IsGenericType && y.GetGenericTypeDefinition() == typeTarg
                                select t;

                foreach (Type t in instances)
                {
                    VWUtils.doLog($"Adding hooks: {t}");
                    gameObject.AddComponent(t);
                }

                initialized = true;
            }
        }




        public void Start()
        {
            KlyteModsPanel.instance.AddTab(ModTab.VehicleWealthier, typeof(VWPanel), taVW, "VWIcon", "Vehicle Wealthizer (v" + VWSingleton.version + ")").width = 500;
        }

        public void OpenVWPanel()
        {
            KlyteModsPanel.instance.OpenAt(ModTab.VehicleWealthier);
        }

    }


}