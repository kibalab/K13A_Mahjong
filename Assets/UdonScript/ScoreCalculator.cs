
using UdonSharp;
using UnityEngine;

public class ScoreCalculator : UdonSharpBehaviour
{
    [SerializeField] public HandUtil HandUtil;
    [SerializeField] public CalculatingContextHandler Ctx;

    public void CalculateTsumo(
        PlayerStatus playerStatus,
        AgariContext agariContext,
        Card[] sealedCards,
        Card[] openedCards,
        object[] ctxs,
        bool isLastTsumo,
        bool isByRinshan)
    {
        var maxHan = 0;
        var maxFu = 0;

        foreach(object[] ctx in ctxs)
        {
            playerStatus.Han = 0;
            playerStatus.Fu = 20; // 기본 20부

            // 1판역
            AddScore_Riichi(playerStatus);
            AddScore_MenzenTsumo(playerStatus);
            AddScore_Pinfu(playerStatus, agariContext, ctx);
            AddScore_OneSetOfIdenticalSequences(playerStatus, ctx);
            AddScore_LastTileFromTheWall(playerStatus, isLastTsumo);
            AddScore_WordCards(playerStatus, ctx);
            AddScore_DeadWallDraw(playerStatus, isByRinshan);
            AddScore_Dora(playerStatus, sealedCards, openedCards);

            if (playerStatus.Han > maxHan)
            {
                maxHan = playerStatus.Han;
                maxFu = playerStatus.Fu;
            }
        }

        playerStatus.Han = maxHan;
        playerStatus.Fu = maxFu;
    }

    public void CalculateRon(PlayerStatus playerStatus,
        AgariContext agariContext,
        Card[] sealedCards,
        Card[] openedCards,
        object[] ctxs)
    {
        var maxHan = 0;
        var maxFu = 0;

        foreach (object[] ctx in ctxs)
        {
            playerStatus.Han = 0;
            playerStatus.Fu = 20; // 기본 20부

            // 1판역
            AddScore_Riichi(playerStatus);
            AddScore_Pinfu(playerStatus, agariContext, ctx);
            AddScore_OneSetOfIdenticalSequences(playerStatus, ctx);
            AddScore_WordCards(playerStatus, ctx);
            AddScore_Dora(playerStatus, sealedCards, openedCards);

            if (playerStatus.Han > maxHan)
            {
                maxHan = playerStatus.Han;
                maxFu = playerStatus.Fu;
            }
        }

        playerStatus.Han = maxHan;
        playerStatus.Fu = maxFu;
    }

    void AddScore_MenzenTsumo(PlayerStatus playerStatus)
    {
        if (playerStatus.IsMenzen)
        {
            playerStatus.Han += 1;
        }
    }

    void AddScore_Riichi(PlayerStatus playerStatus)
    {
        // 리치 화료
        if (playerStatus.IsRiichiMode)
        {
            playerStatus.Han += 1;
        }

        // 일발
        if (playerStatus.IsOneShotRiichi)
        {
            playerStatus.Han += 1;
        }

        //더블리치
        if (playerStatus.IsFirstOrder)
        {
            playerStatus.Han += 2;
        }
    }


     void AddScore_Pinfu(PlayerStatus playerStatus, AgariContext agariContext, object[] ctx)
    {
        var head = GetHeadGlobalOrder(ctx);
        var ponCount = Ctx.ReadPonCount(ctx);

        if (ponCount == 0
            && playerStatus.IsMenzen
            && !agariContext.IsSingleWaiting
            && !HandUtil.IsWordCard(head)
            && agariContext.AgariableCount == 2 
            && agariContext.AgariableCardGlobalOrders[0] == agariContext.AgariableCardGlobalOrders[0] + 1)
        {
            playerStatus.Han += 1;
        }
    }

    // 이페코
    void AddScore_OneSetOfIdenticalSequences(PlayerStatus playerStatus, object[] ctx)
    {
        if (playerStatus.IsMenzen)
        {
            var chiCount = Ctx.ReadChiCount(ctx);
            var chiList = Ctx.ReadChiList(ctx);

            if (chiCount > 1 && GetSameChiCount(chiList, chiCount) == 2)
            {
                playerStatus.Han += 1;
            }
        }
    }

    void AddScore_LastTileFromTheWall(PlayerStatus playerStatus, bool isLastTsumo)
    {
        if (isLastTsumo)
        {
            playerStatus.Han += 1;
        }
    }

    void AddScore_AllSimples(PlayerStatus playerStatus, object[] ctx)
    {
        var ponList = Ctx.ReadPonList(ctx);
        var ponCount = Ctx.ReadPonCount(ctx);

        for (var i = 0; i < ponCount; ++i)
        {
            if (HandUtil.IsYaojuhai(ponList[i]))
            {
                return;
            }
        }

        var chiCount = Ctx.ReadChiCount(ctx);
        var chiList = Ctx.ReadChiList(ctx);

        for (var i = 0; i < chiCount; ++i)
        {
            if (HandUtil.IsYaojuhai(chiList[i]))
            {
                return;
            }
        }

        var head = GetHeadGlobalOrder(ctx);
        if (HandUtil.IsYaojuhai(head))
        {
            return;
        }

        playerStatus.Han += 1;
    }

    void AddScore_WordCards(PlayerStatus playerStatus, object[] ctx)
    {
        var ponList = Ctx.ReadPonList(ctx);
        var ponCount = Ctx.ReadPonCount(ctx);

        for (var i = 0; i < ponCount; ++i)
        {
            var pon = ponList[i];
            if (IsWhiteGreenRed(pon))
            {
                playerStatus.Han += 1;
            }

            if (IsMyWindWordCard(playerStatus, pon))
            {
                playerStatus.Han += 1;
            }

            if (IsCurrentWindWordCard("", playerStatus, pon))
            {
                playerStatus.Han += 1;
            }
        }
    }

    // 영상개화
    void AddScore_DeadWallDraw(PlayerStatus playerStatus, bool isByRinshan)
    {
        if (isByRinshan)
        {
            playerStatus.Han += 1;
        }
    }

    // 하저로어

    void AddScore_LastDiscard(PlayerStatus playerStatus, bool isLastDiscard)
    {
        if (isLastDiscard)
        {
            playerStatus.Han += 1;
        }
    }

    void AddScore_Dora(
        PlayerStatus playerStatus,
        Card[] sealedCards,
        Card[] openedCards)
    {
        var totalDoraCount = 0;

        foreach(var card in sealedCards)
        {
            if (card.IsDora)
            {
                totalDoraCount += 1;
            }
        }
        foreach(var card in openedCards)
        {
            if (card.IsDora)
            {
                totalDoraCount += 1;
            }
        }

        playerStatus.Han += totalDoraCount;
    }

    //삼색동순
    void AddScore_ThreeColorStraight(PlayerStatus playerStatus, object[] ctx)
    {
        // 아직 Ctx 쪽을 재대로 이해못해서 TODO
    }

    bool IsWhiteGreenRed(int globalOrder)
    {
        var white = HandUtil.GetWordsStartGlobalOrder() + HandUtil.GetWhiteCardNumber() - 1;
        var red = HandUtil.GetWordsEndGlobalOrder();

        return white <= globalOrder && globalOrder <= red;
    }

    bool IsMyWindWordCard(PlayerStatus playerStatus, int globalOrder)
    {
        // 어떻게 할지 좀 고민해봄

        return true;
    }

    bool IsCurrentWindWordCard(string currentWind, PlayerStatus playerStatus, int globalOrder)
    {
        // 어떻게 할지 좀 고민해봄
        switch (currentWind)
        {
            case "East":
            case "West":
            case "North":
            case "South":
                break;
        }

        return true;
    }

    int GetSameChiCount(int[] chiList, int chiCount)
    {
        var maxCount = 0;

        for (var i = 0; i< chiCount-1; ++i)
        {
            var sameChiCount = 0;

            for (var k = i+1; i < chiCount; ++i)
            {
                if (chiList[i] == chiList[k])
                {
                    sameChiCount++;
                }
            }

            if (maxCount < sameChiCount)
            {
                maxCount = sameChiCount;
            }
        }
        return maxCount;
    }

    int GetHeadGlobalOrder(object[] ctx)
    {
        var remainCards = Ctx.ReadGlobalOrders(ctx);
        var pair = HandUtil.FindPairs(remainCards);

        return pair[0];
    }
}


