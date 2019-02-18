using ColossalFramework;
using ColossalFramework.UI;
using Klyte.Commons.Interfaces;
using Klyte.Commons.Utils;
using Klyte.VehicleWealthizer.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;

namespace Klyte.VehicleWealthizer.TextureAtlas
{
    public class VWCommonTextureAtlas : TextureAtlasDescriptor<VWCommonTextureAtlas, VWResourceLoader>
    {
        protected override string ResourceName => "UI.Images.sprites.png";
        protected override string CommonName => "VehicleWealthizerSprites";
        protected override string[] SpriteNames => new string[] {
                     "VWIcon","AutoNameIcon","AutoColorIcon","RemoveUnwantedIcon","ConfigIcon","24hLineIcon", "PerHourIcon","AbsoluteMode","RelativeMode"
                };
    }
}
