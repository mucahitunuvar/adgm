using System.Linq.Expressions;

namespace GenclikMerkezi.Modules.Website.Domain;

// Faz 2 Görev 6 master prompt §6: a Popup's own visibility - IsActive plus its publish/unpublish
// window - defined once as an Expression<Func<...>> so it is a single source of truth for both a
// compiled in-memory check and EF Core's SQL translation, exactly like ContentItemVisibility - whose
// own remarks record a real bug from this rule once being hand-copied into a repository query and
// silently dropping its PublishAtUtc/UnpublishAtUtc half.
public static class PopupVisibility
{
    private static readonly Expression<Func<Popup, DateTime, bool>> Rule = (popup, now) =>
        popup.IsActive
        && (popup.PublishAtUtc == null || popup.PublishAtUtc <= now)
        && (popup.UnpublishAtUtc == null || popup.UnpublishAtUtc > now);

    private static readonly Func<Popup, DateTime, bool> CompiledRule = Rule.Compile();

    public static bool Evaluate(Popup popup, DateTime now) => CompiledRule(popup, now);

    public static Expression<Func<Popup, bool>> IsVisibleAt(DateTime now)
    {
        var popup = Expression.Parameter(typeof(Popup), "popup");
        var body = new ReplaceParameterVisitor(Rule.Parameters[0], popup)
            .Visit(new ReplaceParameterVisitor(Rule.Parameters[1], Expression.Constant(now)).Visit(Rule.Body))!;

        return Expression.Lambda<Func<Popup, bool>>(body, popup);
    }

    // §6 "aynı anda en fazla 20 aktif ve süresi dolmamış pop-up" - active and not-yet-expired,
    // deliberately ignoring PublishAtUtc (a popup scheduled for the future still counts against the
    // limit).
    private static readonly Expression<Func<Popup, DateTime, bool>> ActiveAndNotExpiredRule = (popup, now) =>
        popup.IsActive && (popup.UnpublishAtUtc == null || popup.UnpublishAtUtc > now);

    private static readonly Func<Popup, DateTime, bool> CompiledActiveAndNotExpiredRule = ActiveAndNotExpiredRule.Compile();

    public static bool IsActiveAndNotExpired(Popup popup, DateTime now) => CompiledActiveAndNotExpiredRule(popup, now);

    public static Expression<Func<Popup, bool>> IsActiveAndNotExpiredAt(DateTime now)
    {
        var popup = Expression.Parameter(typeof(Popup), "popup");
        var body = new ReplaceParameterVisitor(ActiveAndNotExpiredRule.Parameters[0], popup)
            .Visit(new ReplaceParameterVisitor(ActiveAndNotExpiredRule.Parameters[1], Expression.Constant(now)).Visit(ActiveAndNotExpiredRule.Body))!;

        return Expression.Lambda<Func<Popup, bool>>(body, popup);
    }

    private sealed class ReplaceParameterVisitor(Expression from, Expression to) : ExpressionVisitor
    {
        protected override Expression VisitParameter(ParameterExpression node) => node == from ? to : node;
    }
}
