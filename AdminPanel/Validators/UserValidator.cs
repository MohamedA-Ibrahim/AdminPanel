using AdminPanel.Models;
using FluentValidation;

namespace AdminPanel.Validators;

public class UserValidator : AbstractValidator<User>
{
    public UserValidator()
    {
        RuleFor(user => user.FirstName)
            .NotEmpty()
            .MinimumLength(2);
        
        RuleFor(user => user.LastName)
            .NotEmpty()
            .MinimumLength(2);

        RuleFor(user => user.Email)
            .NotEmpty()
            .EmailAddress();

        RuleFor(user => user.Phone)
            .NotEmpty()
            .MinimumLength(2);
    }
}
