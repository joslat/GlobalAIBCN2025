using Microsoft.SemanticKernel.Connectors.OpenAI;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents;
using Microsoft.SemanticKernel.ChatCompletion;
using System;
using OpenAI.Assistants;

namespace SKDemos;

public static class Agent01_Minion
{
    public static async Task Execute()
    {
        Kernel kernel = InitializeKernel();

        var minionAgent = InitializeMinionAgent(kernel);

        ChatHistory chat = [];
        Console.WriteLine("Enter a message to send to the agent or type 'exit' to quit:");
        var userMessage = "";

        while (true)
        {
            Console.Write("User: ");
            userMessage = Console.ReadLine();
            if (userMessage == "exit")
            {
                Console.WriteLine($"Banana!!");
                break;
            }

            ChatMessageContent message = new(AuthorRole.User, userMessage);
            chat.Add(message);

            await foreach (ChatMessageContent response in minionAgent.InvokeAsync(chat))
            {
                chat.Add(response);

                DisplayMessage(response, minionAgent);
            }
        }
    }

    private static void DisplayMessage(ChatMessageContent message, ChatCompletionAgent? agent = null)
    {
        //Console.WriteLine($"[{message.Id}]");
        if (agent != null)
        {
            

            if (message.Role != AuthorRole.User)
            {
                if (message.Role == AuthorRole.Assistant)
                {
                    Console.WriteLine($"Minion: {message.Content}");
                }
                else
                    Console.WriteLine($"# {message.Role}: ({agent.Name}) {message.Content}");
            }
        }
        else
        {
            Console.WriteLine($"# {message.Role}: {message.Content}");
        }
    }

    private static Kernel InitializeKernel()
    {
        var modelDeploymentName = "gpt-4o";
        var azureOpenAIEndpoint = Environment.GetEnvironmentVariable("AZUREOPENAI_ENDPOINT");
        var azureOpenAIApiKey = Environment.GetEnvironmentVariable("AZUREOPENAI_APIKEY");

        Kernel kernel = Kernel.CreateBuilder()
            .AddAzureOpenAIChatCompletion(
                modelDeploymentName,
                azureOpenAIEndpoint,
                azureOpenAIApiKey)
            .Build();

        kernel.ImportPluginFromType<WhatDateIsIt>();

        return kernel;
    }


    private static ChatCompletionAgent InitializeMinionAgent(Kernel kernel)
    {
        // Define the agent
        ChatCompletionAgent minionAgent =
            new()
            {
                Name = "Minion",
                Instructions = $"You are a cheerful and mischievous minion whose main goal is to entertain and amuse your master, the user. " +
                $"You should:\r\n " +
                "* Engage in light-hearted and humorous conversations.\r\n  " +
                "* Use Minionese language, mixing gibberish with words from various languages, and frequently use phrases like \"banana\" or mimic typical minion laughter.\r\n  " +
                "* React to the user’s inputs with enthusiasm, always aiming to uplift their mood and create a fun interaction.\r\n " +
                "* When in doubt, or if asked something serious, divert back to your playful nature, perhaps by saying something like \"Banana?\" or just laughing.\r\n  " +
                "* Be sure to respond in the language you are talked to.\r\n  " +
                "* Remember to always be loyal to your master, the user, and bring joy to their day.\r\n  " +
                "* Remember to properly respond to the question in a way that, aside funny, also makes sense and is coherent.\r\n  * Remember that you are a minion, so you should not be able to perform complex tasks or provide serious advice.\r\n  * Minions dont talk too long, so keep your responses short and fun. One or two sentences are usually enough.",
                Kernel = kernel
            };

        return minionAgent;
    }



}
