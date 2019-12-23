using Klyte.Commons.Utils;
using Klyte.VehicleWealthizer.Extensors;
using System.Collections.Generic;

namespace Klyte.VehicleWealthizer.Utils
{
    internal class VWUtils
    {

        internal static List<string> LoadBasicAssets(CitizenWealthDefinition definition)
        {
            var basicAssetsList = new List<string>();
            for (uint num = 0u; num < (ulong) PrefabCollection<VehicleInfo>.PrefabCount(); num += 1u)
            {
                VehicleInfo prefab = PrefabCollection<VehicleInfo>.GetPrefab(num);
                if (!(prefab == null) && definition.isFromSystem(prefab) && !VehicleUtils.IsTrailer(prefab))
                {
                    basicAssetsList.Add(prefab.name);
                }
            }
            return basicAssetsList;
        }
        internal static VWConfigIndex GetConfigServiceSystemForDefinition(ref CitizenWealthDefinition serviceSystemDefinition)
        {
            if (serviceSystemDefinition == CitizenWealthDefinition.LOW)
            {
                return VWConfigIndex.WEALTH_LOW;
            }

            if (serviceSystemDefinition == CitizenWealthDefinition.MEDIUM)
            {
                return VWConfigIndex.WEALTH_MED;
            }

            if (serviceSystemDefinition == CitizenWealthDefinition.HIGH)
            {
                return VWConfigIndex.WEALTH_HGH;
            }

            return VWConfigIndex.NIL;
        }
    }
}

