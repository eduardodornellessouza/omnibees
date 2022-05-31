using OB.DL.Common.Filter;
using OB.DL.Common.Infrastructure.Impl;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Common;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Linq.Expressions;
using Kendo.DynamicLinq.Extensions;
using dynLinq = Kendo.DynamicLinq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Linq.Dynamic;

namespace OB.DL.Common
{
    public static class QueryableExtensions
    {
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        [SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters")]
        public static IQueryable<T> AsNoTracking<T>(this IQueryable<T> queryable) where T : class
        {
            return System.Data.Entity.QueryableExtensions.AsNoTracking<T>(queryable);
        }

        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        [SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters")]
        public static IQueryable<T> Include<T>(this IQueryable<T> queryable, string path)
        {
            return System.Data.Entity.QueryableExtensions.Include<T>(queryable, path);
        }

        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        [SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters")]
        public static IQueryable<T> Include<T, TProperty>(this IQueryable<T> source, Expression<Func<T, TProperty>> path)
        {
            return System.Data.Entity.QueryableExtensions.Include(source, path);
        }

        //
        // Summary:
        //     Creates a System.Collections.Generic.List<T> from an System.Linq.IQueryable<T>
        //     by enumerating it asynchronously.
        //
        // Parameters:
        //   source:
        //     An System.Linq.IQueryable<T> to create a System.Collections.Generic.List<T>
        //     from.
        //
        // Type parameters:
        //   TSource:
        //     The type of the elements of source.
        //
        // Returns:
        //     A task that represents the asynchronous operation.  The task result contains
        //     a System.Collections.Generic.List<T> that contains elements from the input
        //     sequence.
        //
        // Remarks:
        //     Multiple active operations on the same context instance are not supported.
        //     Use 'await' to ensure that any asynchronous operations have completed before
        //     calling another method on this context.
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public static Task<List<TSource>> ToListAsync<TSource>(this IQueryable<TSource> source)
        {
            return System.Data.Entity.QueryableExtensions.ToListAsync<TSource>(source);
        }

        //
        // Summary:
        //     Asynchronously returns the number of elements in a sequence.
        //
        // Parameters:
        //   source:
        //     An System.Linq.IQueryable<T> that contains the elements to be counted.
        //
        // Type parameters:
        //   TSource:
        //     The type of the elements of source.
        //
        // Returns:
        //     A task that represents the asynchronous operation.  The task result contains
        //     the number of elements in the input sequence.
        //
        // Exceptions:
        //   System.ArgumentNullException:
        //     source is null .
        //
        //   System.InvalidOperationException:
        //     source doesn't implement System.Data.Entity.Infrastructure.IDbAsyncQueryProvider
        //     .
        //
        //   System.OverflowException:
        //     The number of elements in source is larger than System.Int32.MaxValue .
        //
        // Remarks:
        //     Multiple active operations on the same context instance are not supported.
        //     Use 'await' to ensure that any asynchronous operations have completed before
        //     calling another method on this context.
        public static Task<int> CountAsync<TEntity>(this IQueryable<TEntity> source)
        {
            return System.Data.Entity.QueryableExtensions.CountAsync(source);
        }

        public static IQueryable<TEntity> Paginate<TEntity>(this IQueryable<TEntity> source, int pageSize, int pageIndex)
        {
            if(pageSize > 0)
            {
                return source.Skip(pageSize * pageIndex).Take(pageSize);
            }

            return source;
        }
        //
        // Summary:
        //     Asynchronously returns the number of elements in a sequence.
        //
        // Parameters:
        //   source:
        //     An System.Linq.IQueryable<T> that contains the elements to be counted.
        //
        //   cancellationToken:
        //     A System.Threading.CancellationToken to observe while waiting for the task
        //     to complete.
        //
        // Type parameters:
        //   TSource:
        //     The type of the elements of source.
        //
        // Returns:
        //     A task that represents the asynchronous operation.  The task result contains
        //     the number of elements in the input sequence.
        //
        // Exceptions:
        //   System.ArgumentNullException:
        //     source is null .
        //
        //   System.InvalidOperationException:
        //     source doesn't implement System.Data.Entity.Infrastructure.IDbAsyncQueryProvider
        //     .
        //
        //   System.OverflowException:
        //     The number of elements in source is larger than System.Int32.MaxValue .
        //
        // Remarks:
        //     Multiple active operations on the same context instance are not supported.
        //     Use 'await' to ensure that any asynchronous operations have completed before
        //     calling another method on this context.
        public static Task<int> CountAsync<TEntity>(this IQueryable<TEntity> source, CancellationToken cancellationToken)
        {
            return System.Data.Entity.QueryableExtensions.CountAsync(source, cancellationToken);
        }

        //
        // Summary:
        //     Asynchronously returns the number of elements in a sequence that satisfy
        //     a condition.
        //
        // Parameters:
        //   source:
        //     An System.Linq.IQueryable<T> that contains the elements to be counted.
        //
        //   predicate:
        //     A function to test each element for a condition.
        //
        // Type parameters:
        //   TSource:
        //     The type of the elements of source.
        //
        // Returns:
        //     A task that represents the asynchronous operation.  The task result contains
        //     the number of elements in the sequence that satisfy the condition in the
        //     predicate function.
        //
        // Exceptions:
        //   System.ArgumentNullException:
        //     source or predicate is null .
        //
        //   System.InvalidOperationException:
        //     source doesn't implement System.Data.Entity.Infrastructure.IDbAsyncQueryProvider
        //     .
        //
        //   System.OverflowException:
        //     The number of elements in source that satisfy the condition in the predicate
        //     function is larger than System.Int32.MaxValue .
        //
        // Remarks:
        //     Multiple active operations on the same context instance are not supported.
        //     Use 'await' to ensure that any asynchronous operations have completed before
        //     calling another method on this context.
        public static Task<int> CountAsync<TEntity>(this IQueryable<TEntity> source, Expression<Func<TEntity, bool>> predicate)
        {
            return System.Data.Entity.QueryableExtensions.CountAsync(source, predicate);
        }

        //
        // Summary:
        //     Asynchronously returns the number of elements in a sequence that satisfy
        //     a condition.
        //
        // Parameters:
        //   source:
        //     An System.Linq.IQueryable<T> that contains the elements to be counted.
        //
        //   predicate:
        //     A function to test each element for a condition.
        //
        //   cancellationToken:
        //     A System.Threading.CancellationToken to observe while waiting for the task
        //     to complete.
        //
        // Type parameters:
        //   TSource:
        //     The type of the elements of source.
        //
        // Returns:
        //     A task that represents the asynchronous operation.  The task result contains
        //     the number of elements in the sequence that satisfy the condition in the
        //     predicate function.
        //
        // Exceptions:
        //   System.ArgumentNullException:
        //     source or predicate is null .
        //
        //   System.InvalidOperationException:
        //     source doesn't implement System.Data.Entity.Infrastructure.IDbAsyncQueryProvider
        //     .
        //
        //   System.OverflowException:
        //     The number of elements in source that satisfy the condition in the predicate
        //     function is larger than System.Int32.MaxValue .
        //
        // Remarks:
        //     Multiple active operations on the same context instance are not supported.
        //     Use 'await' to ensure that any asynchronous operations have completed before
        //     calling another method on this context.
        public static Task<int> CountAsync<TEntity>(this IQueryable<TEntity> source, Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken)
        {
            return System.Data.Entity.QueryableExtensions.CountAsync(source, predicate, cancellationToken);
        }

        /// <summary>
        /// Creates a ConnectionScope that manages the lifetime of a ADO.NET Connection object closing it when the scope is disposed.
        /// Returns a handle on an IDisposable that can be used to safely control the lifetime
        /// of an open connection. If the connection is closed, it will be opened immediately
        /// and closed when the result of this method (the scope) is disposed. If the connection is already
        /// open, it remains open.
        /// <code>
        /// // Example with CreateConnectionScope
        /// using (command.Connection.CreateConnectionScope())
        /// {
        ///     command.ExecuteNonQuery();
        /// }
        ///
        /// // Example without
        /// bool connectionOpened = command.Connection.State == ConnectionState.Closed;
        /// if (connectionOpened)
        /// {
        ///     command.Connection.Open();
        /// }
        /// try
        /// {
        ///     command.ExecuteNonQuery();
        /// }
        /// finally
        /// {
        ///     if (connectionOpened &amp;&amp; command.Connection.State == ConnectionState.Open)
        ///     {
        ///         command.Connection.Close();
        ///     }
        /// }
        /// </code>
        /// </summary>
        /// <param name="connection">Connection to open.</param>
        /// <returns>Scope closing the connection on dispose.</returns>
        public static IDisposable CreateConnectionScope(this DbConnection connection)
        {
            return new OpenConnectionLifetime(connection);
        }

        #region OrderBy Extension

        //public static IEnumerable<T> OrderBy<T>(this IEnumerable<T> enumerable, string orderBy)
        //{
        //    return enumerable.AsQueryable().OrderBy(orderBy).AsEnumerable();
        //}

        //public static IQueryable<T> OrderBy<T>(this IQueryable<T> collection, string orderBy)
        //{
        //    foreach (SortByInfo orderByInfo in ParseOrderBy(orderBy))
        //        collection = ApplyOrderBy<T>(collection, orderByInfo);

        //    return collection;
        //}

        public static IEnumerable<T> OrderBy<T>(this IEnumerable<T> enumerable, List<SortByInfo> orderBy)
        {
            return enumerable.AsQueryable().OrderBy(orderBy).AsEnumerable();
        }

        public static IQueryable<T> OrderBy<T>(this IQueryable<T> collection, List<SortByInfo> orderBy)
        {
            // One order must allways have initial to true
            if (orderBy.Any() && !orderBy.Any(x => x.Initial))
                orderBy.First().Initial = true;
            else
                orderBy = orderBy.OrderByDescending(x => x.Initial).ToList();

            foreach (SortByInfo orderByInfo in orderBy)
                collection = ApplyOrderBy<T>(collection, orderByInfo);

            return collection;
        }

        private static IQueryable<T> ApplyOrderBy<T>(IQueryable<T> collection, SortByInfo orderByInfo)
        {
            string[] props = orderByInfo.OrderBy.Split('.');
            Type type = typeof(T);

            ParameterExpression arg = Expression.Parameter(type, "x");
            Expression expr = arg;

            if(props.Length > 0)
            {
                PropertyInfo pi = type.GetProperty(props[0]);

                if (pi == null)
                    return collection;

                if (!typeof(string).IsAssignableFrom(pi.PropertyType) && typeof(IEnumerable).IsAssignableFrom(pi.PropertyType))
                    return IEnumerableOrderBy(collection, orderByInfo);
                else
                    return NormalOrderBy(collection, orderByInfo);
            }

            return collection;
        }

        private static IQueryable<T> NormalOrderBy<T>(IQueryable<T> collection, SortByInfo orderByInfo)
        {
            string[] props = orderByInfo.OrderBy.Split('.');
            Type type = typeof(T);

            ParameterExpression arg = Expression.Parameter(type, "x");
            Expression expr = arg;

            foreach (string prop in props)
            {
                // use reflection (not ComponentModel) to mirror LINQ
                PropertyInfo pi = type.GetProperty(prop);

                if (pi == null)
                    return collection;

                expr = Expression.Property(expr, pi);
                type = pi.PropertyType;
            }

            Type delegateType = typeof(Func<,>).MakeGenericType(typeof(T), type);
            LambdaExpression lambda = Expression.Lambda(delegateType, expr, arg);
            string methodName = String.Empty;

            if (!orderByInfo.Initial && collection is IOrderedQueryable<T>)
            {
                if (orderByInfo.Direction == SortDirection.Ascending)
                    methodName = "ThenBy";
                else
                    methodName = "ThenByDescending";
            }
            else
            {
                if (orderByInfo.Direction == SortDirection.Ascending)
                    methodName = "OrderBy";
                else
                    methodName = "OrderByDescending";
            }

            //TODO: apply caching to the generic methodsinfos?
            return (IOrderedQueryable<T>)typeof(Queryable).GetMethods().Single(
                method => method.Name == methodName
                        && method.IsGenericMethodDefinition
                        && method.GetGenericArguments().Length == 2
                        && method.GetParameters().Length == 2)
                .MakeGenericMethod(typeof(T), type)
                .Invoke(null, new object[] { collection, lambda });

        }

        private static IQueryable<T> IEnumerableOrderBy<T>(IQueryable<T> collection, SortByInfo orderByInfo)
        {
            string[] props = orderByInfo.OrderBy.Split('.');

            if (props.Length > 2)
                throw new InvalidOperationException("Sorting IEnumerable only supports 1 nested level");

            string sortingOrder;
            if (orderByInfo.Direction == SortDirection.Ascending)
            {
                sortingOrder = "Min";
                string dynamicOrder = props[0] + "." + sortingOrder + "(" + props[1] + ")";
                return collection.OrderBy(dynamicOrder);
            }
            else
            {
                sortingOrder = "Max";
                string dynamicOrder = props[0] + "." + sortingOrder + "(" + props[1] + ") desc";
                return collection.OrderBy(dynamicOrder);
            }

            

            
        }

        private static IEnumerable<SortByInfo> ParseOrderBy(string orderBy)
        {
            if (String.IsNullOrEmpty(orderBy))
                yield break;

            string[] items = orderBy.Split(',');
            bool initial = true;
            foreach (string item in items)
            {
                string[] pair = item.Trim().Split(' ');

                if (pair.Length > 2)
                    throw new ArgumentException(String.Format("Invalid OrderBy string '{0}'. Order By Format: Property, Property2 ASC, Property2 DESC", item));

                string prop = pair[0].Trim();

                if (String.IsNullOrEmpty(prop))
                    throw new ArgumentException("Invalid Property. Order By Format: Property, Property2 ASC, Property2 DESC");

                SortDirection dir = SortDirection.Ascending;

                if (pair.Length == 2)
                    dir = ("desc".Equals(pair[1].Trim(), StringComparison.OrdinalIgnoreCase) ? SortDirection.Descending : SortDirection.Ascending);

                yield return new SortByInfo() { OrderBy = prop, Direction = dir, Initial = initial };

                initial = false;
            }

        }



        #endregion

        #region FilterBy Extension

        //TODO: eliminar método quando estiverem todos os filtos migrados para os da Kendo
        private static dynLinq.Filter ConvertToKendoFilter(List<FilterByInfo> filters)
        {
            var orFilters = filters.Where(x => x.Conjunction == FilterConjunction.OR).ToList();
            var andFilters = filters.Where(x => x.Conjunction == FilterConjunction.AND).ToList();

            var kendoFilter = new dynLinq.Filter()
            {
                Logic = dynLinq.FilterLogic.And,
                Filters = new List<dynLinq.Filter>()
            };

            // Create Or Filters
            if (orFilters.Any())
            {
                var convertedOrFilters = new List<dynLinq.Filter>();
                foreach (var filter in orFilters)
                {
                    convertedOrFilters.Add(new dynLinq.Filter()
                    {
                        Field = filter.FilterBy,
                        Operator = ConvertToKendoOperator(filter.Operator),
                        Value = filter.Value
                    });
                }
                ((List<dynLinq.Filter>)kendoFilter.Filters).Add(new dynLinq.Filter()
                {
                    Logic = dynLinq.FilterLogic.Or,
                    Filters = convertedOrFilters
                });
            }

            // Create And Filters
            if (andFilters.Any())
            {
                var convertedAndFilters = new List<dynLinq.Filter>();
                foreach (var filter in andFilters)
                {
                    convertedAndFilters.Add(new dynLinq.Filter()
                    {
                        Field = filter.FilterBy,
                        Operator = ConvertToKendoOperator(filter.Operator),
                        Value = filter.Value
                    });
                }
                ((List<dynLinq.Filter>)kendoFilter.Filters).AddRange(convertedAndFilters);
            }

            return kendoFilter;
        }

        //TODO: eliminar método quando estiverem todos os filtos migrados para os da Kendo
        private static dynLinq.FilterOperator ConvertToKendoOperator(FilterOperator filterOperator)
        {
            switch (filterOperator)
            {
                case DL.Common.Filter.FilterOperator.IsLessThan: return dynLinq.FilterOperator.IsLessThan;
                case DL.Common.Filter.FilterOperator.IsLessThanOrEqualTo: return dynLinq.FilterOperator.IsLessThanOrEqualTo;
                case DL.Common.Filter.FilterOperator.IsEqualTo: return dynLinq.FilterOperator.IsEqualTo;
                case DL.Common.Filter.FilterOperator.IsNotEqualTo: return dynLinq.FilterOperator.IsNotEqualTo;
                case DL.Common.Filter.FilterOperator.IsGreaterThanOrEqualTo: return dynLinq.FilterOperator.IsGreaterThanOrEqualTo;
                case DL.Common.Filter.FilterOperator.IsGreaterThan: return dynLinq.FilterOperator.IsGreaterThan;
                case DL.Common.Filter.FilterOperator.StartsWith: return dynLinq.FilterOperator.StartsWith;
                case DL.Common.Filter.FilterOperator.EndsWith: return dynLinq.FilterOperator.EndsWith;
                case DL.Common.Filter.FilterOperator.Contains: return dynLinq.FilterOperator.Contains;
                case DL.Common.Filter.FilterOperator.DoesNotContain: return dynLinq.FilterOperator.DoesNotContain;
                case DL.Common.Filter.FilterOperator.IsContainedIn: return dynLinq.FilterOperator.IsContainedIn;
                case DL.Common.Filter.FilterOperator.IsNotContainedIn: return dynLinq.FilterOperator.IsNotContainedIn;
                case DL.Common.Filter.FilterOperator.IsNull: return dynLinq.FilterOperator.IsNull;
                case DL.Common.Filter.FilterOperator.IsNotNull: return dynLinq.FilterOperator.IsNotNull;
                default: return default(dynLinq.FilterOperator);
            }
        }

        [Obsolete("Use Filter with Kendo Filter")]
        public static IEnumerable<T> FilterBy<T>(this IEnumerable<T> enumerable, List<FilterByInfo> filtorDescriptors)
        {
            return enumerable.AsQueryable().FilterBy(filtorDescriptors).AsEnumerable();

            //if (filtorDescriptors.Any(x => x.FilterBy.Contains(".")))
            //    return enumerable.AsQueryable().FilterBy(filtorDescriptors).AsEnumerable();
            //else
            //    return enumerable.FilterBy(ConvertToKendoFilter(filtorDescriptors)); // Uses new kendo filter
        }

        [Obsolete("Use Filter with Kendo Filter")]
        public static IQueryable<T> FilterBy<T>(this IQueryable<T> collection, List<FilterByInfo> filterDescriptors)
        {
            // Uses new kendo filter
            //if (!filterDescriptors.Any(x => x.FilterBy.Contains(".")))
            //    return collection.AsQueryable().FilterBy(ConvertToKendoFilter(filterDescriptors));

            var orFilters = filterDescriptors.Where(x => x.Conjunction == FilterConjunction.OR).ToList();
            var andFilters = filterDescriptors.Where(x => x.Conjunction == FilterConjunction.AND).ToList();

            // OR Filters
            if (orFilters != null && orFilters.Count > 0)
                collection = collection.Search<T>(orFilters);

            // AND Filters
            foreach (var filter in andFilters)
                collection = ApplyFiltering<T>(collection, filter);

            return collection;
        }

        #region Search (Filter with OR operator)

        [Obsolete]
        public static IQueryable<T> Search<T>(this IQueryable<T> source, List<FilterByInfo> filterDescriptors)
        {
            //Variable to hold merged 'OR' expression
            Expression orExpression = null;
            Type type = typeof(T);
            ParameterExpression arg = Expression.Parameter(type, "x");

            //Build a contains expression for each property
            foreach (var filter in filterDescriptors)
            {
                if (filter.Value != null)
                {
                    string[] props = filter.FilterBy.Split('.');
                    type = typeof(T);
                    Expression expr = arg;

                    PropertyInfo pi = type.GetProperty(props[0]);
                    expr = Expression.Property(expr, pi);
                    type = pi.PropertyType;

                    expr = BuildAccessors(expr, props, 1, filter);

                    if (expr == null)
                        continue;

                    orExpression = BuildOrExpression(orExpression, expr);
                }
            }

            if (orExpression == null)
                return source;

            var completeExpression = Expression.Lambda<Func<T, bool>>(orExpression, arg);
            return source.Where(completeExpression);
        }

        [Obsolete]
        private static Expression BuildOrExpression(Expression existingExpression, Expression expressionToAdd)
        {
            if (existingExpression == null)
            {
                return expressionToAdd;
            }

            //Build 'OR' expression for each property
            return Expression.OrElse(existingExpression, expressionToAdd);
        }

        #endregion

        [Obsolete]
        private static IQueryable<T> ApplyFiltering<T>(IQueryable<T> collection, FilterByInfo filter)
        {
            string[] props = filter.FilterBy.Split('.');
            Type type = typeof(T);
            ParameterExpression arg = Expression.Parameter(type, "x");
            Expression expr = arg;

            PropertyInfo pi = type.GetProperty(props[0]);
            expr = Expression.Property(expr, pi);
            type = pi.PropertyType;

            expr = BuildAccessors(expr, props, 1, filter);

            if (expr == null)
                return collection;

            var lambda = Expression.Lambda<Func<T, bool>>(expr, arg);

            return collection.Where(lambda);
        }

        [Obsolete]
        private static Expression BuildAccessors(Expression parent, string[] properties, int index, FilterByInfo filter)
        {
            if (properties.Length == 1 && (typeof(IEnumerable).IsAssignableFrom(parent.Type) && parent.Type != typeof(string)))
            {
                var listItemType = parent.Type.GetGenericArguments().SingleOrDefault();
                var listItemParam = Expression.Parameter(listItemType, "x");

                object value = ChangeObjectType(listItemParam, filter.Value);
                var constant = Expression.Constant(value, listItemType);

                MethodInfo method = typeof(Enumerable).GetMethods(BindingFlags.Static | BindingFlags.Public)
              .Single(m => m.Name == "Contains"
                    && m.GetParameters().Count() == 2)
              .MakeGenericMethod(listItemType);

                var invokeSelect = Expression.Call(method, parent, constant);
                return invokeSelect;
            }
            else if (index < properties.Length)
            {
                var member = properties[index];

                // If it's IEnumerable like ReservationRooms
                if (typeof(IEnumerable).IsAssignableFrom(parent.Type) && parent.Type != typeof(string))
                {
                    var enumerableType = parent.Type.GetGenericArguments().SingleOrDefault(); // input eg: Reservation.ReservationRooms (type ICollection<ReservationRoom>), output: type ReservationRoom

                    var param = Expression.Parameter(enumerableType, "x"); // declare parameter for the lambda expression of ReservationRooms.Any(x => x.PropertyName)

                    var lambdaBody = BuildAccessors(param, properties, index, filter); // Recurse to build the inside of the lambda, so x => x.PropertyName. 

                    var funcType = typeof(Func<,>).MakeGenericType(enumerableType, lambdaBody.Type); // Lambda is of type Func<ReservationRoom, int> in the case of x => x.PropertyName

                    var lambda = Expression.Lambda(funcType, lambdaBody, param); // Create Lambda of property

                    // Get Generic Method - HardCoded to Any, later can be passed as parameter
                    MethodInfo method = typeof(Enumerable).GetMethods(BindingFlags.Static | BindingFlags.Public)
                  .Single(m => m.Name == "Any"
                        && m.GetParameters().Count() == 2
                        && m.GetParameters()
                                   .ElementAt(1)
                                   .ParameterType
                                   .GetGenericTypeDefinition() == typeof(Func<,>))
                  .MakeGenericMethod(enumerableType);

                    // Do ReservationRooms.Any(x => x.PropertyName)
                    var invokeSelect = Expression.Call(null, method, parent, lambda);

                    return invokeSelect;
                }
                else
                {
                    // Simply access a property like DateFrom
                    var newParent = Expression.PropertyOrField(parent, member);

                    // Recurse
                    return BuildAccessors(newParent, properties, ++index, filter);
                }

            }
            else
            {
                // Return the final expression once we're done recursing.
                parent = GetExpression(parent, filter.Value, filter.Operator);
                return parent;
            }

        }

        [Obsolete]
        private static object ChangeObjectType(Expression member, object constant)
        {
            Type type = member.Type;
            object value;
            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
            {
                if (constant == null)
                    value = null;
                else
                    value = Convert.ChangeType(constant, type.GetGenericArguments()[0]);
            }
            else
                value = Convert.ChangeType(constant, type);

            return value;
        }

        /// <summary>
        /// Build expression based on filteroperator
        /// </summary>
        /// <param name="member"></param>
        /// <param name="constant"></param>
        /// <param name="filter"></param>
        /// <returns></returns>
        [Obsolete]
        private static Expression GetExpression(Expression member, object constant, FilterOperator filter)
        {
            Expression expression = null;
            ConstantExpression constantExpression = null;

            var memberType = member.Type;
            if ((memberType == typeof(Nullable<DateTime>) || memberType == typeof(DateTime)) && constant is string)
            {
                if (!string.IsNullOrWhiteSpace((string)constant))
                {
                    constant = DateTime.Parse((string)constant).Date;
                }
                else constant = default(DateTime?);
            }

            object value = ChangeObjectType(member, constant);

            switch (filter)
            {
                case FilterOperator.Contains:
                    constantExpression = Expression.Constant(constant.ToString().ToLower());
                    var toLower = Expression.Call(member,
                                  typeof(string).GetMethod("ToLower", System.Type.EmptyTypes));
                    expression = Expression.Call(toLower, typeof(string).GetMethod("Contains"), constantExpression);
                    break;
                case FilterOperator.DoesNotContain:
                    constantExpression = Expression.Constant(constant.ToString().ToLower());
                    var contains = Expression.Call(member, typeof(string).GetMethod("Contains"), constantExpression);
                    expression = Expression.Not(contains);
                    break;
                case FilterOperator.IsEqualTo:
                    constantExpression = Expression.Constant(value, member.Type);
                    expression = Expression.Equal(member, constantExpression);
                    break;
                case FilterOperator.IsNotEqualTo:
                    constantExpression = Expression.Constant(value, member.Type);
                    expression = Expression.NotEqual(member, constantExpression);
                    break;
                case FilterOperator.IsGreaterThan:
                    constantExpression = Expression.Constant(value, member.Type);
                    expression = Expression.GreaterThan(member, constantExpression);
                    break;
                case FilterOperator.IsGreaterThanOrEqualTo:
                    constantExpression = Expression.Constant(value, member.Type);
                    expression = Expression.GreaterThanOrEqual(member, constantExpression);
                    break;
                case FilterOperator.IsLessThan:
                    constantExpression = Expression.Constant(value, member.Type);
                    expression = Expression.LessThan(member, constantExpression);
                    break;
                case FilterOperator.IsLessThanOrEqualTo:
                    constantExpression = Expression.Constant(value, member.Type);
                    expression = Expression.LessThanOrEqual(member, constantExpression);
                    break;
                case FilterOperator.IsContainedIn:
                    constantExpression = Expression.Constant(constant.ToString().ToLower());
                    var memberToLower = Expression.Call(member, typeof(string).GetMethod("ToLower", System.Type.EmptyTypes));
                    expression = Expression.Call(constantExpression, typeof(string).GetMethod("Contains"), memberToLower);
                    break;
                default:
                    constantExpression = Expression.Constant(value, member.Type);
                    expression = Expression.Equal(member, constantExpression);
                    break;
            }

            return expression;
        }

        #endregion

        #region New FilterBy/OrderBy Extensions

        public static IEnumerable<T> FilterAndSort<T>(this IEnumerable<T> enumerable, dynLinq.DataSourceRequest filterSortRequest)
        {
            return enumerable.AsQueryable().FilterAndSort(filterSortRequest).AsEnumerable();
        }

        public static IQueryable<T> FilterAndSort<T>(this IQueryable<T> collection, dynLinq.DataSourceRequest filterSortRequest)
        {
            return collection.ToDataSourceResult(filterSortRequest).Data.Cast<T>().AsQueryable();
        }

        public static IEnumerable<T> FilterBy<T>(this IEnumerable<T> enumerable, dynLinq.Filter filter)
        {
            return enumerable.AsQueryable().FilterBy(filter).AsEnumerable();
        }

        public static IQueryable<T> FilterBy<T>(this IQueryable<T> collection, dynLinq.Filter filter)
        {
            return collection.ToDataSourceResult(0, 0, null, filter).Data.Cast<T>().AsQueryable();
        }

        #endregion

        /// <summary>
        /// Get GenericMethod info
        /// </summary>
        /// <param name="type"></param>
        /// <param name="name"></param>
        /// <param name="typeArgs"></param>
        /// <param name="argTypes"></param>
        /// <param name="flags"></param>
        /// <returns></returns>
        public static MethodBase GetGenericMethod(Type type, string name, Type[] typeArgs, Type[] argTypes, BindingFlags flags)
        {
            int typeArity = typeArgs.Length;
            var methods = type.GetMethods()
                .Where(m => m.Name == name)
                .Where(m => m.GetGenericArguments().Length == typeArity)
                .Select(m => m.MakeGenericMethod(typeArgs));

            return Type.DefaultBinder.SelectMethod(flags, methods.ToArray(), argTypes, null);
        }

        public static string ToTraceString<T>(this IQueryable<T> query)
        {
            var internalQueryField = query.GetType().GetFields(System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).Where(f => f.Name.Equals("_internalQuery")).FirstOrDefault();

            var internalQuery = internalQueryField.GetValue(query);

            var objectQueryField = internalQuery.GetType().GetFields(System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).Where(f => f.Name.Equals("_objectQuery")).FirstOrDefault();

            var objectQuery = objectQueryField.GetValue(internalQuery) as System.Data.Entity.Core.Objects.ObjectQuery<T>;

            return ToTraceStringWithParameters<T>(objectQuery);
        }

        public static string ToTraceStringWithParameters<T>(System.Data.Entity.Core.Objects.ObjectQuery<T> query)
        {
            System.Text.StringBuilder sb = new StringBuilder();

            string traceString = query.ToTraceString() + Environment.NewLine;

            foreach (var parameter in query.Parameters)
            {
                traceString += parameter.Name + " [" + parameter.ParameterType.FullName + "] = " + parameter.Value + "\n";
            }

            return traceString;
        }
    }
}