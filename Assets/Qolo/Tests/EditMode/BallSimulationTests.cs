using NUnit.Framework;
using UnityEngine;
using Qolo.Foundation;

namespace Qolo.Foundation.Tests
{
    public class BallSimulationTests
    {
        [Test]
        public void StepWithZeroDeltaTimePreservesState()
        {
            var start = new BallState(new Vector3(1f, 2f, 3f), new Vector3(4f, 5f, 6f));
            var next = BallSimulation.Step(start, BallTuning.Default, 0f);
            Assert.AreEqual(start.Position, next.Position);
            Assert.AreEqual(start.Velocity, next.Velocity);
        }

        [Test]
        public void GravityChangesVerticalVelocityAndPosition()
        {
            var start = new BallState(Vector3.zero, Vector3.zero);
            var next = BallSimulation.Step(start, new BallTuning(new Vector3(0f, -10f, 0f), 0f), 0.1f);
            Assert.Less(next.Velocity.y, 0f);
            Assert.Less(next.Position.y, 0f);
        }

        [Test]
        public void MatchClockTicksOnlyWhenRunning()
        {
            var clock = new MatchClock();
            clock.Tick(1f);
            Assert.AreEqual(0f, clock.ElapsedSeconds);
            clock.Start();
            clock.Tick(2f);
            clock.Pause();
            clock.Tick(3f);
            Assert.AreEqual(2f, clock.ElapsedSeconds);
        }
    }
}
