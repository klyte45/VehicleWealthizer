using ColossalFramework;
using Klyte.VehicleWealthizer.Overrides;
using Klyte.VehicleWealthizer.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using static Citizen;
using static ItemClass;

namespace Klyte.VehicleWealthizer.Extensors
{
    internal class CitizenWealthDefinition
    {
        public static readonly CitizenWealthDefinition HIGH = new CitizenWealthDefinition(Citizen.Wealth.High);
        public static readonly CitizenWealthDefinition MEDIUM = new CitizenWealthDefinition(Citizen.Wealth.Medium);
        public static readonly CitizenWealthDefinition LOW = new CitizenWealthDefinition(Citizen.Wealth.Low);

        private static readonly SubService[] allowedSubservices = new[] { ItemClass.SubService.ResidentialLow, ItemClass.SubService.ResidentialLowEco };


        public static Dictionary<CitizenWealthDefinition, IVWVehiclesWealthExtension> availableDefinitions
        {
            get {
                if (m_availableDefinitions.Count == 0)
                {
                    m_availableDefinitions[LOW] = VWVehiclesWealthExtensionLow.instance;
                    m_availableDefinitions[MEDIUM] = VWVehiclesWealthExtensionMed.instance;
                    m_availableDefinitions[HIGH] = VWVehiclesWealthExtensionHgh.instance;
                }
                return m_availableDefinitions;
            }
        }
        public static readonly Dictionary<CitizenWealthDefinition, IVWVehiclesWealthExtension> m_availableDefinitions = new Dictionary<CitizenWealthDefinition, IVWVehiclesWealthExtension>();
        public static Dictionary<CitizenWealthDefinition, Type> sysDefinitions
        {
            get {
                if (m_sysDefinitions.Count == 0)
                {
                    m_sysDefinitions[HIGH] = typeof(VWWthDefHgh);
                    m_sysDefinitions[MEDIUM] = typeof(VWWthDefMed);
                    m_sysDefinitions[LOW] = typeof(VWWthDefLow);
                }
                return m_sysDefinitions;
            }
        }
        private static readonly Dictionary<CitizenWealthDefinition, Type> m_sysDefinitions = new Dictionary<CitizenWealthDefinition, Type>();

        public Citizen.Wealth wealth
        {
            get;
        }

        private CitizenWealthDefinition(Citizen.Wealth wealth)
        {
            this.wealth = wealth;
        }

        internal IVWVehiclesWealthExtension GetVehicleExtension()
        {
            return availableDefinitions[this];
        }

        internal Type GetDefType()
        {
            return sysDefinitions[this];
        }

        public bool isFromSystem(Citizen citizen)
        {
            return citizen.WealthLevel == wealth;
        }

        public bool isFromSystem(VehicleInfo info)
        {
            return info.GetService() == ItemClass.Service.Residential && allowedSubservices.Contains(info.GetSubService());
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }
            if (obj.GetType() != typeof(CitizenWealthDefinition))
            {
                return false;
            }
            CitizenWealthDefinition other = (CitizenWealthDefinition)obj;

            return wealth == other.wealth;
        }

        public static bool operator ==(CitizenWealthDefinition a, CitizenWealthDefinition b)
        {
            if (Object.Equals(a, null) || Object.Equals(b, null))
            {
                return Object.Equals(a, null) == Object.Equals(b, null);
            }
            return a.Equals(b);
        }
        public static bool operator !=(CitizenWealthDefinition a, CitizenWealthDefinition b)
        {
            return !(a == b);
        }

        public static CitizenWealthDefinition from(Wealth wealth)
        {
            return availableDefinitions.Keys.FirstOrDefault(x => x.wealth == wealth);
        }


        public VWConfigWarehouse.ConfigIndex toConfigIndex()
        {
            var th = this;
            return VWConfigWarehouse.getConfigServiceSystemForDefinition(ref th);
        }

        public override string ToString()
        {
            return wealth.ToString();
        }

        public override int GetHashCode()
        {
            var hashCode = 286451371;
            hashCode = hashCode * -1521134295 + wealth.GetHashCode();
            return hashCode;
        }
    }

    internal abstract class VWWthDef<T> : Singleton<T> where T : VWWthDef<T>
    {
        internal abstract CitizenWealthDefinition GetCWD();
        public void Awake()
        {
            this.transform.SetParent(VehicleWealthizerMod.instance.refTransform);
        }
    }
    internal sealed class VWWthDefLow : VWWthDef<VWWthDefLow> { internal override CitizenWealthDefinition GetCWD() { return CitizenWealthDefinition.LOW; } }
    internal sealed class VWWthDefMed : VWWthDef<VWWthDefMed> { internal override CitizenWealthDefinition GetCWD() { return CitizenWealthDefinition.MEDIUM; } }
    internal sealed class VWWthDefHgh : VWWthDef<VWWthDefHgh> { internal override CitizenWealthDefinition GetCWD() { return CitizenWealthDefinition.HIGH; } }

}
