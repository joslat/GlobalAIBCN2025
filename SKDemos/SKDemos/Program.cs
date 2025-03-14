using SKDemos;

Console.WriteLine("Hello, Semantic Kernel!");

// simple usage of SK
await BasicSK.Execute();

// tool usage in a chat loop
//await BasicSKChat.Execute();

// Simple agent
//await Agent01_Minion.Execute();

// Agent with some plugins
//await Agent02_MinionWithPlugins.Execute();

// Group chat with 2 agents collaborating in Critic Workflow
//await BlogPostWorkflow.Execute();