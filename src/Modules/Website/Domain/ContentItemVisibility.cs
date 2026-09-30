using System.Linq.Expressions;

namespace GenclikMerkezi.Modules.Website.Domain;

// ADR-024 §4.4: the single definition of "visible on the public site" for a ContentItem alone (own
// status/schedule - ancestor visibility is a separate, cross-aggregate Application-layer check, see
// ContentPathCascadeService.IsVisibleWithAncestorsAsync). This used to be copied by hand into every
// query that needed it (ContentItem.IsVisible, and three separate inlined Where(...) clauses in
// ContentItemRepository) - one of those copies (SearchPublicListAsync) was missing the
// PublishAtUtc/UnpublishAtUtc half of the rule entirely, letting scheduled and expired content leak
// into the public list. Rule is defined once, here, as an Expression<Func<...>> so it stays a single
// source of truth for both a compiled in-memory check and EF Core's SQL translation.
public static class ContentItemVisibility
{
    private static readonly Expression<Func<ContentItem, DateTime, bool>> Rule = (contentItem, now) =>
        contentItem.DeletedAtUtc == null
        && contentItem.Status == ContentItemStatus.Published
        && (contentItem.PublishAtUtc == null || contentItem.PublishAtUtc <= now)
        && (contentItem.UnpublishAtUtc == null || contentItem.UnpublishAtUtc > now);

    private static readonly Func<ContentItem, DateTime, bool> CompiledRule = Rule.Compile();

    // ContentItem.IsVisible(now) calls this - Rule is compiled once (above) and the same delegate is
    // reused on every call, not recompiled per invocation.
    public static bool Evaluate(ContentItem contentItem, DateTime now) => CompiledRule(contentItem, now);

    // ContentItemRepository composes this into a Where(...) - rebuilding Rule's body with `now` bound
    // to a parameter (rather than reusing Rule's own two-parameter form via Expression.Invoke) keeps
    // the tree a plain conjunction of member accesses and comparisons, which every EF Core LINQ
    // provider translates straight to SQL.
    public static Expression<Func<ContentItem, bool>> IsVisibleAt(DateTime now)
    {
        var contentItem = Expression.Parameter(typeof(ContentItem), "contentItem");
        var body = new ReplaceParameterVisitor(Rule.Parameters[0], contentItem)
            .Visit(new ReplaceParameterVisitor(Rule.Parameters[1], Expression.Constant(now)).Visit(Rule.Body))!;

        return Expression.Lambda<Func<ContentItem, bool>>(body, contentItem);
    }

    private sealed class ReplaceParameterVisitor(Expression from, Expression to) : ExpressionVisitor
    {
        protected override Expression VisitParameter(ParameterExpression node) => node == from ? to : node;
    }
}
