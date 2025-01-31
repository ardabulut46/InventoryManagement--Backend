using AutoMapper;
using InventoryManagement.Core.DTOs.Group;
using InventoryManagement.Core.Entities;
using InventoryManagement.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]

public class GroupController : ControllerBase
{
   private readonly IGenericRepository<Group> _groupRepository;
   private readonly IGenericRepository<Department> _departmentRepository;
   private readonly IMapper _mapper;

   public GroupController(
       IGenericRepository<Group> groupRepository,
       IGenericRepository<Department> departmentRepository, 
       IMapper mapper)
   {
       _groupRepository = groupRepository;
       _departmentRepository = departmentRepository;
       _mapper = mapper;
   }

   [HttpGet]
   public async Task<ActionResult<IEnumerable<GroupDto>>> GetGroups()
   {
       var groups = await _groupRepository.GetAllAsync();
       return Ok(_mapper.Map<IEnumerable<GroupDto>>(groups));
   }

   [HttpGet("{id}")]
   public async Task<ActionResult<GroupDto>> GetGroup(int id)
   {
       var group = await _groupRepository.GetByIdAsync(id);
       if (group == null) return NotFound($"Group with ID {id} not found");
       return Ok(_mapper.Map<GroupDto>(group));
   }

   [HttpGet("department/{departmentId}")]
   public async Task<ActionResult<IEnumerable<GroupDto>>> GetGroupsByDepartment(int departmentId)
   {
       var department = await _departmentRepository.GetByIdAsync(departmentId);
       if (department == null) return NotFound($"Department with ID {departmentId} not found");

       var groups = await _groupRepository.SearchAsync(g => g.DepartmentId == departmentId);
       return Ok(_mapper.Map<IEnumerable<GroupDto>>(groups));
   }

   [Authorize(Roles = "Admin")]
   [HttpPost]
   public async Task<ActionResult<GroupDto>> CreateGroup(CreateGroupDto createGroupDto)
   {
       var department = await _departmentRepository.GetByIdAsync(createGroupDto.DepartmentId);
       if (department == null)
           return BadRequest($"Department with ID {createGroupDto.DepartmentId} not found");

       var group = _mapper.Map<Group>(createGroupDto);
       var createdGroup = await _groupRepository.AddAsync(group);

       return CreatedAtAction(
           nameof(GetGroup),
           new { id = createdGroup.Id },
           _mapper.Map<GroupDto>(createdGroup));
   }

   [HttpPut("{id}")]
   public async Task<IActionResult> UpdateGroup(int id, GroupDto groupDto)
   {
       if (id != groupDto.Id) return BadRequest("ID mismatch");

       var group = await _groupRepository.GetByIdAsync(id);
       if (group == null) return NotFound($"Group with ID {id} not found");

       var department = await _departmentRepository.GetByIdAsync(groupDto.DepartmentId);
       if (department == null)
           return BadRequest($"Department with ID {groupDto.DepartmentId} not found");

       _mapper.Map(groupDto, group);
       await _groupRepository.UpdateAsync(group);

       return NoContent();
   }

   [HttpDelete("{id}")]
   public async Task<IActionResult> DeleteGroup(int id)
   {
       var group = await _groupRepository.GetByIdAsync(id);
       if (group == null) return NotFound($"Group with ID {id} not found");

       await _groupRepository.DeleteAsync(id);
       return NoContent();
   }
}