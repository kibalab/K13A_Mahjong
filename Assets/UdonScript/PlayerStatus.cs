using UdonSharp;

public class PlayerStatus : UdonSharpBehaviour
{
    // 16개 이상의 역이 날 수 있나?
    const int MAX_YAKU_COUNT = 16;

    // 밖에서 정해줌
    public bool IsRiichiMode;
    public bool IsOneShotRiichi;
    public bool IsMenzen;
    public string Wind; // 내 방위 
    public string RoundWind; // 현재 판의 방위
    public bool IsFirstOrder;
    public int KkanCount;

    // 계산 중에 정함

    // 이페코 1판 이런 식으로 표기하기 떄문에
    // (이페코, 1판) 배열로 정리해야 한다
    public string[] YakuKey;
    public int[] Han;
    public int TotalHan;
    public int YakuIndex;

    // 부수는 별도로 계산하는 것 같으니 배열화 안 함 
    public int Fu;

    public void Initialize()
    {
        IsRiichiMode = false;
        IsMenzen = false;
        IsFirstOrder = true;

        InitializeHanFu();
    }

    public void InitializeHanFu()
    {
        YakuKey = new string[MAX_YAKU_COUNT];
        Han = new int[MAX_YAKU_COUNT];
        TotalHan = 0;
        Fu = 0;
        YakuIndex = 0;
    }

    public void AddHan(string key, int han)
    {
        // key라고 저장하는 이유는, 로컬라이징 할때 써야하기 때문
        YakuKey[YakuIndex] = key;
        Han[YakuIndex] = han;
        TotalHan += han;
        ++YakuIndex;
    }
}
