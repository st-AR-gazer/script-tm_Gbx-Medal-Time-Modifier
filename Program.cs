using System;
using GBX.NET;
using GBX.NET.Engines.Game;
using GBX.NET.LZO;
using TmEssentials;

namespace GbxMedalTimeModifier
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length < 6)
            {
                Console.WriteLine("Usage: GbxMedalTimeModifier <inputMapPath> <outputMapPath> <AT> <Gold> <Silver> <Bronze>");
                Console.WriteLine("Use '_' to leave a medal unchanged.");
                Console.WriteLine("Use 'auto' for Gold/Silver/Bronze to generate times from the Author medal.");
                Console.WriteLine("NOTE: <AT> CANNOT be 'auto'.");
                return;
            }

            Gbx.LZO = new Lzo();

            string inputMap   = args[0];
            string outputMap  = args[1];
            string atArg      = args[2].ToLowerInvariant();
            string goldArg    = args[3].ToLowerInvariant();
            string silverArg  = args[4].ToLowerInvariant();
            string bronzeArg  = args[5].ToLowerInvariant();

            if (atArg == "auto")
            {
                Console.WriteLine("Error: <AT> (Author Time) cannot be 'auto'.");
                return;
            }

            try
            {
                var gbx = Gbx.Parse<CGameCtnChallenge>(inputMap);
                var map = gbx.Node;

                if (atArg != "_" && int.TryParse(atArg, out int newAuthor))
                {
                    map.AuthorTime = new TimeInt32(newAuthor);
                    Console.WriteLine($"AT time updated to {newAuthor}");
                }
                else
                {
                    Console.WriteLine("AT time not changed.");
                }

                int authorTimeMs = map.AuthorTime?.TotalMilliseconds ?? 0;

                if (authorTimeMs <= 0 && (goldArg == "auto" || silverArg == "auto" || bronzeArg == "auto"))
                {
                    Console.WriteLine("Error: cannot use 'auto' without a valid Author time.");
                    return;
                }

                map.GoldTime   = HandleMedalArg(map.GoldTime,   goldArg,   authorTimeMs, 1.06, "Gold");
                map.SilverTime = HandleMedalArg(map.SilverTime, silverArg, authorTimeMs, 1.20, "Silver");
                map.BronzeTime = HandleMedalArg(map.BronzeTime, bronzeArg, authorTimeMs, 1.50, "Bronze");

                gbx.Save(outputMap);
                Console.WriteLine($"Modified map saved to {outputMap}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
            }
        }

        private static TimeInt32? HandleMedalArg(TimeInt32? current, string arg,
                                                 int authorMs, double factor, string label)
        {
            if (arg == "_")
            {
                Console.WriteLine($"{label} time not changed.");
                return current;
            }

            if (arg == "auto")
            {
                int autoVal = CalculateMedal(authorMs, factor);
                Console.WriteLine($"{label} time set to AUTO ({autoVal})");
                return new TimeInt32(autoVal);
            }

            if (int.TryParse(arg, out int explicitMs))
            {
                Console.WriteLine($"{label} time updated to {explicitMs}");
                return new TimeInt32(explicitMs);
            }

            Console.WriteLine($"{label} argument '{arg}' invalid – leaving unchanged.");
            return current;
        }

        private static int CalculateMedal(int authorMs, double factor)
        {
            double raw  = (authorMs * factor + 1000.0) / 1000.0;
            int floored = (int)Math.Floor(raw);
            return floored * 1000;
        }
    }
}