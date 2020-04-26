using System.Collections.Generic;
using System.IO;

namespace Plazacraft.HOMM3.DamageSymulator.Core
{
    public class FightSimulationResult
    {

        public class Info
        {
            public CreatureRef Attacker
            { get; set; }
            public CreatureRef Defender
            { get; set; }

            public FightResult Strike
            { get; set; }

            public FightResult CounterMin
            { get; set; }

            public FightResult CounterMax
            { get; set; }

            public FightResult CounterAvg
            { get; set; }

            public FightFinalResult FinalMin
            { get; set;}
            public FightFinalResult FinalMax
            { get; set;}
            public FightFinalResult FinalAvg
            { get; set;}
        }

        private List<Info> _data = new List<Info>();
        public List<Info> Data 
        { get { return _data; } }

        public void Add(Info info)
        {
            _data.Add(info);
        }

        public void WriteToCsv(TextWriter writer)
        {
            writer.WriteLine("Attacker;Defender;"
                +"Attack Min Damage; Attack Max Damage; Attack Avg Damage; Attack Min Count; Attack Max Count;Attack Avg Count;"
                +"Counterstrike Min-Min Damage; Counterstrike Min-Max Damage;Counterstrike Min-Avg Damage;Counterstrike Min-Min Count; Counterstrike Min-Max Count; Conterstrike Min-Avg Count;"
                +"Counterstrike Max-Min Damage;Counterstrike Max-Max Damage;Counterstrike Max-Avg Damage;Counterstrike Max-Min Count;Counterstrike Max-Max Count;Counterstrike Max-Avg Count;"
                +"Counterstrike Avg-Min Damage;Counterstrike Avg-Max Damage;Counterstrike Avg-Avg Damage;Counterstrike Avg-Min Count;Counterstrike Avg-Max Count;Counterstrike Avg-Avg Count;"
                
                +"Final Min-Attacker;Final Min-Defender;Final Min-Rounds;"
                +"Final Max-Attacker;Final Max-Defender;Final Max-Rounds;"
                +"Final Avg-Attacker;Final Avg-Defender;Final Avg-Rounds;"

                );
                
            foreach (Info info in _data)
            {
                writer.Write("{0}({2});{1}({3});", info.Attacker.Name, info.Defender.Name, info.Attacker.Count, info.Defender.Count);
                writer.Write("{0};{1};{2};{3};{4};{5};", info.Strike.DamageMin, info.Strike.DamageMax, info.Strike.DamageAvg, info.Strike.KilledMin, info.Strike.KilledMax, info.Strike.KilledAvg);
                writer.Write("{0};{1};{2};{3};{4};{5};", info.CounterMin.DamageMin, info.CounterMin.DamageMax, info.CounterMin.DamageAvg, info.CounterMin.KilledMin, info.CounterMin.KilledMax, info.CounterMin.KilledAvg);
                writer.Write("{0};{1};{2};{3};{4};{5};", info.CounterMax.DamageMin, info.CounterMax.DamageMax, info.CounterMax.DamageAvg, info.CounterMax.KilledMin, info.CounterMax.KilledMax, info.CounterMax.KilledAvg);
                writer.Write("{0};{1};{2};{3};{4};{5};", info.CounterAvg.DamageMin, info.CounterAvg.DamageMax, info.CounterAvg.DamageAvg, info.CounterAvg.KilledMin, info.CounterAvg.KilledMax, info.CounterAvg.KilledAvg);

                writer.Write("{0};{1};{2}", info.FinalMin.AttackerLeft, info.FinalMin.DefenderLeft, info.FinalMin.Rounds);
                writer.Write("{0};{1};{2}", info.FinalMax.AttackerLeft, info.FinalMax.DefenderLeft, info.FinalMax.Rounds);
                writer.Write("{0};{1};{2}", info.FinalAvg.AttackerLeft, info.FinalAvg.DefenderLeft, info.FinalAvg.Rounds);
                writer.WriteLine();
            }
        }

        public void WriteToHtmlChart(TextWriter writer)
        {
            writer.WriteLine("<table>");

            if (_data != null && _data.Count > 0)
            {
                writer.WriteLine("<tr><th></th>");
                CreatureRef currentAttacker = _data[0].Attacker;
                foreach (Info info in _data)
                {
                    // read only columns for first row
                    if (info.Attacker.Name != currentAttacker.Name)
                        break;
                    writer.Write("<th>{0}({1})</th>", info.Defender.Name, info.Defender.Count);
                }

                writer.Write("<th>Wins</th><th>Losts</th>");
                currentAttacker = null;
                int win=0;
                int lost = 0;

                foreach (Info info in _data)
                {
                    if (currentAttacker == null || info.Attacker.Name != currentAttacker.Name)
                    {
                        if (currentAttacker != null)
                        {
                            writer.Write("<td>{0}</td>", win);
                            writer.Write("<td>{0}</td>", lost);
                        }
                        writer.WriteLine("</tr><tr>");
                        currentAttacker = info.Attacker;
                        win = lost = 0;
                        writer.Write("<td>{0}({1})</td>", currentAttacker.Name, currentAttacker.Count);
                    }


                    if (info.FinalAvg.DefenderLeft == 0)
                    {
                        writer.Write("<td class='win'>{0}</td>", info.FinalAvg.Rounds);
                        win ++;
                    }
                    else
                    {
                        writer.Write("<td class='lost'>{0}</td>", info.FinalAvg.Rounds);
                        lost ++;
                    }

                }

                writer.Write("<td>{0}</td>", win);
                writer.Write("<td>{0}</td>", lost);
                writer.WriteLine("</tr>");

            }

            writer.WriteLine("</table>");
        }
        public void WriteToHtml(TextWriter writer)
        {
            writer.WriteLine("<table>");
            writer.WriteLine("<tr><th>Attacker</th><th>Defender</th><th>Attack Damage</th><th>Attack Count</th>"
                            +"<th>Counterstrike Count (For Min Attack)</th>"
                            +"<th>Counterstrike Count (For Max Attack)</th>"
                            +"<th>Counterstrike Count (For Avg Attack)</th>"
                            +"<th>Final result</th></tr>");
            foreach (Info info in _data)
            {
                writer.Write("<tr>");
                writer.Write("<td>{0}({2})</td><td>{1}({3})</td>", info.Attacker.Name, info.Defender.Name, info.Attacker.Count, info.Defender.Count);
                writer.Write("<td>{0}-{1}({4})</td><td>{2}-{3}({5})</td>", info.Strike.DamageMin, info.Strike.DamageMax, info.Strike.KilledMin, info.Strike.KilledMax, info.Strike.DamageAvg, info.Strike.KilledAvg);
                //writer.Write("<td><div class='optimistic'>{0}</div>-{1}({2})</td>", info.CounterMin.DamageMin, info.CounterMin.DamageMax, info.CounterMin.DamageAvg);
                writer.Write("<td>{0}-<div class='pesimistic'>{1}</div>({2})</td>", info.CounterMin.KilledMin, info.CounterMin.KilledMax, info.CounterMin.KilledAvg);
                //writer.Write("<td><div class='optimistic'>{0}</div>-{1}({2})</td>", info.CounterMax.DamageMin, info.CounterMax.DamageMax, info.CounterMax.DamageAvg);
                writer.Write("<td><div class='optimistic'>{0}</div>-{1}({2})</td>", info.CounterMax.KilledMin, info.CounterMax.KilledMax, info.CounterMax.KilledAvg);
                //writer.Write("<td>{0}-{1}({2})</td>", info.CounterAvg.DamageMin, info.CounterAvg.DamageMax, info.CounterAvg.DamageAvg);
                writer.Write("<td>{0}-{1}({2})</td>", info.CounterAvg.KilledMin, info.CounterAvg.KilledMax, info.CounterAvg.KilledAvg);

                writer.Write("<td>");
                writer.Write("{0}-{1}({2})</br>", info.FinalMin.AttackerLeft,info.FinalMin.DefenderLeft, info.FinalMin.Rounds);
                writer.Write("{0}-{1}({2})</br>", info.FinalMax.AttackerLeft,info.FinalMax.DefenderLeft, info.FinalMax.Rounds);
                writer.Write("{0}-{1}({2})</br>", info.FinalAvg.AttackerLeft,info.FinalAvg.DefenderLeft, info.FinalAvg.Rounds);
                writer.Write("</td>");

                writer.WriteLine("</tr>");
            }
            writer.WriteLine("</table>");
        }

    }
}