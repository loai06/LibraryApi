using AutoMapper;
using LibraryManagementSystem.Dtos;
using LibraryManagementSystem.Models;

namespace LibraryManagementSystem.Mappings;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<Book, BookResponseDto>()
         .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.BookId))
         .ForMember(dest => dest.CategoryName,
         opt => opt.MapFrom(src => src.Category != null ? src.Category.Name : string.Empty));

        CreateMap<Category, CategoryResponseDto>()
           .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.CategoryId));
        CreateMap<BookCreateDto, Book>(MemberList.Source)
        .ForMember(dest => dest.Title,
    opt => opt.MapFrom(src => src.Title.Trim()))
     .ForMember(dest => dest.Author,
    opt => opt.MapFrom(src => src.Author.Trim()))
    .ForMember(dest => dest.BookId, opt => opt.Ignore())
    .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
    .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
    .ForMember(dest => dest.Category, opt => opt.Ignore());

        CreateMap<BookUpdateDto, Book>(MemberList.Source)
            .ForMember(dest => dest.Title,
                opt => opt.MapFrom(src => src.Title.Trim()))
            .ForMember(dest => dest.Author,
                opt => opt.MapFrom(src => src.Author.Trim()))
            .ForMember(dest => dest.BookId, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
            .ForMember(dest => dest.Category, opt => opt.Ignore());

        CreateMap<CategoryCreateDto, Category>(MemberList.Source)
            .ForMember(dest => dest.Name,
                opt => opt.MapFrom(src => src.Name.Trim()))
            .ForMember(dest => dest.Description,
                opt => opt.MapFrom(src => src.Description ?? string.Empty))
            .ForMember(dest => dest.CategoryId, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.IsDeleted, opt => opt.Ignore());

        CreateMap<CategoryUpdateDto, Category>(MemberList.Source)
            .ForMember(dest => dest.Name,
                opt => opt.MapFrom(src => src.Name.Trim()))
            .ForMember(dest => dest.Description,
                opt => opt.MapFrom(src => src.Description ?? string.Empty))
            .ForMember(dest => dest.CategoryId, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.IsDeleted, opt => opt.Ignore());
    }

}