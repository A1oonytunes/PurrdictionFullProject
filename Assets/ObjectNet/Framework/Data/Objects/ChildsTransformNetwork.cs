
using System.Collections.Generic;
using UnityEngine;

namespace com.onlineobject.objectnet {
    /// <summary>
    /// Represents a network entity that synchronizes the scale of a game object across the network.
    /// </summary>
    public class ChildsTransformNetwork : NetworkEntity<Vector3, IDataStream> {

        // List of all childs to synchronize
        private List<ChildTransformEntry> childs = new List<ChildTransformEntry>();

        // Flag to determine if the entity has been initialized
        private bool initialized = false;

        // Threshold to determine if the position has changed significantly.
        const float DISTANCE_THRESHOULD = 0.01f;

        // Threshold to determine if the rotation has changed significantly.
        const float ROTATION_THRESHOULD = 0.01f;

        // Threshold for scale changes to be considered significant
        const float SCALE_THRESHOULD = 0.01f;

        /// <summary>
        /// Default constructor of ChildsTransformNetwork
        /// </summary>
        public ChildsTransformNetwork() : base() {
        }


        /// <summary>
        /// Constructor that initializes the ScaleNetwork with a network object.
        /// </summary>
        /// <param name="networkObject">The network object to be associated with this entity.</param>
        public ChildsTransformNetwork(INetworkElement networkObject) : base(networkObject) {
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="child"></param>
        public void RegisterChild(GameObject child, bool position, bool rotation, bool scale) {
            this.childs.Add(new ChildTransformEntry((ushort)this.childs.Count, child, position, rotation, scale));
        }

        /// <summary>
        /// Computes the active state of the network entity and flags it for update if the scale has changed significantly.
        /// </summary>
        public override void ComputeActive() {
            foreach(ChildTransformEntry childEntry in this.childs) {
                if (childEntry.IsToSyncPosition())   this.FlagUpdated(Vector3.Distance(childEntry.GetPosition(), childEntry.GetChildObject().transform.localScale) > DISTANCE_THRESHOULD);
                if (childEntry.IsToSyncRotation())   this.FlagUpdated(Quaternion.Angle(childEntry.GetRotation(), childEntry.GetChildObject().transform.localRotation) > ROTATION_THRESHOULD);
                if (childEntry.IsToSyncScale())      this.FlagUpdated(Vector3.Distance(childEntry.GetScale(), childEntry.GetChildObject().transform.localScale) > SCALE_THRESHOULD);

                if (childEntry.IsToSyncPosition())   childEntry.SetPosition(childEntry.GetChildObject().transform.localPosition);
                if (childEntry.IsToSyncRotation())   childEntry.SetRotation(childEntry.GetChildObject().transform.localRotation);
                if (childEntry.IsToSyncScale())      childEntry.SetScale(childEntry.GetChildObject().transform.localScale);
            }
            
        }

        /// <summary>
        /// Computes the passive state of the network entity, initializing or updating the scale of the associated game object.
        /// </summary>
        public override void ComputePassive() {
            if (!this.initialized) {
                this.initialized = true;
                foreach (ChildTransformEntry childEntry in this.childs) {
                    if (childEntry.IsToSyncPosition())   childEntry.SetPosition(childEntry.GetChildObject().transform.localPosition);
                    if (childEntry.IsToSyncRotation())   childEntry.SetRotation(childEntry.GetChildObject().transform.localRotation);
                    if (childEntry.IsToSyncScale())      childEntry.SetScale(childEntry.GetChildObject().transform.localScale);
                }
            }

            foreach (ChildTransformEntry childEntry in this.childs) {
                if (childEntry.IsToSyncPosition())   childEntry.GetChildObject().transform.localPosition     = childEntry.GetPosition();
                if (childEntry.IsToSyncRotation())   childEntry.GetChildObject().transform.localRotation     = childEntry.GetRotation();
                if (childEntry.IsToSyncScale())      childEntry.GetChildObject().transform.localScale        = childEntry.GetScale();
            }
        }

        /// <summary>
        /// Gets the current scale arguments for passive synchronization.
        /// </summary>
        /// <returns>The current scale of the network entity.</returns>
        public override Vector3 GetPassiveArguments() {
            return Vector3.zero;
        }

        /// <summary>
        /// Synchronizes the active state by writing the scale to the data stream.
        /// </summary>
        /// <param name="writer">The data stream to write the scale to.</param>
        public override void SynchonizeActive(IDataStream writer) {
            // Write if update is paused
            writer.Write(this.IsPaused());
            if (this.IsPaused() == false) {
                // Write how many childs they need to read
                writer.Write(this.childs.Count);
                foreach (ChildTransformEntry childEntry in this.childs) {
                    writer.Write(childEntry.GetChildIndex());
                    if (childEntry.IsToSyncPosition())   writer.Write(childEntry.GetPosition());
                    if (childEntry.IsToSyncRotation())   writer.Write(childEntry.GetRotation().eulerAngles);
                    if (childEntry.IsToSyncScale())      writer.Write(childEntry.GetScale());
                }
            }
        }

        /// <summary>
        /// Synchronizes the passive state by updating the scale from the data.
        /// </summary>
        /// <param name="data">The new scale data to apply.</param>
        public override void SynchonizePassive(Vector3 data) {
            // Do nothing
        }

        /// <summary>
        /// Extracts the scale from the data stream and applies it to the network entity.
        /// </summary>
        /// <param name="reader">The data stream to read the scale from.</param>
        public override void Extract(IDataStream reader) {
            this.initialized = true;
            // First extract if position is paused by other side
            bool isSenderPaused = reader.Read<bool>();
            if (isSenderPaused == false) {
                this.Resume();
                int childsCount = reader.Read<int>();
                while (childsCount > 0) {
                    ushort  childIndex          = reader.Read<ushort>();
                    if (childIndex < this.childs.Count) {
                        Vector3 extractedPosition = (this.childs[childIndex].IsToSyncPosition()) ? reader.Read<Vector3>() : Vector3.zero;
                        Vector3 extractedRotation = (this.childs[childIndex].IsToSyncRotation()) ? reader.Read<Vector3>() : Vector3.zero;
                        Vector3 extractedScale = (this.childs[childIndex].IsToSyncScale()) ? reader.Read<Vector3>() : Vector3.zero;

                        if (this.childs[childIndex].IsToSyncPosition())
                            this.childs[childIndex].SetPosition(extractedPosition);
                        if (this.childs[childIndex].IsToSyncRotation())
                            this.childs[childIndex].SetRotation(Quaternion.Euler(extractedRotation));
                        if (this.childs[childIndex].IsToSyncScale())
                            this.childs[childIndex].SetScale(extractedScale);
                    } else if (this.childs.Count > 0) {
                        NetworkDebugger.LogTrace("[WARNING] Object [{0}] is trying to synchronize more childs than send by origin", this.childs[0].GetChildObject().transform.parent.name);
                    } else {
                        NetworkDebugger.LogTrace("[WARNING] Object [{0}] is trying to synchronize more childs than send by origin", this.GetNetworkObject().GetGameObject().name);
                    }
                    childsCount--;
                }
            } else {
                this.Pause();
            }
        }
    }
}