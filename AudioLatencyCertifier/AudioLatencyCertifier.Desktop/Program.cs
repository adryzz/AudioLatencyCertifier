using osu.Framework.Platform;
using osu.Framework;
using AudioLatencyCertifier.Game;

namespace AudioLatencyCertifier.Desktop
{
    public static class Program
    {
        public static void Main()
        {
            using (GameHost host = Host.GetSuitableDesktopHost(@"AudioLatencyCertifier"))
            using (osu.Framework.Game game = new AudioLatencyCertifierGame())
                host.Run(game);
        }
    }
}