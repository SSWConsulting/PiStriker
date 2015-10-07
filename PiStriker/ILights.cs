using System.Threading;

namespace PiStriker
{
    public interface ILights
    {
        /// <summary>
        ///  Turns the neo pixel strip off
        /// </summary>
        void SetToBlack();

        /// <summary>
        /// Displays the score to the user by turning on the neo pixel strips on
        /// </summary>
        /// <param name="height">The highest beam sensor broken by the wood block.</param>
        void TurnOnLedToDisplayScore(int height);

        /// <summary>
        ///  Predefined light byte command sequence - Which turns the entire selected neo pixel strip a predefined color one led at a time
        /// </summary>
        void Crawling(byte r, byte g, byte b, byte stripId, CancellationToken cancellationToken);

        /// <summary>
        ///  Predefined light byte command sequence - Which turns the entire selected neo pixel strip a predefined color in batches of 5 at a time
        /// </summary>
        void Batched(byte r, byte g, byte b, byte stripId, CancellationToken cancellationToken);

        void PartyMode(CancellationToken cancellationToken);
    }
}