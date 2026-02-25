using Microsoft.AspNetCore.Mvc;
using ProgressionEcole.Models;
using ProgressionEcole.Repositories;

namespace ProgressionEcole.Controllers
{
    [ApiController]
    [Route("api/activite-digramme")]
    public class ActiviteDigrammeController : ControllerBase
    {
        private readonly ActiviteDigrammeRepository _repo;

        public ActiviteDigrammeController(ActiviteDigrammeRepository repo)
        {
            _repo = repo;
        }

        [HttpGet("eleve/{eleveId}/periode/{periode}")]
        public ActionResult<ActiviteDigramme> GetByEleveAndPeriode(string eleveId, string periode)
        {
            var activite = _repo.GetByEleveAndPeriode(eleveId, periode);
            if (activite == null)
            {
                return NotFound();
            }
            return Ok(activite);
        }

        [HttpGet("eleve/{eleveId}")]
        public ActionResult<List<ActiviteDigramme>> GetByEleve(string eleveId)
        {
            var activites = _repo.GetByEleve(eleveId);
            return Ok(activites);
        }

        [HttpPost]
        public IActionResult Save([FromBody] ActiviteDigramme activite)
        {
            if (string.IsNullOrEmpty(activite.EleveId) || string.IsNullOrEmpty(activite.Periode))
            {
                return BadRequest("EleveId et Periode sont requis");
            }

            _repo.Save(activite);
            return Ok();
        }

        [HttpDelete("eleve/{eleveId}/periode/{periode}")]
        public IActionResult Delete(string eleveId, string periode)
        {
            _repo.Delete(eleveId, periode);
            return Ok();
        }
    }
}
