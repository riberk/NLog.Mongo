namespace NLog.Mongo
{
    using System;
    using JetBrains.Annotations;
    using NLog.Config;

    public class MongoIndexField : IMongoIndexField
    {
        /// <summary>
        ///     <seealso cref="FieldIndexType" />
        /// </summary>
        [RequiredParameter]
        public string IndexType
        {
            get { return Type.ToString(); }
            [UsedImplicitly]
            private set
            {
                FieldIndexType t;
                if (!Enum.TryParse(value, true, out t))
                {
                    throw new FormatException("Coud not parse index type");
                }
                Type = t;
            }
        }

        [RequiredParameter]
        public string Name { get; [UsedImplicitly] private set; }

        public FieldIndexType Type { get; private set; }
    }
}