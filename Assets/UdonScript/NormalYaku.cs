
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

            // �� 4, ī�� 1�� ��� -> �ܸ��� ������            
            if (bodies == 4 && pairs.Length == 0)
            {
                agariContext.IsSingleWaiting = true;
                for (var i = 0; i < remainsGlobalOrders.Length; ++i)
                {
                    if (remainsGlobalOrders[i] > 0)
                    {
                        agariContext.AddAgariableGlobalOrder(i);

                        Debug.Log($"��4 ī�� 1, �ܸ��� ������ GlobalOrder:{i}");
                        break;
                    }
                }
            }
            // �� 3, �Ӹ� 2�� ��� -> ����� ������
            else if (bodies == 3 && pairs.Length == 2)
            {
                agariContext.IsSingleWaiting = false;
                agariContext.AddAgariableGlobalOrder(pairs[0]);
                agariContext.AddAgariableGlobalOrder(pairs[1]);

                Debug.Log($"��4 �Ӹ� 2, ����� ������ GlobalOrder:{pairs[0]}, {pairs[1]}");
            }
            // �� 3, �Ӹ� 1, ī��2���� ��� -> �ܸ� or ��� or ���ƴ�
            else if (bodies == 3 && pairs.Length == 1)
            {
                // ��2 �Ӹ�1�� 34567�� ����, 2 5 8�� ����Ⱑ �Ǵµ� ���� ���� ���ذ���
                // 1. �Ӹ�1, ��2 + (345), (6, 7)���� �� 5, 8 �����
                // 2. �Ӹ�1, ��2 + (567), (3, 4)���� �� 2, 5 �����
                // ���� ���� ī�尡 chiable���� �Ǵ��غ��ƾ� ��
                //  - 2, 4���� ��ĭ ������ �ִٴ���
                //  - 2, 3���� �پ� �ִٴ���

                for (var i = 0; i < 34 - 1; ++i)
                {
                    if (remainsGlobalOrders[i] == 1 && remainsGlobalOrders[i + 1] == 1)
                    {
                        agariContext.AddAgariableGlobalOrder(i);
                        agariContext.AddAgariableGlobalOrder(i + 1);
                        agariContext.IsSingleWaiting = false;

                        Debug.Log($"��3 �Ӹ� 1 ī�� 2, ����� ������ GlobalOrder:{i}, {i + 1}");
                        break;
                    }

                    if (i > 0 && remainsGlobalOrders[i - 1] == 1 && remainsGlobalOrders[i + 1] == 1)
                    {
                        agariContext.AddAgariableGlobalOrder(i);
                        agariContext.IsSingleWaiting = true;

                        Debug.Log($"��3 �Ӹ� 1 ī�� 2, �ܸ��� ������ GlobalOrder:{i}");
                        break;
                    }
                }
            }
        }

        return agariContext.AgariableCount != 0;
    }
}
