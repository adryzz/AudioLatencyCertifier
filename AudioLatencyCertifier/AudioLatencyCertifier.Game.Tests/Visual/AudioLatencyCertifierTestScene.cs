using osu.Framework.Testing;

namespace AudioLatencyCertifier.Game.Tests.Visual
{
    public class AudioLatencyCertifierTestScene : TestScene
    {
        protected override ITestSceneTestRunner CreateRunner() => new AudioLatencyCertifierTestSceneTestRunner();

        private class AudioLatencyCertifierTestSceneTestRunner : AudioLatencyCertifierGameBase, ITestSceneTestRunner
        {
            private TestSceneTestRunner.TestRunner runner;

            protected override void LoadAsyncComplete()
            {
                base.LoadAsyncComplete();
                Add(runner = new TestSceneTestRunner.TestRunner());
            }

            public void RunTestBlocking(TestScene test) => runner.RunTestBlocking(test);
        }
    }
}