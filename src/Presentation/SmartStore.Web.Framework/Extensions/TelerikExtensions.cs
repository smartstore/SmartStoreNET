using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Web.Mvc;
using Telerik.Web.Mvc;
using Telerik.Web.Mvc.Extensions;
using Telerik.Web.Mvc.UI.Fluent;

namespace SmartStore.Web.Framework
{
    [Serializable]
    public class GridStateInfo
    {
        public GridStateInfo.GridState State { get; set; }
        public string Path { get; set; }

        [Serializable]
        public class GridState
        {
            public string Filter { get; set; }
            public string GroupBy { get; set; }
            public string OrderBy { get; set; }
            public int Page { get; set; }
            public int Size { get; set; }
        }
    }

    public static class TelerikExtensions
    {
        [SuppressMessage("ReSharper", "Mvc.AreaNotResolved")]
        public static GridBuilder<T> PreserveGridState<T>(this GridBuilder<T> builder) where T : class
        {
            var grid = builder.ToComponent();

            if (!grid.DataBinding.Ajax.Enabled)
                return builder;

            if (grid.Id.IsEmpty())
                throw new SmartException("A grid with preservable state must have a valid Id or Name");

            var urlHelper = new UrlHelper(grid.ViewContext.RequestContext);

            var gridId = "GridState." + grid.Id + "__" + grid.ViewContext.RouteData.GenerateRouteIdentifier();

            grid.AppendCssClass("grid-preservestate");
            grid.HtmlAttributes.Add("data-statepreserver-href", urlHelper.Action("SetGridState", "Common", new { area = "admin" }));
            grid.HtmlAttributes.Add("data-statepreserver-key", gridId);

            // Try restore state from a previous request
            var info = (GridStateInfo)grid.ViewContext.TempData[gridId];

            if (info == null)
                return builder;

            var state = info.State;
            var command = GridCommand.Parse(state.Page, state.Size, state.OrderBy, state.GroupBy, state.Filter);

            if (grid.Paging.Enabled)
            {
                var pathChanged = !info.Path.Equals(grid.ViewContext.HttpContext.Request.RawUrl, StringComparison.OrdinalIgnoreCase);
                if (!pathChanged)
                {
                    if (command.PageSize > 0)
                        grid.Paging.PageSize = command.PageSize;
                    if (command.Page > 0)
                        grid.Paging.CurrentPage = command.Page;
                }
            }

            if (grid.Sorting.Enabled)
            {
                grid.Sorting.OrderBy.Clear();

                foreach (var sort in command.SortDescriptors)
                {
                    var existingSort = grid.Sorting.OrderBy.FirstOrDefault(x => x.Member.IsCaseInsensitiveEqual(sort.Member));
                    if (existingSort != null)
                    {
                        grid.Sorting.OrderBy.Remove(existingSort);
                    }
                    grid.Sorting.OrderBy.Add(sort);
                }
            }

            if (grid.Grouping.Enabled)
            {
                grid.Grouping.Groups.Clear();

                foreach (var group in command.GroupDescriptors)
                {
                    var existingGroup = grid.Grouping.Groups.FirstOrDefault(x => x.Member.IsCaseInsensitiveEqual(group.Member));
                    if (existingGroup != null)
                    {
                        grid.Grouping.Groups.Remove(existingGroup);
                    }
                    grid.Grouping.Groups.Add(group);
                }
            }

            if (grid.Filtering.Enabled)
            {
                grid.Filtering.Filters.Clear();

                foreach (var filter in command.FilterDescriptors)
                {
                    var compositeFilter = filter as CompositeFilterDescriptor;
                    if (compositeFilter == null)
                    {
                        compositeFilter = new CompositeFilterDescriptor { LogicalOperator = FilterCompositionLogicalOperator.And };
                        compositeFilter.FilterDescriptors.Add(filter);
                    }
                    grid.Filtering.Filters.Add(compositeFilter);
                }
            }

            // persist again for the next request
            grid.ViewContext.TempData[gridId] = info;

            return builder;
        }

        public static IEnumerable<T> ForCommand<T>(this IEnumerable<T> current, GridCommand command)
        {
            var query = current.AsQueryable() as IQueryable;
            if (command.FilterDescriptors.Any())
            {
                query = query.Where(command.FilterDescriptors.AsEnumerable()).AsQueryable() as IQueryable;
            }

            IList<SortDescriptor> temporarySortDescriptors = new List<SortDescriptor>();

            if (!command.SortDescriptors.Any() && query.Provider.IsEntityFrameworkProvider())
            {
                // The Entity Framework provider demands OrderBy before calling Skip.
                SortDescriptor sortDescriptor = new SortDescriptor
                {
                    Member = GetFirstSortableProperty(query.ElementType)
                };
                command.SortDescriptors.Add(sortDescriptor);
                temporarySortDescriptors.Add(sortDescriptor);
            }

            if (command.GroupDescriptors.Any())
            {
                command.GroupDescriptors.Reverse().Each(groupDescriptor =>
                {
                    SortDescriptor sortDescriptor = new SortDescriptor
                    {
                        Member = groupDescriptor.Member,
                        SortDirection = groupDescriptor.SortDirection
                    };

                    command.SortDescriptors.Insert(0, sortDescriptor);
                    temporarySortDescriptors.Add(sortDescriptor);
                });
            }

            if (command.SortDescriptors.Any())
            {
                query = query.Sort(command.SortDescriptors);
            }

            return query as IQueryable<T>;
        }

        public static IEnumerable<T> PagedForCommand<T>(this IEnumerable<T> current, GridCommand command)
        {
            return current.Skip((command.Page - 1) * command.PageSize).Take(command.PageSize);
        }


        public static GridBoundColumnBuilder<T> Centered<T>(this GridBoundColumnBuilder<T> columnBuilder) where T : class
        {
            return columnBuilder.HtmlAttributes(new { align = "center" }).HeaderHtmlAttributes(new { style = "text-align:center;" });
        }

        public static GridTemplateColumnBuilder<T> Centered<T>(this GridTemplateColumnBuilder<T> columnBuilder) where T : class
        {
            return columnBuilder.HtmlAttributes(new { align = "center" }).HeaderHtmlAttributes(new { style = "text-align:center;" });
        }

        public static GridBoundColumnBuilder<T> RightAlign<T>(this GridBoundColumnBuilder<T> columnBuilder) where T : class
        {
            return columnBuilder.HtmlAttributes(new { style = "text-align:right;" }).HeaderHtmlAttributes(new { style = "text-align:right;" });
        }

        public static GridTemplateColumnBuilder<T> RightAlign<T>(this GridTemplateColumnBuilder<T> columnBuilder) where T : class
        {
            return columnBuilder.HtmlAttributes(new { style = "text-align:right;" }).HeaderHtmlAttributes(new { style = "text-align:right;" });
        }

        public static string GetValueFromAppliedFilter(this IFilterDescriptor filter, string valueName, FilterOperator? filterOperator = null)
        {
            if (filter is CompositeFilterDescriptor)
            {
                foreach (IFilterDescriptor childFilter in ((CompositeFilterDescriptor)filter).FilterDescriptors)
                {
                    var val1 = GetValueFromAppliedFilter(childFilter, valueName, filterOperator);
                    if (!String.IsNullOrEmpty(val1))
                        return val1;
                }
            }
            else
            {
                var filterDescriptor = (FilterDescriptor)filter;
                if (filterDescriptor != null &&
                    filterDescriptor.Member.Equals(valueName, StringComparison.InvariantCultureIgnoreCase))
                {
                    if (!filterOperator.HasValue || filterDescriptor.Operator == filterOperator.Value)
                        return Convert.ToString(filterDescriptor.Value);
                }
            }

            return "";
        }

        public static string GetValueFromAppliedFilters(this IList<IFilterDescriptor> filters, string valueName, FilterOperator? filterOperator = null)
        {
            foreach (var filter in filters)
            {
                var val1 = GetValueFromAppliedFilter(filter, valueName, filterOperator);
                if (!String.IsNullOrEmpty(val1))
                    return val1;
            }
            return "";
        }

        private static string GetFirstSortableProperty(Type type)
        {
            PropertyInfo firstSortableProperty = type.GetProperties().Where(property => property.PropertyType.IsPredefinedSimpleType()).FirstOrDefault();

            if (firstSortableProperty == null)
            {
                throw new NotSupportedException("Cannot find property to sort by.");
            }

            return firstSortableProperty.Name;
        }
    }
}
