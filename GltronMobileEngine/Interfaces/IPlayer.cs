namespace GltronMobileEngine.Interfaces;

public interface IPlayer
{
    int getPlayerNum();
    int getTrailOffset();
    ISegment? getTrail(int index);
}
