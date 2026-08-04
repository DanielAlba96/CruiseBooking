using Shared.Domain.Entities;
using HotChocolate.Resolvers;

namespace Cruises.Api.GraphQL.Types;

/// <summary>
/// Tipo de cabaña de barco para la capa GraphQL.
/// Define la configuración y resolvers de campos para la entidad ShipCabin.
/// </summary>
public class ShipCabinType : ObjectType<ShipCabin>
{
    /// <summary>
    /// Configura la exposición explícita de campos para el tipo ShipCabin en GraphQL.
    /// </summary>
    /// <param name="descriptor">El descriptor de tipo de objeto GraphQL para ShipCabin.</param>
    protected override void Configure(IObjectTypeDescriptor<ShipCabin> descriptor)
    {
        descriptor.BindFieldsExplicitly();

        descriptor.Field(x => x.Id).IsProjected(true);
        descriptor.Field(x => x.Number);
        descriptor.Field(x => x.Price)
            .ResolveWith<ShipCabinResolvers>(x => x.GetPrice(default!, default!));
        descriptor.Field("name")
            .ResolveWith<ShipCabinResolvers>(x => x.GetCabinName(default!));
        descriptor.Field("description")
            .ResolveWith<ShipCabinResolvers>(x => x.GetCabinDescription(default!));
        descriptor.Field("maxOccupancy")
            .ResolveWith<ShipCabinResolvers>(x => x.GetMaxOccupancy(default!));
    }
}

/// <summary>
/// Resolutores de campos para la cabaña de barco en GraphQL.
/// Proporciona métodos para resolver campos calculados de cabaña de barco.
/// </summary>
public class ShipCabinResolvers
{
    /// <summary>
    /// Obtiene el nombre de la cabaña a partir del tipo de cabaña.
    /// </summary>
    /// <param name="shipCabin">La cabaña de barco de donde se extrae el nombre.</param>
    /// <returns>El nombre de la cabaña, o una cadena vacía si no está disponible.</returns>
    public string? GetCabinName(
        [Parent] ShipCabin shipCabin)
        => shipCabin.CabinType?.Name ?? string.Empty;

    /// <summary>
    /// Obtiene la descripción de la cabaña a partir del tipo de cabaña.
    /// </summary>
    /// <param name="shipCabin">La cabaña de barco de donde se extrae la descripción.</param>
    /// <returns>La descripción de la cabaña, o una cadena vacía si no está disponible.</returns>
    public string? GetCabinDescription(
        [Parent] ShipCabin shipCabin)
        => shipCabin.CabinType?.Description ?? string.Empty;

    /// <summary>
    /// Obtiene la ocupación máxima de la cabaña a partir del tipo de cabaña.
    /// </summary>
    /// <param name="shipCabin">La cabaña de barco de donde se extrae la ocupación máxima.</param>
    /// <returns>El número máximo de pasajeros que pueden ocupar la cabaña, o 0 si no está disponible.</returns>
    public int GetMaxOccupancy(
        [Parent] ShipCabin shipCabin)
        => shipCabin.CabinType?.MaxOccupancy ?? 0;

    /// <summary>
    /// Calcula el precio de la cabaña con modificadores aplicados desde el contexto de resolución.
    /// </summary>
    /// <param name="shipCabin">La cabaña de barco cuyo precio se debe calcular.</param>
    /// <param name="context">El contexto del resolver de GraphQL que contiene modificadores de precio.</param>
    /// <returns>El precio calculado de la cabaña, redondeado a 2 decimales.</returns>
    public decimal GetPrice(
       [Parent] ShipCabin shipCabin,
       IResolverContext context)
    {
        var priceModifier = context.GetScopedStateOrDefault<decimal?>("priceModifier");
        return Math.Round(priceModifier.HasValue ? shipCabin.Price * priceModifier.Value : shipCabin.Price, 2);
    }
}
