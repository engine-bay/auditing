﻿namespace EngineBay.Auditing
{

    using EngineBay.Core;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Routing;

    public static class AuditEntryEndpoints
    {
        private static readonly string[] AuditTags = { ApiGroupNames.AuditEntries };

        public static void MapEndpoints(IEndpointRouteBuilder endpoints)
        {
            endpoints.MapGet(
                "/audit-entries/{id:guid}",
                async (GetAuditEntry query, Guid id, CancellationToken cancellation) =>
            {
                var getAuditEntryRequest = new GetAuditEntryRequest(id);
                var dto = await query.Handle(getAuditEntryRequest, cancellation);
                return Results.Ok(dto);
            })
              //.RequireAuthorization(ModulePolicies.ViewAuditEntries)
              .WithTags(AuditTags);

            endpoints.MapGet(
                "/audit-entries",
                async (QueryAuditEntries query, int? skip, int? limit, string? sortBy, SortOrderType? sortOrder, CancellationToken cancellation) =>
                {
                    var paginationParameters = new PaginationParameters(skip, limit, sortBy, sortOrder);
                    var queryAuditEntriesRequest = new QueryAuditEntriesRequest(paginationParameters);

                    var paginatedDtos = await query.Handle(queryAuditEntriesRequest, cancellation);
                    return Results.Ok(paginatedDtos);
                })
              //.RequireAuthorization(ModulePolicies.ViewAuditEntries)
              .WithTags(AuditTags);

            // Don't expose create entry as an endpoint, only our app should have access to it
        }
    }
}
