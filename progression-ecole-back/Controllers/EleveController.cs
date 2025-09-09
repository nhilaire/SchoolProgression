using Microsoft.AspNetCore.Mvc;
using ProgressionEcole.Models;
using ProgressionEcole.Repositories;

namespace ProgressionEcole.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EleveController : ControllerBase
    {
        private readonly EleveRepository _repo;

        public EleveController(EleveRepository repo)
        {
            _repo = repo;
        }

        [HttpGet]
        public ActionResult<List<Eleve>> GetAll() => _repo.GetAll();

        [HttpGet("{id}")]
        public ActionResult<Eleve?> GetById(string id) => _repo.GetById(id);

        [HttpPost]
        public IActionResult Add(Eleve eleve)
        {
            eleve.Id = Guid.NewGuid().ToString();
            _repo.Add(eleve);
            return Ok();
        }

        [HttpPut]
        public IActionResult Update(Eleve eleve)
        {
            _repo.Update(eleve);
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
