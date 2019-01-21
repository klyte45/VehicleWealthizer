using ColossalFramework;
using ColossalFramework.UI;
using Klyte.Commons.Extensors;
using Klyte.VehicleWealthizer.Extensors;
using Klyte.VehicleWealthizer.Utils;
using System;
using System.Linq;
using UnityEngine;

namespace Klyte.VehicleWealthizer.Listing
{

    internal class VWVehicleList : UICustomControl
    {
        public static VWVehicleList instance { get; private set; }
        public static bool exists
        {
            get { return instance != null; }
        }

        public bool m_LinesUpdated = false;
        private UIScrollablePanel mainPanel;
        private static readonly string kLineTemplate = "LineTemplate";


        #region Awake
        private void Awake()
        {
            instance = this;
            mainPanel = GetComponentInChildren<UIScrollablePanel>();
            mainPanel.autoLayout = true;
            mainPanel.autoLayoutDirection = LayoutDirection.Vertical;
        }
        #endregion

        private void Update()
        {
            if (!mainPanel.isVisible)
            {
                m_LinesUpdated = false;
                return;
            }
            if (!this.m_LinesUpdated)
            {
                this.RefreshLines();
            }
        }

        private void AddToList(string infoName, ref int count)
        {
            VWBuildingInfoItem buildingInfoItem;
            if (count >= mainPanel.components.Count)
            {
                var temp = UITemplateManager.Get<PublicTransportLineInfo>(kLineTemplate).gameObject;
                GameObject.Destroy(temp.GetComponent<PublicTransportLineInfo>());
                buildingInfoItem = temp.AddComponent<VWBuildingInfoItem>();
                mainPanel.AttachUIComponent(buildingInfoItem.gameObject);
            }
            else
            {
                buildingInfoItem = mainPanel.components[count].GetComponent<VWBuildingInfoItem>();
            }
            buildingInfoItem.prefabName = infoName;
            buildingInfoItem.RefreshData();
            count++;
        }

        public void RefreshLines()
        {
            int count = 0;
            var vehicleList = VWUtils.LoadBasicAssets(CitizenWealthDefinition.LOW);

            VWUtils.doLog("{0} vehicleList = [{1}] (s={2})", GetType(), string.Join(",", vehicleList.Select(x => x.ToString()).ToArray()), vehicleList.Count);
            foreach (string prefabName in vehicleList)
            {
                AddToList(prefabName, ref count);
            }
            RemoveExtraLines(count);
            VWUtils.doLog("{0} final count = {1}", GetType(), count);
            ReSort();

            m_LinesUpdated = true;
        }

        private void RemoveExtraLines(int linesCount)
        {
            while (mainPanel.components.Count > linesCount)
            {
                UIComponent uIComponent = mainPanel.components[linesCount];
                mainPanel.RemoveUIComponent(uIComponent);
                Destroy(uIComponent.gameObject);
            }
        }

        #region Sorting

        private SortCriterion m_LastSortCriterionLines;
        private bool reverseOrder = false;

        public enum SortCriterion
        {
            DEFAULT,
            NAME,
            LOWWTH,
            MEDWTH,
            HGHWTH
        }

        private static int CompareNames(UIComponent left, UIComponent right)
        {
            VWBuildingInfoItem component = left.GetComponent<VWBuildingInfoItem>();
            VWBuildingInfoItem component2 = right.GetComponent<VWBuildingInfoItem>();
            return string.Compare(component.vehicleName, component2.vehicleName, StringComparison.InvariantCulture);
        }

        private static int CompareLow(UIComponent left, UIComponent right)
        {
            VWBuildingInfoItem component = left.GetComponent<VWBuildingInfoItem>();
            VWBuildingInfoItem component2 = right.GetComponent<VWBuildingInfoItem>();
            return ((component2.lowWealth) ? 1 : 0).CompareTo(component.lowWealth ? 1 : 0);
        }

        private static int CompareMed(UIComponent left, UIComponent right)
        {
            VWBuildingInfoItem component = left.GetComponent<VWBuildingInfoItem>();
            VWBuildingInfoItem component2 = right.GetComponent<VWBuildingInfoItem>();
            return (component2.mediumWealth ? 1 : 0).CompareTo(component.mediumWealth ? 1 : 0);
        }

        private static int CompareHgh(UIComponent left, UIComponent right)
        {
            VWBuildingInfoItem component = left.GetComponent<VWBuildingInfoItem>();
            VWBuildingInfoItem component2 = right.GetComponent<VWBuildingInfoItem>();
            return (component2.highWealth ? 1 : 0).CompareTo(component.highWealth ? 1 : 0);
        }

        private Comparison<UIComponent> getSortingMethod(SortCriterion crit)
        {
            switch (crit)
            {
                case SortCriterion.HGHWTH:
                    return new Comparison<UIComponent>(CompareHgh);
                case SortCriterion.LOWWTH:
                    return new Comparison<UIComponent>(CompareLow);
                case SortCriterion.MEDWTH:
                    return new Comparison<UIComponent>(CompareMed);
                default:
                case SortCriterion.NAME:
                    return new Comparison<UIComponent>(CompareNames);
            }
        }

        public void SetSorting(SortCriterion criteria)
        {
            reverseOrder = m_LastSortCriterionLines == criteria ? !reverseOrder : false;
            m_LastSortCriterionLines = criteria;
            ReSort();
        }

        private void ReSort()
        {
            VWUtils.Quicksort(mainPanel.components, getSortingMethod(m_LastSortCriterionLines), reverseOrder);
            mainPanel.Invalidate();
        }

        #endregion
    }

}
