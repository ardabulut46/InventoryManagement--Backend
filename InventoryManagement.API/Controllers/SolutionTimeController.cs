using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using InventoryManagement.Core.Entities;
using InventoryManagement.Core.Interfaces;
using InventoryManagement.Core.DTOs.SolutionTime;
using Microsoft.EntityFrameworkCore;

namespace InventoryManagement.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin")]
    public class SolutionTimeController : ControllerBase
    {
        private readonly IGenericRepository<SolutionTime> _solutionTimeRepository;
        private readonly IMapper _mapper;

        public SolutionTimeController(
            IGenericRepository<SolutionTime> solutionTimeRepository,
            IMapper mapper)
        {
            _solutionTimeRepository = solutionTimeRepository;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<SolutionTimeDto>>> GetSolutionTimes()
        {
            var solutionTimes = await _solutionTimeRepository.GetAllAsync(
                include: query => query.Include(x => x.ProblemType));
            return Ok(_mapper.Map<IEnumerable<SolutionTimeDto>>(solutionTimes));
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<SolutionTimeDto>> GetSolutionTime(int id)
        {
            var solutionTime = await _solutionTimeRepository.GetByIdWithIncludesAsync(id, "ProblemType");
            
            if (solutionTime == null)
                return NotFound();

            return Ok(_mapper.Map<SolutionTimeDto>(solutionTime));
        }

        [HttpPost]
        [Authorize]
        public async Task<ActionResult<SolutionTimeDto>> CreateSolutionTime(CreateUpdateSolutionTimeDto createSolutionTimeDto)
        {
            var solutionTime = _mapper.Map<SolutionTime>(createSolutionTimeDto);
            var createdSolutionTime = await _solutionTimeRepository.AddAsync(solutionTime);

            return CreatedAtAction(
                nameof(GetSolutionTime),
                new { id = createdSolutionTime.Id },
                _mapper.Map<SolutionTimeDto>(createdSolutionTime));
        }

        [HttpPut("{id}")]
        [Authorize]
        public async Task<IActionResult> UpdateSolutionTime(int id, CreateUpdateSolutionTimeDto updateSolutionTimeDto)
        {
            var solutionTime = await _solutionTimeRepository.GetByIdAsync(id);
            if (solutionTime == null)
                return NotFound();

            _mapper.Map(updateSolutionTimeDto, solutionTime);
            await _solutionTimeRepository.UpdateAsync(solutionTime);
            return NoContent();
        }

        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> DeleteSolutionTime(int id)
        {
            var solutionTime = await _solutionTimeRepository.GetByIdAsync(id);
            if (solutionTime == null)
                return NotFound();

            await _solutionTimeRepository.DeleteAsync(id);
            return NoContent();
        }

        [HttpGet("by-solution-type/{problemTypeId}")]
        public async Task<ActionResult<SolutionTimeDto>> GetBySolutionType(int problemTypeId)
        {
            var solutionTime = await _solutionTimeRepository.SearchWithIncludesAsync(
                st => st.ProblemTypeId == problemTypeId,
                "ProblemType");

            if (!solutionTime.Any())
                return NotFound();

            return Ok(_mapper.Map<IEnumerable<SolutionTimeDto>>(solutionTime));
        }
    }
}