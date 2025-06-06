using InventoryManagement.Core.DTOs.User;
using InventoryManagement.Core.Entities;
using AutoMapper;
using InventoryManagement.Core.DTOs.AssignmentTime;
using InventoryManagement.Core.DTOs.Auth;
using InventoryManagement.Core.DTOs.CancelReason;
using InventoryManagement.Core.DTOs.Company;
using InventoryManagement.Core.DTOs.DelayReason;
using InventoryManagement.Core.DTOs.Department;
using InventoryManagement.Core.DTOs.Family;
using InventoryManagement.Core.DTOs.Group;
using InventoryManagement.Core.DTOs.IdleDurationLimit;
using InventoryManagement.Core.DTOs.Inventory;
using InventoryManagement.Core.DTOs.InventoryAttachment;
using InventoryManagement.Core.DTOs.SolutionReview;
using InventoryManagement.Core.DTOs.SolutionTime;
using InventoryManagement.Core.DTOs.Ticket;
using InventoryManagement.Core.DTOs.TicketHistory;
using InventoryManagement.Core.DTOs.TicketNote;
using InventoryManagement.Core.DTOs.TicketSolution;
using InventoryManagement.Core.Enums;
using InventoryManagement.Core.Helpers;


namespace InventoryManagement.Core.Mapping;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<User, UserDto>()
            .ForMember(dest => dest.Department,
                opt => opt.MapFrom(src => src.Department));
        CreateMap<CreateUserDto, User>();
        CreateMap<UpdateUserDto, User>();
        CreateMap<Department, DepartmentDto>();
        CreateMap<DepartmentDto, Department>();
        CreateMap<Group, GroupDto>();
        CreateMap<GroupDto, Group>();
        CreateMap<CreateGroupDto, Group>();
        // In your MappingProfile.cs
        CreateMap<UpdateInventoryDto, Inventory>()
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status));
        
        CreateMap<Inventory, InventoryDto>()
            .ForMember(dest => dest.AssignedUser, opt => opt.MapFrom(src => src.AssignedUser))
            .ForMember(dest => dest.SupportCompany, opt => opt.MapFrom(src => src.SupportCompany));
        CreateMap<CreateInventoryDto, Inventory>();
        CreateMap<UpdateInventoryDto, Inventory>();
        
        CreateMap<Ticket, TicketDto>()
            .ForMember(dest => dest.User, opt => opt.MapFrom(src => src.User))
            .ForMember(dest => dest.ProblemTypeName, opt => opt.MapFrom(src => src.ProblemType != null ? src.ProblemType.Name : null))
            .ForMember(dest => dest.Inventory, opt => opt.MapFrom(src => src.Inventory));
        CreateMap<CreateTicketDto, Ticket>();
        CreateMap<UpdateTicketDto, Ticket>();
        
        CreateMap<TicketSolution, TicketSolutionDto>()
            .ForMember(dest => dest.TicketId, opt => opt.MapFrom(src => src.TicketId))
            .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.UserId))
            .ForMember(dest => dest.Subject, opt => opt.MapFrom(src => src.Subject))
            .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
            .ForMember(dest => dest.SolutionTypeId, opt => opt.MapFrom(src => src.SolutionTypeId))
            .ForMember(dest => dest.SolutionDate, opt => opt.MapFrom(src => src.SolutionDate));

        // Mapping from TicketSolutionCreateUpdateDto to TicketSolution
        CreateMap<TicketSolutionCreateUpdateDto, TicketSolution>()
            .ForMember(dest => dest.TicketId, opt => opt.MapFrom(src => src.TicketId))
            .ForMember(dest => dest.Subject, opt => opt.MapFrom(src => src.Subject))
            .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
            .ForMember(dest => dest.SolutionTypeId, opt => opt.MapFrom(src => src.SolutionTypeId))
            // Ignore properties that are set in the controller
            .ForMember(dest => dest.UserId, opt => opt.Ignore())
            .ForMember(dest => dest.AssignedUserId, opt => opt.Ignore())
            .ForMember(dest => dest.SolutionDate, opt => opt.Ignore())
            // Ignore navigation properties
            .ForMember(dest => dest.Ticket, opt => opt.Ignore())
            .ForMember(dest => dest.User, opt => opt.Ignore())
            .ForMember(dest => dest.AssignedUser, opt => opt.Ignore())
            .ForMember(dest => dest.SolutionType, opt => opt.Ignore());

        CreateMap<SolutionTime, SolutionTimeDto>()
            .ForMember(dest => dest.ProblemTypeName, 
                opt => opt.MapFrom(src => src.ProblemType.Name));
        CreateMap<IdleDurationLimit, IdleDurationLimitDto>()
            .ForMember(dest => dest.ProblemTypeName,
                opt => opt.MapFrom(src => src.ProblemType.Name));

        CreateMap<CreateUpdateIdleDurationLimitDto, IdleDurationLimit>();
        
        
        CreateMap<CreateUpdateSolutionTimeDto, SolutionTime>();
        
        CreateMap<AssignmentTime, AssignmentTimeDto>()
            .ForMember(dest => dest.ProblemTypeName, 
                opt => opt.MapFrom(src => src.ProblemType.Name));
        CreateMap<CreateUpdateAssignmentTimeDto, AssignmentTime>();
        
        
        CreateMap<CreateSolutionReviewDto, SolutionReview>()
            .ForMember(dest => dest.HowMuchLate, 
                opt => opt.MapFrom(src => src.HowMuchLate));

        CreateMap<SolutionReview, SolutionReviewDto>()
            .ForMember(dest => dest.HowLate, 
                opt => opt.MapFrom(src => src.HowMuchLate ?? TimeSpan.Zero));
        

        
        CreateMap<TicketHistoryCreateUpdateDto, TicketHistory>()
            .ForMember(dest => dest.Ticket, opt => opt.Ignore())
            .ForMember(dest => dest.FromAssignedUser, opt => opt.Ignore())
            .ForMember(dest => dest.ToUser, opt => opt.Ignore())
            .ForMember(dest => dest.Group, opt => opt.Ignore());

        // Mapping from TicketHistory to TicketHistoryDto
        CreateMap<TicketHistory, TicketHistoryDto>()
            .ForMember(dest => dest.TicketId, opt => opt.MapFrom(src => src.TicketId))
            .ForMember(dest => dest.TicketRegistrationNumber, opt => opt.MapFrom(src => src.Ticket.RegistrationNumber))
            .ForMember(dest => dest.FromAssignedUserId, opt => opt.MapFrom(src => src.FromAssignedUserId))
            .ForMember(dest => dest.FromAssignedUserEmail, opt => opt.MapFrom(src => src.FromAssignedUser.Email))
            .ForMember(dest => dest.ToUserId, opt => opt.MapFrom(src => src.ToUserId))
            .ForMember(dest => dest.ToUserEmail, opt => opt.MapFrom(src => src.ToUser.Email))
            .ForMember(dest => dest.GroupId, opt => opt.MapFrom(src => src.GroupId))
            .ForMember(dest => dest.GroupName, opt => opt.MapFrom(src => src.Group.Name))
            .ForMember(dest => dest.Subject, opt => opt.MapFrom(src => src.Subject))
            .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description));
        
        CreateMap<Ticket, TicketDto>()
            .ForMember(dest => dest.IdleDurationDisplay, opt => opt.MapFrom(src => 
                src.IdleDuration.HasValue 
                    ? TimeSpanFormatter.Format(src.IdleDuration.Value) 
                    : "Not assigned"))
            .ForMember(dest => dest.TimeToAssign, opt => opt.MapFrom(src => 
                src.ProblemType != null && src.ProblemType.AssignmentTimes != null && src.ProblemType.AssignmentTimes.Any() 
                    ? src.ProblemType.AssignmentTimes.FirstOrDefault().TimeToAssign 
                    : (TimeSpan?)null))
            .ForMember(dest => dest.TimeToAssignDisplay, opt => opt.MapFrom(src => 
                src.ProblemType != null && src.ProblemType.AssignmentTimes != null && src.ProblemType.AssignmentTimes.Any() 
                    ? TimeSpanFormatter.Format(src.ProblemType.AssignmentTimes.FirstOrDefault().TimeToAssign)
                    : "No time limit defined"))
            .ForMember(dest => dest.TimeToSolve, opt => opt.MapFrom(src => 
                src.ProblemType != null && src.ProblemType.SolutionTime != null && src.ProblemType.SolutionTime.Any() 
                    ? src.ProblemType.SolutionTime.FirstOrDefault().TimeToSolve 
                    : (TimeSpan?)null))
            .ForMember(dest => dest.TimeToSolveDisplay, opt => opt.MapFrom(src => 
                src.ProblemType != null && src.ProblemType.SolutionTime != null && src.ProblemType.SolutionTime.Any() 
                    ? TimeSpanFormatter.Format(src.ProblemType.SolutionTime.FirstOrDefault().TimeToSolve)
                    : "No solution time defined"));
        
        CreateMap<TicketNote, TicketNoteDto>()
            .ForMember(d => d.CreatedByEmail, o => o.MapFrom(s => s.CreatedBy.Email))
            .ForMember(d => d.TicketRegistrationNumber, o => o.MapFrom(s => s.Ticket.RegistrationNumber));

        CreateMap<TicketNoteAttachment, TicketNoteAttachmentDto>();
        
        
        CreateMap<TicketHistoryCreateUpdateDto, TicketHistory>();
        
        CreateMap<Family, FamilyDto>();
        CreateMap<FamilyDto, Family>();
        
        CreateMap<InventoryType, InventoryTypeDto>();
        CreateMap<InventoryTypeDto, InventoryType>();
        
        CreateMap<Brand, BrandDto>();
        CreateMap<BrandDto, Brand>();
        
        CreateMap<Model, ModelDto>()
            .ForMember(dest => dest.BrandName, opt => opt.MapFrom(src => src.Brand != null ? src.Brand.Name : null));
        CreateMap<ModelDto, Model>();

        // Update the Inventory mapping
        CreateMap<Inventory, InventoryDto>()
            .ForMember(dest => dest.AssignedUser, opt => opt.MapFrom(src => src.AssignedUser))
            .ForMember(dest => dest.SupportCompany, opt => opt.MapFrom(src => src.SupportCompany))
            .ForMember(dest => dest.FamilyName, opt => opt.MapFrom(src => src.Family != null ? src.Family.Name : null))
            .ForMember(dest => dest.TypeName, opt => opt.MapFrom(src => src.Type != null ? src.Type.Name : null))
            .ForMember(dest => dest.BrandName, opt => opt.MapFrom(src => src.Brand != null ? src.Brand.Name : null))
            .ForMember(dest => dest.ModelName, opt => opt.MapFrom(src => src.Model != null ? src.Model.Name : null));
        
        // In your mapping profile class
        CreateMap<Company, CompanyDto>()
            .ForMember(dest => dest.Type, opt => opt.MapFrom(src => (int)src.CompanyType))
            .ForMember(dest => dest.TypeName, opt => opt.MapFrom(src => 
                src.CompanyType == CompanyType.Support ? "Destek Şirketi" : 
                src.CompanyType == CompanyType.Supplier ? "Tedarikçi Şirket" : 
                "Bilinmeyen"));

        CreateMap<CreateCompanyDto, Company>()
            .ForMember(dest => dest.CompanyType, opt => opt.MapFrom(src => src.Type));

        CreateMap<UpdateCompanyDto, Company>()
            .ForMember(dest => dest.CompanyType, opt => opt.MapFrom(src => src.Type));
        CreateMap<InventoryAttachment, InventoryAttachmentDto>();
        
        CreateMap<InventoryHistory, InventoryHistoryDto>()
            .ForMember(dest => dest.User, opt => opt.MapFrom(src => src.User));
        
        CreateMap<Inventory, InventoryDto>()
            .ForMember(dest => dest.AssignedUser, opt => opt.MapFrom(src => src.AssignedUser))
            .ForMember(dest => dest.SupportCompany, opt => opt.MapFrom(src => src.SupportCompany))
            .ForMember(dest => dest.InventoryHistory, opt => opt.MapFrom(src => src.InventoryHistory));
        
        CreateMap<RegisterDto, User>()
            .ForMember(dest => dest.UserName, opt => 
                opt.MapFrom(src => src.Username))
            .ForMember(dest => dest.Email, opt => 
                opt.MapFrom(src => src.Email))
            .ForMember(dest => dest.Name, opt => 
                opt.MapFrom(src => src.Name))
            .ForMember(dest => dest.Surname, opt => 
                opt.MapFrom(src => src.Surname))
            .ForMember(dest => dest.Location, opt => 
                opt.MapFrom(src => src.Location))
            .ForMember(dest => dest.Department, opt => 
                opt.MapFrom(src => src.Department))
            .ForMember(dest => dest.IsActive, opt => 
                opt.MapFrom(src => true))
            // Password ve diğer IdentityUser property'leri UserManager tarafından yönetilecek
            .ForMember(dest => dest.PasswordHash, opt => opt.Ignore())
            .ForMember(dest => dest.SecurityStamp, opt => opt.Ignore())
            .ForMember(dest => dest.ConcurrencyStamp, opt => opt.Ignore());
        
        
        // DelayReason mappings
        CreateMap<DelayReason, DelayReasonDto>();
        CreateMap<CreateDelayReasonDto, DelayReason>();
        CreateMap<UpdateDelayReasonDto, DelayReason>();

        // CancelReason mappings
        CreateMap<CancelReason, CancelReasonDto>();
        CreateMap<CreateCancelReasonDto, CancelReason>();
        CreateMap<UpdateCancelReasonDto, CancelReason>();
        
        CreateMap<CreateGroupDto, Group>(); // This should now pick up ManagerId by convention if names match
        CreateMap<Group, GroupDto>();     // This should also pick up ManagerId
        
        CreateMap<GroupDto, Group>()
            .ForMember(dest => dest.Id, opt => opt.Ignore()); // Typically ignore Id on updates from DTO
    }
    
}