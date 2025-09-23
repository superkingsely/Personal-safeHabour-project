using System;

namespace SafeHabour.Models.Response;

public class PagedList<T> where T : class
{
    public PagedList(IQueryable<T> source, int pageNumber, int pageSize)
    {
        TotalItems = source.Count();
        PageNumber = pageNumber;
        PageSize = pageSize;
        List = source
                        .Skip(pageSize * (pageNumber - 1))
                        .Take(pageSize)
                        .ToList();
    }

    /// <summary>
    /// Constructor for pre-computed data with known pagination metadata
    /// </summary>
    public PagedList(List<T> data, int totalItems, int pageNumber, int pageSize)
    {
        TotalItems = totalItems;
        PageNumber = pageNumber;
        PageSize = pageSize;
        List = data;
    }

    public int TotalItems { get; }
    public int PageNumber { get; }
    public int PageSize { get; }
    public List<T> List { get; }
    public int TotalPages =>
            (int)Math.Ceiling(this.TotalItems / (double)this.PageSize);
    public bool HasPreviousPage => this.PageNumber > 1;
    public bool HasNextPage => this.PageNumber < this.TotalPages;
    public int NextPageNumber =>
            this.HasNextPage ? this.PageNumber + 1 : this.TotalPages;
    public int PreviousPageNumber =>
            this.HasPreviousPage ? this.PageNumber - 1 : 1;

    public PagingHeader GetHeader()
    {
        return new PagingHeader(
                TotalItems, PageNumber,
                PageSize, TotalPages, NextPageNumber, PreviousPageNumber);
    }
    public static PagingHeader GetEmptyHeader()
    {
        return new PagingHeader(
                0, 0,
                0, 0, 0, 0);
    }

}

public class PagingHeader
{
    public PagingHeader(
        int totalItems, int pageNumber, int pageSize, int totalPages, int nextPage, int previousPage)
    {
        TotalItems = totalItems;
        PageNumber = pageNumber;
        PageSize = pageSize;
        TotalPages = totalPages;
        NextPage = nextPage;
        PreviousPage = previousPage;
    }

    public int TotalItems { get; }
    public int PageNumber { get; }
    public int PageSize { get; }
    public int TotalPages { get; }
    public int NextPage { get; }
    public int PreviousPage { get; }
    public dynamic? Result { get; set; }
}
