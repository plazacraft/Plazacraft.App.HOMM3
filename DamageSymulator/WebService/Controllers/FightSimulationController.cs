using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using System.Collections;
using System.IO;
using Plazacraft.HOMM3.DamageSymulator.Core;
using Microsoft.AspNetCore.Cors;
using System.IO.Compression;


namespace Plazacraft.HOMM3.DamageSymulator.WebService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [EnableCors("AllowSpecificOrigin")]    
    public class FightSimulationController : ControllerBase
    {
        public class ApiFightTextSimulationResult
        {
            public string Log
            { get; set;}
            public string Text
            { get; set; }
        }

        // POST api/values
        [HttpPost("csv")]
        public ActionResult<ApiFightTextSimulationResult> PostCsv([FromBody] string xmlFight)
        {
            ApiFightTextSimulationResult res = new ApiFightTextSimulationResult();
            FightSimulation sim = new FightSimulation(Program.World, xmlFight);
            StringWriter log = new StringWriter();
            StringWriter csv = new StringWriter();
            FightSimulationResult fightResult = sim.Run(log);
            fightResult.WriteToCsv(csv);
            log.Flush();
            csv.Flush();
            res.Text = csv.ToString();
            res.Log = log.ToString();

            return res;
        }

        [HttpPost("csv-file")]
        public ActionResult<FileResult> PostCsvFile([FromBody] string xmlFight)
        {
            try
            {
                ApiFightTextSimulationResult res = new ApiFightTextSimulationResult();
                FightSimulation sim = new FightSimulation(Program.World, xmlFight);
                StringWriter log = new StringWriter();
                StringWriter csv = new StringWriter();
                FightSimulationResult fightResult = sim.Run(log);
                fightResult.WriteToCsv(csv);
                log.Flush();
                csv.Flush();
                res.Text = csv.ToString();
                res.Log = log.ToString();


                using (MemoryStream memoryStream = new MemoryStream())
                {
                    using (ZipArchive archive = new ZipArchive(memoryStream, ZipArchiveMode.Create, true))
                    {
                        ZipArchiveEntry outputFile = archive.CreateEntry("homm3-damage-simulation.csv");

                        using (Stream entryStream = outputFile.Open())
                            using (StreamWriter streamWriter = new StreamWriter(entryStream))
                            {
                                entryStream.Write(System.Text.Encoding.Unicode.GetBytes(res.Text));
                                //streamWriter.Write(res.Text);
                            }
                        
                        outputFile = archive.CreateEntry("homm3-damage-simulation-log.txt");                        
                        using (Stream entryStream = outputFile.Open())
                            using (StreamWriter streamWriter = new StreamWriter(entryStream))
                            {
                                
                                entryStream.Write(System.Text.Encoding.Unicode.GetBytes(res.Log));
                                //streamWriter.Write(res.Log);
                            }

                    }


                    memoryStream.Seek(0, SeekOrigin.Begin);
                    FileResult fr = new FileContentResult(memoryStream.ToArray(), "application/zip");
                    fr.FileDownloadName = "homm3-damage-simulation.zip";
                    return fr;
                }
            }
            catch (Exception e)
            {
                return StatusCode(400, e);
            }



        }


        [HttpPost("html")]
        public ActionResult<ApiFightTextSimulationResult> PostHtml([FromBody] string xmlFight)
        {
            try
            {
                ApiFightTextSimulationResult res = new ApiFightTextSimulationResult();
                FightSimulation sim = new FightSimulation(Program.World, xmlFight);
                StringWriter log = new StringWriter();
                StringWriter html = new StringWriter();
                FightSimulationResult fightResult = sim.Run(log);
                fightResult.WriteToHtml(html);
                log.Flush();
                html.Flush();
                res.Text = html.ToString();
                res.Log = log.ToString();

                if (res.Log.Length > (1024 * 1024) )
                    res.Log = res.Log.Substring(0, (1024 * 1024)) + " ...";

                return res;
            }
            catch (Exception e)
            {
                return StatusCode(400, e);
            }
        }

        [HttpPost("html-chart")]
        public ActionResult<ApiFightTextSimulationResult> PostHtmlChart([FromBody] string xmlFight)
        {
            try
            {
                ApiFightTextSimulationResult res = new ApiFightTextSimulationResult();
                FightSimulation sim = new FightSimulation(Program.World, xmlFight);
                StringWriter log = new StringWriter();
                StringWriter html = new StringWriter();
                FightSimulationResult fightResult = sim.Run(log);
                fightResult.WriteToHtmlChart(html);
                log.Flush();
                html.Flush();
                res.Text = html.ToString();
                res.Log = log.ToString();

                return res;
            }
            catch (Exception e)
            {
                return StatusCode(400, e);
            }
        }

        public class ApiFightJsonSimulationResult
        {
            public string Log
            { get; set;}
            public FightSimulationResult Json
            { get; set; }
        }

        // POST api/values
        [HttpPost("json")]
        public ActionResult<ApiFightJsonSimulationResult> PostJson([FromBody] string xmlFight)
        {
            ApiFightJsonSimulationResult res = new ApiFightJsonSimulationResult();
            FightSimulation sim = new FightSimulation(Program.World, xmlFight);
            StringWriter log = new StringWriter();
            StringWriter csv = new StringWriter();
            FightSimulationResult fightResult = sim.Run(log);
            fightResult.WriteToCsv(csv);
            log.Flush();
            csv.Flush();
            res.Json = fightResult;
            res.Log = log.ToString();

            return res;
        }
    }
}
