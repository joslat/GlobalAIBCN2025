using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents;
using Microsoft.SemanticKernel.Agents.Chat;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Plugins.Web.Bing;
using Microsoft.SemanticKernel.Plugins.Web;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace SKDemos
{
    /// <summary>
    /// This sample demonstrates a two-agent group chat using the new Semantic Kernel syntax.
    /// The writer agent is tasked with creating a blog post article about the Global AI Barcelona event.
    /// The critic agent reviews the blog post and signals approval by including "approve" in its response.
    /// </summary>
    public static class BlogPostWorkflow
    {
        private const string CriticName = "CriticAgent";
        private const string CriticInstructions =
            """
            You are an expert critic with years of experience reviewing blog posts.
            Evaluate the blog post article for clarity, engagement, factual accuracy, and style.
            Always begin your response with the iteration number and the blog post content.
            If the blog post meets your high standards, conclude your critique with the word "approve".
            Otherwise, provide concise suggestions for improvement.
            Also, always provide feedback on the first critic, even if the blog post is perfect. Think very carefully and provide feedback to improve the article no matter what for the first iteration.
            """;

        private const string WriterName = "WriterAgent";
        private const string WriterInstructions =
            """
            You are a seasoned writer with expertise in technology and events.
            Your task is to write an engaging and informative blog post article about a provided topic.
            The article should include a captivating title, a brief introduction, main content sections, and a conclusion.
            Before starting writing you will search the web for information about the topic to retrieve relevant information.
            Provide a complete draft in one response, and incorporate creativity and clarity throughout.
            Also you can use the Bing search engine to find information about the topic.
            You will receive critic feedback and you will revise the article accordingly.
            """;

        public static async Task Execute()
        {
            Kernel kernel = InitializeKernel();


            // Define the critic agent.
            ChatCompletionAgent agentCritic = new()
            {
                Instructions = CriticInstructions,
                Name = CriticName,
                Kernel = kernel,
                Arguments = new KernelArguments(
                    new PromptExecutionSettings()
                    {
                        FunctionChoiceBehavior = FunctionChoiceBehavior.Auto()
                    }),
            };

            // Define the writer agent.
            ChatCompletionAgent agentWriter = new()
            {
                Instructions = WriterInstructions,
                Name = WriterName,
                Kernel = kernel,
                Arguments = new KernelArguments(
                    new PromptExecutionSettings()
                    {
                        FunctionChoiceBehavior = FunctionChoiceBehavior.Auto()
                    }),
            };

            // Create a chat for agent interaction.
            AgentGroupChat chat = new(agentWriter, agentCritic)
            {
                ExecutionSettings = new()
                {
                    // The termination strategy stops the conversation when the critic's message contains "approve".
                    TerminationStrategy = new ApprovalTerminationStrategy()
                    {
                        Agents = new List<ChatCompletionAgent> { agentCritic },
                        MaximumIterations = 10,
                    }
                }
            };

            // Input message to kick off the blog post creation.
            ChatMessageContent input = new(AuthorRole.User,
                "Please write a blog post article about the Global AI Barcelona event. " +
                "The article should be engaging and informative, include a captivating title, " +
                "an introduction, main content sections, and a conclusion tailored for a tech-savvy audience.");
            chat.AddChatMessage(input);
            DisplayMessage(input);

            // Invoke the chat and display messages as they come.
            await foreach (ChatMessageContent response in chat.InvokeAsync())
            {
                DisplayMessage(response);
            }

            Console.WriteLine($"\n[IS COMPLETED: {chat.IsComplete}]");
        }

        private sealed class ApprovalTerminationStrategy : TerminationStrategy
        {
            // Terminate the conversation when the latest message includes the term "approve"
            protected override Task<bool> ShouldAgentTerminateAsync(Agent agent, IReadOnlyList<ChatMessageContent> history, CancellationToken cancellationToken)
                => Task.FromResult(history[history.Count - 1].Content?.Contains("approve", StringComparison.OrdinalIgnoreCase) ?? false);
        }

        private static void DisplayMessage(ChatMessageContent message)
        {
            Console.WriteLine($"# {message.Role}: {message.Content}");
        }

        private static Kernel InitializeKernel()
        {
            var modelDeploymentName = "gpt-4o";
            var azureOpenAIEndpoint = Environment.GetEnvironmentVariable("AZUREOPENAI_ENDPOINT");
            var azureOpenAIApiKey = Environment.GetEnvironmentVariable("AZUREOPENAI_APIKEY");

            Kernel kernel = Kernel.CreateBuilder()
                .AddAzureOpenAIChatCompletion(
                    modelDeploymentName,
                    azureOpenAIEndpoint!,
                    azureOpenAIApiKey!)
                .Build();

            var bingApiKey = Environment.GetEnvironmentVariable("Bing_ApiKey");
            BingConnector bing = new BingConnector(bingApiKey!);
            kernel.ImportPluginFromObject(new WebSearchEnginePlugin(bing), "bing");

            return kernel;
        }


    }
}
