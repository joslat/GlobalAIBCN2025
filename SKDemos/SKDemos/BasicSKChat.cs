﻿using Microsoft.SemanticKernel.Connectors.OpenAI;
using Microsoft.SemanticKernel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.SemanticKernel.ChatCompletion;

namespace SKDemos;

public static class BasicSKChat
{
    public static async Task Execute()
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
        var promptPlugin = CreatePromptPlugin.CreatePromptPluginWithFunctions(kernel);
        kernel.Plugins.Add(promptPlugin);

        var chatService = kernel.GetRequiredService<IChatCompletionService>();
        ChatHistory chatHistory = new();

        var executionSettings = new OpenAIPromptExecutionSettings
        {
            ToolCallBehavior = ToolCallBehavior.AutoInvokeKernelFunctions
        };

        bool exitnow = false;
        while (exitnow == false)
        {
            Console.WriteLine("Enter your question or type 'exit' to quit:");
            var userInput = Console.ReadLine();
            if (userInput == "exit")
            {
                Console.WriteLine($"Banana!!");
                exitnow = true;
            }
            else
            {
                chatHistory.AddUserMessage(userInput);

                var response = await chatService.GetChatMessageContentAsync(
                    chatHistory,
                    executionSettings,
                    kernel);

                Console.WriteLine(response.ToString());
                chatHistory.Add(response);
            }
        }
    }
}
