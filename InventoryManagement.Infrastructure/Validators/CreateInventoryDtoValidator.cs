using FluentValidation;
using InventoryManagement.Core.DTOs.Inventory;
using InventoryManagement.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace InventoryManagement.Infrastructure.Validators;


public class CreateInventoryDtoValidator : AbstractValidator<CreateInventoryDto>
{
    private readonly ApplicationDbContext _context;

    public CreateInventoryDtoValidator(ApplicationDbContext context)
    {
        _context = context;

        RuleFor(x => x.Barcode)
            .NotEmpty().WithMessage("Barkod boş olamaz")
            .MustAsync(async (barcode, cancellation) =>
            {
                return !await _context.Inventories.AnyAsync(i => i.Barcode == barcode);
            }).WithMessage("Bu barkod zaten kullanımda");

        RuleFor(x => x.SerialNumber)
            .NotEmpty().WithMessage("Seri numarası boş olamaz")
            .MustAsync(async (serialNumber, cancellation) =>
            {
                return !await _context.Inventories.AnyAsync(i => i.SerialNumber == serialNumber);
            }).WithMessage("Bu seri numarası zaten kullanımda");

        RuleFor(x => x.WarrantyStartDate)
            .LessThan(x => x.WarrantyEndDate)
            .When(x => x.WarrantyStartDate.HasValue && x.WarrantyEndDate.HasValue)
            .WithMessage("Garanti başlangıç tarihi, bitiş tarihinden önce olmalıdır");
        
    }
}