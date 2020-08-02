using UdonSharp;
using UnityEngine;
using UnityEngine.UI;
using VRC.SDKBase;
using VRC.Udon;
using VRC.Udon.Common.Interfaces;

//게임 마스터가 자막 표시를 명령하는부분
public class Subtitle : UdonSharpBehaviour
{
    // 마스터 전용
    private int messageNumber = 0;

    // 모든 유저용
    private int lastMessageNumber = -1;

    [UdonSynced(UdonSyncMode.None)] public string NetworkMessage = "";
    [UdonSynced(UdonSyncMode.None)] public float playtime = 3.0f;

    public SubtitleComponent[] components;

    // 마스터가 호출
    public void SetSubtitle(string name, string text)
    {
        NetworkMessage = Serialize(name, text);
    }

    string Serialize(string name, string text)
    {
        var serializedString = $"{messageNumber++},{name},{text}";
        return serializedString;
    }

    public void SetPlaytime(float playtime)
    {
        this.playtime = playtime;
    }

    void Update()
    {
        if (string.IsNullOrEmpty(NetworkMessage))
        {
            return;
        }

        var splited = NetworkMessage.Split(',');
        var networkMessageNumber = int.Parse(splited[0]);
        
        if (lastMessageNumber != networkMessageNumber)
        {
            lastMessageNumber = networkMessageNumber;
            var name = splited[1];
            var text = splited[2];

            components = gameObject.GetComponentsInChildren<SubtitleComponent>();
            components[components.Length - 1].Set(name, text, playtime);
        }
    }
}
