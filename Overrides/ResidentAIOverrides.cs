using Klyte.Commons.Extensors;
using Klyte.Commons.Utils;
using Klyte.VehicleWealthizer.Extensors;
using System;
using System.Reflection;
using UnityEngine;

namespace Klyte.VehicleWealthizer.Overrides
{

    internal class ResidentAIOverrides : MonoBehaviour, IRedirectable
    {
        public Redirector RedirectorInstance { get; internal set; }

        public void Awake()
        {
            RedirectorInstance = new Redirector();

            MethodInfo from = typeof(ResidentAI).GetMethod("GetVehicleInfo", RedirectorUtils.allFlags);
            MethodInfo from2 = typeof(TouristAI).GetMethod("GetVehicleInfo", RedirectorUtils.allFlags);
            MethodInfo to = typeof(ResidentAIOverrides).GetMethod("GetVehicleInfoPost", RedirectorUtils.allFlags);

            LogUtils.DoLog($"REDIRECT VW {from}=>{to}");
            RedirectorInstance.AddRedirect(from, null, to);
            LogUtils.DoLog($"REDIRECT VW {from2}=>{to}");
            RedirectorInstance.AddRedirect(from2, null, to);

            var customResident = Type.GetType("TrafficManager.Custom.AI.CustomResidentAI");
            if (customResident != null)
            {
                MethodInfo to2 = typeof(ResidentAIOverrides).GetMethod("CustomGetVehicleInfoPost", RedirectorUtils.allFlags);
                MethodInfo from3 = customResident.GetMethod("CustomGetVehicleInfo");
                LogUtils.DoLog($"REDIRECT VW {from3}=>{to}");
                RedirectorInstance.AddRedirect(from3, null, to2);
            }
        }

        protected static void GetVehicleInfoPost(ref CitizenInstance citizenData, ref VehicleInfo __result)
        {
            if (__result == null || __result.m_vehicleType != VehicleInfo.VehicleType.Car || __result.m_class.m_service == ItemClass.Service.PublicTransport || (citizenData.m_flags & CitizenInstance.Flags.OnTour) != CitizenInstance.Flags.None)
            {
                return;
            }

            LogUtils.DoLog($"Selecting car for: {citizenData.Info.m_citizenAI}");

            var definition = CitizenWealthDefinition.from(CitizenManager.instance.m_citizens.m_buffer[citizenData.m_citizen].WealthLevel);
            LogUtils.DoLog($"definition: {definition}");
            IVWVehiclesWealthExtension ext = definition.GetVehicleExtension();
            LogUtils.DoLog($"ext: {ext}");
            if (!ext.IsModelCompatible(__result))
            {
                __result = ext.GetAModel();
            }
        }

        public static void CustomGetVehicleInfoPost(ref CitizenInstance citizenData, ref VehicleInfo __result) => GetVehicleInfoPost(ref citizenData, ref __result);
    }
}
