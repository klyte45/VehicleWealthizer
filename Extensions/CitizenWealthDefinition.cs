using ColossalFramework;
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
                    m_availableDefinitions[LOW] = VWVehiclesWealthExtensionLow.Instance;
                    m_availableDefinitions[MEDIUM] = VWVehiclesWealthExtensionMed.Instance;
                    m_availableDefinitions[HIGH] = VWVehiclesWealthExtensionHgh.Instance;
                }
                return m_availableDefinitions;
            }
        }
        public static readonly Dictionary<CitizenWealthDefinition, IVWVehiclesWealthExtension> m_availableDefinitions = new Dictionary<CitizenWealthDefinition, IVWVehiclesWealthExtension>();
        public static Dictionary<CitizenWealthDefinition, Type> sysDefinitions { get; } = new Dictionary<CitizenWealthDefinition, Type>()
        {
            [HIGH] = typeof(VWWthDefHgh),
            [MEDIUM] = typeof(VWWthDefMed),
            [LOW] = typeof(VWWthDefLow)
        };
        public Citizen.Wealth wealth
        {
            get;
        }

        private CitizenWealthDefinition(Citizen.Wealth wealth) => this.wealth = wealth;

        internal IVWVehiclesWealthExtension GetVehicleExtension() => availableDefinitions[this];

        internal Type GetDefType() => sysDefinitions[this];

        public bool isFromSystem(Citizen citizen) => citizen.WealthLevel == wealth;

        public bool isFromSystem(VehicleInfo info) => info.GetService() == ItemClass.Service.Residential && allowedSubservices.Contains(info.GetSubService());

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
            var other = (CitizenWealthDefinition) obj;

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
        public static bool operator !=(CitizenWealthDefinition a, CitizenWealthDefinition b) => !(a == b);

        public static CitizenWealthDefinition from(Wealth wealth) => availableDefinitions.Keys.FirstOrDefault(x => x.wealth == wealth);


        public VWConfigIndex toConfigIndex()
        {
            CitizenWealthDefinition th = this;
            return VWUtils.GetConfigServiceSystemForDefinition(ref th);
        }

        public override string ToString() => wealth.ToString();

        public override int GetHashCode()
        {
            int hashCode = 286451371;
            hashCode = (hashCode * -1521134295) + wealth.GetHashCode();
            return hashCode;
        }
    }

    public abstract class VWWthDef<T> : Singleton<T> where T : VWWthDef<T>
    {
        internal abstract CitizenWealthDefinition GetCWD();
        public void Awake() => transform.SetParent(VehicleWealthizerMod.Instance.RefTransform);
    }
    public sealed class VWWthDefLow : VWWthDef<VWWthDefLow> { internal override CitizenWealthDefinition GetCWD() => CitizenWealthDefinition.LOW; }
    public sealed class VWWthDefMed : VWWthDef<VWWthDefMed> { internal override CitizenWealthDefinition GetCWD() => CitizenWealthDefinition.MEDIUM; }
    public sealed class VWWthDefHgh : VWWthDef<VWWthDefHgh> { internal override CitizenWealthDefinition GetCWD() => CitizenWealthDefinition.HIGH; }

}
