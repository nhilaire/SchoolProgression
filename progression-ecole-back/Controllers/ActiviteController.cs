using Microsoft.AspNetCore.Mvc;
using ProgressionEcole.Models;
using ProgressionEcole.Repositories;

namespace ProgressionEcole.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ActiviteController : ControllerBase
    {
        private readonly ActiviteRepository _repo;

        public ActiviteController(ActiviteRepository repo)
        {
            _repo = repo;
        }

        [HttpGet]
        public IActionResult GetAll() => Ok(_repo.GetAll());

        [HttpPost]
        public IActionResult Add([FromBody] Activite activite)
        {
            activite.Id = Guid.NewGuid().ToString();
            // S'assurer que les valeurs par défaut sont définies si non spécifiées
            if (!activite.EstRegroupement && activite.ParentId == null)
            {
                // Activité isolée
                activite.EstRegroupement = false;
                activite.ParentId = null;
            }
            _repo.Add(activite);
            return Ok();
        }

        [HttpPut]
        public IActionResult Update([FromBody] Activite activite)
        {
            _repo.Update(activite);
            return Ok();
        }

        [HttpDelete("{id}")]
        public IActionResult Delete(string id)
        {
            _repo.Delete(id);
            return Ok();
        }

        [HttpGet("enfants/{parentId}")]
        public IActionResult GetEnfants(string parentId) => Ok(_repo.GetEnfants(parentId));

        [HttpGet("regroupements")]
        public IActionResult GetRegroupements() => Ok(_repo.GetRegroupements());

        [HttpGet("isolees")]
        public IActionResult GetActivitesIsolees() => Ok(_repo.GetActivitesIsolees());

        [HttpPost("regroupement")]
        public IActionResult CreateRegroupement([FromBody] Activite regroupement)
        {
            regroupement.Id = Guid.NewGuid().ToString();
            regroupement.EstRegroupement = true;
            regroupement.ParentId = null;
            _repo.Add(regroupement);
            return Ok();
        }

        [HttpPost("activite-enfant")]
        public IActionResult AddActiviteToRegroupement([FromBody] Activite activite)
        {
            activite.Id = Guid.NewGuid().ToString();
            activite.EstRegroupement = false;
            _repo.Add(activite);
            return Ok();
        }

        [HttpPost("reorganiser")]
        public IActionResult ReorganiserOrdre([FromBody] List<string> ids)
        {
            _repo.ReorganiserOrdre(ids);
            return Ok();
        }
    }
}
