using System.Diagnostics;
using UdonSharp;
using UnityEngine;

public class CardSyncQueue : UdonSharpBehaviour
{
    // Update 한 틱에 카드 동기화 하나씩 해야 동기화가 씹히지 않는다 (ㅠㅠ)
    // 동기화를 요청하는 부분이 카드 버림 or 첫 시작 시 인데
    // 두 군데에서 하는 동기화를 모두 Update 한 틱에 하나 처리하기 위해 별도 클래스를 만듬

    int[] syncQueue;
    int topIndex;
    int currIndex;

    bool isInitialized = false;

    Card[] yama;

    public void Initialize(Card[] yama)
    {
        // TableManager에서 yama 초기화 후 호출
        // syncQueue는 SortPosition용으로, 20개정도면 넉넉하나 혹시모르니 100개 박음
        syncQueue = new int[300];
        topIndex = 0;
        currIndex = 0;
        isInitialized = true;

        this.yama = yama;
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

        // 안전장치
        if (Time.time < 5.0f)
        {
            return;
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
