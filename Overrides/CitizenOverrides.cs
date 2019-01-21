using ColossalFramework;
using ColossalFramework.Globalization;
using ColossalFramework.Math;
using ColossalFramework.UI;
using Klyte.Commons.Extensors;
using Klyte.Commons.UI;
using Klyte.Commons.Utils;
using Klyte.VehicleWealthizer.Utils;
using Klyte.VehicleWealthizer.Extensors;
using System;
using System.Collections.Generic;
using UnityEngine;
using static Citizen;
using System.Reflection;

namespace Klyte.VehicleWealthizer.Overrides
{

    internal class ResidentAIOverrides : Redirector<ResidentAIOverrides>
    {
        public override void AwakeBody()
        {
            var from = typeof(ResidentAI).GetMethod("GetVehicleInfo", allFlags);
            var from2 = typeof(TouristAI).GetMethod("GetVehicleInfo", allFlags);
            var to = typeof(ResidentAIOverrides).GetMethod("GetVehicleInfoPost", allFlags);

            VWUtils.doLog($"REDIRECT VW {from}=>{to}");
            AddRedirect(from, null, to);
            VWUtils.doLog($"REDIRECT VW {from2}=>{to}");
            AddRedirect(from2, null, to);

            Type customResident = Type.GetType("TrafficManager.Custom.AI.CustomResidentAI");
            if (customResident != null)
            {
                var from3 = customResident.GetMethod("CustomGetVehicleInfo");
                VWUtils.doLog($"REDIRECT VW {from3}=>{to}");
                AddRedirect(from3, null, to);
            }
        }

        public override void doLog(string text, params object[] param)
        {
            VWUtils.doLog(text, param);
        }

        protected static void GetVehicleInfoPost(ResidentAI __instance, ushort instanceID, ref CitizenInstance citizenData, ref VehicleInfo trailer, ref VehicleInfo __result)
        {
            if (__result == null || __result.m_vehicleType != VehicleInfo.VehicleType.Car) return;

            VWUtils.doLog($"Selecting car for: {citizenData.Info.m_citizenAI}");

            var definition = CitizenWealthDefinition.from(CitizenManager.instance.m_citizens.m_buffer[citizenData.m_citizen].WealthLevel);
            var ext = definition.GetVehicleExtension();

            if (!ext.IsModelCompatible(__result))
            {
                __result = ext.GetAModel();
            }
        }
    }
}
