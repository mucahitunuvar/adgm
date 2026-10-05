using FluentValidation;

namespace GenclikMerkezi.Modules.Website.Features.SetFormFields;

public sealed class SetFormFieldsCommandValidator : AbstractValidator<SetFormFieldsCommand>
{
    public SetFormFieldsCommandValidator()
    {
        RuleFor(c => c.RowVersion).NotEmpty();
        RuleFor(c => c.Fields).NotNull();
        RuleForEach(c => c.Fields).ChildRules(field =>
        {
            field.RuleFor(f => f.Key).NotEmpty();
            field.RuleFor(f => f.Type).NotEmpty();
            field.RuleFor(f => f.SortOrder).GreaterThanOrEqualTo(0);
            field.RuleFor(f => f.Options).NotNull();
            field.RuleFor(f => f.AllowedFileTypes).NotNull();
            field.RuleFor(f => f.Translations).NotNull();
            field.RuleForEach(f => f.Translations).ChildRules(translation =>
            {
                translation.RuleFor(t => t.LanguageCode).NotEmpty();
                translation.RuleFor(t => t.Label).NotEmpty();
            });
            field.RuleForEach(f => f.Options).ChildRules(option =>
            {
                option.RuleFor(o => o.Key).NotEmpty();
                option.RuleFor(o => o.Translations).NotNull();
                option.RuleForEach(o => o.Translations).ChildRules(translation =>
                {
                    translation.RuleFor(t => t.LanguageCode).NotEmpty();
                    translation.RuleFor(t => t.Label).NotEmpty();
                });
            });
        });
    }
}
