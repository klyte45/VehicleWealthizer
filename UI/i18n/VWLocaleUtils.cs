using ColossalFramework.Globalization;
using Klyte.Commons.i18n;
using Klyte.VehicleWealthizer.Utils;
using System;

namespace Klyte.VehicleWealthizer.i18n
{
    public class VWLocaleUtils : KlyteLocaleUtils<VWLocaleUtils, VWResourceLoader>
    {

        public override string prefix => "VW_";

        protected override string packagePrefix => "Klyte.VehicleWealthizer";
    }
}
