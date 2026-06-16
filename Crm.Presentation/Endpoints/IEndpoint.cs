using Microsoft.AspNetCore.Routing;

namespace Crm.Presentation.Endpoints;

public interface IEndpoint
{
    void  MapEndpoint(IEndpointRouteBuilder app);
}