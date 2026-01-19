using Microsoft.AspNetCore.Mvc;
using ProgressionEcole.Repositories;
using ProgressionEcole.Services;

namespace ProgressionEcole.Controllers
{
    [ApiController]
    [Route("api/periode")]
    public class PeriodeController : ControllerBase
    {
        private readonly PeriodeRepository _repo;
        private readonly GenerationService _generationService;

        public PeriodeController(PeriodeRepository repo, GenerationService generationService)
        {
            _repo = repo;
            _generationService = generationService;
        }

        [HttpGet("{periode}/generate")]
        public IActionResult Generate(string periode)
        {
            var content = _generationService.GenerateDocx(periode);
            var fileName = $"Progression_{periode}.docx";
            return File(content, "application/vnd.openxmlformats-officedocument.wordprocessingml.document", fileName);
        }

        [HttpGet]
        public ActionResult<List<PeriodeActivites>> GetAll()
        {
            return Ok(_repo.GetAll());
        }

        [HttpGet("eleve/{eleveId}")]
        public ActionResult<List<PeriodeActivites>> GetByEleve(string eleveId)
        {
            var all = _repo.GetAll().Where(x => x.EleveId == eleveId).ToList();
            return Ok(all);
        }

        [HttpGet("{periode}")]
        [HttpGet("eleve/{eleveId}/periode/{periode}")]
        public ActionResult<PeriodeActivites> Get(string eleveId, string periode)
        {
            var result = _repo.GetAll().FirstOrDefault(x => x.EleveId == eleveId && x.Periode == periode);
            if (result == null) return NotFound();
            return Ok(result);
        }

        [HttpPost]
        public IActionResult Save([FromBody] PeriodeActivites data)
        {
            if (string.IsNullOrEmpty(data.EleveId)) return BadRequest("EleveId requis");
            _repo.Save(data);
            return Ok();
        }
    }
}
