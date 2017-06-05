namespace NLog.Mongo
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using JetBrains.Annotations;
    using Moq;

    public class ArgumentsVerifier
    {
        [NotNull] private readonly Dictionary<Type, ConstructorInfo[]> _constructors;

        [NotNull] private readonly MethodInfo _createMockMethod;

        [NotNull] private readonly List<ConstructorInfo> _ctorsWithoutParameters;

        [NotNull] private readonly List<string> _errors;

        [NotNull] private readonly MockRepository _mockFactory;

        public ArgumentsVerifier(IEnumerable<Type> types)
        {
            var filteredTypes = types.Where(x => !x.Name.StartsWith("<>"))
#if CORE
                                     .Select(x => x.GetTypeInfo())
#endif
                                     .Where(x => !x.IsNestedPrivate)
                                     .Where(x => !x.IsGenericTypeDefinition);
            _mockFactory = new MockRepository(MockBehavior.Loose);
            _errors = new List<string>();
            _constructors = filteredTypes
                .Select(x => new
                {
                    Type = x,
                    Constructors = x.GetConstructors()
                                    .Where(c => c.GetParameters()
                                                 .All(p => IsReferenceType(p.ParameterType) && p.ParameterType != typeof(string)))
                                    .ToArray()
                })
                .Where(x => x.Constructors.Any())
                .ToDictionary(x =>
                {
#if CORE
                    return x.Type.AsType();
#else
                    return x.Type;
#endif
                }, x => x.Constructors);
            _ctorsWithoutParameters = filteredTypes.SelectMany(x => x.GetConstructors()).Where(x => !x.GetParameters().Any()).ToList();
            _createMockMethod = typeof(MockRepository).GetMethod("Create", new Type[0]);
        }

        [NotNull]
        public IReadOnlyCollection<string> Errors => _errors;

        public static bool IsReferenceType(Type t)
        {
            if (t == null)
            {
                return false;
            }
#if CORE
            var ti = t.GetTypeInfo();
            if (!ti.IsClass)
            {
                return ti.IsInterface;
            }
#else
            if (!t.IsClass)
            {
                return t.IsInterface;
            }
#endif
            return true;

        }

        [NotNull]
        public ArgumentsVerifier CheckAllCtorsWithoutParametersCreateObject()
        {
            foreach (var voidConstructor in _ctorsWithoutParameters)
            {
                try
                {
                    voidConstructor.Invoke(new object[0]);
                }
                catch (Exception e)
                {
                    _errors.Add(e.ToString());
                }
            }
            return this;
        }

        [NotNull]
        public ArgumentsVerifier CheckNullArgumentsOnConstructors<T>()
        {
            return CheckNullArgumentsOnConstructors(typeof(T));
        }

        [NotNull]
        public ArgumentsVerifier CheckNullArgumentsOnConstructors([NotNull] Type t)
        {
            return CheckNullArgumentOnConstructors(t.GetConstructors());
        }

        private object CreateObject(Type type)
        {
            var mock = _createMockMethod
                .MakeGenericMethod(type)
                .Invoke(_mockFactory, new object[0]);
            var mockObject = mock.GetType().GetProperties()
                                 .Single(pp => pp.Name == "Object" && pp.DeclaringType == mock.GetType())
                                 .GetValue(mock);
            return mockObject;
        }

        [NotNull]
        private ArgumentsVerifier CheckNullArgumentOnConstructors([NotNull] IEnumerable<ConstructorInfo> infos)
        {
            foreach (var mi in infos)
            {
                var args = mi.GetParameters();
                var dict = args.ToDictionary<ParameterInfo, ParameterInfo, object>(
                                             x => x,
                                             parameterInfo =>
                                             {
                                                 try
                                                 {
                                                     return CreateObject(parameterInfo.ParameterType);
                                                 }
                                                 catch (Exception e)
                                                 {
                                                     throw new AggregateException(
                                                                                  $"Exception on create mock for {mi.DeclaringType} constructor {mi} with parameter {parameterInfo}",
                                                                                  e);
                                                 }
                                             });
                foreach (var source in dict)
                {
                    var p = args.Select(x =>
                                {
                                    if (source.Key == x) return null;
                                    var s = dict[x];
                                    return s;
                                })
                                .ToArray();
                    try
                    {
                        mi.Invoke(p);
                        _errors.Add($"Not throw exception constructor {mi} on type {mi.DeclaringType} where {source.Key} is null");
                    }
                    catch (TargetInvocationException tie) when (tie.InnerException is ArgumentNullException)
                    {
                    }
                    catch (Exception ex)
                    {
                        _errors.Add(ex.ToString());
                    }
                }
            }
            return this;
        }

        [NotNull]
        public ArgumentsVerifier CheckNullArgumentsOnConstructors()
        {
            foreach (var info in _constructors)
            {
                CheckNullArgumentOnConstructors(info.Value);
            }
            return this;
        }

        [NotNull]
        public static ArgumentsVerifierBuilder Builder(Type t)
        {
            return new ArgumentsVerifierBuilder(t);
        }

        public class ArgumentsVerifierBuilder
        {
            [NotNull] private readonly HashSet<Type> _allTypes;

            public ArgumentsVerifierBuilder(Type assemblyType)
            {
#if CORE
                var assembly = assemblyType.GetTypeInfo().Assembly;
                var types = assembly.GetTypes().Select(x => x.GetTypeInfo()).Where(x => !x.IsAbstract && !x.IsInterface);
                _allTypes = new HashSet<Type>(types.Select(x => x.AsType()));
#else
                var assembly = assemblyType.Assembly;
                var types = assembly.GetTypes().Where(x => !x.IsAbstract && !x.IsInterface);
                _allTypes = new HashSet<Type>(types);
#endif
            }

            [NotNull]
            public ArgumentsVerifierBuilder Exclude<T>()
            {
                _allTypes.Remove(typeof(T));
                return this;
            }

            [NotNull]
            public ArgumentsVerifier ToVerifier()
            {
                return new ArgumentsVerifier(_allTypes);
            }
        }
    }
}