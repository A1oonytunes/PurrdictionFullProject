using System;
using System.Collections.Generic;
using UnityEngine;

namespace com.onlineobject.objectnet {
    /// <summary>
    /// Represents a network entity that handles input data streams.
    /// </summary>
    public class InputNetwork : NetworkEntity<INetworkInput, IDataStream> {

        /// <summary>
        /// Stores information about a specific input including its code, type, and current value.
        /// </summary>
        private class InputInfo {
            // Unique identifier for the input.
            public byte Code;
            
            // The data type of the input.
            public Type InputType;

            // The current value of the input.
            public object Value;
        }

        // The interface for network input handling.
        private INetworkInput input; 

        // Indicates if the input network has been initialized.
        private bool initialized = false; 

        // Maps input codes to their corresponding information.
        private Dictionary<byte, InputInfo> inputsInfo = new Dictionary<byte, InputInfo>(); 

        // Constants representing the data types of the inputs.
        const byte FLOAT_TYPE = 0;
        const byte BOOLEAN_TYPE = 1;
        const byte VECTOR2_TYPE = 3;

        /// <summary>
        /// Initializes a new instance of the InputNetwork class.
        /// </summary>
        /// <param name="input">The network input handler.</param>
        /// <param name="active">Indicates whether the network should be active upon creation.</param>
        public InputNetwork(INetworkInput input, bool active = true) : base() {
            this.input = input;
            this.SetActive(active);
        }

        /// <summary>
        /// Computes and updates the active state of the network inputs.
        /// </summary>
        public override void ComputeActive() {
            this.InitializeInputs();
            if (this.input != null) {
                // Write each input
                foreach (IInput inputEntry in this.input.GetInputs()) {
                    object inputValue   = null;
                    if (inputEntry.GetInputType() == typeof(bool)) {
                        inputValue = (inputEntry as BooleanInput).Evaluate();                        
                    } else if (inputEntry.GetInputType() == typeof(float)) {
                        inputValue = (inputEntry as FloatInput).Evaluate();
                    } else if (inputEntry.GetInputType() == typeof(Vector2)) {
                        inputValue = (inputEntry as Vector2Input).Evaluate();
                    }
                    this.FlagUpdated(this.inputsInfo[inputEntry.GetCode()].Value != inputValue);
                    this.inputsInfo[inputEntry.GetCode()].Value = inputValue;
                    if (inputEntry.IsLocalInput()) {
                        if (inputEntry.GetInputType() == typeof(bool)) {
                            (inputEntry as BooleanInput).SetValue((bool)inputValue);
                        } else if (inputEntry.GetInputType() == typeof(float)) {
                            (inputEntry as FloatInput).SetValue((float)inputValue);
                        } else if (inputEntry.GetInputType() == typeof(Vector2)) {
                            (inputEntry as Vector2Input).SetValue((Vector2)inputValue);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Computes and updates the passive state of the network inputs.
        /// </summary>
        public override void ComputePassive() {
            this.InitializeInputs();
            if (this.input != null) {
                foreach (var inputInfo in this.inputsInfo) {
                    if (inputInfo.Value.Value != null) {
                        if (inputInfo.Value.InputType == typeof(bool)) {
                            this.input.SetInput<bool>(inputInfo.Key, (bool)((inputInfo.Value.Value != null) ? inputInfo.Value.Value : false));
                        } else if (inputInfo.Value.InputType == typeof(float)) {
                            this.input.SetInput<float>(inputInfo.Key, (float)((inputInfo.Value.Value != null) ? inputInfo.Value.Value : 0.0f));
                        } else if (inputInfo.Value.InputType == typeof(Vector2)) {
                            this.input.SetInput<Vector2>(inputInfo.Key, (Vector2)((inputInfo.Value.Value != null) ? inputInfo.Value.Value : Vector2.zero));
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Retrieves the passive arguments for the network input.
        /// </summary>
        /// <returns>The network input handler.</returns>
        public override INetworkInput GetPassiveArguments() {
            return this.input;
        }

        /// <summary>
        /// Synchronizes the passive state with the provided network input data.
        /// </summary>
        /// <param name="data">The network input data to synchronize with.</param>
        public override void SynchonizePassive(INetworkInput data) {
            this.input = data;
        }

        /// <summary>
        /// Synchronizes the active state by writing the input information to the data stream.
        /// </summary>
        /// <param name="writer">The data stream writer to write input information to.</param>
        public override void SynchonizeActive(IDataStream writer) {
            // Write if update is paused
            writer.Write(this.IsPaused());
            if (this.IsPaused() == false) {
                // Build packet
                writer.Write(this.inputsInfo.Count); // How many element exists on input
                // Write each input
                foreach (InputInfo inputInfo in this.inputsInfo.Values) {
                    if (inputInfo.InputType == typeof(bool)) {
                        writer.Write<byte>(BOOLEAN_TYPE);
                        writer.Write<byte>(inputInfo.Code);
                        writer.Write<bool>((bool)((inputInfo.Value != null) ? inputInfo.Value : false));
                    } else if (inputInfo.InputType == typeof(float)) {
                        writer.Write<byte>(FLOAT_TYPE);
                        writer.Write<byte>(inputInfo.Code);
                        writer.Write<float>((float)((inputInfo.Value != null) ? inputInfo.Value : 0.0f));
                    } else if (inputInfo.InputType == typeof(Vector2)) {
                        writer.Write<byte>(VECTOR2_TYPE);
                        writer.Write<byte>(inputInfo.Code);
                        writer.Write<Vector2>((Vector2)((inputInfo.Value != null) ? inputInfo.Value : Vector2.zero));
                    }
                }
            }
        }

        /// <summary>
        /// Extracts and updates the input information from the provided data stream.
        /// </summary>
        /// <param name="reader">The data stream to read input information from.</param>
        public override void Extract(IDataStream reader) {
            this.InitializeInputs();
            // First extract if position is paused by other side
            bool isSenderPaused = reader.Read<bool>();
            if (isSenderPaused == false) {
                this.Resume();
                int inputCount = reader.Read<int>();
                while (inputCount > 0) {
                    inputCount--;
                    byte inputType = reader.Read<byte>();
                    byte inputCode = reader.Read<byte>();
                    if (inputType == BOOLEAN_TYPE) {
                        this.inputsInfo[inputCode].Value = reader.Read<bool>();
                    } else if (inputType == FLOAT_TYPE) {
                        this.inputsInfo[inputCode].Value = reader.Read<float>();
                    } else if (inputType == VECTOR2_TYPE) {
                        this.inputsInfo[inputCode].Value = reader.Read<Vector2>();
                    }
                }
            } else {
                this.inputsInfo.Clear();
                this.Pause();
            }
        }

        /// <summary>
        /// Initializes the inputs if they have not been initialized yet.
        /// </summary>
        private void InitializeInputs() {
            if (!this.initialized) {
                foreach (IInput inputEntry in this.input.GetInputs()) {
                    this.inputsInfo.Add(inputEntry.GetCode(), new InputInfo());
                    this.inputsInfo[inputEntry.GetCode()].Code      = inputEntry.GetCode();
                    this.inputsInfo[inputEntry.GetCode()].InputType = inputEntry.GetInputType();
                }
                this.initialized = (this.inputsInfo.Count > 0);
            }
        }

        /// <summary>
        /// Gets the network input handler.
        /// </summary>
        /// <returns>The network input handler.</returns>
        public INetworkInput GetInput() {
            return this.input;
        }
    
    }
}