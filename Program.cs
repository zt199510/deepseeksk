using System.Data.Common;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.Ollama;

#pragma warning disable SKEXP0010
var endpoint = new Uri("http://你的ollama地址:11434");
var modelId = "deepseek-r1:14b";
var builder = Kernel.CreateBuilder();
builder.Services.AddScoped<HttpClient>();
builder.Services.AddScoped<SearchSkill>();
#pragma warning disable SKEXP0070 

builder.AddOllamaChatCompletion(modelId, endpoint);
var kernel = builder.Build();
var chatService = kernel.GetRequiredService<IChatCompletionService>();
var chatHistory = new ChatHistory();
var searchService = kernel.GetRequiredService<SearchSkill>();
string query = "基于 vue-pure-admin 这个模板，做一个表格页面";
List<SearchResult> result = await searchService.SearchAsync(query);
if (!result.Any())
{
    chatHistory.AddSystemMessage("抱歉，未找到相关搜索结果。我会基于已有知识继续为您服务。");
}
else
{
    chatHistory.AddSystemMessage($"已为您找到 {result.Count()} 条相关结果：");
    foreach (var item in result)
        chatHistory.AddSystemMessage($"• {item.Title}\n  {item.Snippet}");
}
chatHistory.AddUserMessage(query);
Console.WriteLine(result);
await foreach (var item in chatService.GetStreamingChatMessageContentsAsync(chatHistory))
{
    Console.Write(item.Content);
}