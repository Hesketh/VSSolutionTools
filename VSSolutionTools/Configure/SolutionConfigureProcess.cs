using Microsoft.Build.Construction;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;

namespace VSSolutionTools.Configure
{
    public static class SolutionConfigureProcess
    {
        public static void ProcessSolution(string solutionPath, string configurationPath)
        {
            var config = LoadConfigurationFromFile(configurationPath);
            if (config == null) { throw new ArgumentException($"The configuration at {configurationPath} is not valid", nameof(configurationPath)); }

            var solution = SolutionFile.Parse(solutionPath);
            if (solution == null) { throw new ArgumentException($"The solution at {solutionPath} is not valid", nameof(solutionPath)); }

            foreach (var solutionConfiguration in solution.SolutionConfigurations)
            {
                var defaultIncludeInBuild = config.DefaultBuildInfos?.FirstOrDefault(x =>
                                        x.Platform == solutionConfiguration.PlatformName &&
                                        x.Configuration == solutionConfiguration.ConfigurationName)?.Build ?? true;

                Console.WriteLine($"Configuring {solutionConfiguration.FullName} with default include in build as {defaultIncludeInBuild}");

                foreach (var project in solution.ProjectsInOrder)
                {
                    if (project.ProjectConfigurations.TryGetValue(solutionConfiguration.FullName, out var projectConfiguration))
                    {
                        var overrideConfig = config?.ProjectConfigurations?.FirstOrDefault(x => x.Name == project.ProjectName
                        && x.BuildInfo.Configuration == solutionConfiguration.ConfigurationName
                        && x.BuildInfo.Platform == solutionConfiguration.PlatformName)?.BuildInfo;

                        if (overrideConfig != null)
                        {
                            Console.WriteLine($"Configuring {project.ProjectName} with override include in build as {defaultIncludeInBuild}");
                        }
                        else
                        {
                            //And this is why we should have tested the api 
                            //projectConfiguration.IncludeInBuild = defaultIncludeInBuild;
                        }
                    }
                }
            }
            
        }

        public static void GenerateConfiguration(string configurationPath)
        {
            const string ExampleProjectName = "ExampleProject";
            const string Configuration_Debug = "Debug";
            const string Configuration_Release = "Release";
            const string Platform_64Bit = "x64";
            const string Platform_32Bit = "x86";

            try
            {
                using FileStream fs = new(configurationPath, FileMode.CreateNew);
                var serializer = new DataContractSerializer(typeof(SolutionConfiguration));
                var configuration = new SolutionConfiguration()
                {
                    DefaultBuildInfos = new List<BuildInfo>
                    {
                        new BuildInfo(Platform_64Bit, Configuration_Debug, true),
                        new BuildInfo(Platform_64Bit, Configuration_Release, true),
                        new BuildInfo(Platform_32Bit, Configuration_Debug, true),
                        new BuildInfo(Platform_32Bit, Configuration_Release, true)
                    },

                    ProjectConfigurations = new List<ProjectConfiguration>
                    {
                        // Example Project where we say it can't be build in x64
                        new ProjectConfiguration
                        {
                            Name = ExampleProjectName,
                            BuildInfo = new BuildInfo(Platform_64Bit, Configuration_Debug, false)
                        },
                        new ProjectConfiguration
                        {
                            Name = ExampleProjectName,
                            BuildInfo = new BuildInfo(Platform_64Bit, Configuration_Release, false)
                        }
                    }
                };

                serializer.WriteObject(fs, configuration);
                Console.WriteLine($"Written configuration file to {configurationPath}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed writing configuration file to {configurationPath}");
                Console.WriteLine(ex.Message);
            }
        }

        private static SolutionConfiguration? LoadConfigurationFromFile(string filepath)
        {
            try
            {
                using FileStream fs = new(filepath, FileMode.Open);
                var serializer = new DataContractSerializer(typeof(SolutionConfiguration));
                return serializer.ReadObject(fs) as SolutionConfiguration;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed reading configuration file from {filepath}");
                Console.WriteLine(ex.ToString());
                return null;
            }
        }
    }
}
