using System.CommandLine;
using VSSolutionTools;
using VSSolutionTools.Configure;

// Build Options
var directoryOption = new Option<DirectoryInfo?>(
    name: "-directory",
    description: "The root directory of your solutions and projects");
directoryOption.AddAlias("-d");
directoryOption.SetDefaultValue(new DirectoryInfo(Directory.GetCurrentDirectory()));

var solutionOption = new Option<FileInfo?>(
    name: "-solution",
    description: "Your Visual Studio Solution file");
solutionOption.AddAlias("-sln");

var solutionConfigOption = new Option<FileInfo?>(
    name: "-solutionConfig",
    description: "The root directory of your solutions and projects");
solutionConfigOption.AddAlias("-slncfg");

var generateConfigFileOption = new Option<bool>(
    name: "-generate",
    description: "Instead of running the configuration operation, generate a configuration file");
generateConfigFileOption.AddAlias("-g");
generateConfigFileOption.SetDefaultValue(false);

// Build Commands
var rootCommand = new RootCommand("Perform bulk operations on or across solutions");

// Generate Report
var reportCommand = new Command("report", "Output a CSV showing all Projects and which Solutions reference them");
reportCommand.AddOption(directoryOption);
reportCommand.SetHandler((directoryInfo) => ProjectSolutionReport.GenerateReport(directoryInfo!.FullName), directoryOption);
rootCommand.AddCommand(reportCommand);

// Set Platforms and Build Targets
var configureTargetsCommand = new Command("configure", "Configure the Platforms and Configurations for projects within a solution through a configuration file");
configureTargetsCommand.AddOption(solutionOption);
configureTargetsCommand.AddOption(solutionConfigOption);
configureTargetsCommand.AddOption(generateConfigFileOption);
configureTargetsCommand.SetHandler((solutionFile, solutionConfig, generateConfig) =>
{
    if (generateConfig)
    {
        // TODO: Maybe this could generate a config based on the current visual studio configuration setup?
        SolutionConfigureProcess.GenerateConfiguration(solutionConfig!.FullName);
    }
    else
    {
        SolutionConfigureProcess.ProcessSolution(solutionFile!.FullName, solutionConfig!.FullName);
    }
}, solutionOption, solutionConfigOption, generateConfigFileOption);
rootCommand.AddCommand(configureTargetsCommand);

// Actually Run the App
return await rootCommand.InvokeAsync(args);