namespace Core.Agents.Common;

/// <summary>
/// Nombres con los que las herramientas de creación de reservas se exponen al modelo de lenguaje.
/// </summary>
internal static class BookingToolNames
{
    public const string SearchCruises = "search_cruises";
    public const string GetAvailableCabins = "get_available_cabins";
    public const string GetExtras = "get_extras";
    public const string AddCabin = "add_cabin";
    public const string RemoveCabin = "remove_cabin";
    public const string AddExtra = "add_extra";
    public const string RemoveExtra = "remove_extra";
    public const string ConfirmBooking = "confirm_booking";

    public static readonly IReadOnlySet<string> DraftScoped = new HashSet<string>(StringComparer.Ordinal)
    {
        SearchCruises,
        GetAvailableCabins,
        GetExtras,
        AddCabin,
        RemoveCabin,
        AddExtra,
        RemoveExtra,
        ConfirmBooking
    };
}
