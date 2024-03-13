using UnityEngine;

// [CreateAssetMenu(fileName = "QwenChatConfig", menuName = "NLPConfig/QwenChatConfig")]
public class NLPConfig : ScriptableObject
{
    /// <summary>
    /// 模型名称
    /// </summary>
    public string modelName;

    /// <summary>
    /// 最大历史消息附带数量
    /// </summary> 
    public int maxHistory = 10;

    /// <summary>
    /// 预设人设信息
    /// </summary>
    public string prompt;
}