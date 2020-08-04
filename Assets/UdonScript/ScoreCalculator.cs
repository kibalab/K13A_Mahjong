
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
        int[] hanList = new int[0];
        var maxHan = 0;
        var maxFu = 0;

        foreach (object[] ctx in ctxs)
        {
            playerStatus.InitializeHanFu();
            playerStatus.Fu = 20; // 기본 20부


            // 나중에 문전 한정인 친구들 조건 밖으로 빼내서 미리 검사하자


            // ---- 1판역 ----
            {
                // 리치
                AddScore_Riichi(playerStatus);
                // 쯔모
                AddScore_MenzenTsumo(playerStatus);
                // 핑후
                AddScore_Pinfu(playerStatus, agariContext, ctx);
                // 이페코
                AddScore_OneSetOfIdenticalSequences(playerStatus, ctx);
                // 해저로월
                AddScore_LastTileFromTheWall(playerStatus, isLastTsumo);
                // 자패
                AddScore_WordCards(playerStatus, ctx);
                // 영상개화
                AddScore_DeadWallDraw(playerStatus, isByRinshan);
                // 도라
                AddScore_Dora(playerStatus, sealedCards, openedCards);
                // 탕야오
                AddScore_AllSimples(playerStatus, ctx);
            }

            // ---- 2판역 ----
            {
                // 삼색동순
                AddScore_ThreeColorStraight(playerStatus, ctx);
                // 일기통관
                AddScore_Straight(playerStatus, ctx);
                // 혼전대요구
                AddScore_TerminalOrHonorInEachSet(playerStatus, ctx);
                // 또이또이
                AddScore_AllTripletHand(playerStatus, ctx);
                // 삼암각
                AddScore_ThreeClosedTriplets(playerStatus, ctx, openedCards);
                // 삼색동각
                AddScore_ThreeColorTriplets(playerStatus, ctx);
                // 소삼원
                AddScore_LittleThreeDragons(playerStatus, ctx);
                // 혼노두
                AddScore_AllTerminalsAndHonors(playerStatus, ctx);
                // 삼깡쯔
                AddScore_ThreeQuads(playerStatus);
            }

            // ---- 3판역 ----
            {
                // 이배구
                AddScore_TwoSetOfIdenticalSequences(playerStatus, ctx);
                // 순전대요구
                AddScore_TerminalInEachSet(playerStatus, ctx);
            }

            if (playerStatus.TotalHan > maxHan)
            {
                hanList = playerStatus.Han;
                maxHan = playerStatus.TotalHan;
                maxFu = playerStatus.Fu;
            }
        }

        playerStatus.Han = hanList;
        playerStatus.Fu = maxFu;
    }

    public void CalculateRon(PlayerStatus playerStatus,
        AgariContext agariContext,
        Card[] sealedCards,
        Card[] openedCards,
        object[] ctxs)
    {
        int[] hanList = new int[0];
        var maxHan = 0;
        var maxFu = 0;

        foreach (object[] ctx in ctxs)
        {
            playerStatus.InitializeHanFu();
            playerStatus.Fu = 20; // 기본 20부

            // ---- 1판역 ----
            {
                // 리치
                AddScore_Riichi(playerStatus);
                // 핑후
                AddScore_Pinfu(playerStatus, agariContext, ctx);
                // 이페코
                AddScore_OneSetOfIdenticalSequences(playerStatus, ctx);
                // 자패
                AddScore_WordCards(playerStatus, ctx);
                // 도라
                AddScore_Dora(playerStatus, sealedCards, openedCards);
            }

            // ---- 2판역 ----
            {
                // 삼색동순
                AddScore_ThreeColorStraight(playerStatus, ctx);
                // 일기통관
                AddScore_Straight(playerStatus, ctx);
                // 혼전대요구
                AddScore_TerminalOrHonorInEachSet(playerStatus, ctx);
                // 또이또이
                AddScore_AllTripletHand(playerStatus, ctx);
                // 삼암각
                AddScore_ThreeClosedTriplets(playerStatus, ctx, openedCards);
                // 삼색동각
                AddScore_ThreeColorTriplets(playerStatus, ctx);
                // 소삼원
                AddScore_LittleThreeDragons(playerStatus, ctx);
                // 혼노두
                AddScore_AllTerminalsAndHonors(playerStatus, ctx);
                // 삼깡쯔
                AddScore_ThreeQuads(playerStatus);
            }

            // ---- 3판역 ----
            {
                // 이배구
                AddScore_TwoSetOfIdenticalSequences(playerStatus, ctx);
                // 순전대요구
                AddScore_TerminalInEachSet(playerStatus, ctx);
            }

            if (playerStatus.TotalHan > maxHan)
            {
                hanList = playerStatus.Han;
                maxHan = playerStatus.TotalHan;
                maxFu = playerStatus.Fu;
            }
        }

        playerStatus.Han = hanList;
        playerStatus.Fu = maxFu;
    }

    void AddScore_MenzenTsumo(PlayerStatus playerStatus)
    {
        if (playerStatus.IsMenzen)
        {
            playerStatus.AddHan("MenzenTsumo", 1);
        }
    }


    void AddScore_Riichi(PlayerStatus playerStatus)
    {
        // 리치 화료
        if (playerStatus.IsRiichiMode)
        {
            playerStatus.AddHan("Riichi", 1);
        }

        // 일발
        if (playerStatus.IsOneShotRiichi)
        {
            playerStatus.AddHan("OneShotRiichi", 1);
        }

        //더블리치
        if (playerStatus.IsFirstOrder)
        {
            playerStatus.AddHan("DoubleRiichi", 2);
        }
    }

    void AddScore_Pinfu(PlayerStatus playerStatus, AgariContext agariContext, object[] ctx)
    {
        var head = GetHeadGlobalOrder(ctx);
        var ponCount = Ctx.ReadPonCount(ctx);

        // 커쯔가 없어야 한다(편의상 pon으로 표기한 것)
        if (ponCount == 0
            // 멘젠이어야 한다
            && playerStatus.IsMenzen
            // 양면대기여야 함
            && !agariContext.IsSingleWaiting
            // 머리가 역패가 아니어야 함
            && !HandUtil.IsWordCard(head)
            // 양면대기여야 함
            && agariContext.AgariableCount == 2 
            // 양면대기인데 슌쯔여야 함 23 있으면 1,4(3칸)을 기다리는 것
            && agariContext.AgariableCardGlobalOrders[0] == agariContext.AgariableCardGlobalOrders[0] + 3)
        {
            playerStatus.AddHan("Pinfu", 1);
        }
    }

    void AddScore_OneSetOfIdenticalSequences(PlayerStatus playerStatus, object[] ctx)
    {
        if (playerStatus.IsMenzen)
        {
            var chiCount = Ctx.ReadChiCount(ctx);
            var chiList = Ctx.ReadChiList(ctx);

            if (chiCount > 1 && GetSameChiCount(chiList, chiCount) == 2)
            {
                playerStatus.AddHan("OneSetOfIdenticalSequences", 1);
            }
        }
    }

    void AddScore_LastTileFromTheWall(PlayerStatus playerStatus, bool isLastTsumo)
    {
        if (isLastTsumo)
        {
            playerStatus.AddHan("LastTileFromTheWall", 1);
        }
    }

    void AddScore_AllSimples(PlayerStatus playerStatus, object[] ctx)
    {
        // 탕야오
        var ponList = Ctx.ReadPonList(ctx);
        var ponCount = Ctx.ReadPonCount(ctx);

        // 모든 커쯔에 요구패 없음
        for (var i = 0; i < ponCount; ++i)
        {
            if (HandUtil.IsYaojuhai(ponList[i]))
            {
                return;
            }
        }

        var chiCount = Ctx.ReadChiCount(ctx);
        var chiList = Ctx.ReadChiList(ctx);

        // 모든 슌쯔에 요구패 없음
        for (var i = 0; i < chiCount; ++i)
        {
            if (HandUtil.IsYaojuhai(chiList[i]))
            {
                return;
            }
        }

        // 머리가 요구패가 아님
        var head = GetHeadGlobalOrder(ctx);
        if (HandUtil.IsYaojuhai(head))
        {
            return;
        }

        playerStatus.AddHan("AllSimples", 1);
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
                playerStatus.AddHan("WhiteGreenRed", 1);
            }

            if (IsMyWindWordCard(playerStatus, pon))
            {
                playerStatus.AddHan("MyWindWord", 1);
            }

            if (IsCurrentWindWordCard(playerStatus, pon))
            {
                playerStatus.AddHan("CurrentRoundWind", 1);
            }
        }
    }

    // 영상개화
    void AddScore_DeadWallDraw(PlayerStatus playerStatus, bool isByRinshan)
    {
        if (isByRinshan)
        {
            playerStatus.AddHan("DeadWallDraw", 1);
        }
    }

    // 하저로어

    void AddScore_LastDiscard(PlayerStatus playerStatus, bool isLastDiscard)
    {
        if (isLastDiscard)
        {
            playerStatus.AddHan("LastDiscard", 1);
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

        playerStatus.AddHan("Dora", totalDoraCount);
    }

    //삼색동순
    void AddScore_ThreeColorStraight(PlayerStatus playerStatus, object[] ctx)
    {
        var chiCount = Ctx.ReadChiCount(ctx);
        var chiList = Ctx.ReadChiList(ctx);

        if (chiCount < 3)
        {
            return;
        }

        // 만(1,2,3) 만(3,4,5) 통(1,2,3) 삭(1,2,3)이 있다고 하자
        // chiList에는 이렇게 들어가있을 것임
        // chiList = [0, 2, 9, 18]
        var startNumbers = new int[chiCount];
        for (var i = 0; i < chiCount; ++i)
        {
            startNumbers[i] = chiList[i] % 9;
        }
        // 9 나머지 연산을 하면 이렇게 된다
        // startNumbers = [0, 2, 0, 0]

        // 같은 숫자가 3개인지 알아보기 위해
        // 1. 타겟 숫자를 찾고
        // 2. 해당 숫자를 제외한 배열을 돌면서 갯수를 센다
        for (var targetIndex = 0; targetIndex < chiCount; ++targetIndex)
        {
            var targetNumber = startNumbers[targetIndex];
            var sameChiCount = 1;

            for (var i = 0; i < chiCount; ++i)
            {
                // 현재 비교대상과 같은 인덱스면 벗어남
                if (targetIndex == i) { continue; }
                if (startNumbers[i] == targetNumber)
                {
                    ++sameChiCount;
                }
            }

            // 3개 있으면 삼색동순
            if (sameChiCount == 3)
            {
                var han = playerStatus.IsMenzen ? 2 : 1;
                playerStatus.AddHan("ThreeColorStraight", han);
                return;
            }
        }
    }

    void AddScore_Straight(PlayerStatus playerStatus, object[] ctx) 
    {
        // 일기통관
        var chiCount = Ctx.ReadChiCount(ctx);
        var chiList = Ctx.ReadChiList(ctx);

        if (chiCount < 3)
        {
            return;
        }

        // 만(1,2,3) 만(4,5,6) 통(7,8,9) 삭(1,2,3)이 있다고 하자
        // chiList에는 이렇게 들어가있을 것임
        // chiList = [0, 3, 6, 18]
        // chiList[0] + 3 == chiList[1] && chiList[1] + 3 == chiList[2] 이다

        for (var i = 0; i < chiCount - 2; ++i)
        {
            if (chiList[i] + 3 == chiList[i + 1] && chiList[i + 1]+3 == chiList[i + 2])
            {
                var han = playerStatus.IsMenzen ? 2 : 1;
                playerStatus.AddHan("Straight", han);
                return;
            }
        }
    }

    void AddScore_TerminalOrHonorInEachSet(PlayerStatus playerStatus, object[] ctx)
    {
        // 혼전대요구
        var ponList = Ctx.ReadPonList(ctx);
        var ponCount = Ctx.ReadPonCount(ctx);

        // 모든 커쯔가 요구패
        for (var i = 0; i < ponCount; ++i)
        {
            if (!HandUtil.IsYaojuhai(ponList[i]))
            {
                return;
            }
        }

        var chiCount = Ctx.ReadChiCount(ctx);
        var chiList = Ctx.ReadChiList(ctx);

        // 모든 슌쯔에 요구패가 하나 들어감
        for (var i = 0; i < chiCount; ++i)
        {
            var isStartsWithOne = chiList[i] % 9 == 0;
            var isEndsWithNine = chiList[i] % 9 == 6;
            if (!isStartsWithOne && !isEndsWithNine)
            {
                return;
            }
        }

        // 머리도 요구패
        var head = GetHeadGlobalOrder(ctx);
        if (!HandUtil.IsYaojuhai(head))
        {
            return;
        }

        var han = playerStatus.IsMenzen ? 2 : 1;
        playerStatus.AddHan("TerminalOrHonorInEachSet", han);
    }

    void AddScore_AllTripletHand(PlayerStatus playerStatus, object[] ctx)
    {
        var ponCount = Ctx.ReadPonCount(ctx);
        if (ponCount == 4)
        {
            playerStatus.AddHan("AllTripletHand", 2);
        }
    }
    
    void AddScore_ThreeClosedTriplets(PlayerStatus playerStatus, object[] ctx, Card[] openedCards)
    {
        var ponCount = Ctx.ReadPonCount(ctx);
        if (ponCount < 3)
        {
            return;
        }

        var ponList = Ctx.ReadPonList(ctx);
        var openedPonCount = 0;
        for (var i = 0; i < ponCount; ++i)
        {
            var isOpenedPon = false;

            foreach(var card in openedCards)
            {
                if (card.GlobalOrder == ponList[i])
                {
                    isOpenedPon = true;
                    break;
                }
            }

            if (isOpenedPon)
            {
                ++openedPonCount;
            }
        }

        if (openedPonCount <= 1)
        {
            playerStatus.AddHan("AllTripletHand", 2);
        }
    }

    void AddScore_ThreeColorTriplets(PlayerStatus playerStatus, object[] ctx)
    {
        var ponCount = Ctx.ReadPonCount(ctx);
        var ponList = Ctx.ReadPonList(ctx);

        if (ponCount < 3)
        {
            return;
        }

        // 만(1,1,1) 만(3,3,3) 통(1,1,1) 삭(1,1,1)이 있다고 하자
        // ponList에는 이렇게 들어가있을 것임
        // ponList = [0, 2, 9, 18]
        var startNumbers = new int[ponCount];
        for (var i = 0; i < ponCount; ++i)
        {
            startNumbers[i] = ponList[i] % 9;
        }
        // 9 나머지 연산을 하면 이렇게 된다
        // startNumbers = [0, 2, 0, 0]

        // 같은 숫자가 3개인지 알아보기 위해
        // 1. 타겟 숫자를 찾고
        // 2. 해당 숫자를 제외한 배열을 돌면서 갯수를 센다
        for (var targetIndex = 0; targetIndex < ponCount; ++targetIndex)
        {
            var targetNumber = startNumbers[targetIndex];
            var samePonCount = 1;

            for (var i = 0; i < ponCount; ++i)
            {
                // 현재 비교대상과 같은 인덱스면 벗어남
                if (targetIndex == i) { continue; }
                if (startNumbers[i] == targetNumber)
                {
                    ++samePonCount;
                }
            }

            // 3개 있으면 삼색동각
            if (samePonCount == 3)
            {
                playerStatus.AddHan("ThreeColorTriplets", 2);
                return;
            }
        }
    }

    void AddScore_LittleThreeDragons(PlayerStatus playerStatus, object[] ctx)
    {
        var ponCount = Ctx.ReadPonCount(ctx);
        var ponList = Ctx.ReadPonList(ctx);

        if (ponCount < 2)
        {
            return;
        }

        var dragonPonCount = 0;
        for (var i = 0; i < ponCount; ++i)
        {
            if (HandUtil.IsDragonCard(ponList[i]))
            {
                ++dragonPonCount;
            }
        }

        if (dragonPonCount != 2) 
        {
            return;
        }

        var head = GetHeadGlobalOrder(ctx);
        if (!HandUtil.IsDragonCard(head))
        {
            return;
        }

        playerStatus.AddHan("LittleThreeDragons", 2);
    }

    void AddScore_AllTerminalsAndHonors(PlayerStatus playerStatus, object[] ctx)
    {
        // 혼노두
        // 혼노두는 모두 커쯔임
        var chiCount = Ctx.ReadChiCount(ctx);
        if (chiCount > 0)
        {
            return;
        }

        var ponList = Ctx.ReadPonList(ctx);
        var ponCount = Ctx.ReadPonCount(ctx);

        // 모든 커쯔가 요구패
        for (var i = 0; i < ponCount; ++i)
        {
            if (!HandUtil.IsYaojuhai(ponList[i]))
            {
                return;
            }
        }

        // 머리도 요구패
        var head = GetHeadGlobalOrder(ctx);
        if (!HandUtil.IsYaojuhai(head))
        {
            return;
        }

        playerStatus.AddHan("AllTerminalsAndHonors", 2);
    }

    void AddScore_ThreeQuads(PlayerStatus playerStatus)
    {
        if (playerStatus.KkanCount == 3)
        {
            playerStatus.AddHan("ThreeQuads", 2);
        }
    }

    void AddScore_TwoSetOfIdenticalSequences(PlayerStatus playerStatus, object[] ctx)
    {
        if (!playerStatus.IsMenzen)
        {
            return;
        }

        // 이배구
        var chiCount = Ctx.ReadChiCount(ctx);
        if (chiCount != 4)
        {
            return;
        }

        var chiList = Ctx.ReadChiList(ctx);

        // 만(1,2,3) 만(3,4,5) 통(1,2,3) 통(3,4,5)가 있다고 하자
        // chiList에는 이렇게 들어가있을 것임
        // chiList = [0, 2, 9, 11]
        var startNumbers = new int[chiCount];
        for (var i = 0; i < chiCount; ++i)
        {
            startNumbers[i] = chiList[i] % 9;
        }
        // 9 나머지 연산을 하면 이렇게 된다
        // startNumbers = [0, 2, 0, 2]

        // 어느 대상을 잡고 돌아도, 같은게 2개 있을 것이다
        for (var targetIndex = 0; targetIndex < chiCount; ++targetIndex)
        {
            var targetNumber = startNumbers[targetIndex];
            var sameChiCount = 0;

            for (var i = 0; i < chiCount; ++i)
            {
                // 현재 비교대상과 같은 인덱스면 벗어남
                if (targetIndex == i) { continue; }
                if (startNumbers[i] == targetNumber)
                {
                    ++sameChiCount;
                }
            }

            if (sameChiCount != 2)
            {
                return;
            }
        }

        playerStatus.AddHan("TwoSetOfIdenticalSequences", 3);
    }

    void AddScore_TerminalInEachSet(PlayerStatus playerStatus, object[] ctx)
    {
        // 순전대요구
        var ponList = Ctx.ReadPonList(ctx);
        var ponCount = Ctx.ReadPonCount(ctx);

        // 모든 커쯔가 요구패
        for (var i = 0; i < ponCount; ++i)
        {
            if (!HandUtil.IsNoduhai(ponList[i]))
            {
                return;
            }
        }

        var chiCount = Ctx.ReadChiCount(ctx);
        var chiList = Ctx.ReadChiList(ctx);

        // 모든 슌쯔에 노두패가 하나 들어감
        for (var i = 0; i < chiCount; ++i)
        {
            var isStartsWithOne = chiList[i] % 9 == 0;
            var isEndsWithNine = chiList[i] % 9 == 6;
            if (!isStartsWithOne && !isEndsWithNine)
            {
                return;
            }
        }

        // 머리도 노두패
        var head = GetHeadGlobalOrder(ctx);
        if (!HandUtil.IsNoduhai(head))
        {
            return;
        }

        var han = playerStatus.IsMenzen ? 3 : 2;
        playerStatus.AddHan("TerminalInEachSet", han);
    }

    void AddScore_HalfFlush(PlayerStatus playerStatus, object[] ctx)
    {
        // 음 어떻게 하지...
    }

    bool IsWhiteGreenRed(int globalOrder)
    {
        var white = HandUtil.GetWordsStartGlobalOrder() + HandUtil.GetWhiteCardNumber() - 1;
        var red = HandUtil.GetWordsEndGlobalOrder();

        return white <= globalOrder && globalOrder <= red;
    }

    bool IsMyWindWordCard(PlayerStatus playerStatus, int globalOrder)
    {
        return IsSameDirectionCard(playerStatus.Wind, globalOrder);
    }

    bool IsCurrentWindWordCard(PlayerStatus playerStatus, int globalOrder)
    {
        return IsSameDirectionCard(playerStatus.RoundWind, globalOrder);
    }

    bool IsSameDirectionCard(string wind, int globalOrder)
    {
        switch (wind)
        {
            case "East":
                return globalOrder == HandUtil.GetWordsStartGlobalOrder() + HandUtil.GetEastCardNumber() - 1;
            case "North":
                return globalOrder == HandUtil.GetWordsStartGlobalOrder() + HandUtil.GetNorthCardNumber() - 1;
            case "South":
                return globalOrder == HandUtil.GetWordsStartGlobalOrder() + HandUtil.GetSouthCardNumber() - 1;
            case "West":
                return globalOrder == HandUtil.GetWordsStartGlobalOrder() + HandUtil.GetWestCardNumber() - 1;
            // 여긴 들어오면 안되지만..
            default:
                return false;
        }
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


