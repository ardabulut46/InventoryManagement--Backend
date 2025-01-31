using AutoMapper;
using InventoryManagement.Core.DTOs.SolutionReview;
using InventoryManagement.Core.Entities;
using InventoryManagement.Core.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace InventoryManagement.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SolutionReviewsController : ControllerBase
    {
        private readonly IGenericRepository<SolutionReview> _solutionReviewRepository;
        private readonly IMapper _mapper;

        public SolutionReviewsController(
            IGenericRepository<SolutionReview> solutionReviewRepository,
            IMapper mapper)
        {
            _solutionReviewRepository = solutionReviewRepository;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<SolutionReviewDto>>> GetSolutionReviews()
        {
            var reviews = await _solutionReviewRepository.GetAllWithIncludesAsync(
                "TicketSolution",
                "DelayReason");
            return Ok(_mapper.Map<IEnumerable<SolutionReviewDto>>(reviews));
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<SolutionReviewDto>> GetSolutionReview(int id)
        {
            var review = await _solutionReviewRepository.GetByIdWithIncludesAsync(
                id,
                "TicketSolution",
                "DelayReason");

            if (review == null)
                return NotFound();

            return Ok(_mapper.Map<SolutionReviewDto>(review));
        }

        [HttpPost]
        public async Task<ActionResult<SolutionReviewDto>> CreateSolutionReview(CreateSolutionReviewDto createDto)
        {
            var review = _mapper.Map<SolutionReview>(createDto);
            var createdReview = await _solutionReviewRepository.AddAsync(review);

            return CreatedAtAction(
                nameof(GetSolutionReview),
                new { id = createdReview.Id },
                _mapper.Map<SolutionReviewDto>(createdReview));
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateSolutionReview(int id, UpdateSolutionReviewDto updateDto)
        {
            var review = await _solutionReviewRepository.GetByIdAsync(id);
            if (review == null)
                return NotFound();

            _mapper.Map(updateDto, review);
            await _solutionReviewRepository.UpdateAsync(review);
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteSolutionReview(int id)
        {
            var review = await _solutionReviewRepository.GetByIdAsync(id);
            if (review == null)
                return NotFound();

            await _solutionReviewRepository.DeleteAsync(id);
            return NoContent();
        }
    }
}