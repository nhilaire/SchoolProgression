using Microsoft.AspNetCore.Mvc;
using ProgressionEcole.Models;
using ProgressionEcole.Repositories;

namespace ProgressionEcole.Controllers
{
    [ApiController]
    [Route("api/definition-periodes")]
    public class DefinitionPeriodeController : ControllerBase
    {
        private readonly DefinitionPeriodeRepository _repo;

        public DefinitionPeriodeController(DefinitionPeriodeRepository repo)
        {
            _repo = repo;
        }

        [HttpGet]
        public IActionResult GetAll() => Ok(_repo.GetAll());

        [HttpGet("{id}")]
        public IActionResult GetById(string id)
        {
            var periode = _repo.GetById(id);
            if (periode == null) return NotFound();
            return Ok(periode);
        }

        [HttpGet("current")]
        public IActionResult GetCurrent()
        {
            var periode = _repo.GetCurrentPeriode();
            if (periode == null) return NotFound("Aucune période active");
            return Ok(periode);
        }

        [HttpPut]
        public IActionResult Update([FromBody] Periode periode)
        {
            _repo.Update(periode);
            return Ok();
        }
    }
}
