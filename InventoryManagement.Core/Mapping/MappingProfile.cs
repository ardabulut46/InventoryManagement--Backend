using InventoryManagement.Core.DTOs.User;
using InventoryManagement.Core.Entities;
using AutoMapper;
using InventoryManagement.Core.DTOs.Auth;
using InventoryManagement.Core.DTOs.Company;
using InventoryManagement.Core.DTOs.Inventory;
using InventoryManagement.Core.DTOs.Ticket;

namespace InventoryManagement.Core.Mapping;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<User, UserDto>();
        CreateMap<CreateUserDto, User>();
        CreateMap<UpdateUserDto, User>();
        
        CreateMap<Inventory, InventoryDto>()
            .ForMember(dest => dest.AssignedUser, opt => opt.MapFrom(src => src.AssignedUser))
            .ForMember(dest => dest.SupportCompany, opt => opt.MapFrom(src => src.SupportCompany));
        CreateMap<CreateInventoryDto, Inventory>();
        CreateMap<UpdateInventoryDto, Inventory>();
        
        CreateMap<Ticket, TicketDto>()
            .ForMember(dest => dest.User, opt => opt.MapFrom(src => src.User))
            .ForMember(dest => dest.Inventory, opt => opt.MapFrom(src => src.Inventory));
        CreateMap<CreateTicketDto, Ticket>();
        CreateMap<UpdateTicketDto, Ticket>();
        
        CreateMap<Company, CompanyDto>();
        CreateMap<CreateCompanyDto, Company>();
        CreateMap<UpdateCompanyDto, Company>();
        
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
    }
    
}