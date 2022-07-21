using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using izolabella.Maui.Util.GenericStructures.Animations.Bases;

namespace izolabella.LoFi.App.Wide.Constants
{
    public static class ColorConfigs
    {

        public static MauiActorConfiguration Config { get; } = new(
            PrimaryAnimationTime: TimeSpan.FromMilliseconds(2000),
            SecondaryAnimationTime: TimeSpan.FromMilliseconds(2000),
            PinkText,
            PinkDisabled,
            TransparencyEnabled: 0.8f,
            SecondaryTransparencyEnabled: 0.65f,
            TransparencyDisabled: 0.25f,
            Easing.CubicInOut);

        public static Color PinkText => new(247f, 235f, 255f);

        public static Color PinkDisabled => new(123f, 117f, 128f);
    }
}
