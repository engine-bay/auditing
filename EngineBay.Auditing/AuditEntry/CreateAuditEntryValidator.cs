namespace EngineBay.Auditing
{
    using FluentValidation;

    public class CreateAuditEntryValidator : AbstractValidator<CreateAuditEntryRequest>
    {
        public CreateAuditEntryValidator()
        {
            this.RuleFor(request => request.ApplicationUserId).NotNull().NotEmpty();
            this.RuleFor(request => request.ApplicationUserName).NotNull().NotEmpty().MaximumLength(CreateAuditEntryRequest.FullNameMaxLength);
            this.RuleFor(request => request.ActionType).NotNull().NotEmpty().MaximumLength(CreateAuditEntryRequest.ActionTypeMaxLength);
            this.RuleFor(request => request.EntityName).MaximumLength(CreateAuditEntryRequest.EntityNameMaxLength);
            this.RuleFor(request => request.EntityId).MaximumLength(CreateAuditEntryRequest.EntityIdMaxLength);
            this.RuleFor(request => request.Changes).NotNull().NotEmpty();
        }
    }
}
