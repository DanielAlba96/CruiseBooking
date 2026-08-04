using Shared.Domain.Entities;
using HotChocolate.Resolvers;
using Shared.Domain.Repositories;

namespace Cruises.Api.GraphQL.Types;

/// <summary>
/// Tipo GraphQL que configura la exposición de la entidad Ship en el esquema GraphQL.
/// Define los campos disponibles para consultas sobre buques, incluyendo la resolución
/// de cabinas asociadas a cada buque.
/// </summary>
public class ShipType : ObjectType<Ship>
{
    /// <summary>
    /// Configura los campos y resolvedores del tipo Ship en el esquema GraphQL.
    /// Establece que solo se exponen explícitamente los campos definidos y vincula
    /// el campo de cabinas con su resolvedores correspondiente.
    /// </summary>
    /// <param name="descriptor">Descriptor del tipo de objeto utilizado para configurar los campos disponibles.</param>
    protected override void Configure(IObjectTypeDescriptor<Ship> descriptor)
    {
        descriptor.BindFieldsExplicitly();

        descriptor.Field(x => x.Id).IsProjected(true);
        descriptor.Field(x => x.Code);
        descriptor.Field(x => x.Name);
        descriptor.Field(x => x.Description);
        descriptor.Field(x => x.Company);

        descriptor
            .Field("cabins")
            .ResolveWith<ShipResolvers>(r => r.GetCabins(default!, default!))
            .UseSorting();
    }
}

/// <summary>
/// Clase que contiene los resolvedores personalizados para la entidad Ship en GraphQL.
/// Proporciona métodos para resolver campos complejos que no se pueden mapear directamente
/// desde la entidad, como la obtención de cabinas asociadas a un buque.
/// </summary>
public class ShipResolvers
{
    /// <summary>
    /// Resuelve las cabinas disponibles en un buque específico.
    /// Recupera todas las cabinas asociadas al buque actual desde el repositorio de cabinas.
    /// </summary>
    /// <param name="ship">El buque padre del cual se desean obtener las cabinas.</param>
    /// <param name="repository">El repositorio de cabinas utilizado para acceder a los datos.</param>
    /// <returns>Una secuencia consultable de cabinas pertenecientes al buque especificado.</returns>
    public IQueryable<ShipCabin> GetCabins(
        [Parent] Ship ship,
        [Service] ICabinRepository repository)
    {
        return repository.GetShipCabins(ship.Id);
    }
}
