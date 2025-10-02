using System;
using System.Collections.Generic;
using UnityEngine;

namespace com.onlineobject.objectnet {

    /// <summary>
    /// Represents a network input for a network element.
    /// </summary>
    public sealed class NetworkInput : INetworkInput {

        /// <summary>
        /// The network element associated with this input.
        /// </summary>
        private INetworkElement networkElement;

        /// <summary>
        /// A list of input interfaces that this network input manages.
        /// </summary>
        private List<IInput> Inputs = new List<IInput>();

        /// <summary>
        /// Indicates whether the input is local to the machine or remote.
        /// </summary>
        /// <remarks>
        /// A value of true indicates that the input is local, while false indicates it is remote.
        /// </remarks>
        private bool Local = false;

        /// <summary>
        /// Indicates whether the network input is currently active.
        /// </summary>
        /// <remarks>
        /// A value of true means the input is active and should be considered in network operations.
        /// A value of false means the input is inactive and should be ignored.
        /// </remarks>
        private bool Active = true;

        /// <summary>
        /// A byte value used as a volatile code factory.
        /// </summary>
        /// <remarks>
        /// This field is marked as volatile to ensure that it is accessed in a thread-safe manner,
        /// preventing potential issues with multi-threaded access.
        /// The specific use of this field is not described, but it is likely used for generating
        /// some form of codes or identifiers that require thread-safe operations.
        /// </remarks>
        private volatile byte codeFactory = 0;

        /// <summary>
        /// Initializes a new instance of the NetworkInput class with the specified network element, local and active status.
        /// </summary>
        /// <param name="networkElement">The network element associated with the input.</param>
        /// <param name="local">Specifies whether the input is local.</param>
        /// <param name="active">Specifies whether the input is active.</param>
        public NetworkInput(INetworkElement networkElement, bool local = false, bool active = false) {
            this.networkElement = networkElement;
            this.Local          = local;
            this.Active         = active;
        }

        /// <summary>
        /// Gets the input value of the specified type and name.
        /// </summary>
        /// <typeparam name="T">The type of the input value.</typeparam>
        /// <param name="inputName">The name of the input.</param>
        /// <returns>The input value of the specified type and name.</returns>
        public T GetInput<T>(string inputName) {
            T result = default(T);
            foreach (IInput input in this.Inputs) {
                if (input.GetName() == inputName) {
                    result = (input as IInputEntry<T>).GetValue();
                    break;
                }
            }
            return result;
        }

        /// <summary>
        /// Gets the input value of the specified type and code.
        /// </summary>
        /// <typeparam name="T">The type of the input value.</typeparam>
        /// <param name="inputCode">The code of the input.</param>
        /// <returns>The input value of the specified type and code.</returns>
        public T GetInput<T>(byte inputCode) {
            T result = default(T);
            foreach (IInput input in this.Inputs) {
                if (input.GetCode() == inputCode) {
                    result = (input as IInputEntry<T>).GetValue();
                    break;
                }
            }
            return result;
        }

        /// <summary>
        /// Gets all the inputs associated with the network element.
        /// </summary>
        /// <returns>An array of inputs associated with the network element.</returns>
        public IInput[] GetInputs() {
            return this.Inputs.ToArray();
        }

        /// <summary>
        /// Registers an input of the specified type and name.
        /// </summary>
        /// <typeparam name="T">The type of the input value.</typeparam>
        /// <param name="inputName">The name of the input.</param>
        /// <returns>The registered input of the specified type and name.</returns>
        public IInputEntry<T> RegisterInput<T>(string inputName) {
            bool            exists  = false;
            IInputEntry<T>  entry   = null;
            foreach (IInput input in this.Inputs) {
                if (input.GetName() == inputName) {
                    exists  = true;
                    entry   = (input as IInputEntry<T>);
                    break;
                }
            }
            if ( !exists ) {
                entry = (this.GenerateInput(typeof(T), inputName) as IInputEntry<T>);
                this.Inputs.Add(entry);
            }
            return entry;
        }

        /// <summary>
        /// Sets the input value of the specified type and name.
        /// </summary>
        /// <typeparam name="T">The type of the input value.</typeparam>
        /// <param name="inputName">The name of the input.</param>
        /// <param name="state">The new state of the input.</param>
        public void SetInput<T>(string inputName, T state) {
            IInputEntry<T> result = default(IInputEntry<T>);
            foreach (IInput input in this.Inputs) {
                if (input.GetName() == inputName) {
                    result = (IInputEntry<T>)input;
                    break;
                }
            }
            result.SetValue(state);
        }

        /// <summary>
        /// Sets the input value of the specified type and code.
        /// </summary>
        /// <typeparam name="T">The type of the input value.</typeparam>
        /// <param name="inputCode">The code of the input.</param>
        /// <param name="state">The new state of the input.</param>
        public void SetInput<T>(byte inputCode, T state) {
            IInputEntry<T> result = default(IInputEntry<T>);
            foreach (IInput input in this.Inputs) {
                if (input.GetCode() == inputCode) {
                    result = (IInputEntry<T>)input;
                    break;
                }
            }
            result.SetValue(state);
        }

        /// <summary>
        /// Checks if the input is local.
        /// </summary>
        /// <returns>True if the input is local; otherwise, false.</returns>
        public bool IsLocal() {
            return this.Local;
        }

        /// <summary>
        /// Checks if the input is active.
        /// </summary>
        /// <returns>True if the input is active; otherwise, false.</returns>
        public bool IsActive() {
            return this.Active;
        }

        /// <summary>
        /// Generate a new input based on type
        /// </summary>
        /// <param name="type">Input type to be generated</param>
        /// <param name="inputName">Input name</param>
        /// <returns></returns>
        private IInput GenerateInput(Type type, string inputName) {
            IInput result = null;
            if (type == typeof(bool)) {
                result = new BooleanInput(inputName, ++this.codeFactory, this.Local);
            } else if (type == typeof(float)) {
                result = new FloatInput(inputName, ++this.codeFactory, this.Local);                
            } else if (type == typeof(Vector2)) {
                result = new Vector2Input(inputName, ++this.codeFactory, this.Local);                
            }
            return result;
        }
    }
}