using ColossalFramework;
using ColossalFramework.Globalization;
using ColossalFramework.UI;
using Klyte.Commons.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Klyte.VehicleWealthizer.Extensors;

namespace Klyte.VehicleWealthizer.Utils
{
    internal class VWUtils : KlyteUtils
    {
        #region Logging
        public static void doLog(string format, params object[] args)
        {
            try
            {
                if (VWSingleton.debugMode)
                {
                    if (VWSingleton.instance != null)
                    {
                        Debug.LogWarningFormat("VWv" + VWSingleton.version + " " + format, args);

                    }
                    else
                    {
                        Console.WriteLine("VWv" + VWSingleton.version + " " + format, args);
                    }
                }
            }
            catch
            {
                Debug.LogErrorFormat("VWv" + VWSingleton.version + " Erro ao fazer log: {0} (args = {1})", format, args == null ? "[]" : string.Join(",", args.Select(x => x != null ? x.ToString() : "--NULL--").ToArray()));
            }
        }
        public static void doErrorLog(string format, params object[] args)
        {
            try
            {
                if (VWSingleton.instance != null)
                {
                    Debug.LogErrorFormat("VWv" + VWSingleton.version + " " + format, args);
                }
                else
                {
                    Console.WriteLine("VWv" + VWSingleton.version + " " + format, args);
                }
            }
            catch
            {
                Debug.LogErrorFormat("VWv" + VWSingleton.version + " Erro ao logar ERRO!!!: {0} (args = [{1}])", format, args == null ? "" : string.Join(",", args.Select(x => x != null ? x.ToString() : "--NULL--").ToArray()));
            }

        }
        #endregion

        internal static List<string> LoadBasicAssets(CitizenWealthDefinition definition)
        {
            List<string> basicAssetsList = new List<string>();
            for (uint num = 0u; (ulong)num < (ulong)((long)PrefabCollection<VehicleInfo>.PrefabCount()); num += 1u)
            { 
                VehicleInfo prefab = PrefabCollection<VehicleInfo>.GetPrefab(num);
                if (!(prefab == null) && definition.isFromSystem(prefab) && !IsTrailer(prefab))
                {
                    basicAssetsList.Add(prefab.name);
                }
            }
            return basicAssetsList;
        }
    }
}

