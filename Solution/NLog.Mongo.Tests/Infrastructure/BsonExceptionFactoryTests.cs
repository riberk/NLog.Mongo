namespace NLog.Mongo.Infrastructure
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;
    using System.Runtime.Serialization;
    using NUnit.Framework;
    using MongoDB.Bson;
    using Moq;
    using NLog.Mongo.Convert;

    [TestFixture]
    public class BsonExceptionFactoryTests
    {
        private MockRepository _mockFactory;
        private Mock<IBsonStructConverter> _structConverter;
        private Mock<IBsonDocumentValueAppender> _valueAppender;


        [SetUp]
        public void Init()
        {
            _mockFactory = new MockRepository(MockBehavior.Strict);
            _valueAppender = _mockFactory.Create<IBsonDocumentValueAppender>();
            _structConverter = _mockFactory.Create<IBsonStructConverter>();
        }

        private BsonExceptionFactory Create()
        {
            return new BsonExceptionFactory(_valueAppender.Object, _structConverter.Object);
        }

        [Test]
        public void CreateWithNullTest() => Assert.AreEqual(BsonNull.Value, Create().Create(null));

        [Test]
        public void CreateTest()
        {
            TestException ex;
            try
            {
                ex = new TestException("message", new Exception("inner message"), new Dictionary<string, string>(), "help", "source", "stack", 155);
                throw ex;
            }
            catch (TestException e)
            {
                ex = e;
            }
            var bMessage = _mockFactory.Create<BsonValue>(MockBehavior.Loose);
            var bBaseMessage = _mockFactory.Create<BsonValue>(MockBehavior.Loose);
            var bText = _mockFactory.Create<BsonValue>(MockBehavior.Loose);
            var bType = _mockFactory.Create<BsonValue>(MockBehavior.Loose);
            var bStack = _mockFactory.Create<BsonValue>(MockBehavior.Loose);
            var bSource = _mockFactory.Create<BsonValue>(MockBehavior.Loose);
            var bMethodName = _mockFactory.Create<BsonValue>(MockBehavior.Loose);
            var bModuleName = _mockFactory.Create<BsonValue>(MockBehavior.Loose);
            var bModuleVersion = _mockFactory.Create<BsonValue>(MockBehavior.Loose);

            var assembly = ex.TargetSite.Module.Assembly.GetName();
            var version = assembly.Version?.ToString();

            _structConverter.Setup(x => x.BsonString(ex.Message)).Returns(bMessage.Object).Verifiable();
            _structConverter.Setup(x => x.BsonString(ex.GetBaseException().Message)).Returns(bBaseMessage.Object).Verifiable();
            _structConverter.Setup(x => x.BsonString(ex.ToString())).Returns(bText.Object).Verifiable();
            _structConverter.Setup(x => x.BsonString(ex.GetType().ToString())).Returns(bType.Object).Verifiable();
            _structConverter.Setup(x => x.BsonString(ex.StackTrace)).Returns(bStack.Object).Verifiable();
            _structConverter.Setup(x => x.BsonString(ex.Source)).Returns(bSource.Object).Verifiable();
            _structConverter.Setup(x => x.BsonString(ex.TargetSite.Name)).Returns(bMethodName.Object).Verifiable();
            _structConverter.Setup(x => x.BsonString(assembly.Name)).Returns(bModuleName.Object).Verifiable();
            _structConverter.Setup(x => x.BsonString(version)).Returns(bModuleVersion.Object).Verifiable();


            _valueAppender.Setup(x => x.Append(It.IsAny<BsonDocument>(), "Message", bMessage.Object)).Verifiable();
            _valueAppender.Setup(x => x.Append(It.IsAny<BsonDocument>(), "BaseMessage", bBaseMessage.Object)).Verifiable();
            _valueAppender.Setup(x => x.Append(It.IsAny<BsonDocument>(), "Text", bText.Object)).Verifiable();
            _valueAppender.Setup(x => x.Append(It.IsAny<BsonDocument>(), "Type", bType.Object)).Verifiable();
            _valueAppender.Setup(x => x.Append(It.IsAny<BsonDocument>(), "Stack", bStack.Object)).Verifiable();
            _valueAppender.Setup(x => x.Append(It.IsAny<BsonDocument>(), "ErrorCode", It.IsAny<BsonValue>()))
                          .Callback((BsonDocument bd, string s, BsonValue v) => Assert.AreEqual(ex.ErrorCode, v.AsInt32))
                          .Verifiable();
            _valueAppender.Setup(x => x.Append(It.IsAny<BsonDocument>(), "Source", bSource.Object)).Verifiable();
            _valueAppender.Setup(x => x.Append(It.IsAny<BsonDocument>(), "MethodName", bMethodName.Object)).Verifiable();
            _valueAppender.Setup(x => x.Append(It.IsAny<BsonDocument>(), "ModuleName", bModuleName.Object)).Verifiable();
            _valueAppender.Setup(x => x.Append(It.IsAny<BsonDocument>(), "ModuleVersion", bModuleVersion.Object)).Verifiable();


            var actual = Create().Create(ex) as BsonDocument;
        }

        [TearDown]
        public void Clean()
        {
            _mockFactory.VerifyAll();
        }

        public class TestException : ExternalException
        {
            /// <summary>
            ///     Инициализирует новый экземпляр класса <see cref="T:System.Exception" /> с указанным сообщением об ошибке и ссылкой
            ///     на внутреннее исключение, вызвавшее данное исключение.
            /// </summary>
            /// <param name="message">Сообщение об ошибке, указывающее причину создания исключения. </param>
            /// <param name="innerException">
            ///     Исключение, вызвавшее текущее исключение, или пустая ссылка (Nothing в Visual Basic), если
            ///     внутреннее исключение не задано.
            /// </param>
            public TestException(string message, Exception innerException, IDictionary data, string helpLink, string source, string stackTrace, int errorCode)
                    : base(message, innerException)
            {
                Data = data;
                HelpLink = helpLink;
                Message = message;
                Source = source;
                StackTrace = stackTrace;
                ErrorCode = errorCode;
            }

            /// <summary>
            ///     Возвращает коллекцию пар ключ/значение, предоставляющие дополнительные сведения об исключении, определяемые
            ///     пользователем.
            /// </summary>
            /// <returns>
            ///     Объект, который реализует интерфейс <see cref="T:System.Collections.IDictionary" /> и содержит коллекцию заданных
            ///     пользователем пар «ключ — значение».По умолчанию является пустой коллекцией.
            /// </returns>
            public override IDictionary Data { get; }

            /// <summary>
            ///     Получает или задает ссылку на файл справки, связанный с этим исключением.
            /// </summary>
            /// <returns>
            ///     URN или URL-адрес.
            /// </returns>
            public override string HelpLink { get; set; }

            /// <summary>
            ///     Получает сообщение, описывающее текущее исключение.
            /// </summary>
            /// <returns>
            ///     Сообщение об ошибке с объяснением причин исключения или пустая строка ("").
            /// </returns>
            public override string Message { get; }

            /// <summary>
            ///     Возвращает или задает имя приложения или объекта, вызывавшего ошибку.
            /// </summary>
            /// <returns>
            ///     Имя приложения или объекта, вызвавшего ошибку.
            /// </returns>
            /// <exception cref="T:System.ArgumentException">The object must be a runtime <see cref="N:System.Reflection" /> object</exception>
            public override string Source { get; set; }

            /// <summary>
            ///     Получает строковое представление непосредственных кадров в стеке вызова.
            /// </summary>
            /// <returns>
            ///     Строка, описывающая непосредственные фреймы стека вызова.
            /// </returns>
            /// <PermissionSet>
            ///     <IPermission
            ///         class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089"
            ///         version="1" PathDiscovery="*AllFiles*" />
            /// </PermissionSet>
            public override string StackTrace { get; }

            /// <summary>
            ///     Получает значение HRESULT ошибки.
            /// </summary>
            /// <returns>
            ///     Значение HRESULT ошибки.
            /// </returns>
            public override int ErrorCode { get; }

            /// <summary>
            ///     При переопределении в производном классе возвращает исключение <see cref="T:System.Exception" />, которое является
            ///     корневой причиной одного или нескольких последующих исключений.
            /// </summary>
            /// <returns>
            ///     В цепочке исключений создается первое исключение.Если значением свойства
            ///     <see cref="P:System.Exception.InnerException" /> текущего исключения является пустая ссылка (Nothing в Visual
            ///     Basic), это свойство возвращает текущее исключение.
            /// </returns>
            public override Exception GetBaseException()
            {
                return base.GetBaseException();
            }

            /// <summary>
            ///     При переопределении в производном классе задает объект
            ///     <see cref="T:System.Runtime.Serialization.SerializationInfo" /> со сведениями об исключении.
            /// </summary>
            /// <param name="info">
            ///     Объект <see cref="T:System.Runtime.Serialization.SerializationInfo" />, содержащий сериализованные
            ///     данные объекта о созданном исключении.
            /// </param>
            /// <param name="context">
            ///     Объект <see cref="T:System.Runtime.Serialization.StreamingContext" />, содержащий контекстные
            ///     сведения об источнике или назначении.
            /// </param>
            /// <exception cref="T:System.ArgumentNullException">
            ///     The <paramref name="info" /> parameter is a null reference (Nothing in
            ///     Visual Basic).
            /// </exception>
            /// <PermissionSet>
            ///     <IPermission
            ///         class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089"
            ///         version="1" Read="*AllFiles*" PathDiscovery="*AllFiles*" />
            ///     <IPermission
            ///         class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089"
            ///         version="1" Flags="SerializationFormatter" />
            /// </PermissionSet>
            public override void GetObjectData(SerializationInfo info, StreamingContext context)
            {
                base.GetObjectData(info, context);
            }

            /// <summary>
            ///     Создает и возвращает строковое представление текущего исключения.
            /// </summary>
            /// <returns>
            ///     Строковое представление текущего исключения.
            /// </returns>
            /// <PermissionSet>
            ///     <IPermission
            ///         class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089"
            ///         version="1" PathDiscovery="*AllFiles*" />
            /// </PermissionSet>
            public override string ToString()
            {
                return base.ToString();
            }
        }
    }
}