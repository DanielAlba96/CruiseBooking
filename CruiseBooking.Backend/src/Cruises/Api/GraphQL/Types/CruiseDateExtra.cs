using Shared.Domain.Entities;
using HotChocolate.Resolvers;

namespace Cruises.Api.GraphQL.Types;

/// <summary>
/// Configuración del tipo GraphQL para las extras de fechas de cruceros.
/// Define los campos y resolutores para la consulta de información de extras asociados a una fecha de crucero.
/// </summary>
public class ExtraType : ObjectType<CruiseDateExtra>
{
    /// <summary>
    /// Configura los campos del tipo GraphQL para <see cref="CruiseDateExtra"/>.
    /// </summary>
    /// <param name="descriptor">Descriptor del tipo de objeto que permite definir campos y resoluciones.</param>
    protected override void Configure(IObjectTypeDescriptor<CruiseDateExtra> descriptor)
    {
        descriptor.BindFieldsExplicitly();

        descriptor.Field(x => x.ExtraId).Name("id");
        descriptor.Field(x => x.Price)
            .ResolveWith<CruiseDateExtraResolvers>(x => x.GetPrice(default!, default!));
        descriptor.Field("code")
            .ResolveWith<CruiseDateExtraResolvers>(x => x.GetCode(default!));
        descriptor.Field("name")
             .ResolveWith<CruiseDateExtraResolvers>(x => x.GetName(default!));
        descriptor.Field("description")
             .ResolveWith<CruiseDateExtraResolvers>(x => x.GetDescription(default!));
    }
}

/// <summary>
/// Resolutores para los campos del tipo GraphQL <see cref="ExtraType"/>.
/// Proporciona métodos para resolver datos relacionados con extras de cruceros desde el contexto.
/// </summary>
public class CruiseDateExtraResolvers
{
    /// <summary>
    /// Resuelve el código de la extra asociada a la fecha del crucero.
    /// </summary>
    /// <param name="cruiseDateExtra">La extra de la fecha del crucero desde la que se obtiene el código.</param>
    /// <returns>El código de la extra, o una cadena vacía si la extra no existe.</returns>
    public string GetCode(
        [Parent] CruiseDateExtra cruiseDateExtra)
        => cruiseDateExtra.Extra?.Code ?? string.Empty;

    /// <summary>
    /// Resuelve el nombre de la extra asociada a la fecha del crucero.
    /// </summary>
    /// <param name="cruiseDateExtra">La extra de la fecha del crucero desde la que se obtiene el nombre.</param>
    /// <returns>El nombre de la extra, o una cadena vacía si la extra no existe.</returns>
    public string GetName(
        [Parent] CruiseDateExtra cruiseDateExtra)
        => cruiseDateExtra.Extra?.Name ?? string.Empty;

    /// <summary>
    /// Resuelve la descripción de la extra asociada a la fecha del crucero.
    /// </summary>
    /// <param name="cruiseDateExtra">La extra de la fecha del crucero desde la que se obtiene la descripción.</param>
    /// <returns>La descripción de la extra, o una cadena vacía si la extra no existe.</returns>
    public string GetDescription(
        [Parent] CruiseDateExtra cruiseDateExtra)
        => cruiseDateExtra.Extra?.Description ?? string.Empty;

    /// <summary>
    /// Resuelve el precio de la extra, aplicando un modificador de precio si está disponible en el contexto.
    /// </summary>
    /// <param name="cruiseDateExtra">La extra de la fecha del crucero de la que se obtiene el precio base.</param>
    /// <param name="context">El contexto del resolutor que proporciona acceso al estado compartido.</param>
    /// <returns>El precio de la extra redondeado a 2 decimales, aplicando el modificador si existe.</returns>
    public decimal GetPrice(
       [Parent] CruiseDateExtra cruiseDateExtra,
       IResolverContext context)
    {
        var priceModifier = context.GetScopedStateOrDefault<decimal?>("priceModifier");
        return Math.Round(priceModifier.HasValue ? cruiseDateExtra.Price * priceModifier.Value : cruiseDateExtra.Price, 2);
    }
}
