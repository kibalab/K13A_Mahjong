
using UdonSharp;
using UnityEngine;
using UnityEngine.UI;
using VRC.SDKBase;
using VRC.Udon;

public class ResultViewer : UdonSharpBehaviour
{
    public Text playerName;
    public Image[] resultCards;
    public Text winType;
    public Text[] resultYaku;
    public Text hanText;
    public Text fuText;
    public Image pointLevel;
    public Image[] pointLevels;

    [UdonSynced(UdonSyncMode.None)] public string NetworkMessage = "";

    private int messageNumber = 0; // 마스터 전용
    private int lastMessageNumber = -1; // 모든 유저용

        
    public void setResult(string agariType, string nickName, int count, string[] yakuKeyList, int[] hanList, int fu)
    {
        if (Networking.LocalPlayer != null)
        {
            if (Networking.LocalPlayer.IsOwner(gameObject))
            {
                NetworkMessage = SerializeResult(agariType, nickName, count, yakuKeyList, hanList, fu);
            }
        }

        playerName.text = nickName;
        winType.text = agariType;
        resultYaku[0].text = "";
        resultYaku[1].text = "";
        var hanTotal = 0;
        for(var i = 0; i < count; i++)
        {
            hanTotal += hanList[i];
            resultYaku[i % 2].text += $"{hanList[i]}판] {yakuKeyList[i]}\n";
            Debug.Log($"[ResultViewer] {yakuKeyList[i]}");
        }
        hanText.text = $"{hanTotal}판";
        fuText.text = $"{fu}부";

        setPointLevel(hanTotal, fu);
    }

    public void setPointLevel(int han, int fu)
    {
        //조건을 심플하게 걸기위해 부수를 판수의 소수점에 배치함
        var point = han + fu * 0.001;

        if(point >= 13)
        {
            //해아림 역만
            pointLevel.sprite = pointLevels[4].sprite;
        }
        else if (point >= 11)
        {
            //삼배만
            pointLevel.sprite = pointLevels[3].sprite;
        }
        else if (point >= 8)
        {
            //배만
            pointLevel.sprite = pointLevels[2].sprite;
        }
        else if (point >= 6)
        {
            //하네만
            pointLevel.sprite = pointLevels[1].sprite;
        }
        else if ( point >= 4.040 || point >= 3.070 && point < 4 )
        {
            //만관
            pointLevel.sprite = pointLevels[0].sprite;
        }
        else
        {
            //일반 화료
            pointLevel.sprite = null;
        }

    }

    public string SerializeResult(string agariType, string nickName, int count, string[] yakuKeyList, int[] hanList, int fu)
    {
        var serializedString = $"{messageNumber++}&{agariType}&{nickName}&{count}&{stringsToString(yakuKeyList)}&{intsToString(hanList)}&{fu}";
        return serializedString;
    }

    private void Update()
    {
        if (string.IsNullOrEmpty(NetworkMessage))
        {
            return;
        }

        var splited = NetworkMessage.Split('&');
        var networkMessageNumber = int.Parse(splited[0]);

        if (lastMessageNumber != networkMessageNumber)
        {
            lastMessageNumber = networkMessageNumber;
            var agariType = splited[1];
            var nickName = splited[2];
            var count = int.Parse(splited[3]);
            var yakuKeyList = splited[4].Split(',');
            var hanList_strings = splited[5].Split(',');
            var fu = int.Parse(splited[6]);

            var hanList = new int[hanList_strings.Length];
            for(var i = 0; i < hanList_strings.Length; i++)
            {
                hanList[i] = int.Parse(hanList_strings[i]);
            }
            setResult(agariType, nickName, count, yakuKeyList, hanList, fu);
        }
    }

    public string stringsToString(string[] list)
    {
        var strings = "";

        foreach (string str in list)
        {
            if (strings != "")
            {
                strings += ",";
            }
            strings += str;
        }

        return strings;
    }

    public string intsToString(int[] list)
    {
        var strings = "";

        foreach (int str in list)
        {
            if (strings != "")
            {
                strings += ",";
            }
            strings += str.ToString();
        }

        return strings;
    }
}
