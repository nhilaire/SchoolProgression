using Microsoft.AspNetCore.Mvc;
using ProgressionEcole.Models;
using ProgressionEcole.Repositories;

namespace ProgressionEcole.Controllers
{
    [ApiController]
    [Route("api/categories")]
    public class CategorieController : ControllerBase
    {
        private readonly CategorieRepository _repo;
        public CategorieController(CategorieRepository repo)
        {
            _repo = repo;
        }

        [HttpGet]
        public IActionResult GetAll() => Ok(_repo.GetAll());

        [HttpPost]
        public IActionResult Add([FromBody] Categorie categorie)
        {
            categorie.Id = Guid.NewGuid().ToString();
            _repo.Add(categorie);
            return Ok();
        }

        [HttpPut]
        public IActionResult Update([FromBody] Categorie categorie)
        {
            _repo.Update(categorie);
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
