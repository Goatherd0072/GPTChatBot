using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public abstract class NLPBaseHelper : MonoBehaviour
{
    /// <summary>
    /// 发生的信息list
    /// </summary> 
    public List<ChatContent> sendDataList = new();
    public EmotionAnalyzeAli emotionAnalyze;
    public TMP_Text emotionText;

    /// <summary>
    /// 初始化模型配置
    /// </summary>
    public abstract void Init();
    public abstract void PostMessage(string message, Action<string> callback);

    // public abstract IEnumerator RequestMessage(Action<string> callback);
    protected void CheckSendLength(int maxHistory)
    {
        if (maxHistory < 0)
            return;

        while (sendDataList.Count > maxHistory)
        {
            sendDataList.RemoveAt(1);
        }
    }
    public void EmotionAnalyze(string context)
    {
        var (emotion, allEmotionJson) = emotionAnalyze.GetEmotion(context);
        // Debug.Log(output);
        emotionText.text = emotion;
        Debug.Log($"情绪分析结果：{allEmotionJson}");
    }
}

[Serializable]
public class ChatContent
{
    public string role;
    public string content;

    public ChatContent(string r, string c)
    {
        role = r;
        content = c;
    }

    public ChatContent() { }
}
