using GBX.NET;
using GBX.NET.Engines.Game;
using GBX.NET.LZO;
using TmEssentials;

namespace GbxMedalTimeModifier;

internal static class Program
{
    private const string DefaultPattern = "*.Gbx";

    private static int Main(string[] args)
    {
        if (args.Length == 0 || HasHelpFlag(args))
        {
            PrintUsage();
            return 0;
        }

        if (!TryParseOptions(args, out CliOptions options, out string? parseError))
        {
            Console.Error.WriteLine(parseError);
            Console.Error.WriteLine();
            PrintUsage();
            return 1;
        }

        Gbx.LZO = new Lzo();

        return options.Batch ? RunBatch(options) : RunSingle(options);
    }

    private static bool HasHelpFlag(string[] args)
    {
        foreach (string arg in args)
        {
            string normalized = arg.ToLowerInvariant();
            if (normalized is "-h" or "--help" or "/?")
            {
                return true;
            }
        }

        return false;
    }

    private static void PrintUsage()
    {
        Console.WriteLine("GbxMedalTimeModifier");
        Console.WriteLine();
        Console.WriteLine("Single map:");
        Console.WriteLine("  GbxMedalTimeModifier.exe <inputMapPath> <outputMapPath> <AT> <Gold> <Silver> <Bronze>");
        Console.WriteLine();
        Console.WriteLine("Batch mode:");
        Console.WriteLine("  GbxMedalTimeModifier.exe --batch <inputDir> <outputDir> <AT> <Gold> <Silver> <Bronze> [--recursive] [--pattern <glob>]");
        Console.WriteLine();
        Console.WriteLine("Notes:");
        Console.WriteLine("  - Use '_' to leave a medal unchanged.");
        Console.WriteLine("  - Use 'auto' for Gold/Silver/Bronze to generate times from Author.");
        Console.WriteLine("  - <AT> cannot be 'auto'.");
        Console.WriteLine("  - If <inputMapPath> is a directory, batch mode is enabled automatically.");
        Console.WriteLine($"  - Default batch pattern is '{DefaultPattern}'.");
    }

    private static bool TryParseOptions(string[] args, out CliOptions options, out string? error)
    {
        options = null!;
        error = null;

        bool batchRequested = false;
        bool recursive = false;
        string pattern = DefaultPattern;
        List<string> positional = new();

        for (int i = 0; i < args.Length; i++)
        {
            string arg = args[i];
            string normalized = arg.ToLowerInvariant();

            switch (normalized)
            {
                case "--batch":
                case "-b":
                    batchRequested = true;
                    break;
                case "--recursive":
                case "-r":
                    recursive = true;
                    break;
                case "--pattern":
                    if (i + 1 >= args.Length)
                    {
                        error = "Missing value for --pattern.";
                        return false;
                    }

                    pattern = args[++i];
                    break;
                default:
                    if (arg.StartsWith("--pattern=", StringComparison.OrdinalIgnoreCase))
                    {
                        pattern = arg.Substring("--pattern=".Length);
                        break;
                    }

                    if (arg.StartsWith("-", StringComparison.Ordinal))
                    {
                        error = $"Unsupported option: {arg}";
                        return false;
                    }

                    positional.Add(arg);
                    break;
            }
        }

        if (positional.Count != 6)
        {
            error = "Expected exactly 6 positional arguments: <input> <output> <AT> <Gold> <Silver> <Bronze>.";
            return false;
        }

        string inputPath = positional[0];
        string outputPath = positional[1];
        string atArg = positional[2];
        string goldArg = positional[3];
        string silverArg = positional[4];
        string bronzeArg = positional[5];

        bool inferredBatch = Directory.Exists(inputPath);
        bool batch = batchRequested || inferredBatch;

        if (batchRequested && !Directory.Exists(inputPath))
        {
            error = $"Batch mode requires an existing input directory: {inputPath}";
            return false;
        }

        if (batch && string.IsNullOrWhiteSpace(pattern))
        {
            error = "Batch pattern cannot be empty.";
            return false;
        }

        options = new CliOptions(batch, recursive, pattern, inputPath, outputPath, atArg, goldArg, silverArg, bronzeArg);
        return true;
    }

    private static int RunSingle(CliOptions options)
    {
        string inputMap = options.InputPath;
        string outputMap = options.OutputPath;

        if (Directory.Exists(outputMap))
        {
            outputMap = Path.Combine(outputMap, Path.GetFileName(inputMap));
        }

        Console.WriteLine($"Processing map: {inputMap}");

        if (!TryProcessMap(inputMap, outputMap, options, out string? error))
        {
            Console.Error.WriteLine($"Failed: {error}");
            return 1;
        }

        Console.WriteLine($"Saved: {outputMap}");
        return 0;
    }

    private static int RunBatch(CliOptions options)
    {
        if (!Directory.Exists(options.InputPath))
        {
            Console.Error.WriteLine($"Input directory does not exist: {options.InputPath}");
            return 1;
        }

        string inputDir = options.InputPath;
        string outputDir = options.OutputPath;

        Directory.CreateDirectory(outputDir);

        SearchOption searchOption = options.Recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;
        string[] files = Directory.EnumerateFiles(inputDir, options.Pattern, searchOption).ToArray();

        if (files.Length == 0)
        {
            Console.WriteLine($"No files matched pattern '{options.Pattern}' in '{inputDir}'.");
            return 0;
        }

        int succeeded = 0;
        int failed = 0;

        Console.WriteLine($"Batch mode: {files.Length} map(s) matched.");

        for (int i = 0; i < files.Length; i++)
        {
            string inputMap = files[i];
            string relative = Path.GetRelativePath(inputDir, inputMap);
            string outputMap = Path.Combine(outputDir, relative);

            string? outputParent = Path.GetDirectoryName(outputMap);
            if (!string.IsNullOrWhiteSpace(outputParent))
            {
                Directory.CreateDirectory(outputParent);
            }

            Console.WriteLine($"[{i + 1}/{files.Length}] {relative}");

            if (TryProcessMap(inputMap, outputMap, options, out string? error))
            {
                succeeded++;
                continue;
            }

            failed++;
            Console.Error.WriteLine($"  Failed: {error}");
        }

        Console.WriteLine();
        Console.WriteLine($"Batch finished. Success: {succeeded}, Failed: {failed}, Total: {files.Length}");

        return failed == 0 ? 0 : 1;
    }

    private static bool TryProcessMap(string inputMap, string outputMap, CliOptions options, out string? error)
    {
        error = null;

        string atArg = options.AtArg.ToLowerInvariant();
        string goldArg = options.GoldArg.ToLowerInvariant();
        string silverArg = options.SilverArg.ToLowerInvariant();
        string bronzeArg = options.BronzeArg.ToLowerInvariant();

        if (atArg == "auto")
        {
            error = "<AT> (Author time) cannot be 'auto'.";
            return false;
        }

        try
        {
            var gbx = Gbx.Parse<CGameCtnChallenge>(inputMap);
            var map = gbx.Node;

            if (atArg != "_")
            {
                if (!int.TryParse(atArg, out int newAuthorMs))
                {
                    error = $"AT value '{options.AtArg}' is invalid.";
                    return false;
                }

                map.AuthorTime = new TimeInt32(newAuthorMs);
                Console.WriteLine($"  AT -> {newAuthorMs}");
            }
            else
            {
                Console.WriteLine("  AT unchanged.");
            }

            int authorTimeMs = map.AuthorTime?.TotalMilliseconds ?? 0;
            bool anyAuto = goldArg == "auto" || silverArg == "auto" || bronzeArg == "auto";

            if (anyAuto && authorTimeMs <= 0)
            {
                error = "Cannot use 'auto' for medals without a valid Author time.";
                return false;
            }

            if (!TryHandleMedalArg(map.GoldTime, goldArg, authorTimeMs, 1.06, "Gold", out TimeInt32? goldTime, out error))
            {
                return false;
            }

            if (!TryHandleMedalArg(map.SilverTime, silverArg, authorTimeMs, 1.20, "Silver", out TimeInt32? silverTime, out error))
            {
                return false;
            }

            if (!TryHandleMedalArg(map.BronzeTime, bronzeArg, authorTimeMs, 1.50, "Bronze", out TimeInt32? bronzeTime, out error))
            {
                return false;
            }

            map.GoldTime = goldTime;
            map.SilverTime = silverTime;
            map.BronzeTime = bronzeTime;

            string? parentDir = Path.GetDirectoryName(outputMap);
            if (!string.IsNullOrWhiteSpace(parentDir))
            {
                Directory.CreateDirectory(parentDir);
            }

            gbx.Save(outputMap);
            return true;
        }
        catch (Exception ex)
        {
            error = ex.Message;
            return false;
        }
    }

    private static bool TryHandleMedalArg(TimeInt32? current, string arg, int authorMs, double factor, string label, out TimeInt32? updated, out string? error)
    {
        error = null;
        updated = current;

        if (arg == "_")
        {
            Console.WriteLine($"  {label} unchanged.");
            return true;
        }

        if (arg == "auto")
        {
            int autoValue = CalculateAutoMedal(authorMs, factor);
            Console.WriteLine($"  {label} -> AUTO ({autoValue})");
            updated = new TimeInt32(autoValue);
            return true;
        }

        if (!int.TryParse(arg, out int explicitMs))
        {
            error = $"{label} value '{arg}' is invalid.";
            return false;
        }

        Console.WriteLine($"  {label} -> {explicitMs}");
        updated = new TimeInt32(explicitMs);
        return true;
    }

    private static int CalculateAutoMedal(int authorMs, double factor)
    {
        double raw = (authorMs * factor + 1000.0) / 1000.0;
        int floored = (int)Math.Floor(raw);
        return floored * 1000;
    }

    private sealed record CliOptions(
        bool Batch,
        bool Recursive,
        string Pattern,
        string InputPath,
        string OutputPath,
        string AtArg,
        string GoldArg,
        string SilverArg,
        string BronzeArg);
}
