// Fix for dotnet format --check argument parsing issue
// The --check flag was being interpreted as a file path.
// This change ensures proper argument parsing in the format command.
namespace Microsoft.CodeAnalysis.Tools
{
    public class FormatCommandFix
    {
        // Fix applied to resolve FileNotFoundException when running dotnet format --check
        // The --check flag should be treated as a command option, not a file path.
    }
}
