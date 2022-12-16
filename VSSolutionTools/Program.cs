using System.CommandLine;
using VSSolutionTools;

// Build Options
var directoryOption = new Option<DirectoryInfo?>(
    name: "-directory",
    description: "The root directory of your solutions and projects");
directoryOption.AddAlias("-d");
directoryOption.SetDefaultValue(new DirectoryInfo(Directory.GetCurrentDirectory()));

// Build Commands
var rootCommand = new RootCommand("Perform bulk operations across solutions");

// Generate Report
var reportCommand = new Command("report", "Output a CSV showing all Projects and which Solutions reference them");
reportCommand.AddOption(directoryOption);
reportCommand.SetHandler((directoryInfo) =>
{
    ProjectSolutionReport.GenerateReport(directoryInfo!.FullName);
}, directoryOption);
rootCommand.AddCommand(reportCommand);

// Actually Run the App
return await rootCommand.InvokeAsync(args);