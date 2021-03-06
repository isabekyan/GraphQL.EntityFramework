﻿using GraphQL.EntityFramework;

public class Query :
    EfObjectGraphType
{
    public Query(IEfGraphQLService efGraphQlService) :
        base(efGraphQlService)
    {
        AddQueryField(
            name: "customType",
            resolve: context =>
            {
                var dataContext = (MyDataContext) context.UserContext;
                return dataContext.CustomTypeEntities;
            });

        AddQueryField(
            name: "skipLevel",
            resolve: context =>
            {
                var dataContext = (MyDataContext) context.UserContext;
                return dataContext.Level1Entities;
            }, graphType: typeof(SkipLevelGraph));

        AddQueryField(
            name: "manyChildren",
            resolve: context =>
            {
                var dataContext = (MyDataContext) context.UserContext;
                return dataContext.WithManyChildrenEntities;
            });

        AddQueryField(
            name: "level1Entities",
            resolve: context =>
            {
                var dataContext = (MyDataContext) context.UserContext;
                return dataContext.Level1Entities;
            }, graphType: typeof(Level1Graph));

        efGraphQlService.AddQueryField(
            this,
            name: "withNullableEntities",
            resolve: context =>
            {
                var dataContext = (MyDataContext) context.UserContext;
                return dataContext.WithNullableEntities;
            });

        efGraphQlService.AddQueryField(
            this,
            name: "misNamed",
            resolve: context =>
            {
                var dataContext = (MyDataContext) context.UserContext;
                return dataContext.WithMisNamedQueryParentEntities;
            });

        efGraphQlService.AddQueryField(
            this,
            name: "parentEntities",
            resolve: context =>
            {
                var dataContext = (MyDataContext) context.UserContext;
                return dataContext.ParentEntities;
            });

        efGraphQlService.AddQueryField(
            this,
            name: "childEntities",
            resolve: context =>
            {
                var dataContext = (MyDataContext) context.UserContext;
                return dataContext.ChildEntities;
            });

        efGraphQlService.AddQueryConnectionField<ParentGraph, ParentEntity>(
            this,
            name: "parentEntitiesConnection",
            resolve: context =>
            {
                var dataContext = (MyDataContext) context.UserContext;
                return dataContext.ParentEntities;
            });

        efGraphQlService.AddQueryConnectionField<ChildGraph, ChildEntity>(
            this,
            name: "childEntitiesConnection",
            resolve: context =>
            {
                var dataContext = (MyDataContext) context.UserContext;
                return dataContext.ChildEntities;
            });

        efGraphQlService.AddQueryField(
            this,
            name: "parentEntitiesFiltered",
            resolve: context =>
            {
                var dataContext = (MyDataContext) context.UserContext;
                return dataContext.FilterParentEntities;
            });

        efGraphQlService.AddQueryConnectionField<FilterParentGraph, FilterParentEntity>(
            this,
            name: "parentEntitiesConnectionFiltered",
            resolve: context =>
            {
                var dataContext = (MyDataContext) context.UserContext;
                return dataContext.FilterParentEntities;
            });

        efGraphQlService.AddSingleField(
            this,
            name: "parentEntity",
            resolve: context =>
            {
                var dataContext = (MyDataContext) context.UserContext;
                return dataContext.ParentEntities;
            });
    }
}