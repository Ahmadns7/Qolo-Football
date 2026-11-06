using UnityEngine;

namespace Qolo.Foundation
{
    /// <summary>Small deterministic-step ball integrator starter. It is not full match physics.</summary>
    public static class BallSimulation
    {
        public static BallState Step(BallState state, BallTuning tuning, float deltaTime)
        {
            if (deltaTime <= 0f) return state;

            Vector3 acceleration = tuning.Gravity;
            Vector3 velocity = state.Velocity + acceleration * deltaTime;
            float dragFactor = Mathf.Max(0f, 1f - Mathf.Max(0f, tuning.LinearDrag) * deltaTime);
            velocity *= dragFactor;

            return new BallState(state.Position + velocity * deltaTime, velocity);
        }
    }

    public struct BallState
    {
        public Vector3 Position { get; }
        public Vector3 Velocity { get; }

        public BallState(Vector3 position, Vector3 velocity)
        {
            Position = position;
            Velocity = velocity;
        }
    }

    public struct BallTuning
    {
        public Vector3 Gravity { get; }
        public float LinearDrag { get; }

        public BallTuning(Vector3 gravity, float linearDrag)
        {
            Gravity = gravity;
            LinearDrag = linearDrag;
        }

        public static BallTuning Default => new BallTuning(new Vector3(0f, -9.81f, 0f), 0.08f);
    }
}
