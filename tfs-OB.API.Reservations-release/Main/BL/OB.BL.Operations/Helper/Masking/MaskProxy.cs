using Newtonsoft.Json;
using OB.BL.Operations.Helper.Masking.Guard;
using OB.BL.Operations.Helper.Masking.Interfaces;
using OB.Reservation.BL.Contracts.Attributes;
using PciHideCC;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace OB.BL.Operations.Helper.Masking
{
    public static class MaskProxy
    {
        public static IDisposable AsOriginal<T>(this T obj)
        {
            return new UnmaskGuard<T>(obj);
        }

        public static IDisposable AsMasked<T>(this T obj)
        {
            return new MaskGuard<T>(obj);
        }

        #region Dynamic Proxy Creation
        static ConcurrentDictionary<Type, Type> dictionary = new ConcurrentDictionary<Type, Type>();

        public static T MaskCC<T>(this T original) where T : new()
        {
            var newObject = JsonConvert.DeserializeObject<T>(JsonConvert.SerializeObject(original));
            return Mask(original, newObject, (object obj) => {
                if (obj == null)
                    return null;
                try
                {
                    string input = JsonConvert.SerializeObject(obj);
                    string output = PciReplacer.Replace(input);
                    return JsonConvert.DeserializeObject(output, obj.GetType());
                }
                catch
                {
                    return obj;
                }
            });
        }

        /// <summary>
        /// Masks an object creating a proxy that either points to the masked properties and switches it to original when needed. 
        /// If the provided class is anotated with MaskFilterAttribute, then the proxy will only mask the properties annotated with MaskFilterAttribute.
        /// The filter is only applied recursively if the property is anotated with RecursiveMaskAttribute.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="original"></param>
        /// <param name="modified"></param>
        /// <returns></returns>
        public static T Mask<T>(this T original, T modified)
        {
            Type objType = dictionary.GetOrAdd(typeof(T), (type) => CompileResultType(original, modified));

            //Due to the case of primitives and the use of reflection to invoke the constructor, it returns an object.
            //Unfortunately, casting from an object is not possible to call the explicit conversion, since this is done at compile time.
            //Due to this, we are using dynamic. If we wanted to avoid dynamic type, then we would need to create a factory at runtime and,
            //instead of using Activator.CreateInstance, we would need to use the factory to create a method Like:
            //Factory.Create<T>(original, modified). Note that the method "Create" would need to be created at runtime.
            dynamic result = Activator.CreateInstance(objType, original, modified);
            return (T)result;
        }

        public static T Mask<T>(this T original, T baseObject, Func<object, object> conversion)
        {
            Type objType = dictionary.GetOrAdd(typeof(T), (type) => CompileResultType(original, baseObject));

            //Due to the case of primitives and the use of reflection to invoke the constructor, it returns an object.
            //Unfortunately, casting from an object is not possible to call the explicit conversion, since this is done at compile time.
            //Due to this, we are using dynamic. If we wanted to avoid dynamic type, then we would need to create a factory at runtime and,
            //instead of using Activator.CreateInstance, we would need to use the factory to create a method Like:
            //Factory.Create<T>(original, modified). Note that the method "Create" would need to be created at runtime.
            dynamic result = Activator.CreateInstance(objType, original, baseObject, conversion);
            return (T)result;
        }

        private static Type CompileResultType<T>(T original, T modified)
        {
            if (typeof(T).IsPrimitive)
                return PrimitiveType.CompileResultType<T>();
            if (typeof(IEnumerable).IsAssignableFrom(typeof(T)))
                return Enumerable.CompileResultType<T>(modified.GetType());
            return RegularObject.CompileResultType<T>();
        }

        private static class PrimitiveType
        {
            public class IntProxy
            {
                private int Original { get; set; }
                public static explicit operator int(IntProxy d) => d.Original;
                public IntProxy(int original, int modified) => Original = original;
                public IntProxy(int original, int modified, Func<object, object> function) => Original = original;
            }

            internal static Type CompileResultType<T>()
            {
                Type type = typeof(T);
                if (type == typeof(int))
                    return typeof(IntProxy);

                throw new NotImplementedException();
            }
        }

        private static class RegularObject
        {
            internal static Type CompileResultType<T>()
            {
                TypeBuilder proxyBuilder = GetTypeBuilder<T>($"{typeof(T).FullName}Proxy");

                //Add Active field
                FieldBuilder activeFieldBuilder = proxyBuilder.DefineField("Active", typeof(T), FieldAttributes.Private);

                //Add Original field
                FieldBuilder originalFieldBuilder = proxyBuilder.DefineField("Original", typeof(T), FieldAttributes.Private);

                //Add Modified field
                FieldBuilder modifiedFieldBuilder = proxyBuilder.DefineField("Modified", typeof(T), FieldAttributes.Private);


                bool checkFilter = typeof(T).IsDefined(typeof(MaskFilterAttribute));
                //Get all virtual properties
                var filteredProperties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance).Where(p => p.GetGetMethod()?.IsVirtual == true && p.GetSetMethod()?.IsVirtual == true);

                IDictionary<PropertyBuilder, PropertyInfo> recursiveMaskList = new Dictionary<PropertyBuilder, PropertyInfo>();
                foreach (var property in filteredProperties)
                {
                    bool recursiveMask = property.IsDefined(typeof(RecursiveMaskAttribute));
                    if (recursiveMask)
                    {
                        PropertyBuilder maskedPropertyBuilder = CreateProperty<T>(proxyBuilder, property, activeFieldBuilder);
                        recursiveMaskList[maskedPropertyBuilder] = property;
                    }
                    else
                    {
                        bool hasFilter = checkFilter && property.IsDefined(typeof(MaskFilterAttribute));
                        CreateProperty<T>(proxyBuilder, property, checkFilter ? (hasFilter ? activeFieldBuilder : originalFieldBuilder) : activeFieldBuilder);
                    }
                }

                CreateConstructorWithMaskObject<T>(proxyBuilder, recursiveMaskList, activeFieldBuilder, originalFieldBuilder, modifiedFieldBuilder);
                CreateConstructorWithTransformFunction<T>(proxyBuilder, recursiveMaskList, activeFieldBuilder, originalFieldBuilder, modifiedFieldBuilder);
                CreateMaskedInterfaceMethods<T>(proxyBuilder, activeFieldBuilder, originalFieldBuilder, modifiedFieldBuilder);

                Type objectType = proxyBuilder.CreateType();
                return objectType;
            }

            private static void CreateConstructorWithMaskObject<T>(TypeBuilder proxyBuilder, IDictionary<PropertyBuilder, PropertyInfo> recursiveMaskList, FieldBuilder activeFieldBuilder, FieldBuilder originalFieldBuilder, FieldBuilder modifiedFieldBuilder)
            {
                //Constructor is defined as {nameof(T)}Proxy(T original, T modified)
                ConstructorBuilder constructor = proxyBuilder.DefineConstructor(MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.RTSpecialName, CallingConventions.Standard, new Type[2] { typeof(T), typeof(T) });
                ILGenerator ctorIL = constructor.GetILGenerator();

                //Apply mask to algorithm to all properties of the modified variable annotated with RecursiveMaskAttribute.
                foreach (var e in recursiveMaskList)
                {
                    var propertyBuilder = e.Key;
                    var property = e.Value;

                    MethodInfo maskMethod = typeof(MaskProxy).GetMethods().First(x => x.Name == nameof(Mask) && x.GetParameters().Length == 2).MakeGenericMethod(property.PropertyType);
                    MethodInfo getter = property.GetGetMethod();

                    ctorIL.Emit(OpCodes.Ldarg_2); //Stack: modified
                    ctorIL.Emit(OpCodes.Ldarg_1); //Stack: modified original
                    ctorIL.Emit(OpCodes.Callvirt, getter); //Called original.{Property} => Stack: modified original.{PROPERTY}
                    ctorIL.Emit(OpCodes.Ldarg_2); //Stack: modified original.{PROPERTY} modified
                    ctorIL.Emit(OpCodes.Callvirt, getter);//Called modified.{Property} => Stack: modified original.{PROPERTY} modified.{PROPERTY}
                    ctorIL.Emit(OpCodes.Call, maskMethod);//Called: Mask(original.{PROPERTY}, modified.{PROPERTY}) | Stack: modified proxiedObject

                    ctorIL.Emit(OpCodes.Callvirt, propertyBuilder.GetSetMethod()); //Called modified.{PROPERTY} = proxiedObject => Stack: Empty
                }

                //This block is equivalent to: this.Original = original;
                ctorIL.Emit(OpCodes.Ldarg_0); //Stack: this
                ctorIL.Emit(OpCodes.Ldarg_1); //Stack: this original
                ctorIL.Emit(OpCodes.Stfld, originalFieldBuilder); //Called this.Original = original => Stack: Empty

                //This block is equivalent to: this.Modified = modified;
                ctorIL.Emit(OpCodes.Ldarg_0);//Stack: this
                ctorIL.Emit(OpCodes.Ldarg_2);//Stack: this modified
                ctorIL.Emit(OpCodes.Stfld, modifiedFieldBuilder); //Called this.Modified = modified => Stack: Empty

                //This block is equivalent to: this.Active = this.Modified;
                ctorIL.Emit(OpCodes.Ldarg_0);//Stack: this
                ctorIL.Emit(OpCodes.Ldarg_0);//Stack: this this
                ctorIL.Emit(OpCodes.Ldfld, modifiedFieldBuilder); //Stack: this this.Modified
                ctorIL.Emit(OpCodes.Stfld, activeFieldBuilder); //Called this.Active = this.Modified => Stack: Empty 

                ctorIL.Emit(OpCodes.Ret);
            }

            private static void CreateConstructorWithTransformFunction<T>(TypeBuilder proxyBuilder, IDictionary<PropertyBuilder, PropertyInfo> recursiveMaskList, FieldBuilder activeFieldBuilder, FieldBuilder originalFieldBuilder, FieldBuilder modifiedFieldBuilder)
            {
                //Constructor is defined as {nameof(T)}Proxy(T original, T newObject, Func<object, object> transformation)
                ConstructorBuilder constructor = proxyBuilder.DefineConstructor(MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.RTSpecialName, CallingConventions.Standard, new Type[3] { typeof(T), typeof(T), typeof(Func<object, object>) });
                ILGenerator ctorIL = constructor.GetILGenerator();

                MethodInfo funcMethod = typeof(Func<object, object>).GetMethod(nameof(Func<object, object>.Invoke));
                if (!typeof(T).IsDefined(typeof(MaskFilterAttribute)))
                {
                    ctorIL.Emit(OpCodes.Ldarg_3); //Stack: transformation
                    ctorIL.Emit(OpCodes.Ldarg_1); //Stack: transformation original
                    ctorIL.Emit(OpCodes.Callvirt, funcMethod); //Called => transformation(original) | Stack: transformedObject

                    ctorIL.Emit(OpCodes.Starg, 2); //Called => newObject = transformedObject | Stack: Empty
                }
                else
                {
                    //Apply mask to algorithm to all properties of the modified variable annotated with RecursiveMaskAttribute.
                    foreach (var e in recursiveMaskList)
                    {
                        var propertyBuilder = e.Key;
                        var property = e.Value;

                        MethodInfo maskMethod = typeof(MaskProxy).GetMethods().First(x => x.Name == nameof(Mask) && x.GetParameters().Length == 3).MakeGenericMethod(property.PropertyType);
                        MethodInfo getter = property.GetGetMethod();

                        ctorIL.Emit(OpCodes.Ldarg_2); //Stack: modified
                        ctorIL.Emit(OpCodes.Ldarg_1); //Stack: modified original
                        ctorIL.Emit(OpCodes.Callvirt, getter); //Called original.{Property} => Stack: modified original.{PROPERTY}
                        ctorIL.Emit(OpCodes.Ldarg_2); //Stack: modified original.{PROPERTY} modified
                        ctorIL.Emit(OpCodes.Callvirt, getter);//Called modified.{Property} => Stack: modified original.{PROPERTY} modified.{PROPERTY}
                        ctorIL.Emit(OpCodes.Ldarg_3); //Stack: modified original.{PROPERTY} modified.{PROPERTY} transformation
                        ctorIL.Emit(OpCodes.Call, maskMethod);//Called: Mask(original.{PROPERTY}, modified.{PROPERTY}, transformation) | Stack: modified proxiedObject

                        ctorIL.Emit(OpCodes.Callvirt, propertyBuilder.GetSetMethod()); //Called modified.{PROPERTY} = proxiedObject => Stack: Empty
                    }

                    var filteredProperties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance).Where(p => p.GetGetMethod()?.IsVirtual == true && p.GetSetMethod()?.IsVirtual == true && p.IsDefined(typeof(MaskFilterAttribute)));
                    foreach (var property in filteredProperties)
                    {
                        MethodInfo maskMethod = typeof(MaskProxy).GetMethods().First(x => x.Name == nameof(Mask) && x.GetParameters().Length == 3).MakeGenericMethod(property.PropertyType);
                        MethodInfo getter = property.GetGetMethod();

                        ctorIL.Emit(OpCodes.Ldarg_2); //Stack: modified
                        ctorIL.Emit(OpCodes.Ldarg_3); //Stack: modified transformation
                        ctorIL.Emit(OpCodes.Ldarg_1); //Stack: modified transformation original
                        ctorIL.Emit(OpCodes.Callvirt, getter); //Called original.{Property} => Stack: modified transformation original.{PROPERTY}
                        ctorIL.Emit(OpCodes.Callvirt, funcMethod); //Called => transformation(original.{PROPERTY}) | Stack: modified transformedObject

                        ctorIL.Emit(OpCodes.Callvirt, property.SetMethod); //Called modified.{PROPERTY} = proxiedObject => Stack: Empty
                    }
                }

                //This block is equivalent to: this.Original = original;
                ctorIL.Emit(OpCodes.Ldarg_0); //Stack: this
                ctorIL.Emit(OpCodes.Ldarg_1); //Stack: this original
                ctorIL.Emit(OpCodes.Stfld, originalFieldBuilder); //Called this.Original = original => Stack: Empty

                //This block is equivalent to: this.Modified = newObject;
                ctorIL.Emit(OpCodes.Ldarg_0);//Stack: this
                ctorIL.Emit(OpCodes.Ldarg_2);//Stack: this newObject
                ctorIL.Emit(OpCodes.Stfld, modifiedFieldBuilder); //Called this.Modified = newObject => Stack: Empty

                //This block is equivalent to: this.Active = this.Modified;
                ctorIL.Emit(OpCodes.Ldarg_0);//Stack: this
                ctorIL.Emit(OpCodes.Ldarg_0);//Stack: this this
                ctorIL.Emit(OpCodes.Ldfld, modifiedFieldBuilder); //Stack: this this.Modified
                ctorIL.Emit(OpCodes.Stfld, activeFieldBuilder); //Called this.Active = this.Modified => Stack: Empty 

                ctorIL.Emit(OpCodes.Ret);
            }

            private static TypeBuilder GetTypeBuilder<T>(string typeSignature)
            {
                typeSignature = typeSignature.Replace(",", "..").Replace("=", "...");
                var an = new AssemblyName(typeSignature);
                AssemblyBuilder assemblyBuilder = AppDomain.CurrentDomain.DefineDynamicAssembly(an, AssemblyBuilderAccess.Run);
                ModuleBuilder moduleBuilder = assemblyBuilder.DefineDynamicModule("Mask.Builder");
                TypeBuilder tb = moduleBuilder.DefineType(typeSignature, TypeAttributes.Public |
                                                                        TypeAttributes.Class |
                                                                        TypeAttributes.AutoClass |
                                                                        TypeAttributes.AnsiClass |
                                                                        TypeAttributes.BeforeFieldInit |
                                                                        TypeAttributes.AutoLayout, typeof(T));

                tb.AddInterfaceImplementation(typeof(IMasked<T>));
                return tb;
            }
        }

        private static class Enumerable
        {
            private enum EnumerableType
            {
                Enumerable = 0,
                Dictionary = 1
            }

            private static EnumerableType GetEnumerableType(Type t)
            {
                if (typeof(IDictionary).IsAssignableFrom(t))
                    return EnumerableType.Dictionary;
                return EnumerableType.Enumerable;
            }

            internal static Type CompileResultType<T>(Type baseType)
            {
                TypeBuilder proxyBuilder = GetTypeBuilder(baseType, $"{typeof(T).FullName}Proxy");

                CreateConstructorWithMaskObject<T>(proxyBuilder, baseType);
                CreateConstructorWithTransformFunction<T>(proxyBuilder, baseType);

                Type objectType = proxyBuilder.CreateType();
                return objectType;
            }

            private static void CreateConstructorWithMaskObject<T>(TypeBuilder proxyBuilder, Type baseType)
            {
                EnumerableType enumerableType = GetEnumerableType(baseType);
                ConstructorInfo baseConstructor = proxyBuilder.BaseType.GetConstructor(Type.EmptyTypes);
                MethodInfo getEnumeratorMethod = typeof(IEnumerable).GetMethod(nameof(IEnumerable.GetEnumerator));
                MethodInfo moveNextMethod = typeof(IEnumerator).GetMethod(nameof(IEnumerator.MoveNext));

                //Constructor is defined as {nameof(T)}Proxy(T original, T modified)
                ConstructorBuilder constructor = proxyBuilder.DefineConstructor(MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.RTSpecialName, CallingConventions.Standard, new Type[2] { typeof(T), typeof(T) });
                ILGenerator ctorIL = constructor.GetILGenerator();

                //Call Base Constructor
                ctorIL.Emit(OpCodes.Ldarg_0); //Stack: this
                ctorIL.Emit(OpCodes.Call, baseConstructor);//Call => base() | Stack: Empty

                //Initialize original Enumerator: IEnumerator originalEnumerator = original.GetEnumerator();
                var originalEnumerator = ctorIL.DeclareLocal(typeof(IEnumerator));
                ctorIL.Emit(OpCodes.Ldarg_1); //Stack: original
                ctorIL.Emit(OpCodes.Callvirt, getEnumeratorMethod);//Called => original.GetEnumerator() | Stack: original.GetEnumerator()
                ctorIL.Emit(OpCodes.Stloc, originalEnumerator); //Called => originalEnumerator = original.GetEnumerator() | Stack: Empty

                //Initialize modified Enumerator: IEnumerator modifiedEnumerator = modified.GetEnumerator();
                var modifiedEnumerator = ctorIL.DeclareLocal(typeof(IEnumerator));
                ctorIL.Emit(OpCodes.Ldarg_2); //Stack: modified
                ctorIL.Emit(OpCodes.Callvirt, getEnumeratorMethod);//Called => modified.GetEnumerator() | Stack: modified.GetEnumerator()
                ctorIL.Emit(OpCodes.Stloc, modifiedEnumerator); //Called => modifiedEnumerator = modified.GetEnumerator() | Stack: Empty

                // Preparing labels
                var loop = ctorIL.DefineLabel();
                var loopEnd = ctorIL.DefineLabel();

                //While
                ctorIL.MarkLabel(loop);
                ctorIL.Emit(OpCodes.Ldloc, originalEnumerator); //Stack: originalEnumerator
                ctorIL.Emit(OpCodes.Callvirt, moveNextMethod);//Called => originalEnumerator.MoveNext() | Stack: originalEnumerator.MoveNext()
                ctorIL.Emit(OpCodes.Brfalse, loopEnd); //If false, then we reached the end of the enumerator and should jump to the end of the loop
                //Loop Body
                ctorIL.Emit(OpCodes.Ldloc, modifiedEnumerator); //Stack: modifiedEnumerator
                ctorIL.Emit(OpCodes.Callvirt, moveNextMethod);//Called => modifiedEnumerator.MoveNext() | Stack: modifiedEnumerator.MoveNext()
                ctorIL.Emit(OpCodes.Pop); //Stack: Empty
                //Add proxy item
                switch (enumerableType)
                {
                    case EnumerableType.Enumerable:
                        {
                            Type genericType = typeof(T).GetGenericArguments()[0];
                            MethodInfo maskMethodGeneric = typeof(MaskProxy).GetMethods().First(x => x.Name == nameof(Mask) && x.GetParameters().Length == 2).MakeGenericMethod(genericType);
                            MethodInfo listAdd = typeof(IList).GetMethod(nameof(IList.Add));
                            MethodInfo currentMethod = typeof(IEnumerator).GetProperty(nameof(IEnumerator.Current)).GetMethod;

                            ctorIL.Emit(OpCodes.Ldarg_0); //Stack: this
                            ctorIL.Emit(OpCodes.Ldloc, originalEnumerator); //Stack: this originalEnumerator
                            ctorIL.Emit(OpCodes.Callvirt, currentMethod);//Called => originalEnumerator.Current | Stack: this originalEnumerator.Current
                            ctorIL.Emit(OpCodes.Ldloc, modifiedEnumerator); //Stack: this originalEnumerator.Current modifiedEnumerator
                            ctorIL.Emit(OpCodes.Callvirt, currentMethod);//Called => modifiedEnumerator.Current | Stack: this originalEnumerator.Current modifiedEnumerator.Current
                            ctorIL.Emit(OpCodes.Call, maskMethodGeneric);//Called => Mask(originalEnumerator.Current, modifiedEnumerator.Current) | Stack: this proxiedItem
                            ctorIL.Emit(OpCodes.Callvirt, listAdd); //Called => this.Add(proxiedItem) | Stack: [RESULT]
                            ctorIL.Emit(OpCodes.Pop); //Stack: Empty
                            break;
                        }
                    case EnumerableType.Dictionary:
                        {
                            Type keyGenericType = typeof(T).GetGenericArguments()[0];
                            Type valueGenericType = typeof(T).GetGenericArguments()[1];
                            MethodInfo keyMaskMethodGeneric = typeof(MaskProxy).GetMethods().First(x => x.Name == nameof(Mask) && x.GetParameters().Length == 2).MakeGenericMethod(keyGenericType);
                            MethodInfo valueMaskMethodGeneric = typeof(MaskProxy).GetMethods().First(x => x.Name == nameof(Mask) && x.GetParameters().Length == 2).MakeGenericMethod(valueGenericType);
                            MethodInfo listAdd = typeof(IDictionary).GetMethod(nameof(IDictionary.Add));
                            MethodInfo keyMethod = typeof(IDictionaryEnumerator).GetProperty(nameof(IDictionaryEnumerator.Key)).GetMethod;
                            MethodInfo valueMethod = typeof(IDictionaryEnumerator).GetProperty(nameof(IDictionaryEnumerator.Value)).GetMethod;

                            ctorIL.Emit(OpCodes.Ldarg_0); //Stack: this
                            ctorIL.Emit(OpCodes.Ldloc, originalEnumerator); //Stack: this originalEnumerator
                            ctorIL.Emit(OpCodes.Callvirt, keyMethod); //Called => originalEnumerator.Key | Stack: this originalEnumerator.Key
                            ctorIL.Emit(OpCodes.Ldloc, modifiedEnumerator); //Stack: this originalEnumerator.Key modifiedEnumerator
                            ctorIL.Emit(OpCodes.Callvirt, keyMethod); //Called => modifiedEnumerator.Key | Stack: this originalEnumerator.Key modifiedEnumerator.Key
                            ctorIL.Emit(OpCodes.Call, keyMaskMethodGeneric);//Called => Mask(originalEnumerator.Key, modifiedEnumerator.Key) | Stack: this proxiedKey
                            ctorIL.Emit(OpCodes.Ldloc, originalEnumerator); //Stack: this proxiedKey originalEnumerator
                            ctorIL.Emit(OpCodes.Callvirt, valueMethod);//Called => originalEnumerator.Value | Stack: this proxiedKey originalEnumerator.Value
                            ctorIL.Emit(OpCodes.Ldloc, modifiedEnumerator); //Stack: this proxiedKey originalEnumerator.Current modifiedEnumerator
                            ctorIL.Emit(OpCodes.Callvirt, valueMethod);//Called => modifiedEnumerator.Value | Stack: this proxiedKey originalEnumerator.Value modifiedEnumerator.Value
                            ctorIL.Emit(OpCodes.Call, valueMaskMethodGeneric);//Called => Mask(originalEnumerator.Value, modifiedEnumerator.Value) | Stack: this proxiedKey proxiedItem
                            ctorIL.Emit(OpCodes.Callvirt, listAdd); //Called => this.Add(proxiedKey, proxiedItem) | Stack: Empty
                            break;
                        }
                }
                ctorIL.Emit(OpCodes.Br_S, loop); //Return to the beggining of the loop
                ctorIL.MarkLabel(loopEnd);
                ctorIL.Emit(OpCodes.Ret);
            }

            private static void CreateConstructorWithTransformFunction<T>(TypeBuilder proxyBuilder, Type baseType)
            {
                EnumerableType enumerableType = GetEnumerableType(baseType);
                ConstructorInfo baseConstructor = proxyBuilder.BaseType.GetConstructor(Type.EmptyTypes);
                MethodInfo getEnumeratorMethod = typeof(IEnumerable).GetMethod(nameof(IEnumerable.GetEnumerator));
                MethodInfo moveNextMethod = typeof(IEnumerator).GetMethod(nameof(IEnumerator.MoveNext));

                //Constructor is defined as {nameof(T)}Proxy(T original, T newObject, Func<object, object> transformation)
                ConstructorBuilder constructor = proxyBuilder.DefineConstructor(MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.RTSpecialName, CallingConventions.Standard, new Type[3] { typeof(T), typeof(T), typeof(Func<object, object>) });
                ILGenerator ctorIL = constructor.GetILGenerator();

                //Call Base Constructor
                ctorIL.Emit(OpCodes.Ldarg_0); //Stack: this
                ctorIL.Emit(OpCodes.Call, baseConstructor);//Call => base() | Stack: Empty

                //Initialize original Enumerator: IEnumerator originalEnumerator = original.GetEnumerator();
                var originalEnumerator = ctorIL.DeclareLocal(typeof(IEnumerator));
                ctorIL.Emit(OpCodes.Ldarg_1); //Stack: original
                ctorIL.Emit(OpCodes.Callvirt, getEnumeratorMethod);//Called => original.GetEnumerator() | Stack: original.GetEnumerator()
                ctorIL.Emit(OpCodes.Stloc, originalEnumerator); //Called => originalEnumerator = original.GetEnumerator() | Stack: Empty

                //Initialize modified Enumerator: IEnumerator modifiedEnumerator = modified.GetEnumerator();
                var modifiedEnumerator = ctorIL.DeclareLocal(typeof(IEnumerator));
                ctorIL.Emit(OpCodes.Ldarg_2); //Stack: modified
                ctorIL.Emit(OpCodes.Callvirt, getEnumeratorMethod);//Called => modified.GetEnumerator() | Stack: modified.GetEnumerator()
                ctorIL.Emit(OpCodes.Stloc, modifiedEnumerator); //Called => modifiedEnumerator = modified.GetEnumerator() | Stack: Empty

                // Preparing labels
                var loop = ctorIL.DefineLabel();
                var loopEnd = ctorIL.DefineLabel();

                //While
                ctorIL.MarkLabel(loop);
                ctorIL.Emit(OpCodes.Ldloc, originalEnumerator); //Stack: originalEnumerator
                ctorIL.Emit(OpCodes.Callvirt, moveNextMethod);//Called => originalEnumerator.MoveNext() | Stack: originalEnumerator.MoveNext()
                ctorIL.Emit(OpCodes.Brfalse, loopEnd); //If false, then we reached the end of the enumerator and should jump to the end of the loop
                //Loop Body
                ctorIL.Emit(OpCodes.Ldloc, modifiedEnumerator); //Stack: modifiedEnumerator
                ctorIL.Emit(OpCodes.Callvirt, moveNextMethod);//Called => modifiedEnumerator.MoveNext() | Stack: modifiedEnumerator.MoveNext()
                ctorIL.Emit(OpCodes.Pop); //Stack: Empty
                //Add proxy item
                switch (enumerableType)
                {
                    case EnumerableType.Enumerable:
                        {
                            Type genericType = typeof(T).GetGenericArguments()[0];
                            MethodInfo maskMethodGeneric = typeof(MaskProxy).GetMethods().First(x => x.Name == nameof(Mask) && x.GetParameters().Length == 3).MakeGenericMethod(genericType);
                            MethodInfo listAdd = typeof(IList).GetMethod(nameof(IList.Add));
                            MethodInfo currentMethod = typeof(IEnumerator).GetProperty(nameof(IEnumerator.Current)).GetMethod;

                            ctorIL.Emit(OpCodes.Ldarg_0); //Stack: this
                            ctorIL.Emit(OpCodes.Ldloc, originalEnumerator); //Stack: this originalEnumerator
                            ctorIL.Emit(OpCodes.Callvirt, currentMethod);//Called => originalEnumerator.Current | Stack: this originalEnumerator.Current
                            ctorIL.Emit(OpCodes.Ldloc, modifiedEnumerator); //Stack: this originalEnumerator.Current modifiedEnumerator
                            ctorIL.Emit(OpCodes.Callvirt, currentMethod);//Called => modifiedEnumerator.Current | Stack: this originalEnumerator.Current modifiedEnumerator.Current
                            ctorIL.Emit(OpCodes.Ldarg_3); //Stack: this originalEnumerator.Current modifiedEnumerator.Current transformation
                            ctorIL.Emit(OpCodes.Call, maskMethodGeneric);//Called => Mask(originalEnumerator.Current, modifiedEnumerator.Current, transformation) | Stack: this proxiedItem
                            ctorIL.Emit(OpCodes.Callvirt, listAdd); //Called => this.Add(proxiedItem) | Stack: [RESULT]
                            ctorIL.Emit(OpCodes.Pop); //Stack: Empty
                            break;
                        }
                    case EnumerableType.Dictionary:
                        {
                            Type keyGenericType = typeof(T).GetGenericArguments()[0];
                            Type valueGenericType = typeof(T).GetGenericArguments()[1];
                            MethodInfo keyMaskMethodGeneric = typeof(MaskProxy).GetMethods().First(x => x.Name == nameof(Mask) && x.GetParameters().Length == 3).MakeGenericMethod(keyGenericType);
                            MethodInfo valueMaskMethodGeneric = typeof(MaskProxy).GetMethods().First(x => x.Name == nameof(Mask) && x.GetParameters().Length == 3).MakeGenericMethod(valueGenericType);
                            MethodInfo listAdd = typeof(IDictionary).GetMethod(nameof(IDictionary.Add));
                            MethodInfo keyMethod = typeof(IDictionaryEnumerator).GetProperty(nameof(IDictionaryEnumerator.Key)).GetMethod;
                            MethodInfo valueMethod = typeof(IDictionaryEnumerator).GetProperty(nameof(IDictionaryEnumerator.Value)).GetMethod;

                            ctorIL.Emit(OpCodes.Ldarg_0); //Stack: this
                            ctorIL.Emit(OpCodes.Ldloc, originalEnumerator); //Stack: this originalEnumerator
                            ctorIL.Emit(OpCodes.Callvirt, keyMethod); //Called => originalEnumerator.Key | Stack: this originalEnumerator.Key
                            ctorIL.Emit(OpCodes.Ldloc, modifiedEnumerator); //Stack: this originalEnumerator.Key modifiedEnumerator
                            ctorIL.Emit(OpCodes.Callvirt, keyMethod); //Called => modifiedEnumerator.Key | Stack: this originalEnumerator.Key modifiedEnumerator.Key
                            ctorIL.Emit(OpCodes.Ldarg_3); //Stack: this originalEnumerator.Key modifiedEnumerator.Key transformation
                            ctorIL.Emit(OpCodes.Call, keyMaskMethodGeneric);//Called => Mask(originalEnumerator.Key, modifiedEnumerator.Key, transformation) | Stack: this proxiedKey
                            ctorIL.Emit(OpCodes.Ldloc, originalEnumerator); //Stack: this proxiedKey originalEnumerator
                            ctorIL.Emit(OpCodes.Callvirt, valueMethod);//Called => originalEnumerator.Value | Stack: this proxiedKey originalEnumerator.Value
                            ctorIL.Emit(OpCodes.Ldloc, modifiedEnumerator); //Stack: this proxiedKey originalEnumerator.Current modifiedEnumerator
                            ctorIL.Emit(OpCodes.Callvirt, valueMethod);//Called => modifiedEnumerator.Value | Stack: this proxiedKey originalEnumerator.Value modifiedEnumerator.Value
                            ctorIL.Emit(OpCodes.Ldarg_3); //Stack: this proxiedKey originalEnumerator.Value modifiedEnumerator.Value transformation
                            ctorIL.Emit(OpCodes.Call, valueMaskMethodGeneric);//Called => Mask(originalEnumerator.Value, modifiedEnumerator.Value, transformation) | Stack: this proxiedKey proxiedItem
                            ctorIL.Emit(OpCodes.Callvirt, listAdd); //Called => this.Add(proxiedKey, proxiedItem) | Stack: Empty
                            break;
                        }
                }
                ctorIL.Emit(OpCodes.Br_S, loop); //Return to the beggining of the loop
                ctorIL.MarkLabel(loopEnd);
                ctorIL.Emit(OpCodes.Ret);
            }

            private static TypeBuilder GetTypeBuilder(Type baseType, string typeSignature)
            {
                typeSignature = typeSignature.Replace(",", "..").Replace("=", "...");
                var an = new AssemblyName(typeSignature);
                AssemblyBuilder assemblyBuilder = AppDomain.CurrentDomain.DefineDynamicAssembly(an, AssemblyBuilderAccess.Run);
                ModuleBuilder moduleBuilder = assemblyBuilder.DefineDynamicModule("Mask.Builder");
                TypeBuilder tb = moduleBuilder.DefineType(typeSignature, TypeAttributes.Public |
                                                                        TypeAttributes.Class |
                                                                        TypeAttributes.AutoClass |
                                                                        TypeAttributes.AnsiClass |
                                                                        TypeAttributes.BeforeFieldInit |
                                                                        TypeAttributes.AutoLayout, baseType);
                return tb;
            }
        }

        private static PropertyBuilder CreateProperty<T>(TypeBuilder proxyBuilder, PropertyInfo targetProperty, FieldBuilder activeBuilder)
        {
            string propertyName = targetProperty.Name;
            Type propertyType = targetProperty.PropertyType;

            //Property
            PropertyBuilder propertyBuilder = proxyBuilder.DefineProperty(propertyName, PropertyAttributes.HasDefault, propertyType, null);

            //Getter Builder
            MethodBuilder getPropMthdBldr = proxyBuilder.DefineMethod($"get_{propertyName}", MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.HideBySig | MethodAttributes.Virtual, propertyType, Type.EmptyTypes);
            ILGenerator getIl = getPropMthdBldr.GetILGenerator();

            //Load 'this' onto the stack
            getIl.Emit(OpCodes.Ldarg_0);
            //Loads 'Active' of 'this' (loaded on previous line) and pushes it to the stack
            getIl.Emit(OpCodes.Ldfld, activeBuilder);
            // Call the getter method passing the object on the stack pushing the result into the stack
            getIl.Emit(OpCodes.Call, targetProperty.GetGetMethod());
            // Return from the method
            getIl.Emit(OpCodes.Ret);



            //Setter Builder
            MethodBuilder setPropMthdBldr = proxyBuilder.DefineMethod($"set_{propertyName}", MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.HideBySig | MethodAttributes.Virtual, null, new[] { propertyType });

            ILGenerator setIl = setPropMthdBldr.GetILGenerator();
            //Loads 'this' onto the stack
            setIl.Emit(OpCodes.Ldarg_0);
            //Loads 'Active' of 'this' (loaded on previous line) and pushes it to the stack
            setIl.Emit(OpCodes.Ldfld, activeBuilder);
            //Loads 'value' onto the stack
            setIl.Emit(OpCodes.Ldarg_1);
            // Call the setter method
            setIl.Emit(OpCodes.Call, targetProperty.GetSetMethod());
            setIl.Emit(OpCodes.Ret);

            //Adicionar à métodos à property
            propertyBuilder.SetGetMethod(getPropMthdBldr);
            propertyBuilder.SetSetMethod(setPropMthdBldr);

            return propertyBuilder;
        }

        private static void CreateMaskedInterfaceMethods<T>(TypeBuilder tb, FieldBuilder activeBuilder, FieldBuilder originalBuilder, FieldBuilder modifiedBuilder)
        {
            MethodBuilder asMaskedBuilder = tb.DefineMethod($"{nameof(IMasked<T>.AsMasked)}Private", MethodAttributes.Virtual | MethodAttributes.Private);
            ILGenerator maskedIl = asMaskedBuilder.GetILGenerator();

            //This block is equivalent to: this.Active = this.Modified;
            //Loads 'this' onto the stack
            maskedIl.Emit(OpCodes.Ldarg_0);
            //Loads 'this' onto the stack
            maskedIl.Emit(OpCodes.Ldarg_0);
            //Loads 'Modified' of this to the stack
            maskedIl.Emit(OpCodes.Ldfld, modifiedBuilder);
            //Assigns 'Modified' to Active field
            maskedIl.Emit(OpCodes.Stfld, activeBuilder);
            maskedIl.Emit(OpCodes.Ret);


            MethodBuilder asOriginalBuilder = tb.DefineMethod($"{nameof(IMasked<T>.AsOriginal)}Private", MethodAttributes.Virtual | MethodAttributes.Private);
            ILGenerator originalIl = asOriginalBuilder.GetILGenerator();

            //This block is equivalent to: this.Active = this.Original;
            //Loads 'this' onto the stack
            originalIl.Emit(OpCodes.Ldarg_0);
            //Loads 'this' onto the stack
            originalIl.Emit(OpCodes.Ldarg_0);
            //Loads 'Original' of this to the stack
            originalIl.Emit(OpCodes.Ldfld, originalBuilder);
            //Assigns 'Original' to Active field
            originalIl.Emit(OpCodes.Stfld, activeBuilder);
            originalIl.Emit(OpCodes.Ret);


            tb.DefineMethodOverride(asMaskedBuilder, typeof(IMasked<T>).GetMethod(nameof(IMasked<T>.AsMasked)));
            tb.DefineMethodOverride(asOriginalBuilder, typeof(IMasked<T>).GetMethod(nameof(IMasked<T>.AsOriginal)));
        }
        #endregion
    }
}
