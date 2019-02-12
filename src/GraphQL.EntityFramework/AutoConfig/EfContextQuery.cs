using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using GraphQL.Types;
using Microsoft.EntityFrameworkCore;

namespace GraphQL.EntityFramework
{
    /// <summary>
    /// GraphQL query based on EF core context.
    /// </summary>
    /// <typeparam name="TContext">Context type.</typeparam>
    public class EfContextQuery<TContext> : EfObjectGraphType where TContext : DbContext
    {
        /// <summary>
        /// Query graph constructor.
        /// </summary>
        /// <param name="efGraphQlService">Injected service.</param>
        public EfContextQuery(IEfGraphQLService efGraphQlService) : base(efGraphQlService)
        {
            foreach (var setPropertyInfo in typeof(TContext).GetProperties()
                .Where(sp => sp.PropertyType.IsGenericType &&
                             typeof(DbSet<>).IsAssignableFrom(sp.PropertyType.GetGenericTypeDefinition())))
            {
                var targetType = setPropertyInfo.PropertyType.GenericTypeArguments[0];

                dynamic expression = CreateIQueryableExpression(setPropertyInfo.Name, targetType);
                var methodInfo = typeof(EfContextQuery<TContext>).GetMethod(nameof(AddRootQuery),
                    BindingFlags.Instance | BindingFlags.NonPublic);
                methodInfo?.MakeGenericMethod(targetType).Invoke(this, new[] { setPropertyInfo.Name, expression });

            }
        }

        private FieldType AddRootQuery<TResult>(string fieldName,
            Expression<Func<ResolveFieldContext<object>, IQueryable<TResult>>> expression) where TResult : class
        {
            return AddQueryField<EfEntityGraph<TResult>, TResult>(name: fieldName, resolve: expression.Compile());
        }


        private static LambdaExpression CreateIQueryableExpression(string propertyName, Type elementType)
        {
            var param = Expression.Parameter(typeof(ResolveFieldContext<object>), "x");

            Expression body = Expression.PropertyOrField(param, nameof(ResolveFieldContext<object>.UserContext));
            body = Expression.Convert(body, typeof(TContext));
            body = Expression.PropertyOrField(body, propertyName);
            body = Expression.Convert(body, typeof(IQueryable<>).MakeGenericType(elementType));
            return Expression.Lambda(body, param);
        }
    }
}