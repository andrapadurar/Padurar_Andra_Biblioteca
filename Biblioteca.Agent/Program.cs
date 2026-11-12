
using System.Text.Json;
using Microsoft.Extensions.AI;
using ModelContextProtocol.Client;
using ModelContextProtocol.Protocol;
using OpenAI;
using System.ClientModel;

// Biblioteca.Agent
// 1 = Inspector MCP
// 2 = Agent Ollama + MCP

const string McpEndpoint = "https://localhost:7245/mcp";
const string OllamaEndpoint = "http://localhost:11434/v1";
const string Model = "llama3.2";

// Conectare la serverul MCP
var transport = new HttpClientTransport(new HttpClientTransportOptions
{
    Endpoint = new Uri(McpEndpoint)
});

await using McpClient mcp = await McpClient.CreateAsync(transport);

IList<McpClientTool> tools = await mcp.ListToolsAsync();

Console.Write("1 Inspector ; 2 Agent > ");

if (Console.ReadLine() == "1")
{
    await Inspector();
}
else
{
    await Agent();
}

// Misiunea 1 - Inspector MCP
async Task Inspector()
{
    // Afisam instrumentele disponibile si schemele JSON
    foreach (var t in tools)
    {
        Console.WriteLine($"\n== {t.Name}\n{t.Description}");

        Console.WriteLine(JsonSerializer.Serialize(
            t.JsonSchema,
            new JsonSerializerOptions
            {
                WriteIndented = true
            }));
    }

    // Testul existent: search_books
    Console.Write("\nCautati in catalog (search_books): ");

    var query = Console.ReadLine() ?? "";

    CallToolResult result = await mcp.CallToolAsync(
        "search_books",
        new Dictionary<string, object?>
        {
            ["query"] = query
        });

    Console.WriteLine($"IsError: {result.IsError}");

    foreach (var block in result.Content.OfType<TextContentBlock>())
    {
        Console.WriteLine(block.Text);
    }

    // Laboratorul 9 - S2, varianta B2
    // Test valid: top = 5
    Console.WriteLine("\n=== most_ordered_books: top = 5 ===");

    CallToolResult validResult = await mcp.CallToolAsync(
        "most_ordered_books",
        new Dictionary<string, object?>
        {
            ["top"] = 5
        });

    Console.WriteLine($"IsError: {validResult.IsError}");

    foreach (var block in validResult.Content.OfType<TextContentBlock>())
    {
        Console.WriteLine(block.Text);
    }

    // Test invalid: top = 50
    Console.WriteLine("\n=== most_ordered_books: top = 50 ===");

    try
    {
        CallToolResult invalidResult = await mcp.CallToolAsync(
            "most_ordered_books",
            new Dictionary<string, object?>
            {
                ["top"] = 50
            });

        Console.WriteLine($"IsError: {invalidResult.IsError}");

        foreach (var block in invalidResult.Content.OfType<TextContentBlock>())
        {
            Console.WriteLine(block.Text);
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Eroare MCP: {ex.Message}");
    }
}

// Misiunea 2 - Agentul cu Ollama
async Task Agent()
{
    IChatClient chat = new ChatClientBuilder(
            new OpenAIClient(
                new ApiKeyCredential("ollama"),
                new OpenAIClientOptions
                {
                    Endpoint = new Uri(OllamaEndpoint)
                })
                .GetChatClient(Model)
                .AsIChatClient())
        .UseFunctionInvocation()
        .Build();

    List<ChatMessage> messages =
    [
        new(
            ChatRole.System,
            "Esti asistentul Bibliotecii FSEGA. " +
            "Raspunzi in limba romana. " +
            "Pentru orice informatie despre carti, cititori " +
            "sau recomandari foloseste tool-urile MCP. " +
            "Nu inventa titluri, preturi sau autori. " +
            "Daca tool-urile nu returneaza informatia, " +
            "spune ca nu o ai.")
    ];

    var options = new ChatOptions
    {
        Tools = [.. tools]
    };

    Console.WriteLine(
        $"Tool-uri disponibile: {string.Join(", ", tools.Select(t => t.Name))}");

    while (true)
    {
        Console.Write("\nTu: ");

        var question = Console.ReadLine();

        if (string.IsNullOrWhiteSpace(question))
        {
            break;
        }

        messages.Add(new ChatMessage(ChatRole.User, question));

        ChatResponse response =
            await chat.GetResponseAsync(messages, options);

        messages.AddMessages(response);

        foreach (var call in response.Messages
            .SelectMany(m => m.Contents)
            .OfType<FunctionCallContent>())
        {
            var args = string.Join(
                ", ",
                (call.Arguments ?? new Dictionary<string, object?>())
                .Select(a => $"{a.Key}={a.Value}"));

            Console.WriteLine(
                $"  [tool] {call.Name}({args})");
        }

        Console.WriteLine($"Asistent: {response.Text}");
    }
}
