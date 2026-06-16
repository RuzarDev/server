namespace Crm.Api.Application.Sorting;

public sealed class SortMappingProvider                                                                                                                                                                                            
{                                                                                                                                                                                                                                  
    private readonly IEnumerable<ISortMappingDefinition> _definitions;                                                                                                                                                             
                                                                                                                                                                                                                                     
    public SortMappingProvider(IEnumerable<ISortMappingDefinition> definitions)                                                                                                                                                    
    {                                                                                                                                                                                                                              
        _definitions = definitions;                                                                                                                                                                                                
    }

    public SortMapping[] GetMappings<TSource, TDestination>()
    {
        var sortMappingDefinition = _definitions.OfType<SortMappingDefinition<TSource, TDestination>>().FirstOrDefault();
        return sortMappingDefinition is null ? throw new InvalidOperationException($"Mapping for {typeof(TSource).Name} not defined") : sortMappingDefinition.Mappings;
    }
    public bool ValidateMappings<TSource, TDestination>(string? sort)
    {
        if (string.IsNullOrEmpty(sort)) return true;
        var mappings = GetMappings<TSource, TDestination>();
        var sortFields = sort                                                                                                                                                                                                              
            .Split(',')                                                                                                                                                                                                                    
            .Select(f => f.Trim().Split(' ')[0])                                                                                                                                                                                           
            .ToList();
        return sortFields.All(f => mappings.Any(m => m.SortField == f));
    }  
}