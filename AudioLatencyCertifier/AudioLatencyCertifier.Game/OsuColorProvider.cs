using System;
using osuTK;
using osuTK.Graphics;

namespace AudioLatencyCertifier.Game
{
    public class OsuColourProvider
    {
        private readonly OsuColourScheme colourScheme;

        public OsuColourProvider(OsuColourScheme colourScheme)
        {
            this.colourScheme = colourScheme;
        }

        // Note that the following five colours are also defined in `OsuColour` as `{colourScheme}{0,1,2,3,4}`.
        // The difference as to which should be used where comes down to context.
        // If the colour in question is supposed to always match the view in which it is displayed theme-wise, use `OsuColourProvider`.
        // If the colour usage is special and in general differs from the surrounding view in choice of hue, use the `OsuColour` constants.
        public Color4 Colour0 => getColour(1, 0.8f);
        public Color4 Colour1 => getColour(1, 0.7f);
        public Color4 Colour2 => getColour(0.8f, 0.6f);
        public Color4 Colour3 => getColour(0.6f, 0.5f);
        public Color4 Colour4 => getColour(0.4f, 0.3f);

        public Color4 Highlight1 => getColour(1, 0.7f);
        public Color4 Content1 => getColour(0.4f, 1);
        public Color4 Content2 => getColour(0.4f, 0.9f);
        public Color4 Light1 => getColour(0.4f, 0.8f);
        public Color4 Light2 => getColour(0.4f, 0.75f);
        public Color4 Light3 => getColour(0.4f, 0.7f);
        public Color4 Light4 => getColour(0.4f, 0.5f);
        public Color4 Dark1 => getColour(0.2f, 0.35f);
        public Color4 Dark2 => getColour(0.2f, 0.3f);
        public Color4 Dark3 => getColour(0.2f, 0.25f);
        public Color4 Dark4 => getColour(0.2f, 0.2f);
        public Color4 Dark5 => getColour(0.2f, 0.15f);
        public Color4 Dark6 => getColour(0.2f, 0.1f);
        public Color4 Foreground1 => getColour(0.1f, 0.6f);
        public Color4 Background1 => getColour(0.1f, 0.4f);
        public Color4 Background2 => getColour(0.1f, 0.3f);
        public Color4 Background3 => getColour(0.1f, 0.25f);
        public Color4 Background4 => getColour(0.1f, 0.2f);
        public Color4 Background5 => getColour(0.1f, 0.15f);
        public Color4 Background6 => getColour(0.1f, 0.1f);

        private Color4 getColour(float saturation, float lightness) => Color4.FromHsl(new Vector4(getBaseHue(colourScheme), saturation, lightness, 1));

        // See https://github.com/ppy/osu-web/blob/5a536d217a21582aad999db50a981003d3ad5659/app/helpers.php#L1620-L1628
        private static float getBaseHue(OsuColourScheme colourScheme)
        {
            switch (colourScheme)
            {
                default:
                    throw new ArgumentException($@"{colourScheme} colour scheme does not provide a hue value in {nameof(getBaseHue)}.");

                case OsuColourScheme.Red:
                    return 0;

                case OsuColourScheme.Pink:
                    return 333 / 360f;

                case OsuColourScheme.Orange:
                    return 45 / 360f;

                case OsuColourScheme.Lime:
                    return 90 / 360f;

                case OsuColourScheme.Green:
                    return 125 / 360f;

                case OsuColourScheme.Aquamarine:
                    return 160 / 360f;

                case OsuColourScheme.Purple:
                    return 255 / 360f;

                case OsuColourScheme.Blue:
                    return 200 / 360f;

                case OsuColourScheme.Plum:
                    return 320 / 360f;
            }
        }
    }

    public enum OsuColourScheme
    {
        Red,
        Pink,
        Orange,
        Lime,
        Green,
        Purple,
        Blue,
        Plum,
        Aquamarine
    }
}
