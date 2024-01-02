using osu.Framework.Allocation;
using osu.Framework.Platform;

namespace AudioLatencyCertifier.Game.Tests.Visual
{
    public class TestSceneAudioLatencyCertifierGame : AudioLatencyCertifierTestScene
    {
        // Add visual tests to ensure correct behaviour of your game: https://github.com/ppy/osu-framework/wiki/Development-and-Testing
        // You can make changes to classes associated with the tests and they will recompile and update immediately.

        private AudioLatencyCertifierGame game;

        [BackgroundDependencyLoader]
        private void load(GameHost host)
        {
            game = new AudioLatencyCertifierGame();
            game.SetHost(host);

            AddGame(game);
        }
    }
}