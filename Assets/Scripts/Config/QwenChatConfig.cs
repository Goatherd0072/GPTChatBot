using UnityEngine;

[CreateAssetMenu(fileName = "QwenChatConfig", menuName = "NLPConfig/QwenChatConfig")]
public class QwenChatConfig : NLPConfig
{
    QwenChatConfig()
    {
        prompt = "使用中文回答,并移除回答中的Markdown标示";
        modelName = "通义千问";
    }

    public string url;
    public string api_key ;
    public string qwenModel = "qwen-max";

    void OnValidate()
    {

    }
}
