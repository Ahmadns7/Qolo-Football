namespace Qolo.Foundation
{
    /// <summary>Minimal elapsed-time utility; it does not define match rules or stoppage time.</summary>
    public sealed class MatchClock
    {
        public float ElapsedSeconds { get; private set; }
        public bool IsRunning { get; private set; }

        public void Start() => IsRunning = true;
        public void Pause() => IsRunning = false;
        public void Reset()
        {
            ElapsedSeconds = 0f;
            IsRunning = false;
        }

        public void Tick(float deltaTime)
        {
            if (IsRunning && deltaTime > 0f)
                ElapsedSeconds += deltaTime;
        }
    }
}
