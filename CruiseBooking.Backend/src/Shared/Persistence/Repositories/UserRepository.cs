using Shared.Domain.Entities;
using Shared.Domain.Repositories;
using Shared.Persistence.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace Shared.Persistence.Repositories;

public class UserRepository(IDbContextFactory<CruisesDbContext> contextFactory) : IUserRepository
{
    /// <summary>Crea un nuevo usuario o actualiza uno existente según el GUID de Keycloak.</summary>
    public async Task<User> CreateOrUpdateUser(Guid keycloakGuid, string firstName, string lastName, string email, string phone, Address address)
    {
        using var context = contextFactory.CreateDbContext();

        var existingUser = await context.Users
            .FirstOrDefaultAsync(u => u.KeycloakUserGuid == keycloakGuid);

        if (existingUser is null)
        {
            var newUser = new User
            {
                KeycloakUserGuid = keycloakGuid,
                Name = firstName,
                Surname = lastName,
                Email = email,
                Phone = phone,
                Address = address
            };

            context.Add(newUser);
            await context.SaveChangesAsync();
            return newUser;
        }
        else
        {
            existingUser.Name = firstName;
            existingUser.Surname = lastName;
            existingUser.Email = email;
            existingUser.Phone = phone;
            existingUser.Address = address;

            await context.SaveChangesAsync();
            return existingUser;
        }
    }

    /// <summary>Actualiza el identificador de cliente del usuario.</summary>
    public async Task UpdateCustomerId(User user, string customerId)
    {
        using var context = contextFactory.CreateDbContext();
        user.CustomerId = customerId;
        context.Update(user);
        await context.SaveChangesAsync();
    }

    /// <summary>Obtiene un usuario por su GUID de Keycloak.</summary>
    public async Task<User?> GetUserByKeycloakGuid(Guid keycloakGuid)
    {
        using var context = contextFactory.CreateDbContext();

        return await context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.KeycloakUserGuid == keycloakGuid);
    }

    /// <summary>Añade un nuevo método de pago al usuario, desactivando el anterior si existe.</summary>
    public async Task AddPaymentMethod(long userId, UserPayment paymentInfo)
    {
        using var context = contextFactory.CreateDbContext();
        var user = await context.Users.FirstOrDefaultAsync(u => u.Id == userId)
            ?? throw new UserNotFoundException();

        var currentActive = await context.PaymentMethods
            .FirstOrDefaultAsync(p => p.UserId == user.Id && p.Active);

        currentActive?.Active = false;

        paymentInfo.UserId = user.Id;
        paymentInfo.Active = true;
        await context.AddAsync(paymentInfo);

        await context.SaveChangesAsync();
    }

    /// <summary>Obtiene el método de pago activo del usuario si no ha expirado.</summary>
    public async Task<UserPayment?> GetActivePaymentMethod(long userId)
    {
        using var context = contextFactory.CreateDbContext();

        var payment = await context.PaymentMethods
            .Include(p => p.User)
            .FirstOrDefaultAsync(p => p.User!.Id == userId && p.Active);

        if (payment is null)
            return null;

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var expiration = new DateOnly(payment.ExpirationYear, payment.ExpirationMonth, 1);

        if (expiration < today)
        {
            payment.Active = false;
            await context.SaveChangesAsync();
            return null;
        }

        return payment;
    }
}
