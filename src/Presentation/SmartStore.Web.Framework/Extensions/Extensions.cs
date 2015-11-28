using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web.Mvc;
using SmartStore.Core;
using SmartStore.Core.Domain.Stores;
using SmartStore.Core.Infrastructure;
using SmartStore.Core.Localization;
using SmartStore.Core.Plugins;
using SmartStore.Services.Configuration;
using SmartStore.Services.Helpers;
using SmartStore.Services.Localization;
using SmartStore.Web.Framework.Localization;
using Telerik.Web.Mvc;
using Telerik.Web.Mvc.Extensions;
using Telerik.Web.Mvc.UI.Fluent;

namespace SmartStore.Web.Framework
{

	[Serializable]
	public class GridStateInfo
	{
		public GridState State { get; set; }
		public string Path { get; set; }
	}

	
	public static class Extensions
    {

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
            var queryable = current.AsQueryable() as IQueryable;
            if (command.FilterDescriptors.Any())
            {
                queryable = queryable.Where(command.FilterDescriptors.AsEnumerable()).AsQueryable() as IQueryable;
            }

            IList<SortDescriptor> temporarySortDescriptors = new List<SortDescriptor>();

            if (!command.SortDescriptors.Any() && queryable.Provider.IsEntityFrameworkProvider())
            {
                // The Entity Framework provider demands OrderBy before calling Skip.
                SortDescriptor sortDescriptor = new SortDescriptor
                {
                    Member = queryable.ElementType.FirstSortableProperty()
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
                queryable = queryable.Sort(command.SortDescriptors);
            }

            return queryable as IQueryable<T>;
        }

        public static IEnumerable<T> PagedForCommand<T>(this IEnumerable<T> current, GridCommand command)
        {
            return current.Skip((command.Page - 1) * command.PageSize).Take(command.PageSize);
        }

        public static bool IsEntityFrameworkProvider(this IQueryProvider provider)
        {
            return provider.GetType().FullName == "System.Data.Objects.ELinq.ObjectQueryProvider";
        }

        public static bool IsLinqToObjectsProvider(this IQueryProvider provider)
        {
            return provider.GetType().FullName.Contains("EnumerableQuery");
        }

        public static string FirstSortableProperty(this Type type)
        {
            PropertyInfo firstSortableProperty = type.GetProperties().Where(property => property.PropertyType.IsPredefinedType()).FirstOrDefault();

            if (firstSortableProperty == null)
            {
                throw new NotSupportedException("Cannot find property to sort by.");
            }

            return firstSortableProperty.Name;
        }

        internal static bool IsPredefinedType(this Type type)
        {
            return PredefinedTypes.Any(t => t == type);
        }

        public static readonly Type[] PredefinedTypes = {
            typeof(Object),
            typeof(Boolean),
            typeof(Char),
            typeof(String),
            typeof(SByte),
            typeof(Byte),
            typeof(Int16),
            typeof(UInt16),
            typeof(Int32),
            typeof(UInt32),
            typeof(Int64),
            typeof(UInt64),
            typeof(Single),
            typeof(Double),
            typeof(Decimal),
            typeof(DateTime),
            typeof(TimeSpan),
            typeof(Guid),
            typeof(Math),
            typeof(Convert)
        };

        public static GridBoundColumnBuilder<T> Centered<T>(this GridBoundColumnBuilder<T> columnBuilder) where T:class
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

        public static SelectList ToSelectList<TEnum>(this TEnum enumObj, bool markCurrentAsSelected = true) where TEnum : struct
        {
            if (!typeof(TEnum).IsEnum)
                throw new ArgumentException("An Enumeration type is required.", "enumObj");

            var localizationService = EngineContext.Current.Resolve<ILocalizationService>();
            var workContext = EngineContext.Current.Resolve<IWorkContext>();

            var values = from TEnum enumValue in Enum.GetValues(typeof(TEnum))
                         select new { ID = Convert.ToInt32(enumValue), Name = enumValue.GetLocalizedEnum(localizationService, workContext) };
            object selectedValue = null;
            if (markCurrentAsSelected)
                selectedValue = Convert.ToInt32(enumObj);
            return new SelectList(values, "ID", "Name", selectedValue);
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

        /// <summary>
        /// Relative formatting of DateTime (e.g. 2 hours ago, a month ago)
        /// </summary>
        /// <param name="source">Source (UTC format)</param>
        /// <returns>Formatted date and time string</returns>
        public static string RelativeFormat(this DateTime source)
        {
            return RelativeFormat(source, string.Empty);
        }

        /// <summary>
        /// Relative formatting of DateTime (e.g. 2 hours ago, a month ago)
        /// </summary>
        /// <param name="source">Source (UTC format)</param>
        /// <param name="defaultFormat">Default format string (in case relative formatting is not applied)</param>
        /// <returns>Formatted date and time string</returns>
        public static string RelativeFormat(this DateTime source, string defaultFormat)
        {
            return RelativeFormat(source, false, defaultFormat);
        }

        /// <summary>
        /// Relative formatting of DateTime (e.g. 2 hours ago, a month ago)
        /// </summary>
        /// <param name="source">Source (UTC format)</param>
        /// <param name="convertToUserTime">A value indicating whether we should convet DateTime instance to user local time (in case relative formatting is not applied)</param>
        /// <param name="defaultFormat">Default format string (in case relative formatting is not applied)</param>
        /// <returns>Formatted date and time string</returns>
        public static string RelativeFormat(this DateTime source, bool convertToUserTime, string defaultFormat)
        {
            string result = "";
			Localizer T = EngineContext.Current.Resolve<IText>().Get;
            
            var ts = new TimeSpan(DateTime.UtcNow.Ticks - source.Ticks);
            double delta = ts.TotalSeconds;

            if (delta > 0)
            {
                if (delta < 60) // 60 (seconds)
                {
					result = ts.Seconds == 1 ? T("Time.OneSecondAgo") : T("Time.SecondsAgo", ts.Seconds);
                }
                else if (delta < 120) //2 (minutes) * 60 (seconds)
                {
					result = T("Time.OneMinuteAgo");
                }
                else if (delta < 2700) // 45 (minutes) * 60 (seconds)
                {
					result = String.Format(T("Time.MinutesAgo"), ts.Minutes);
                }
                else if (delta < 5400) // 90 (minutes) * 60 (seconds)
                {
					result = T("Time.OneHourAgo");
                }
                else if (delta < 86400) // 24 (hours) * 60 (minutes) * 60 (seconds)
                {
                    int hours = ts.Hours;
                    if (hours == 1)
                        hours = 2;
					result = T("Time.HoursAgo", hours);
                }
                else if (delta < 172800) // 48 (hours) * 60 (minutes) * 60 (seconds)
                {
					result = T("Time.Yesterday");
                }
                else if (delta < 2592000) // 30 (days) * 24 (hours) * 60 (minutes) * 60 (seconds)
                {
					result = String.Format(T("Time.DaysAgo"), ts.Days);
                }
                else if (delta < 31104000) // 12 (months) * 30 (days) * 24 (hours) * 60 (minutes) * 60 (seconds)
                {
                    int months = Convert.ToInt32(Math.Floor((double)ts.Days / 30));
					result = months <= 1 ? T("Time.OneMonthAgo") : T("Time.MonthsAgo", months);
                }
                else
                {
                    int years = Convert.ToInt32(Math.Floor((double)ts.Days / 365));
					result = years <= 1 ? T("Time.OneYearAgo") : T("Time.YearsAgo", years);
                }
            }
            else
            {
                DateTime tmp1 = source;
                if (convertToUserTime)
                {
                    tmp1 = EngineContext.Current.Resolve<IDateTimeHelper>().ConvertToUserTime(tmp1, DateTimeKind.Utc);
                }
                //default formatting
                if (!String.IsNullOrEmpty(defaultFormat))
                {
                    result = tmp1.ToString(defaultFormat);
                }
                else
                {
                    result = tmp1.ToString();
                }
            }
            return result;
        }

		public static string Prettify(this TimeSpan ts)
		{
			Localizer T = EngineContext.Current.Resolve<IText>().Get;
			double seconds = ts.TotalSeconds;

			try
			{
				int secsTemp = Convert.ToInt32(seconds);
				string label = T("Time.SecondsAbbr");
				int remainder = 0;
				string remainderLabel = "";

				if (secsTemp > 59)
				{
					remainder = secsTemp % 60;
					secsTemp /= 60;
					label = T("Time.MinutesAbbr");
					remainderLabel = T("Time.SecondsAbbr");
				}

				if (secsTemp > 59)
				{
					remainder = secsTemp % 60;
					secsTemp /= 60;
					label = (secsTemp == 1) ? T("Time.HourAbbr") : T("Time.HoursAbbr");
					remainderLabel = T("Time.MinutesAbbr");
				}

				if (remainder == 0)
				{
					return string.Format("{0:#,##0.#} {1}", secsTemp, label);
				}
				else
				{
					return string.Format("{0:#,##0} {1} {2} {3}", secsTemp, label, remainder, remainderLabel);
				}
			}
			catch
			{
				return "(-)";
			}
		}

		/// <summary>
		/// Get a list of all stores
		/// </summary>
		/// <remarks>codehint: sm-add</remarks>
		public static IList<SelectListItem> ToSelectListItems(this IEnumerable<Store> stores)
		{
			var lst = new List<SelectListItem>();

			foreach (var store in stores)
			{
				lst.Add(new SelectListItem
				{
					Text = store.Name,
					Value = store.Id.ToString()
				});
			}
			return lst;
		}

		public static void SelectValue(this List<SelectListItem> lst, string value, string defaultValue = null)
		{
			if (lst != null)
			{
				var itm = lst.FirstOrDefault(i => i.Value.IsCaseInsensitiveEqual(value));

				if (itm == null && defaultValue != null)
					itm = lst.FirstOrDefault(i => i.Value.IsCaseInsensitiveEqual(defaultValue));

				if (itm != null)
					itm.Selected = true;
			}
		}

		/// <summary>
		/// Determines whether a plugin is installed and activated for a particular store.
		/// </summary>
		public static bool IsPluginReady(this IPluginFinder pluginFinder, ISettingService settingService, string systemName, int storeId)
		{
			try
			{
				var pluginDescriptor = pluginFinder.GetPluginDescriptorBySystemName(systemName);

				if (pluginDescriptor != null && pluginDescriptor.Installed)
				{
					if (storeId == 0 || settingService.GetSettingByKey<string>(pluginDescriptor.GetSettingKey("LimitedToStores")).ToIntArrayContains(storeId, true))
						return true;
				}
			}
			catch (Exception exc)
			{
				exc.Dump();
			}
			return false;
		}
    }
}
