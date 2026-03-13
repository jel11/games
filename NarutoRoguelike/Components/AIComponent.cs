using Microsoft.Xna.Framework;

namespace NarutoRoguelike.Components
{
    public enum AIState
    {
        Idle,
        Alert,
        Chasing,
        Attacking
    }

    public class AIComponent : IComponent
    {
        public AIState State            { get; set; } = AIState.Idle;
        public Point   LastKnownPlayerPos { get; set; } = Point.Zero;
        public int     AlertTurns       { get; set; }   // turns remaining in Alert before returning to Idle
        public int     MoveIntentX      { get; set; }
        public int     MoveIntentY      { get; set; }
        public int     TurnsUntilAction { get; set; }   // for speed-gating slow enemies

        /// <summary>How many tiles away the enemy can spot the player.</summary>
        public int SightRange { get; set; } = 8;

        public AIComponent() { }

        public AIComponent(int sightRange)
        {
            SightRange = sightRange;
        }

        public void ClearIntent()
        {
            MoveIntentX = 0;
            MoveIntentY = 0;
        }
    }
}
