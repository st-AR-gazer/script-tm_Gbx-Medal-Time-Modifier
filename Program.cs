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
                Console.WriteLine("Use '_' for any medal time you don't want to change.");
                return;
            }

            Gbx.LZO = new Lzo();

            string inputMap = args[0];
            string outputMap = args[1];
            string atArg = args[2];
            string goldArg = args[3];
            string silverArg = args[4];
            string bronzeArg = args[5];

            try
            {
                var gbx = Gbx.Parse<CGameCtnChallenge>(inputMap);
                var map = gbx.Node;

                if (atArg != "_" && int.TryParse(atArg, out int atTime))
                {
                    map.AuthorTime = new TimeInt32(atTime);
                    Console.WriteLine($"AT time updated to {atTime}");
                }
                else
                {
                    Console.WriteLine("AT time not changed.");
                }

                if (goldArg != "_" && int.TryParse(goldArg, out int goldTime))
                {
                    map.GoldTime = new TimeInt32(goldTime);
                    Console.WriteLine($"Gold time updated to {goldTime}");
                }
                else
                {
                    Console.WriteLine("Gold time not changed.");
                }

                if (silverArg != "_" && int.TryParse(silverArg, out int silverTime))
                {
                    map.SilverTime = new TimeInt32(silverTime);
                    Console.WriteLine($"Silver time updated to {silverTime}");
                }
                else
                {
                    Console.WriteLine("Silver time not changed.");
                }

                if (bronzeArg != "_" && int.TryParse(bronzeArg, out int bronzeTime))
                {
                    map.BronzeTime = new TimeInt32(bronzeTime);
                    Console.WriteLine($"Bronze time updated to {bronzeTime}");
                }
                else
                {
                    Console.WriteLine("Bronze time not changed.");
                }

                gbx.Save(outputMap);
                Console.WriteLine($"Modified map saved to {outputMap}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
            }
        }
    }
}
