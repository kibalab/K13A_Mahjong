using UdonSharp;

public class PlayerStatus : UdonSharpBehaviour
{
    public bool IsRiichiMode;
    public bool IsOneShotRiichi;
    public bool IsMenzen;
    public string Wind;
    public bool IsFirstOrder;

    public int Han;
    public int Fu;

    public void Initialize()
    {
        IsRiichiMode = false;
        IsMenzen = false;
        IsFirstOrder = true;
    }
}
