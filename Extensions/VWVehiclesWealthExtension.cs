using ColossalFramework;
using ColossalFramework.Globalization;
using ColossalFramework.Threading;
using Klyte.Commons.Interfaces;
using Klyte.VehicleWealthizer.Utils;
using Klyte.VehicleWealthizer;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static Citizen;
using System.Reflection;
using System.Text;

namespace Klyte.VehicleWealthizer.Extensors
{
    internal interface IVWVehiclesWealthExtension : IVWAssetSelectorExtension
    {
        string[] Serialize();
        void Deserialize(string[] lines);
    }

    internal abstract class VWVehiclesWealthExtension<W, SG> : ExtensionInterfaceSingleImpl<VWConfigWarehouse, VWConfigWarehouse.ConfigIndex, ExtConfigs, SG>, IVWVehiclesWealthExtension where W : VWWthDef<W>, new() where SG : VWVehiclesWealthExtension<W, SG>
    {
        private const uint DISTRICT_FLAG = 0x100000;
        private const uint BUILDING_FLAG = 0x200000;
        private const uint ID_PART = 0x0FFFFF;
        private const uint TYPE_PART = 0xF00000;

        public override VWConfigWarehouse.ConfigIndex ConfigIndexKey
        {
            get {
                if (transform.parent == null && VWController.instance != null)
                {
                    transform.SetParent(VWController.instance.transform);
                }
                var def = definition;
                return VWConfigWarehouse.getConfigAssetsForDef(ref def);
            }
        }
        protected override bool AllowGlobal { get { return false; } }

        private List<string> basicAssetsList;

        private CitizenWealthDefinition definition => Singleton<W>.instance.GetCWD();

        public void Awake()
        {
            this.transform.SetParent(VWController.instance.transform);
        }

        #region Asset List
        public List<string> GetAssetList()
        {
            string value = SafeGet(ExtConfigs.MODELS);
            if (string.IsNullOrEmpty(value))
            {
                return new List<string>();
            }
            else
            {
                return value.Split(ItSepLvl3.ToCharArray()).ToList();
            }
        }
        public void AddAsset(string assetId)
        {
            var temp = GetAssetList();
            if (temp.Contains(assetId)) return;
            if (basicAssetsList == null) LoadBasicAssets();
            temp.Add(assetId);
            SafeSet(ExtConfigs.MODELS, string.Join(ItSepLvl3, temp.Intersect(basicAssetsList).ToArray()));
        }
        public void RemoveAsset(string assetId)
        {
            var temp = GetAssetList();
            if (!temp.Contains(assetId)) return;
            if (basicAssetsList == null) LoadBasicAssets();
            temp.RemoveAll(x => x == assetId);
            SafeSet(ExtConfigs.MODELS, string.Join(ItSepLvl3, temp.Intersect(basicAssetsList).ToArray()));
        }
        public void UseDefaultAssets()
        {
            SafeCleanProperty(ExtConfigs.MODELS);
        }

        public VehicleInfo GetAModel()
        {
            VWUtils.doLog("[{0}] GetAModel", typeof(W).Name);
            List<string> assetList = GetEffectiveAssetList();
            VehicleInfo info = null;
            while (info == null && assetList.Count > 0)
            {
                info = VWUtils.GetRandomModel(assetList, out string modelName);
                if (info == null)
                {
                    RemoveAsset(modelName);
                    assetList = GetEffectiveAssetList();
                }
            }
            return info;
        }

        private List<string> GetEffectiveAssetList()
        {
            List<string> assetList = GetAssetList();

            if ((assetList?.Count ?? 0) == 0)
            {
                if (basicAssetsList == null) LoadBasicAssets();
                assetList = basicAssetsList;
            }
            return assetList;
        }

        public bool IsModelCompatible(VehicleInfo vehicleInfo)
        {
            return IsModelCompatible(vehicleInfo.name);
        }

        public bool IsModelCompatible(string prefabName)
        {
            return GetEffectiveAssetList().Contains(prefabName);
        }

        public bool IsModelSelected(string prefabName)
        {
            return GetAssetList().Contains(prefabName);
        }
        public Dictionary<string, string> GetAllBasicAssets()
        {
            if (basicAssetsList == null) LoadBasicAssets();
            return basicAssetsList.ToDictionary(x => x, x => Locale.Get("VEHICLE_TITLE", x));
        }

        private void LoadBasicAssets()
        {
            basicAssetsList = VWUtils.LoadBasicAssets(definition);
        }

        #endregion


        #region Serialization
        public abstract string serializePrefix { get; }

        public string[] Serialize()
        {
            return GetAssetList().Select(x => $"{serializePrefix} {x}").ToArray();
        }

        public void Deserialize(string[] lines)
        {
            List<string> selectedModels = new List<string>();
            foreach (string line in lines)
            {
                if (line.StartsWith($"{serializePrefix} "))
                {
                    selectedModels.Add(line.Replace($"{serializePrefix} ", ""));
                }
            }
            if (basicAssetsList == null) LoadBasicAssets();
            var resultList = basicAssetsList.Where(x => selectedModels.Contains(x) || selectedModels.Contains(x.Split(".".ToCharArray())[0])).ToArray();
            VWUtils.doLog($"IMPORTED: {resultList.Length} assets for {typeof(W)} => {resultList}");
            if (resultList.Length == 0)
            {
                SafeCleanProperty(ExtConfigs.MODELS);
            }
            else
            {
                SafeSet(ExtConfigs.MODELS, string.Join(ItSepLvl3, resultList));
            }

        }
        #endregion



    }

    internal interface IVWConfigIndexKeyContainer
    {
        VWConfigWarehouse.ConfigIndex ConfigIndexKey { get; }
    }

    internal interface IVWAssetSelectorExtension : IVWConfigIndexKeyContainer
    {
        Dictionary<string, string> GetAllBasicAssets();
        VehicleInfo GetAModel();
        bool IsModelCompatible(VehicleInfo vehicleInfo);


        List<string> GetAssetList();
        void AddAsset(string assetId);
        void RemoveAsset(string assetId);
        void UseDefaultAssets();
    }


    internal sealed class VWVehiclesWealthExtensionLow : VWVehiclesWealthExtension<VWWthDefLow, VWVehiclesWealthExtensionLow> { public override string serializePrefix => "§"; }
    internal sealed class VWVehiclesWealthExtensionMed : VWVehiclesWealthExtension<VWWthDefMed, VWVehiclesWealthExtensionMed> { public override string serializePrefix => "§§"; }
    internal sealed class VWVehiclesWealthExtensionHgh : VWVehiclesWealthExtension<VWWthDefHgh, VWVehiclesWealthExtensionHgh> { public override string serializePrefix => "§§§"; }

    public sealed class VWVehicleExtensionUtils
    {

        public static string GenerateSerializedFile()
        {
            StringBuilder result = new StringBuilder();
            var typeTarg = typeof(VWVehiclesWealthExtension<,>);
            var instances = from t in Assembly.GetAssembly(typeof(VWVehicleExtensionUtils)).GetTypes()
                            let y = t.BaseType
                            where t.IsClass && !t.IsAbstract && y != null && y.IsGenericType && y.GetGenericTypeDefinition() == typeTarg
                            select t;

            foreach (Type t in instances)
            {
                IVWVehiclesWealthExtension ext = (IVWVehiclesWealthExtension)GameObject.FindObjectOfType(t);
                result.AppendLine(string.Join("\r\n", ext.Serialize()));
            }

            return result.ToString();
        }

        public static void DeserializeGeneratedFile(string fileContent)
        {
            string[] lines = fileContent.Split("\r\n".ToCharArray());

            StringBuilder result = new StringBuilder();
            var typeTarg = typeof(VWVehiclesWealthExtension<,>);
            var instances = from t in Assembly.GetAssembly(typeof(VWVehicleExtensionUtils)).GetTypes()
                            let y = t.BaseType
                            where t.IsClass && !t.IsAbstract && y != null && y.IsGenericType && y.GetGenericTypeDefinition() == typeTarg
                            select t;

            foreach (Type t in instances)
            {
                IVWVehiclesWealthExtension ext = (IVWVehiclesWealthExtension)GameObject.FindObjectOfType(t);
                ext.Deserialize(lines);
            }
        }

        public static void RemoveAllUnwantedVehicles()
        {
            new EnumerableActionThread(new Func<ThreadBase, IEnumerator>(VWVehicleExtensionUtils.RemoveAllUnwantedVehicles));
        }
        public static IEnumerator RemoveAllUnwantedVehicles(ThreadBase t)
        {
            ushort vehId = 1;
            while ((uint)vehId < Singleton<VehicleManager>.instance.m_vehicles.m_size)
            {
                var vehicle = Singleton<VehicleManager>.instance.m_vehicles.m_buffer[(int)vehId];
                var vehicleInfo = vehicle.Info;
                if (vehicleInfo != null && !VWUtils.IsTrailer(vehicleInfo) && vehicle.m_transportLine == 0)
                {
                    var citizenOwner = vehicle.Info.m_vehicleAI.GetOwnerID(vehId, ref vehicle).Citizen;
                    if (citizenOwner > 0)
                    {
                        var ownerWealth = CitizenWealthDefinition.from(CitizenManager.instance.m_citizens.m_buffer[citizenOwner].WealthLevel);
                        if (ownerWealth != null)
                        {
                            if (!ownerWealth.GetVehicleExtension().IsModelCompatible(vehicleInfo))
                            {
                                Singleton<VehicleManager>.instance.ReleaseVehicle(vehId);
                            }
                        }

                    }
                }
                if (vehId % 256 == 255)
                {
                    yield return vehId;
                }
                vehId++;
            }

            vehId = 1;
            while ((uint)vehId < Singleton<VehicleManager>.instance.m_parkedVehicles.m_size)
            {
                var vehicle = Singleton<VehicleManager>.instance.m_parkedVehicles.m_buffer[(int)vehId];
                var vehicleInfo = vehicle.Info;
                if (vehicleInfo != null && !VWUtils.IsTrailer(vehicleInfo))
                {
                    var citizenOwner = vehicle.m_ownerCitizen;
                    if (citizenOwner > 0)
                    {
                        var ownerWealth = CitizenWealthDefinition.from(CitizenManager.instance.m_citizens.m_buffer[citizenOwner].WealthLevel);
                        if (ownerWealth != null)
                        {
                            if (!ownerWealth.GetVehicleExtension().IsModelCompatible(vehicleInfo))
                            {
                                Singleton<VehicleManager>.instance.m_parkedVehicles.m_buffer[(int)vehId].Info = ownerWealth.GetVehicleExtension().GetAModel();
                            }
                        }

                    }
                }
                if (vehId % 256 == 255)
                {
                    yield return vehId;
                }
                vehId++;
            }
            yield break;
        }

    }

    internal enum ExtConfigs
    {
        MODELS
    }
}
