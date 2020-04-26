using System;
using System.IO;
using Plazacraft.HOMM3.DamageSymulator.Core;
using System.Data;

namespace Plazacraft.HOMM3.DamageSymulator.Console
{
    class Program
    {

        static void WriteHelp()
        {
            string help = File.ReadAllText("help.txt");
            System.Console.Write(help);
        }
        static void Main(string[] args)
        {

            if (args.Length < 3)
            {
                WriteHelp();
                return;
            }



            string definition = File.ReadAllText(args[0]);
            string fight = File.ReadAllText(args[1]);

            World world = new World(definition);
            StringWriter log = new StringWriter();
            StringWriter outStream = new StringWriter();

            FightSimulation simulation = new FightSimulation(world, fight);
            FightSimulationResult res = simulation.Run(log);

            log.Flush();
            System.Console.WriteLine(log.ToString());


            string outFile = "out.";
            switch(args[2])
            {
                case "csv":
                    outFile += "csv";
                    res.WriteToCsv(outStream);
                break;

                case "html":
                    outFile += "html";
                    res.WriteToHtml(outStream);
                break;

                case "htmlchart":
                    outFile += "htmlchart";
                    res.WriteToHtmlChart(outStream);
                break;

                default:
                    outFile = String.Empty;
                break;
            }
            if (!String.IsNullOrEmpty(outFile))
            {
                outStream.Flush();
                File.WriteAllText(outFile, outStream.ToString());
            }
        }
    }
}
