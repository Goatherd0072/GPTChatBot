using TMPro;
using UnityEngine;
using UnityEngine.UI;


/// <summary>
/// 调用示例
/// </summary>
public class Sample : MonoBehaviour
{
    public TMP_InputField tmpInputField;
    public Button btnSendButton;
    public GameObject goTextBox;
    public Transform spawnRoot;
    ChatBox tempChatBox;

    [Header("chat API相关")]
    public NLPBaseHelper nlpBaseHelper;
    public string inputText;

    [Header("情绪识别相关")]
    public bool needEmotionAnalyze;
    public EmotionAnalyzeAli emotionAnalyze;
    public TMP_Text emotionText;

    void Awake()
    {
        tmpInputField.onSubmit.AddListener(SendData);
        btnSendButton.onClick.AddListener(() => { SendData(tmpInputField.text); });

        ///将调用事件注册到nlpHelper中
        nlpBaseHelper.onReceiveMessage.AddListener(SetChatBoxUI);
        nlpBaseHelper.onReceiveMessage.AddListener(EmotionAnalyze);
    }

    /// <summary>
    /// 发送数据
    /// </summary>
    /// <param name="inputText"></param>
    void SendData(string inputText)
    {
        this.inputText = inputText;
        AddChatBox("user", this.inputText);
        tempChatBox = AddChatBox("AI", "");
        // xFChat.SendData(_inputText, chatBox.SetText);
        nlpBaseHelper.PostMessage(this.inputText);
        tmpInputField.text = null;
    }

    /// <summary>
    /// 根据传入数据设置显示文本
    /// </summary>
    /// <param name="text"></param>
    private void SetChatBoxUI(string text)
    {
        if (tempChatBox == null)
            tempChatBox = AddChatBox("AI", "");

        tempChatBox.SetText(text);
        tempChatBox = null;
    }

    ChatBox AddChatBox(string role, string text)
    {
        GameObject go = Instantiate(goTextBox, spawnRoot);
        ChatBox chatBox = go.GetComponentInChildren<ChatBox>();
        chatBox.rect_Layout = spawnRoot as RectTransform;
        chatBox.SetContent(role, text);

        return chatBox;
    }

    /// <summary>
    /// 根据传入数据显示情绪分析文本
    /// </summary>
    /// <param name="context"></param>
    private void EmotionAnalyze(string context)
    {
        if (!needEmotionAnalyze)
            return;

        if (emotionAnalyze == null || emotionText == null)
        {
            return;
        }
        var (emotion, allEmotionJson) = emotionAnalyze.GetEmotion(context);
        // Debug.Log(output);
        emotionText.text = emotion;
        Debug.Log($"情绪分析结果：{allEmotionJson}");
    }
}
