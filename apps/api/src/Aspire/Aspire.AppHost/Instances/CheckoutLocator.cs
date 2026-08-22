using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Milese.Aspire.AppHost.Instances;

public static class CheckoutLocator
{
    public const string MainCheckoutName = "main";

    private const string GitEntryName = ".git";
    private const string GitDirectoryPrefix = "gitdir:";
    private const string WorktreesDirectoryName = "worktrees";

    public static CheckoutIdentity Resolve(string startDirectory)
    {
        var directory = new DirectoryInfo(startDirectory);

        while (directory is not null)
        {
            var gitEntry = Path.Combine(directory.FullName, GitEntryName);

            if (Directory.Exists(gitEntry))
                return MainCheckout(directory.FullName, WorktreeNamesIn(Path.Combine(gitEntry, WorktreesDirectoryName)));

            if (File.Exists(gitEntry))
                return LinkedWorktree(directory.FullName, ReadGitDirectory(gitEntry));

            directory = directory.Parent;
        }

        return MainCheckout(startDirectory, []);
    }

    private static CheckoutIdentity MainCheckout(string rootPath, IReadOnlyCollection<string> worktreeNames) =>
        new()
        {
            Name = MainCheckoutName,
            IsMainCheckout = true,
            RootPath = rootPath,
            WorktreeNames = worktreeNames,
        };

    private static CheckoutIdentity LinkedWorktree(string rootPath, string gitDirectory)
    {
        var worktreesDirectory = Path.GetDirectoryName(gitDirectory) ?? string.Empty;

        return new CheckoutIdentity
        {
            Name = Path.GetFileName(gitDirectory),
            IsMainCheckout = false,
            RootPath = rootPath,
            WorktreeNames = WorktreeNamesIn(worktreesDirectory),
        };
    }

    private static string ReadGitDirectory(string gitFilePath)
    {
        var content = File.ReadAllText(gitFilePath).Trim();

        var gitDirectory = content.StartsWith(GitDirectoryPrefix, StringComparison.Ordinal)
            ? content[GitDirectoryPrefix.Length..].Trim()
            : content;

        return gitDirectory.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
    }

    private static IReadOnlyCollection<string> WorktreeNamesIn(string worktreesDirectory)
    {
        if (!Directory.Exists(worktreesDirectory))
            return [];

        return Directory
            .EnumerateDirectories(worktreesDirectory)
            .Select(Path.GetFileName)
            .OfType<string>()
            .ToList();
    }
}
