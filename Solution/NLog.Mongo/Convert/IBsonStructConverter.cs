namespace NLog.Mongo.Convert
{
    using MongoDB.Bson;

    /// <summary>
    ///     Конвертирует строки в bson-значения
    /// </summary>
    public interface IBsonStructConverter
    {
        /// <summary>
        ///     Конвертировать как bool
        /// </summary>
        /// <param name="value">Значение</param>
        /// <param name="bsonValue">bson-значение</param>
        /// <returns>Удалось ли сконвертировать</returns>
        bool TryBoolean(string value, out BsonValue bsonValue);

        /// <summary>
        ///     Конвертировать как DateTime
        /// </summary>
        /// <param name="value">Значение</param>
        /// <param name="bsonValue">bson-значение</param>
        /// <returns>Удалось ли сконвертировать</returns>
        bool TryDateTime(string value, out BsonValue bsonValue);

        /// <summary>
        ///     Конвертировать как double
        /// </summary>
        /// <param name="value">Значение</param>
        /// <param name="bsonValue">bson-значение</param>
        /// <returns>Удалось ли сконвертировать</returns>
        bool TryDouble(string value, out BsonValue bsonValue);

        /// <summary>
        ///     Конвертировать как int
        /// </summary>
        /// <param name="value">Значение</param>
        /// <param name="bsonValue">bson-значение</param>
        /// <returns>Удалось ли сконвертировать</returns>
        bool TryInt32(string value, out BsonValue bsonValue);

        /// <summary>
        ///     Конвертировать как long
        /// </summary>
        /// <param name="value">Значение</param>
        /// <param name="bsonValue">bson-значение</param>
        /// <returns>Удалось ли сконвертировать</returns>
        bool TryInt64(string value, out BsonValue bsonValue);

        /// <summary>
        ///     Конвертировать как строку
        /// </summary>
        /// <param name="value">Значение</param>
        /// <param name="bsonValue">bson-значение</param>
        /// <returns>Удалось ли сконвертировать</returns>
        bool TryString(string value, out BsonValue bsonValue);

        /// <summary>
        ///     Конвертировать как строку
        /// </summary>
        /// <param name="value">Значение</param>
        /// <returns>Значение</returns>
        BsonValue BsonString(string value);
    }
}