using System.Linq.Dynamic.Core;
namespace Crm.Api.Application.Sorting;

public static class QueryableExtensions
{
    public static IQueryable<T> Sort<T>(this IQueryable<T> query, string? sort, SortMapping[] mappings)
    {
        if (string.IsNullOrWhiteSpace(sort))
        {
            return query.OrderBy("Id");
        }
        var sortFields = sort.Split(',')                                                                                                                                                                                              
            .Select(s => s.Trim())                                                                                                                                                                                                         
            .ToArray();
       var orderByParts = new List<string>();                                                                                                                                                        
                                                                                                                                                                                                                                     
       foreach (var field in sortFields)                                                                                                                                                                                                  
       {                                                                                                                                                                                                                                  
           string[] parts = field.Split(' ');                                                                                                                                                                                             
           string sortField = parts[0];                                                                                                                                                                                                   
           bool isDescending = parts.Length > 1 && parts[1].Equals("desc", StringComparison.OrdinalIgnoreCase);                                                                                                                           
                                                                                                                                                                                                                                     
           var mapping = mappings.FirstOrDefault(m =>                                                                                                                                                                                     
               m.SortField.Equals(sortField, StringComparison.OrdinalIgnoreCase));                                                                                                                       
                                                                                                                                                                                                                                     
           if (mapping is null) continue;                                                                                                                                                                             
                                                                                                                                                                                                                                     
           string direction = isDescending ? "DESC" : "ASC";                                                                                                                                                                              
           orderByParts.Add($"{mapping.PropertyName} {direction}");                                                                                                                                                   
       }                                                                                                                                                                                                                                  
                                                                                                                                                                                                                                     
       if (orderByParts.Count == 0) return query.OrderBy("Id");                                                                                                                                                                           
       return query.OrderBy(string.Join(",", orderByParts));                                                                                                                                                                              
    }
}