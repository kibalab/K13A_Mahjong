
using UdonSharp;
using UnityEngine;
using UnityEngine.UI;
using VRC.SDKBase;
using VRC.Udon;

public class SubtitleComponent : UdonSharpBehaviour
{
    public bool isShow = false;
    public int id;
    public string name;
    public string text;
    public float playTime = 3.0f;

    private float fadeOutPoint = 0.5f;

    private Text UItext;

    void Start()
    {
        Initializing();
        setActiveText(false);
    }

    public void Initializing()
    {
        UItext = GetComponent<Text>();
    }
    
    public void Set(string name, string text, float showTime)
    {
        this.name = name;
        this.text = text;
        this.playTime = showTime;

        UpdateOnUI();
    }

    void UpdateOnUI()
    {
        transform.SetAsFirstSibling();
        UItext.text = $"<color=orange><b>{name}</b></color> : {text}";
        Toggle();
    }

    void Toggle()
    {
        isShow = !isShow;
        setActiveText(isShow);
    }
    
    void setActiveText(bool t)
    {
        UItext.enabled = t;
    }

    void Update()
    {
        if (!isShow) return;
        playTime -= UnityEngine.Time.deltaTime;


        if(playTime <= 0)
        {
            Toggle();
        }
    }
}
