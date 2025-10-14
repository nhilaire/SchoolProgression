using Microsoft.AspNetCore.Mvc;
using ProgressionEcole.Models;
using ProgressionEcole.Repositories;

namespace ProgressionEcole.Controllers
{
    [ApiController]
    [Route("api/activite-personnalisee")]
    public class ActivitePersonnaliseeController : ControllerBase
    {
        private readonly ActivitePersonnaliseeRepository _repo;

        public ActivitePersonnaliseeController(ActivitePersonnaliseeRepository repo)
        {
            _repo = repo;
        }

        [HttpGet("eleve/{eleveId}/periode/{periode}")]
        public ActionResult<List<ActivitePersonnalisee>> GetByEleveAndPeriode(string eleveId, string periode)
        {
            var activites = _repo.GetByEleveAndPeriode(eleveId, periode);
            return Ok(activites);
        }

        [HttpPost]
        public IActionResult Save([FromBody] ActivitePersonnalisee activite)
        {
            if (string.IsNullOrEmpty(activite.EleveId) || 
                string.IsNullOrEmpty(activite.ActiviteId) || 
                string.IsNullOrEmpty(activite.Periode))
            {
                return BadRequest("EleveId, ActiviteId et Periode sont requis");
            }

            _repo.Save(activite);
            return Ok();
        }

        [HttpDelete("activite/{activiteId}/eleve/{eleveId}/periode/{periode}")]
        public IActionResult Delete(string activiteId, string eleveId, string periode)
        {
            _repo.Delete(activiteId, eleveId, periode);
            return Ok();
        }
    }
}