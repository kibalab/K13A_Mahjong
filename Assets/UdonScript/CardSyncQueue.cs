using System.Diagnostics;
using UdonSharp;

public class CardSyncQueue : UdonSharpBehaviour
{
    // Update 한 틱에 카드 동기화 하나씩 해야 동기화가 씹히지 않는다 (ㅠㅠ)
    // 동기화를 요청하는 부분이 카드 버림 or 첫 시작 시 인데
    // 두 군데에서 하는 동기화를 모두 Update 한 틱에 하나 처리하기 위해 별도 클래스를 만듬

    int[] syncQueue;
    int topIndex;
    int currIndex;

    bool syncAllRequested = false;
    int syncAllIndex = 0;

    bool isInitialized = false;

    Card[] yama;

    public void Initialize(Card[] yama)
    {
        // TableManager에서 yama 초기화 후 호출
        // syncQueue는 SortPosition용으로, 20개정도면 넉넉하나 혹시모르니 100개 박음
        syncQueue = new int[100];
        topIndex = 0;
        currIndex = 0;
        isInitialized = true;

        this.yama = yama;
    }

    public void SyncAll()
    {
        // 기존 Queue를 초기화하고 yama 전체 순회를 시작한다
        // 큐에 134개를 전부 박을 수도 있지만...
        // 그러면 N명이 동시에 월드를 입장할때 134xN개가 큐에 쌓이는데 이건 싫음
        // SyncAll()이 여러번 불리면 0번부터 다시 시작하는 걸 반복하는 것으로
        currIndex = 0;
        topIndex = 0;

        syncAllIndex = 0;
        syncAllRequested = true;
    }

    public void AddSync(int yamaIndex)
    {
        // 하나를 더 추가하려는데 이미 Queue가 100개 쌓여있으면?
        // 맨 앞을 싱크한 후 한칸씩 땡긴다
        // 여기가 불릴 일은 거의 없을거라고 생각
        if (topIndex == syncQueue.Length)
        {
            yama[currIndex++].SyncData();
            SortQueue();
        }

        syncQueue[topIndex++] = yamaIndex;
    }


    void Update()
    {
        // 초기화 되어있지 않으면 부르지 않는다
        // 이는 Master 아닌 곳에서 불리지 않기 하기 위함도 있음
        if (!isInitialized)
        {
            return;
        }

        if (syncAllRequested)
        {
            if (syncAllIndex < yama.Length)
            {
                yama[syncAllIndex++].SyncData();
            }

            if (syncAllIndex == yama.Length)
            {
                syncAllRequested = false;
            }
        }
        else if (currIndex < topIndex)
        {
            var index = syncQueue[currIndex++];

            yama[index].SyncPosition();
        }
        else if (currIndex == topIndex)
        {
            currIndex = 0;
            topIndex = 0;
        }
    }

    void SortQueue()
    {
        for (var i = currIndex; i < syncQueue.Length; ++i)
        {
            syncQueue[i - currIndex] = syncQueue[i];
        }

        topIndex = topIndex - currIndex;
        currIndex = 0;
    }
}
