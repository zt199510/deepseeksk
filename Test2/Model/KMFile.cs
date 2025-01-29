
namespace DeepseekRAG.Model;

public class KMFile
{
    public string DocumentId { get; set; }
    public string Text { get; set; }
    public string? Url { get; set; }
    public string LastUpdate { get; set; }
    public string File { get; set; }
    
    // 新增相关度得分属性
    public double RelevanceScore { get; set; }
    
    // 新增格式化输出方法
    public string ToFormattedString()
    {
        return $"""
        📄 文档ID: {DocumentId}
        📂 文件: {File}
        ⏱️ 更新时间: {LastUpdate}
        🔗 URL: {Url ?? "N/A"}
        🔍 相关度: {RelevanceScore:P1}
        📝 内容摘要: {Text.Truncate(150)}...
        --------------------------
        """;
    }
}

// 字符串扩展方法
public static class StringExtensions
{
    public static string Truncate(this string value, int maxLength) 
        => string.IsNullOrEmpty(value) ? value : value.Length <= maxLength ? value : value[..maxLength];
}
