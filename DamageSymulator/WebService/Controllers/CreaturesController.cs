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
    public class CreaturesController : ControllerBase
    {
        // GET api/values
        [HttpGet]
        public ActionResult<IEnumerable<string>> Get()
        {

            return Program.World.Creatures.Keys;
        }

    }
}
