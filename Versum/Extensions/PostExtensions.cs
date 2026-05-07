using System.Linq;
using Versum.Core.Enums;
using Versum.Models;

namespace Versum.Extensions;

public static class PostExtensions
{
    public static IQueryable<Post> OnlyPublished(this IQueryable<Post> query)
    {
        return query.Where(p => !p.IsDraft);
    }

    public static IQueryable<Post> OnlyDrafts(this IQueryable<Post> query)
    {
        return query.Where(p => p.IsDraft);
    }

    public static IQueryable<Post> ApplySorting(
        this IQueryable<Post> query,
        FilterOptions filter,
        bool ascending)
    {
        return ascending
            ? query.OrderByField(filter)
            : query.OrderByFieldDescending(filter);
    }

    private static IQueryable<Post> OrderByField(this IQueryable<Post> query, FilterOptions filter) =>
        filter switch
        {
            FilterOptions.Title => query.OrderBy(p => p.Title),
            FilterOptions.CreatedAt => query.OrderBy(p => p.CreatedAt),
            FilterOptions.Description => query.OrderBy(p => p.Description),
            _ => query.OrderBy(p => p.Id)
        };

    private static IQueryable<Post> OrderByFieldDescending(this IQueryable<Post> query, FilterOptions filter) =>
        filter switch
        {
            FilterOptions.Title => query.OrderByDescending(p => p.Title),
            FilterOptions.CreatedAt => query.OrderByDescending(p => p.CreatedAt),
            FilterOptions.Description => query.OrderByDescending(p => p.Description),
            _ => query.OrderByDescending(p => p.Id)
        };
}