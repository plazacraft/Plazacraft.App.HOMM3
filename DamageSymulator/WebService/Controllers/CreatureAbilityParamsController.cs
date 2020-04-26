using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Plazacraft.HOMM3.DamageSymulator.Core;

namespace Plazacraft.HOMM3.DamageSymulator.WebService.Controllers
{

    [Route("api/[controller]")]
    [ApiController]
    public class CreatureAbilityParamsController : ControllerBase
    {
        // GET api/values
        [HttpGet]
        public ActionResult<IEnumerable<World.ParamInfo>> Get()
        {

            return Program.World.Params;
        }

        public class CreateAbilityParamsXonomy
        {
            public string value
            {get; set;}
            public string caption
            {get;set;}
        }


        // GET api/values
        [HttpGet("xonomy")]
        public ActionResult<IEnumerable<CreateAbilityParamsXonomy>> GetXonomy()
        {
            List<World.ParamInfo> info = Program.World.Params;
            List<CreateAbilityParamsXonomy> ret = new List<CreateAbilityParamsXonomy>();
            foreach (World.ParamInfo item in info)
            {
                ret.Add(new CreateAbilityParamsXonomy 
                {
                    value = item.ParamName,
                    caption = string.Format("{0} - {1}({2})", item.ParamName, item.AbilityName, item.CreatureName),

                });
            }

            return ret;
        }

    }
}
