namespace NLog.Mongo.Infrastructure
{
    using System;
    using System.Runtime.InteropServices;
    using JetBrains.Annotations;
    using MongoDB.Bson;
    using NLog.Mongo.Convert;

    internal class BsonExceptionFactory : IBsonExceptionFactory
    {
        [NotNull] private readonly IBsonDocumentValueAppender _bsonDocumentValueAppender;
        [NotNull] private readonly IBsonStructConverter _bsonStructConverter;

        /// <summary>
        ///     Инициализирует новый экземпляр класса <see cref="T:System.Object" />.
        /// </summary>
        public BsonExceptionFactory([NotNull] IBsonDocumentValueAppender bsonDocumentValueAppender, [NotNull] IBsonStructConverter bsonStructConverter)
        {
            _bsonDocumentValueAppender = bsonDocumentValueAppender ?? throw new ArgumentNullException(nameof(bsonDocumentValueAppender));
            _bsonStructConverter = bsonStructConverter ?? throw new ArgumentNullException(nameof(bsonStructConverter));
        }

        public BsonValue Create(Exception exception)
        {
            if (exception == null)
            {
                return BsonNull.Value;
            }
            var document = new BsonDocument();
            _bsonDocumentValueAppender.Append(document, "Message", _bsonStructConverter.BsonString(exception.Message));
            _bsonDocumentValueAppender.Append(document, "BaseMessage", _bsonStructConverter.BsonString(exception.GetBaseException().Message));
            _bsonDocumentValueAppender.Append(document, "Text", _bsonStructConverter.BsonString(exception.ToString()));
            _bsonDocumentValueAppender.Append(document, "Type", _bsonStructConverter.BsonString(exception.GetType().ToString()));
            _bsonDocumentValueAppender.Append(document, "Stack", _bsonStructConverter.BsonString(exception.StackTrace));
            _bsonDocumentValueAppender.Append(document, "Source", _bsonStructConverter.BsonString(exception.Source));

#if !CORE
            var external = exception as ExternalException;
            if (external != null)
            {
                _bsonDocumentValueAppender.Append(document, "ErrorCode", new BsonInt32(external.ErrorCode));
            }
#else
            _bsonDocumentValueAppender.Append(document, "ErrorCode", new BsonInt32(exception.HResult));
#endif

#if !CORE
            var method = exception.TargetSite;
            if (method != null)
            {
                var assembly = method.Module.Assembly.GetName();

                _bsonDocumentValueAppender.Append(document, "MethodName", _bsonStructConverter.BsonString(method.Name));
                _bsonDocumentValueAppender.Append(document, "ModuleName", _bsonStructConverter.BsonString(assembly.Name));
                _bsonDocumentValueAppender.Append(document, "ModuleVersion", _bsonStructConverter.BsonString(assembly.Version?.ToString()));
            }
#endif

            return document;
        }
    }
}