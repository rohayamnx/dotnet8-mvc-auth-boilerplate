using dotnet8_mvc_auth_boilerplate.Models;
using Microsoft.AspNetCore.Mvc;

namespace dotnet8_mvc_auth_boilerplate.Extensions
{
    public static class PaginationExtensions
    {
        /// <summary>
        /// Creates a PaginationInfo object for use with the Pagination view component
        /// </summary>
        /// <typeparam name="T">The type of items being paginated</typeparam>
        /// <param name="paginatedList">The paginated list</param>
        /// <param name="actionName">The action name for pagination links</param>
        /// <param name="controllerName">The controller name for pagination links</param>
        /// <param name="additionalRouteValues">Additional route values to include in pagination links</param>
        /// <returns>A configured PaginationInfo object</returns>
        public static PaginationInfo GetPaginationInfo<T>(
            this PaginatedList<T> paginatedList, 
            string actionName, 
            string controllerName, 
            object? additionalRouteValues = null)
        {
            var routeValues = new Dictionary<string, string>
            {
                { "pageSize", paginatedList.PageSize.ToString() }
            };

            // Add any additional route values
            if (additionalRouteValues != null)
            {
                var properties = additionalRouteValues.GetType().GetProperties();
                foreach (var prop in properties)
                {
                    var value = prop.GetValue(additionalRouteValues)?.ToString();
                    if (!string.IsNullOrEmpty(value))
                    {
                        routeValues[prop.Name] = value;
                    }
                }
            }

            return new PaginationInfo
            {
                CurrentPage = paginatedList.PageIndex,
                TotalPages = paginatedList.TotalPages,
                TotalItems = paginatedList.TotalCount,
                PageSize = paginatedList.PageSize,
                HasPrevious = paginatedList.HasPreviousPage,
                HasNext = paginatedList.HasNextPage,
                ActionName = actionName,
                ControllerName = controllerName,
                RouteValues = routeValues
            };
        }
    }
}