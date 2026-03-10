using AutoMapper;
using ExpenseManager.Application.Categories.DTOs;
using ExpenseManager.Application.Expenses.DTOs;
using ExpenseManager.Application.Reports.DTOs;
using ExpenseManager.Domain.Entities;

namespace ExpenseManager.Application.Common.Mappings;

public sealed class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<Expense, ExpenseDto>()
            .ForMember(d => d.Amount, opt => opt.MapFrom(s => s.Amount.Amount))
            .ForMember(d => d.Currency, opt => opt.MapFrom(s => s.Amount.Currency))
            .ForMember(d => d.Status, opt => opt.MapFrom(s => s.Status.ToString()))
            .ForMember(d => d.CategoryName, opt => opt.MapFrom(s => s.Category != null ? s.Category.Name : null));

        CreateMap<Category, CategoryDto>();

        CreateMap<Expense, ExpenseByCategoryDto>()
            .ForMember(d => d.Amount, opt => opt.MapFrom(s => s.Amount.Amount))
            .ForMember(d => d.Currency, opt => opt.MapFrom(s => s.Amount.Currency))
            .ForMember(d => d.Status, opt => opt.MapFrom(s => s.Status.ToString()));
    }
}
