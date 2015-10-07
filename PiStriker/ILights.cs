using System.Threading;

namespace PiStriker
{
    public interface ILights
    {
        void SetToBlack();
        void TurnOnLedToDsplayScore(int height);
        void SlowYellowRise();
        void SlowPinkRise();
        void SlowLightBlueRise();
        void QuickOrange();
        void QuickYellow();
        void QuickPurple();
        void PartyMode(CancellationToken cancellationToken);
    }
}