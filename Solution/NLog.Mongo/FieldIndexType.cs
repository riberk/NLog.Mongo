namespace NLog.Mongo
{
    public enum FieldIndexType
    {
        Ascending = 1,
        Descending = 2,
        GeoHaystack = 3,
        Geo2D = 4,
        Geo2DSphere = 5,
        Hashed = 6,
        Text = 7
    }
}