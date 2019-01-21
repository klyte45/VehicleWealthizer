using ColossalFramework.Globalization;
using Klyte.Commons.i18n;
using Klyte.VehicleWealthizer.Utils;
using System;

namespace Klyte.VehicleWealthizer.i18n
{
    internal class VWLocaleUtils : KlyteLocaleUtils<VWLocaleUtils, VWResourceLoader>
    {
        protected override string[] locales => new string[] { "en", "pt" };

        protected override string prefix => "VW_";

        protected override string packagePrefix => "Klyte.VehicleWealthizer";
    }
}
