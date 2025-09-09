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
    }
}
