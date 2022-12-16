using Microsoft.Build.Construction;
using System.Text;

namespace VSSolutionTools
{
    internal static class ProjectSolutionReport
    {
        public static void GenerateReport(string directoryPath)
        {
            var solutions = Directory.GetFiles(directoryPath, "*.sln", SearchOption.AllDirectories);
            var projects = Directory.GetFiles(directoryPath, "*.csproj", SearchOption.AllDirectories);

            // Absolute paths for both
            var solutionsByProject = new Dictionary<string, HashSet<string>>();
            var parsedSolutions = new HashSet<string>();

            // Build up our lookup of projects with what solutions they are contained in
            Console.WriteLine($"Parsing {solutions.Length} Solutions and {projects.Length} Projects found in {directoryPath}");
            foreach (var solutionPath in solutions)
            {
                var solutionAbsolutePath = Path.GetFullPath(solutionPath);

                var solutionFile = SolutionFile.Parse(solutionPath);
                if (solutionFile == null)
                {
                    Console.WriteLine($"\tCould not parse solution at {solutionFile}");
                    continue;
                }

                foreach (var project in solutionFile.ProjectsInOrder)
                {
                    if (!solutionsByProject.TryGetValue(project.AbsolutePath, out var solutionsContainingProject))
                    {
                        solutionsContainingProject = new();
                        solutionsByProject.Add(project.AbsolutePath, solutionsContainingProject);
                    }
                    solutionsContainingProject.Add(solutionAbsolutePath);
                }

                parsedSolutions.Add(solutionAbsolutePath);
                Console.WriteLine($"\tParsed solution at {solutionFile}");
            }

            // Output a CSV
            var csvStringBuilder = new StringBuilder();
            var destination = Path.Combine(directoryPath, "CSProjSolutionReport.csv");
            Console.WriteLine($"Generating CSV Report at {destination}");

            // Build Header
            csvStringBuilder.Append("project");
            foreach (var solution in parsedSolutions)
            {
                csvStringBuilder.Append(',');
                csvStringBuilder.Append(solution);
            }
            csvStringBuilder.AppendLine();

            // Output the data
            foreach (var project in projects)
            {
                if (!solutionsByProject.TryGetValue(project, out var solutionsWithProject))
                {
                    solutionsWithProject = new();
                }

                csvStringBuilder.Append(project);
                foreach (var solution in parsedSolutions)
                {
                    csvStringBuilder.Append(',');
                    csvStringBuilder.Append(solutionsWithProject.Contains(solution));
                }
                csvStringBuilder.AppendLine();
            }

            File.WriteAllText(destination, csvStringBuilder.ToString());
        }
    }
}
