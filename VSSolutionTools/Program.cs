using Microsoft.Build.Construction;
using Microsoft.Build.Evaluation;
using System.CommandLine;
using System.Text;
using VSSolutionTools;

//var root = args.Length > 0 ? args[0] : Directory.GetCurrentDirectory();

var directoryOption = new Option<DirectoryInfo?>(
    name: "--directory",
    description: "The root directory of your solutions and projects");

var rootCommand = new RootCommand("Output a CSV showing all Projects and which Solutions reference them");
rootCommand.AddOption(directoryOption);

rootCommand.SetHandler((directoryInfo) =>
{
    ProjectSolutionReport.GenerateReport(directoryInfo!.FullName);
}, directoryOption);

return await rootCommand.InvokeAsync(args);