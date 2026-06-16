using Microsoft.EntityFrameworkCore;

namespace Crm.Api.Application.Common;

public class PaginationResult<T>
{
    public List<T> Data { get; init; }
    public int PageIndex { get; init; }
    public int PageSize { get; init; }
    public int TotalCount { get; init; }
    public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
    public bool HasPreviousPage => PageIndex > 1;
    public bool HasNextPage => PageIndex < TotalPages;

    public static async Task<PaginationResult<T>> CreateAsync(IQueryable<T> query , int page , int pageSize)
    {
        var totalCount = await query.CountAsync();
        var data = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
        return new PaginationResult<T>() { Data = data, PageIndex = page, PageSize = pageSize, TotalCount = totalCount };
    }
}