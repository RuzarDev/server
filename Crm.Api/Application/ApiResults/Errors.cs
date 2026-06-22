using Crm.Api.Application.Messaging;
using Crm.Domain.Entities;

namespace Crm.Api.Application.ApiResults;

public static class Errors
{
    public static class Contacts
    {
        public static readonly Error Exists =  new Error("Contact.Exists", "The contact already exist", 409);
        public static readonly Error NotFound =  new Error("Contact.NotFound", "The contact not found", 404);
    }

    public static class Validator
    {
        public static readonly Error Problem = new Error("Validation.Problem", "One of validations didn't pass", 400);
    }
}