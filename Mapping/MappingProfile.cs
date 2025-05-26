using AutoMapper;
using BlogBackend.Models;
using BlogBackend.Dtos.Auth;
using BlogBackend.Dtos.Post;
using BlogBackend.Dtos.Comment;
using BlogBackend.Dtos.Like;

namespace BlogBackend.Mapping
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // Auth mappings
            CreateMap<User, AuthResponseDto>();

            // Post mappings
            CreateMap<Post, PostViewDto>()
                .ForMember(dest => dest.AuthorUsername, opt => opt.MapFrom(src => src.User.Username))
                .ForMember(dest => dest.AuthorEmail, opt => opt.MapFrom(src => src.User.Email))
                .ForMember(dest => dest.LikesCount, opt => opt.MapFrom(src => src.Likes.Count))
                .ForMember(dest => dest.CommentsCount, opt => opt.MapFrom(src => src.Comments.Count))
                .ForMember(dest => dest.Categories, opt => opt.MapFrom(src => src.Categories));

            CreateMap<Category, BlogBackend.Dtos.CategoryResponse>();

            CreateMap<CreatePostDto, Post>();
            CreateMap<UpdatePostDto, Post>();

            // Comment mappings
            CreateMap<Comment, CommentViewDto>()
                .ForMember(dest => dest.AuthorUsername, opt => opt.MapFrom(src => src.User.Username))
                .ForMember(dest => dest.AuthorEmail, opt => opt.MapFrom(src => src.User.Email));

            CreateMap<CreateCommentDto, Comment>();

            // Like mappings (simple, no special mapping needed)
            CreateMap<LikePostDto, PostLike>();
        }
    }
}
