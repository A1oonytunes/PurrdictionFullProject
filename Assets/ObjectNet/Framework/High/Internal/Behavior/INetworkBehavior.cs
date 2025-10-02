using System;

namespace com.onlineobject.objectnet {
    /// <summary>
    /// Define an interface for network behavior management.
    /// </summary>
    public interface INetworkBehavior {

        /// <summary>
        /// Sets the behavior mode for the network element and updates the element if it exists.
        /// </summary>
        /// <param name="behaviorMode">The behavior mode to set.</param>
        /// <param name="detectOnStart">Shall to execute network start detection.</param>
        void SetBehaviorMode(BehaviorMode behaviorMode, bool detectOnStart = true);

        /// <summary>
        /// Retrieves the current behavior mode.
        /// </summary>
        /// <returns>The <see cref="BehaviorMode"/> that is currently set.</returns>
        BehaviorMode GetBehaviorMode();

        /// <summary>
        /// Initializes all synchronized fields that are part of the network behavior.
        /// This method should set up the initial state for fields that need to be kept in sync across the network.
        /// </summary>
        void InitializeSynchonizedFields();

        /// <summary>
        /// Update Synchonized variables
        /// </summary>
        void UpdateSynchonizedVariables();

        /// <summary>
        /// Registers a variable to be synchronized across the network.
        /// </summary>
        /// <param name="variableName">The name of the variable to be registered.</param>
        /// <param name="variable">The variable instance that implements IVariable, which will be synchronized.</param>
        void RegisterSynchronizedVariable(string variableName, IVariable variable);

        /// <summary>
        /// Executes a parameterless action remotely.
        /// </summary>
        /// <param name="method">The action to be executed remotely.</param>
        void NetworkExecute(Action method);

        /// <summary>
        /// Executes a single-parameter action remotely with the specified arguments.
        /// </summary>
        /// <typeparam name="T">The type of the parameter for the action.</typeparam>
        /// <param name="method">The action to be executed remotely.</param>
        /// <param name="args">The arguments to pass to the action.</param>
        void NetworkExecute<T>(Action<T> method, params object[] args);

        /// <summary>
        /// Executes a two-parameter action remotely with the specified arguments.
        /// </summary>
        /// <typeparam name="T0">The type of the first parameter for the action.</typeparam>
        /// <typeparam name="T1">The type of the second parameter for the action.</typeparam>
        /// <param name="method">The action to be executed remotely.</param>
        /// <param name="args">The arguments to pass to the action.</param>
        void NetworkExecute<T0, T1>(Action<T0, T1> method, params object[] args);

        /// <summary>
        /// Executes a two-parameter action remotely with the specified arguments.
        /// </summary>
        /// <typeparam name="T0">The type of the first parameter for the action.</typeparam>
        /// <typeparam name="T1">The type of the second parameter for the action.</typeparam>
        /// <typeparam name="T2">The type of the third parameter for the action.</typeparam>
        /// <param name="method">The action to be executed remotely.</param>
        /// <param name="args">The arguments to pass to the action.</param>
        void NetworkExecute<T0, T1, T2>(Action<T0, T1, T2> method, params object[] args);

        /// <summary>
        /// Executes a three-parameter action remotely with the specified arguments.
        /// </summary>
        /// <typeparam name="T0">The type of the first parameter for the action.</typeparam>
        /// <typeparam name="T1">The type of the second parameter for the action.</typeparam>
        /// <typeparam name="T2">The type of the third parameter for the action.</typeparam>
        /// <typeparam name="T3">The type of the fourth parameter for the action.</typeparam>
        /// <param name="method">The action to be executed remotely.</param>
        /// <param name="args">The arguments to pass to the action.</param>
        void NetworkExecute<T0, T1, T2, T3>(Action<T0, T1, T2, T3> method, params object[] args);

        /// <summary>
        /// Executes a four-parameter action remotely with the specified arguments.
        /// </summary>
        /// <typeparam name="T0">The type of the first parameter for the action.</typeparam>
        /// <typeparam name="T1">The type of the second parameter for the action.</typeparam>
        /// <typeparam name="T2">The type of the third parameter for the action.</typeparam>
        /// <typeparam name="T3">The type of the fourth parameter for the action.</typeparam>
        /// <typeparam name="T4">The type of the fifth parameter for the action.</typeparam>
        /// <param name="method">The action to be executed remotely.</param>
        /// <param name="args">The arguments to pass to the action.</param>
        void NetworkExecute<T0, T1, T2, T3, T4>(Action<T0, T1, T2, T3, T4> method, params object[] args);

        /// <summary>
        /// Executes a five-parameter action remotely with the specified arguments.
        /// </summary>
        /// <typeparam name="T0">The type of the first parameter for the action.</typeparam>
        /// <typeparam name="T1">The type of the second parameter for the action.</typeparam>
        /// <typeparam name="T2">The type of the third parameter for the action.</typeparam>
        /// <typeparam name="T3">The type of the fourth parameter for the action.</typeparam>
        /// <typeparam name="T4">The type of the fifth parameter for the action.</typeparam>
        /// <typeparam name="T5">The type of the sixth parameter for the action.</typeparam>
        /// <param name="method">The action to be executed remotely.</param>
        /// <param name="args">The arguments to pass to the action.</param>
        void NetworkExecute<T0, T1, T2, T3, T4, T5>(Action<T0, T1, T2, T3, T4, T5> method, params object[] args);

        /// <summary>
        /// Executes a six-parameter action remotely with the specified arguments.
        /// </summary>
        /// <typeparam name="T0">The type of the first parameter for the action.</typeparam>
        /// <typeparam name="T1">The type of the second parameter for the action.</typeparam>
        /// <typeparam name="T2">The type of the third parameter for the action.</typeparam>
        /// <typeparam name="T3">The type of the fourth parameter for the action.</typeparam>
        /// <typeparam name="T4">The type of the fifth parameter for the action.</typeparam>
        /// <typeparam name="T5">The type of the sixth parameter for the action.</typeparam>
        /// <typeparam name="T6">The type of the seventh parameter for the action.</typeparam>
        /// <param name="method">The action to be executed remotely.</param>
        /// <param name="args">The arguments to pass to the action.</param>
        void NetworkExecute<T0, T1, T2, T3, T4, T5, T6>(Action<T0, T1, T2, T3, T4, T5, T6> method, params object[] args);

        /// <summary>
        /// Executes a seven-parameter action remotely with the specified arguments.
        /// </summary>
        /// <typeparam name="T0">The type of the first parameter for the action.</typeparam>
        /// <typeparam name="T1">The type of the second parameter for the action.</typeparam>
        /// <typeparam name="T2">The type of the third parameter for the action.</typeparam>
        /// <typeparam name="T3">The type of the fourth parameter for the action.</typeparam>
        /// <typeparam name="T4">The type of the fifth parameter for the action.</typeparam>
        /// <typeparam name="T5">The type of the sixth parameter for the action.</typeparam>
        /// <typeparam name="T6">The type of the seventh parameter for the action.</typeparam>
        /// <typeparam name="T7">The type of the eighth parameter for the action.</typeparam>
        /// <param name="method">The action to be executed remotely.</param>
        /// <param name="args">The arguments to pass to the action.</param>
        void NetworkExecute<T0, T1, T2, T3, T4, T5, T6, T7>(Action<T0, T1, T2, T3, T4, T5, T6, T7> method, params object[] args);

        /// <summary>
        /// Executes an eight-parameter action remotely with the specified arguments.
        /// </summary>
        /// <typeparam name="T0">The type of the first parameter for the action.</typeparam>
        /// <typeparam name="T1">The type of the second parameter for the action.</typeparam>
        /// <typeparam name="T2">The type of the third parameter for the action.</typeparam>
        /// <typeparam name="T3">The type of the fourth parameter for the action.</typeparam>
        /// <typeparam name="T4">The type of the fifth parameter for the action.</typeparam>
        /// <typeparam name="T5">The type of the sixth parameter for the action.</typeparam>
        /// <typeparam name="T6">The type of the seventh parameter for the action.</typeparam>
        /// <typeparam name="T7">The type of the eighth parameter for the action.</typeparam>
        /// <typeparam name="T8">The type of the ninth parameter for the action.</typeparam>
        /// <param name="method">The action to be executed remotely.</param>
        /// <param name="args">The arguments to pass to the action.</param>
        void NetworkExecute<T0, T1, T2, T3, T4, T5, T6, T7, T8>(Action<T0, T1, T2, T3, T4, T5, T6, T7, T8> method, params object[] args);

        /// <summary>
        /// Executes a nine-parameter action remotely with the specified arguments.
        /// </summary>
        /// <typeparam name="T0">The type of the first parameter for the action.</typeparam>
        /// <typeparam name="T1">The type of the second parameter for the action.</typeparam>
        /// <typeparam name="T2">The type of the third parameter for the action.</typeparam>
        /// <typeparam name="T3">The type of the fourth parameter for the action.</typeparam>
        /// <typeparam name="T4">The type of the fifth parameter for the action.</typeparam>
        /// <typeparam name="T5">The type of the sixth parameter for the action.</typeparam>
        /// <typeparam name="T6">The type of the seventh parameter for the action.</typeparam>
        /// <typeparam name="T7">The type of the eighth parameter for the action.</typeparam>
        /// <typeparam name="T8">The type of the ninth parameter for the action.</typeparam>
        /// <typeparam name="T9">The type of the tenth parameter for the action.</typeparam>
        /// <param name="method">The action to be executed remotely.</param>
        /// <param name="args">The arguments to pass to the action.</param>
        void NetworkExecute<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9>(Action<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9> method, params object[] args);

        /// <summary>
        /// Executes a parameterless action remotely on server or host
        /// </summary>
        /// <param name="method">The action to be executed remotely.</param>
        void NetworkExecuteOnClient(Action method);

        /// <summary>
        /// Executes a single-parameter action remotely with the specified arguments on server or host
        /// </summary>
        /// <typeparam name="T">The type of the parameter for the action.</typeparam>
        /// <param name="method">The action to be executed remotely.</param>
        /// <param name="args">The arguments to pass to the action.</param>
        void NetworkExecuteOnClient<T>(Action<T> method, params object[] args);

        /// <summary>
        /// Executes a two-parameter action remotely with the specified arguments on server or host
        /// </summary>
        /// <typeparam name="T0">The type of the first parameter for the action.</typeparam>
        /// <typeparam name="T1">The type of the second parameter for the action.</typeparam>
        /// <param name="method">The action to be executed remotely.</param>
        /// <param name="args">The arguments to pass to the action.</param>
        void NetworkExecuteOnClient<T0, T1>(Action<T0, T1> method, params object[] args);

        /// <summary>
        /// Executes a two-parameter action remotely with the specified arguments.
        /// </summary>
        /// <typeparam name="T0">The type of the first parameter for the action.</typeparam>
        /// <typeparam name="T1">The type of the second parameter for the action.</typeparam>
        /// <typeparam name="T2">The type of the third parameter for the action.</typeparam>
        /// <param name="method">The action to be executed remotely.</param>
        /// <param name="args">The arguments to pass to the action.</param>
        void NetworkExecuteOnClient<T0, T1, T2>(Action<T0, T1, T2> method, params object[] args);

        /// <summary>
        /// Executes a three-parameter action remotely with the specified arguments on server or host
        /// </summary>
        /// <typeparam name="T0">The type of the first parameter for the action.</typeparam>
        /// <typeparam name="T1">The type of the second parameter for the action.</typeparam>
        /// <typeparam name="T2">The type of the third parameter for the action.</typeparam>
        /// <typeparam name="T3">The type of the fourth parameter for the action.</typeparam>
        /// <param name="method">The action to be executed remotely.</param>
        /// <param name="args">The arguments to pass to the action.</param>
        void NetworkExecuteOnClient<T0, T1, T2, T3>(Action<T0, T1, T2, T3> method, params object[] args);

        /// <summary>
        /// Executes a four-parameter action remotely with the specified arguments on server or host
        /// </summary>
        /// <typeparam name="T0">The type of the first parameter for the action.</typeparam>
        /// <typeparam name="T1">The type of the second parameter for the action.</typeparam>
        /// <typeparam name="T2">The type of the third parameter for the action.</typeparam>
        /// <typeparam name="T3">The type of the fourth parameter for the action.</typeparam>
        /// <typeparam name="T4">The type of the fifth parameter for the action.</typeparam>
        /// <param name="method">The action to be executed remotely.</param>
        /// <param name="args">The arguments to pass to the action.</param>
        void NetworkExecuteOnClient<T0, T1, T2, T3, T4>(Action<T0, T1, T2, T3, T4> method, params object[] args);

        /// <summary>
        /// Executes a five-parameter action remotely with the specified arguments on server or host
        /// </summary>
        /// <typeparam name="T0">The type of the first parameter for the action.</typeparam>
        /// <typeparam name="T1">The type of the second parameter for the action.</typeparam>
        /// <typeparam name="T2">The type of the third parameter for the action.</typeparam>
        /// <typeparam name="T3">The type of the fourth parameter for the action.</typeparam>
        /// <typeparam name="T4">The type of the fifth parameter for the action.</typeparam>
        /// <typeparam name="T5">The type of the sixth parameter for the action.</typeparam>
        /// <param name="method">The action to be executed remotely.</param>
        /// <param name="args">The arguments to pass to the action.</param>
        void NetworkExecuteOnClient<T0, T1, T2, T3, T4, T5>(Action<T0, T1, T2, T3, T4, T5> method, params object[] args);

        /// <summary>
        /// Executes a six-parameter action remotely with the specified arguments on server or host
        /// </summary>
        /// <typeparam name="T0">The type of the first parameter for the action.</typeparam>
        /// <typeparam name="T1">The type of the second parameter for the action.</typeparam>
        /// <typeparam name="T2">The type of the third parameter for the action.</typeparam>
        /// <typeparam name="T3">The type of the fourth parameter for the action.</typeparam>
        /// <typeparam name="T4">The type of the fifth parameter for the action.</typeparam>
        /// <typeparam name="T5">The type of the sixth parameter for the action.</typeparam>
        /// <typeparam name="T6">The type of the seventh parameter for the action.</typeparam>
        /// <param name="method">The action to be executed remotely.</param>
        /// <param name="args">The arguments to pass to the action.</param>
        void NetworkExecuteOnClient<T0, T1, T2, T3, T4, T5, T6>(Action<T0, T1, T2, T3, T4, T5, T6> method, params object[] args);

        /// <summary>
        /// Executes a seven-parameter action remotely with the specified arguments on server or host
        /// </summary>
        /// <typeparam name="T0">The type of the first parameter for the action.</typeparam>
        /// <typeparam name="T1">The type of the second parameter for the action.</typeparam>
        /// <typeparam name="T2">The type of the third parameter for the action.</typeparam>
        /// <typeparam name="T3">The type of the fourth parameter for the action.</typeparam>
        /// <typeparam name="T4">The type of the fifth parameter for the action.</typeparam>
        /// <typeparam name="T5">The type of the sixth parameter for the action.</typeparam>
        /// <typeparam name="T6">The type of the seventh parameter for the action.</typeparam>
        /// <typeparam name="T7">The type of the eighth parameter for the action.</typeparam>
        /// <param name="method">The action to be executed remotely.</param>
        /// <param name="args">The arguments to pass to the action.</param>
        void NetworkExecuteOnClient<T0, T1, T2, T3, T4, T5, T6, T7>(Action<T0, T1, T2, T3, T4, T5, T6, T7> method, params object[] args);

        /// <summary>
        /// Executes an eight-parameter action remotely with the specified arguments on server or host
        /// </summary>
        /// <typeparam name="T0">The type of the first parameter for the action.</typeparam>
        /// <typeparam name="T1">The type of the second parameter for the action.</typeparam>
        /// <typeparam name="T2">The type of the third parameter for the action.</typeparam>
        /// <typeparam name="T3">The type of the fourth parameter for the action.</typeparam>
        /// <typeparam name="T4">The type of the fifth parameter for the action.</typeparam>
        /// <typeparam name="T5">The type of the sixth parameter for the action.</typeparam>
        /// <typeparam name="T6">The type of the seventh parameter for the action.</typeparam>
        /// <typeparam name="T7">The type of the eighth parameter for the action.</typeparam>
        /// <typeparam name="T8">The type of the ninth parameter for the action.</typeparam>
        /// <param name="method">The action to be executed remotely.</param>
        /// <param name="args">The arguments to pass to the action.</param>
        void NetworkExecuteOnClient<T0, T1, T2, T3, T4, T5, T6, T7, T8>(Action<T0, T1, T2, T3, T4, T5, T6, T7, T8> method, params object[] args);

        /// <summary>
        /// Executes a nine-parameter action remotely with the specified arguments on server or host
        /// </summary>
        /// <typeparam name="T0">The type of the first parameter for the action.</typeparam>
        /// <typeparam name="T1">The type of the second parameter for the action.</typeparam>
        /// <typeparam name="T2">The type of the third parameter for the action.</typeparam>
        /// <typeparam name="T3">The type of the fourth parameter for the action.</typeparam>
        /// <typeparam name="T4">The type of the fifth parameter for the action.</typeparam>
        /// <typeparam name="T5">The type of the sixth parameter for the action.</typeparam>
        /// <typeparam name="T6">The type of the seventh parameter for the action.</typeparam>
        /// <typeparam name="T7">The type of the eighth parameter for the action.</typeparam>
        /// <typeparam name="T8">The type of the ninth parameter for the action.</typeparam>
        /// <typeparam name="T9">The type of the tenth parameter for the action.</typeparam>
        /// <param name="method">The action to be executed remotely.</param>
        /// <param name="args">The arguments to pass to the action.</param>
        void NetworkExecuteOnClient<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9>(Action<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9> method, params object[] args);

        /// <summary>
        /// Executes a parameterless action remotely on server only
        /// </summary>
        /// <param name="method">The action to be executed remotely.</param>
        void NetworkExecuteOnServer(Action method);

        /// <summary>
        /// Executes a single-parameter action remotely with the specified arguments on server only
        /// </summary>
        /// <typeparam name="T">The type of the parameter for the action.</typeparam>
        /// <param name="method">The action to be executed remotely.</param>
        /// <param name="args">The arguments to pass to the action.</param>
        void NetworkExecuteOnServer<T>(Action<T> method, params object[] args);

        /// <summary>
        /// Executes a two-parameter action remotely with the specified arguments on server only
        /// </summary>
        /// <typeparam name="T0">The type of the first parameter for the action.</typeparam>
        /// <typeparam name="T1">The type of the second parameter for the action.</typeparam>
        /// <param name="method">The action to be executed remotely.</param>
        /// <param name="args">The arguments to pass to the action.</param>
        void NetworkExecuteOnServer<T0, T1>(Action<T0, T1> method, params object[] args);

        /// <summary>
        /// Executes a two-parameter action remotely with the specified arguments.
        /// </summary>
        /// <typeparam name="T0">The type of the first parameter for the action.</typeparam>
        /// <typeparam name="T1">The type of the second parameter for the action.</typeparam>
        /// <typeparam name="T2">The type of the third parameter for the action.</typeparam>
        /// <param name="method">The action to be executed remotely.</param>
        /// <param name="args">The arguments to pass to the action.</param>
        void NetworkExecuteOnServer<T0, T1, T2>(Action<T0, T1, T2> method, params object[] args);

        /// <summary>
        /// Executes a three-parameter action remotely with the specified arguments on server only
        /// </summary>
        /// <typeparam name="T0">The type of the first parameter for the action.</typeparam>
        /// <typeparam name="T1">The type of the second parameter for the action.</typeparam>
        /// <typeparam name="T2">The type of the third parameter for the action.</typeparam>
        /// <typeparam name="T3">The type of the fourth parameter for the action.</typeparam>
        /// <param name="method">The action to be executed remotely.</param>
        /// <param name="args">The arguments to pass to the action.</param>
        void NetworkExecuteOnServer<T0, T1, T2, T3>(Action<T0, T1, T2, T3> method, params object[] args);

        /// <summary>
        /// Executes a four-parameter action remotely with the specified arguments on server only
        /// </summary>
        /// <typeparam name="T0">The type of the first parameter for the action.</typeparam>
        /// <typeparam name="T1">The type of the second parameter for the action.</typeparam>
        /// <typeparam name="T2">The type of the third parameter for the action.</typeparam>
        /// <typeparam name="T3">The type of the fourth parameter for the action.</typeparam>
        /// <typeparam name="T4">The type of the fifth parameter for the action.</typeparam>
        /// <param name="method">The action to be executed remotely.</param>
        /// <param name="args">The arguments to pass to the action.</param>
        void NetworkExecuteOnServer<T0, T1, T2, T3, T4>(Action<T0, T1, T2, T3, T4> method, params object[] args);

        /// <summary>
        /// Executes a five-parameter action remotely with the specified arguments on server only
        /// </summary>
        /// <typeparam name="T0">The type of the first parameter for the action.</typeparam>
        /// <typeparam name="T1">The type of the second parameter for the action.</typeparam>
        /// <typeparam name="T2">The type of the third parameter for the action.</typeparam>
        /// <typeparam name="T3">The type of the fourth parameter for the action.</typeparam>
        /// <typeparam name="T4">The type of the fifth parameter for the action.</typeparam>
        /// <typeparam name="T5">The type of the sixth parameter for the action.</typeparam>
        /// <param name="method">The action to be executed remotely.</param>
        /// <param name="args">The arguments to pass to the action.</param>
        void NetworkExecuteOnServer<T0, T1, T2, T3, T4, T5>(Action<T0, T1, T2, T3, T4, T5> method, params object[] args);

        /// <summary>
        /// Executes a six-parameter action remotely with the specified arguments on server only
        /// </summary>
        /// <typeparam name="T0">The type of the first parameter for the action.</typeparam>
        /// <typeparam name="T1">The type of the second parameter for the action.</typeparam>
        /// <typeparam name="T2">The type of the third parameter for the action.</typeparam>
        /// <typeparam name="T3">The type of the fourth parameter for the action.</typeparam>
        /// <typeparam name="T4">The type of the fifth parameter for the action.</typeparam>
        /// <typeparam name="T5">The type of the sixth parameter for the action.</typeparam>
        /// <typeparam name="T6">The type of the seventh parameter for the action.</typeparam>
        /// <param name="method">The action to be executed remotely.</param>
        /// <param name="args">The arguments to pass to the action.</param>
        void NetworkExecuteOnServer<T0, T1, T2, T3, T4, T5, T6>(Action<T0, T1, T2, T3, T4, T5, T6> method, params object[] args);

        /// <summary>
        /// Executes a seven-parameter action remotely with the specified arguments on server only
        /// </summary>
        /// <typeparam name="T0">The type of the first parameter for the action.</typeparam>
        /// <typeparam name="T1">The type of the second parameter for the action.</typeparam>
        /// <typeparam name="T2">The type of the third parameter for the action.</typeparam>
        /// <typeparam name="T3">The type of the fourth parameter for the action.</typeparam>
        /// <typeparam name="T4">The type of the fifth parameter for the action.</typeparam>
        /// <typeparam name="T5">The type of the sixth parameter for the action.</typeparam>
        /// <typeparam name="T6">The type of the seventh parameter for the action.</typeparam>
        /// <typeparam name="T7">The type of the eighth parameter for the action.</typeparam>
        /// <param name="method">The action to be executed remotely.</param>
        /// <param name="args">The arguments to pass to the action.</param>
        void NetworkExecuteOnServer<T0, T1, T2, T3, T4, T5, T6, T7>(Action<T0, T1, T2, T3, T4, T5, T6, T7> method, params object[] args);

        /// <summary>
        /// Executes an eight-parameter action remotely with the specified arguments on server only
        /// </summary>
        /// <typeparam name="T0">The type of the first parameter for the action.</typeparam>
        /// <typeparam name="T1">The type of the second parameter for the action.</typeparam>
        /// <typeparam name="T2">The type of the third parameter for the action.</typeparam>
        /// <typeparam name="T3">The type of the fourth parameter for the action.</typeparam>
        /// <typeparam name="T4">The type of the fifth parameter for the action.</typeparam>
        /// <typeparam name="T5">The type of the sixth parameter for the action.</typeparam>
        /// <typeparam name="T6">The type of the seventh parameter for the action.</typeparam>
        /// <typeparam name="T7">The type of the eighth parameter for the action.</typeparam>
        /// <typeparam name="T8">The type of the ninth parameter for the action.</typeparam>
        /// <param name="method">The action to be executed remotely.</param>
        /// <param name="args">The arguments to pass to the action.</param>
        void NetworkExecuteOnServer<T0, T1, T2, T3, T4, T5, T6, T7, T8>(Action<T0, T1, T2, T3, T4, T5, T6, T7, T8> method, params object[] args);

        /// <summary>
        /// Executes a nine-parameter action remotely with the specified arguments on server only
        /// </summary>
        /// <typeparam name="T0">The type of the first parameter for the action.</typeparam>
        /// <typeparam name="T1">The type of the second parameter for the action.</typeparam>
        /// <typeparam name="T2">The type of the third parameter for the action.</typeparam>
        /// <typeparam name="T3">The type of the fourth parameter for the action.</typeparam>
        /// <typeparam name="T4">The type of the fifth parameter for the action.</typeparam>
        /// <typeparam name="T5">The type of the sixth parameter for the action.</typeparam>
        /// <typeparam name="T6">The type of the seventh parameter for the action.</typeparam>
        /// <typeparam name="T7">The type of the eighth parameter for the action.</typeparam>
        /// <typeparam name="T8">The type of the ninth parameter for the action.</typeparam>
        /// <typeparam name="T9">The type of the tenth parameter for the action.</typeparam>
        /// <param name="method">The action to be executed remotely.</param>
        /// <param name="args">The arguments to pass to the action.</param>
        void NetworkExecuteOnServer<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9>(Action<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9> method, params object[] args);
    }

}
