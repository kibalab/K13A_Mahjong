
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class NormalYaku : UdonSharpBehaviour
{
    [SerializeField] public HandUtil HandUtil;

    public bool CheckTenpai(CalculatingContextHandler Ctx, object[] ctxs, AgariContext agariContext, int[] globalOrders)
    {
        foreach (object[] ctx in ctxs)
        {
            if (ctx == null) { continue; }

            var remainsGlobalOrders = Ctx.ReadGlobalOrders(ctx);
            var pairs = HandUtil.FindPairs(remainsGlobalOrders);

            foreach (var pair in pairs)
            {
                remainsGlobalOrders[pair] -= 2;
            }

            var bodies = Ctx.ReadChiCount(ctx) + Ctx.ReadPonCount(ctx);

            // 몸 4, 카드 1인 경우 -> 단면대기 텐파이            
            if (bodies == 4 && pairs.Length == 0)
            {
                agariContext.IsSingleWaiting = true;
                for (var i = 0; i < remainsGlobalOrders.Length; ++i)
                {
                    if (remainsGlobalOrders[i] > 0)
                    {
                        agariContext.AddAgariableGlobalOrder(i);

                        Debug.Log($"몸4 카드 1, 단면대기 텐파이 GlobalOrder:{i}");
                        break;
                    }
                }
            }
            // 몸 3, 머리 2인 경우 -> 양면대기 텐파이
            else if (bodies == 3 && pairs.Length == 2)
            {
                agariContext.IsSingleWaiting = false;
                agariContext.AddAgariableGlobalOrder(pairs[0]);
                agariContext.AddAgariableGlobalOrder(pairs[1]);

                Debug.Log($"몸4 머리 2, 양면대기 텐파이 GlobalOrder:{pairs[0]}, {pairs[1]}");
            }
            // 몸 3, 머리 1, 카드2개인 경우 -> 단면 or 양면 or 대기아님
            else if (bodies == 3 && pairs.Length == 1)
            {
                // 몸2 머리1에 34567인 경우는, 2 5 8로 삼면대기가 되는데 다음 경우로 분해가능
                // 1. 머리1, 몸2 + (345), (6, 7)남음 → 5, 8 양면대기
                // 2. 머리1, 몸2 + (567), (3, 4)남음 → 2, 5 양면대기
                // 따라서 남은 카드가 chiable한지 판단해보아야 함
                //  - 2, 4같이 한칸 떨어져 있다던지
                //  - 2, 3같이 붙어 있다던지

                for (var i = 0; i < 34 - 1; ++i)
                {
                    if (remainsGlobalOrders[i] == 1 && remainsGlobalOrders[i + 1] == 1)
                    {
                        agariContext.AddAgariableGlobalOrder(i);
                        agariContext.AddAgariableGlobalOrder(i + 1);
                        agariContext.IsSingleWaiting = false;

                        Debug.Log($"몸3 머리 1 카드 2, 양면대기 텐파이 GlobalOrder:{i}, {i + 1}");
                        break;
                    }

                    if (i > 0 && remainsGlobalOrders[i - 1] == 1 && remainsGlobalOrders[i + 1] == 1)
                    {
                        agariContext.AddAgariableGlobalOrder(i);
                        agariContext.IsSingleWaiting = true;

                        Debug.Log($"몸3 머리 1 카드 2, 단면대기 텐파이 GlobalOrder:{i}");
                        break;
                    }
                }
            }
        }

        return agariContext.AgariableCount != 0;
    }
}
