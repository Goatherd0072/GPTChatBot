using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public TMP_InputField tMPInput_InputField;
    public Button btn_sendButton;
    public GameObject go_textBox;
    public Transform spawnRoot;
    public NLPBaseHelper nLPBaseHelper;
    public string _inputText;

    void Awake()
    {
        tMPInput_InputField.onSubmit.AddListener(SendData);
        btn_sendButton.onClick.AddListener(() => { SendData(tMPInput_InputField.text); });
    }

    void SendData(string inputText)
    {
        _inputText = inputText;
        AddChatBox("user", _inputText);
        ChatBox chatBox = AddChatBox("AI", "");
        // xFChat.SendData(_inputText, chatBox.SetText);
        nLPBaseHelper.PostMessage(_inputText, chatBox.SetText);
        tMPInput_InputField.text = null;
    }

    ChatBox AddChatBox(string role, string text)
    {
        GameObject go = Instantiate(go_textBox, spawnRoot);
        ChatBox chatBox = go.GetComponentInChildren<ChatBox>();
        chatBox.rect_Layout = spawnRoot as RectTransform;
        chatBox.SetContent(role, text);
        
        return chatBox;
    }
}
