using ColossalFramework;
using ColossalFramework.Globalization;
using ColossalFramework.Threading;
using Klyte.Commons.Interfaces;
using Klyte.Commons.Utils;
using Klyte.VehicleWealthizer.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml.Serialization;

namespace Klyte.VehicleWealthizer.Extensors
{
    public interface IVWVehiclesWealthExtension : IVWAssetSelectorExtension
    {
        string[] SerializeToString();
        void DeserializeFromString(string[] lines);
    }

    public abstract class VWVehiclesWealthExtension<W, SG> : DataExtensorBase<SG>, IVWVehiclesWealthExtension where W : VWWthDef<W>, new() where SG : VWVehiclesWealthExtension<W, SG>, new()
    {
        private const uint DISTRICT_FLAG = 0x100000;
        private const uint BUILDING_FLAG = 0x200000;
        private const uint ID_PART = 0x0FFFFF;
        private const uint TYPE_PART = 0xF00000;


        private List<string> m_basicAssetsList;
        [XmlElement("AssetList")]
        public SimpleXmlHashSet<string> AssetList { get; set; } = new SimpleXmlHashSet<string>();

        private CitizenWealthDefinition Definition { get; } = Singleton<W>.instance.GetCWD();

        #region Asset List
        public IEnumerable<string> GetAssetList() => AssetList;

        public void AddAsset(string assetId)
        {
            if (m_basicAssetsList == null)
            {
                LoadBasicAssets();
            }

            AssetList.Add(assetId);
        }


        public void RemoveAsset(string assetId)
        {
            if (m_basicAssetsList == null)
            {
                LoadBasicAssets();
            }

            AssetList.RemoveWhere(x => x == assetId);
        }
        public void UseDefaultAssets() => AssetList.Clear();

        public VehicleInfo GetAModel()
        {
            LogUtils.DoLog("[{0}] GetAModel", typeof(W).Name);
            IEnumerable<string> assetList = GetEffectiveAssetList();
            VehicleInfo info = null;
            while (info == null && assetList.Count() > 0)
            {
                info = VehicleUtils.GetRandomModel(assetList, out string modelName);
                if (info == null)
                {
                    RemoveAsset(modelName);
                    assetList = GetEffectiveAssetList();
                }
            }
            return info;
        }

        private IEnumerable<string> GetEffectiveAssetList()
        {

            if ((AssetList?.Count ?? 0) == 0)
            {
                if (m_basicAssetsList == null)
                {
                    LoadBasicAssets();
                }

                return m_basicAssetsList;
            }
            return AssetList;
        }

        public bool IsModelCompatible(VehicleInfo vehicleInfo) => IsModelCompatible(vehicleInfo.name);

        public bool IsModelCompatible(string prefabName) => GetEffectiveAssetList().Contains(prefabName);

        public bool IsModelSelected(string prefabName) => GetAssetList().Contains(prefabName);
        public Dictionary<string, string> GetAllBasicAssets()
        {
            if (m_basicAssetsList == null)
            {
                LoadBasicAssets();
            }

            return m_basicAssetsList.ToDictionary(x => x, x => Locale.Get("VEHICLE_TITLE", x));
        }

        private void LoadBasicAssets()
        {
            m_basicAssetsList = VWUtils.LoadBasicAssets(Definition);
            AssetList.IntersectWith(m_basicAssetsList);
        }

        #endregion


        #region Serialization
        public abstract string SerializePrefix { get; }

        public string[] SerializeToString() => GetAssetList().Select(x => $"{SerializePrefix} {x}").ToArray();

        public void DeserializeFromString(string[] lines)
        {
            var selectedModels = new List<string>();
            foreach (string line in lines)
            {
                if (line.StartsWith($"{SerializePrefix} "))
                {
                    selectedModels.Add(line.Replace($"{SerializePrefix} ", ""));
                }
            }
            if (m_basicAssetsList == null)
            {
                LoadBasicAssets();
            }

            var resultList = m_basicAssetsList.Where(x => selectedModels.Contains(x) || selectedModels.Contains(x.Split(".".ToCharArray())[0])).ToList();
            LogUtils.DoLog($"IMPORTED: {resultList.Count} assets for {typeof(W)} => {resultList}");
            if (resultList.Count == 0)
            {
                AssetList.Clear();
            }
            else
            {
                AssetList = new SimpleXmlHashSet<string>(resultList);
            }

        }
        #endregion
        public override string SaveId => $"K45_VW_{SerializePrefix}";


    }

    public interface IVWAssetSelectorExtension
    {
        Dictionary<string, string> GetAllBasicAssets();
        VehicleInfo GetAModel();
        bool IsModelCompatible(VehicleInfo vehicleInfo);


        IEnumerable<string> GetAssetList();
        void AddAsset(string assetId);
        void RemoveAsset(string assetId);
        void UseDefaultAssets();
    }


    public sealed class VWVehiclesWealthExtensionLow : VWVehiclesWealthExtension<VWWthDefLow, VWVehiclesWealthExtensionLow> { public override string SerializePrefix => "§"; }
    public sealed class VWVehiclesWealthExtensionMed : VWVehiclesWealthExtension<VWWthDefMed, VWVehiclesWealthExtensionMed> { public override string SerializePrefix => "§§"; }
    public sealed class VWVehiclesWealthExtensionHgh : VWVehiclesWealthExtension<VWWthDefHgh, VWVehiclesWealthExtensionHgh> { public override string SerializePrefix => "§§§"; }

    public sealed class VWVehicleExtensionUtils
    {

        public static string GenerateSerializedFile()
        {
            var result = new StringBuilder();
            Type typeTarg = typeof(VWVehiclesWealthExtension<,>);
            IEnumerable<Type> instances = from t in Assembly.GetAssembly(typeof(VWVehicleExtensionUtils)).GetTypes()
                                          let y = t.BaseType
                                          where t.IsClass && !t.IsAbstract && y != null && y.IsGenericType && y.GetGenericTypeDefinition() == typeTarg
                                          select t;

            foreach (Type t in instances)
            {
                var ext = (IVWVehiclesWealthExtension) ExtensorContainer.instance.Instances[t];
                result.AppendLine(string.Join("\r\n", ext.SerializeToString()));
            }

            return result.ToString();
        }

        public static void DeserializeGeneratedFile(string fileContent)
        {
            string[] lines = fileContent.Split("\r\n".ToCharArray());

            var result = new StringBuilder();
            Type typeTarg = typeof(VWVehiclesWealthExtension<,>);
            IEnumerable<Type> instances = from t in Assembly.GetAssembly(typeof(VWVehicleExtensionUtils)).GetTypes()
                                          let y = t.BaseType
                                          where t.IsClass && !t.IsAbstract && y != null && y.IsGenericType && y.GetGenericTypeDefinition() == typeTarg
                                          select t;

            foreach (Type t in instances)
            {
                var ext = (IVWVehiclesWealthExtension) ExtensorContainer.instance.Instances[t];
                ext.DeserializeFromString(lines);
            }
        }

        public static void RemoveAllUnwantedVehicles() => new EnumerableActionThread(new Func<ThreadBase, IEnumerator>(VWVehicleExtensionUtils.RemoveAllUnwantedVehicles));
        public static IEnumerator RemoveAllUnwantedVehicles(ThreadBase t)
        {
            ushort vehId = 1;
            while (vehId < Singleton<VehicleManager>.instance.m_vehicles.m_size)
            {
                Vehicle vehicle = Singleton<VehicleManager>.instance.m_vehicles.m_buffer[vehId];
                VehicleInfo vehicleInfo = vehicle.Info;
                if (vehicleInfo != null && !VehicleUtils.IsTrailer(vehicleInfo) && vehicle.m_transportLine == 0)
                {
                    uint citizenOwner = vehicle.Info.m_vehicleAI.GetOwnerID(vehId, ref vehicle).Citizen;
                    if (citizenOwner > 0)
                    {
                        var ownerWealth = CitizenWealthDefinition.from(CitizenManager.instance.m_citizens.m_buffer[citizenOwner].WealthLevel);
                        if (ownerWealth != null)
                        {
                            var ext = ownerWealth.GetVehicleExtension();
                            if (!ext.IsModelCompatible(vehicleInfo))
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
            while (vehId < Singleton<VehicleManager>.instance.m_parkedVehicles.m_size)
            {
                VehicleParked vehicle = Singleton<VehicleManager>.instance.m_parkedVehicles.m_buffer[vehId];
                VehicleInfo vehicleInfo = vehicle.Info;
                if (vehicleInfo != null && !VehicleUtils.IsTrailer(vehicleInfo))
                {
                    uint citizenOwner = vehicle.m_ownerCitizen;
                    if (citizenOwner > 0)
                    {
                        var ownerWealth = CitizenWealthDefinition.from(CitizenManager.instance.m_citizens.m_buffer[citizenOwner].WealthLevel);
                        if (ownerWealth != null)
                        {
                            if (!ownerWealth.GetVehicleExtension().IsModelCompatible(vehicleInfo))
                            {
                                Singleton<VehicleManager>.instance.m_parkedVehicles.m_buffer[vehId].Info = ownerWealth.GetVehicleExtension().GetAModel();
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

    public enum ExtConfigs
    {
        MODELS
    }
}
