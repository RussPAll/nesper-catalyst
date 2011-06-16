using System;
using System.Collections.Generic;

using com.espertech.esper.client;

namespace NEsper.Catalyst.Client
{
    public interface ICatalystAdministrator
        : EPAdministrator
    {
        /// <summary>
        /// Registers a type.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        void RegisterType<T>();

        /// <summary>
        /// Declares a native type.  Not required, but encouraged.
        /// </summary>
        /// <param name="nativeType">Type of the native.</param>
        void RegisterType(Type nativeType);

        /// <summary>
        /// Adds the type of the event.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        void AddEventType<T>();

        /// <summary>
        /// Adds the type of the event with a specific name.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="eventTypeName">The name.</param>
        void AddEventType<T>(string eventTypeName);

        /// <summary>
        /// Adds the type of the event.
        /// </summary>
        /// <param name="eventTypeName">Name of the event type.</param>
        /// <param name="nativeEventType">Native event type.</param>
        void AddEventType(string eventTypeName, Type nativeEventType);

        /// <summary>
        /// Adds the type of the event.
        /// </summary>
        /// <param name="eventTypeName">Name of the event type.</param>
        /// <param name="typeMap">The type map.</param>
        void AddEventType(string eventTypeName, IDictionary<string, object> typeMap);

        /// <summary>
        /// Adds the type of the event.
        /// </summary>
        /// <param name="eventTypeName">Name of the event type.</param>
        /// <param name="typeMap">The type map.</param>
        /// <param name="superTypes">The super types.</param>
        void AddEventType(string eventTypeName, IDictionary<string, object> typeMap, params string[] superTypes);

        /// <summary>
        /// Occurs when a native type is registered.
        /// </summary>
        event EventHandler<TypeEventArgs> TypeRegistered;
    }
}