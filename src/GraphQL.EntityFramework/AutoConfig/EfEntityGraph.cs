using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using GraphQL.Types;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace GraphQL.EntityFramework
{
    /// <summary>
    /// Generic entity graph based on Ef core config.
    /// </summary>
    /// <typeparam name="T">Entity type.</typeparam>
    public class EfEntityGraph<T> : EfObjectGraphType<T>
    {
        /// <summary>
        /// Constructor with service and context injection parameters.
        /// </summary>
        /// <param name="efGraphQlService">Ef Gql service.</param>
        /// <param name="model">Db context model.</param>
        public EfEntityGraph(IEfGraphQLService efGraphQlService, IModel model) : base(efGraphQlService)
        {
            Name = typeof(T).Name;
            var efType = model.FindEntityType(typeof(T).FullName ?? "");
            if (efType == null)
            {
                return;
            }

            var typeParam = Expression.Parameter(typeof(T));
            foreach (var property in efType.GetProperties())
            {
                dynamic lambda = Expression.Lambda(Expression.Property(typeParam, property.Name), typeParam);
                var methodInfo =
                    typeof(EfEntityGraph<T>).GetMethod(nameof(AddPlainField), BindingFlags.Instance | BindingFlags.NonPublic);
                methodInfo?.MakeGenericMethod(property.ClrType).Invoke(this, new[] { lambda, property });
            }

            foreach (var navigation in efType.GetNavigations())
            {
                var targetType = navigation.GetTargetType().ClrType;

                var castType = navigation.IsCollection() ? typeof(IEnumerable<>).MakeGenericType(targetType) : null;
                var methodName = navigation.IsCollection() ? nameof(AddCollectionNav) : nameof(AddSingleNav);
                dynamic expression = CreateExpression(typeof(ResolveFieldContext<T>),
                    $"{nameof(ResolveFieldContext.Source)}.{navigation.Name}", castType);

                var methodInfo =
                    typeof(EfEntityGraph<T>).GetMethod(methodName, BindingFlags.Instance | BindingFlags.NonPublic);
                methodInfo?.MakeGenericMethod(targetType).Invoke(this, new[] {navigation, expression});
            }
        }

        private void AddPlainField<TProperty>(Expression<Func<T, TProperty>> expression,
            IProperty property)
        {
            Type type = null;
            if (property.ClrType.IsEnum)
            {
                type = typeof(EnumerationGraphType<>).MakeGenericType(property.ClrType);
            }

            Field(expression, property.IsNullable, type);
        }

        private void AddCollectionNav<TReturn>(INavigation navigation,
            Expression<Func<ResolveFieldContext<T>, IEnumerable<TReturn>>> exp) where TReturn : class
        {

            AddNavigationField(typeof(EfEntityGraph<TReturn>), navigation.Name,
                exp.Compile());
        }

        private void AddSingleNav<TReturn>(INavigation navigation,
            Expression<Func<ResolveFieldContext<T>, TReturn>> exp) where TReturn : class
        {
            AddNavigationField(typeof(EfEntityGraph<TReturn>), navigation.Name,
                exp.Compile());
        }


        /// <summary>
        /// Create multi-level property getter expression (x => x.Prop1.Prop2.Prop3).
        /// </summary>
        /// <param name="type">Type of argument.</param>
        /// <param name="propertyName">Property path including point delimiters.</param>
        /// <param name="typeToConvert">Cast final value to this type.</param>
        /// <returns></returns>
        public static LambdaExpression CreateExpression(Type type, string propertyName, Type typeToConvert = null)
        {
            var param = Expression.Parameter(type, "x");
            Expression body = param;
            foreach (var member in propertyName.Split('.'))
            {
                body = Expression.PropertyOrField(body, member);
            }

            if (typeToConvert != null)
            {
                body = Expression.Convert(body, typeToConvert);
            }
            return Expression.Lambda(body, param);
        }
    }
}