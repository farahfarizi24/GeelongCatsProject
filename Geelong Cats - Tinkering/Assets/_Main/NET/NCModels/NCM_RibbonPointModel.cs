using UnityEngine;
using Normal.Realtime.Serialization;
using Normal.Realtime;

namespace com.DU.CE.NET.NCM
{
    [RealtimeModel]
    public partial class NCM_RibbonPointModel
    {
        [RealtimeProperty(5, true)]
        private Vector3 _position;

        [RealtimeProperty(6, true)]
        private Quaternion _rotation = Quaternion.identity;
    }

    }

/* ----- Begin Normal Autogenerated Code ----- */
namespace com.DU.CE.NET.NCM {
    public partial class NCM_RibbonPointModel : RealtimeModel {
        public UnityEngine.Vector3 position {
            get {
                return _cache.LookForValueInCache(_position, entry => entry.positionSet, entry => entry.position);
            }
            set {
                if (this.position == value) return;
                _cache.UpdateLocalCache(entry => { entry.positionSet = true; entry.position = value; return entry; });
                InvalidateReliableLength();
            }
        }
        
        public UnityEngine.Quaternion rotation {
            get {
                return _cache.LookForValueInCache(_rotation, entry => entry.rotationSet, entry => entry.rotation);
            }
            set {
                if (this.rotation == value) return;
                _cache.UpdateLocalCache(entry => { entry.rotationSet = true; entry.rotation = value; return entry; });
                InvalidateReliableLength();
            }
        }
        
        private struct LocalCacheEntry {
            public bool positionSet;
            public UnityEngine.Vector3 position;
            public bool rotationSet;
            public UnityEngine.Quaternion rotation;
        }
        
        private LocalChangeCache<LocalCacheEntry> _cache = new LocalChangeCache<LocalCacheEntry>();
        
        public enum PropertyID : uint {
            Position = 5,
            Rotation = 6,
        }
        
        public NCM_RibbonPointModel() : this(null) {
        }
        
        public NCM_RibbonPointModel(RealtimeModel parent) : base(null, parent) {
        }
        
        protected override void OnParentReplaced(RealtimeModel previousParent, RealtimeModel currentParent) {
            UnsubscribeClearCacheCallback();
        }
        
        protected override int WriteLength(StreamContext context) {
            int length = 0;
            if (context.fullModel) {
                FlattenCache();
                length += WriteStream.WriteBytesLength((uint)PropertyID.Position, WriteStream.Vector3ToBytesLength());
                length += WriteStream.WriteBytesLength((uint)PropertyID.Rotation, WriteStream.QuaternionToBytesLength());
            } else if (context.reliableChannel) {
                LocalCacheEntry entry = _cache.localCache;
                if (entry.positionSet) {
                    length += WriteStream.WriteBytesLength((uint)PropertyID.Position, WriteStream.Vector3ToBytesLength());
                }
                if (entry.rotationSet) {
                    length += WriteStream.WriteBytesLength((uint)PropertyID.Rotation, WriteStream.QuaternionToBytesLength());
                }
            }
            return length;
        }
        
        protected override void Write(WriteStream stream, StreamContext context) {
            var didWriteProperties = false;
            
            if (context.fullModel) {
                stream.WriteBytes((uint)PropertyID.Position, WriteStream.Vector3ToBytes(_position));
                stream.WriteBytes((uint)PropertyID.Rotation, WriteStream.QuaternionToBytes(_rotation));
            } else if (context.reliableChannel) {
                LocalCacheEntry entry = _cache.localCache;
                if (entry.positionSet || entry.rotationSet) {
                    _cache.PushLocalCacheToInflight(context.updateID);
                    ClearCacheOnStreamCallback(context);
                }
                if (entry.positionSet) {
                    stream.WriteBytes((uint)PropertyID.Position, WriteStream.Vector3ToBytes(entry.position));
                    didWriteProperties = true;
                }
                if (entry.rotationSet) {
                    stream.WriteBytes((uint)PropertyID.Rotation, WriteStream.QuaternionToBytes(entry.rotation));
                    didWriteProperties = true;
                }
                
                if (didWriteProperties) InvalidateReliableLength();
            }
        }
        
        protected override void Read(ReadStream stream, StreamContext context) {
            while (stream.ReadNextPropertyID(out uint propertyID)) {
                switch (propertyID) {
                    case (uint)PropertyID.Position: {
                        _position = ReadStream.Vector3FromBytes(stream.ReadBytes());
                        break;
                    }
                    case (uint)PropertyID.Rotation: {
                        _rotation = ReadStream.QuaternionFromBytes(stream.ReadBytes());
                        break;
                    }
                    default: {
                        stream.SkipProperty();
                        break;
                    }
                }
            }
        }
        
        #region Cache Operations
        
        private StreamEventDispatcher _streamEventDispatcher;
        
        private void FlattenCache() {
            _position = position;
            _rotation = rotation;
            _cache.Clear();
        }
        
        private void ClearCache(uint updateID) {
            _cache.RemoveUpdateFromInflight(updateID);
        }
        
        private void ClearCacheOnStreamCallback(StreamContext context) {
            if (_streamEventDispatcher != context.dispatcher) {
                UnsubscribeClearCacheCallback(); // unsub from previous dispatcher
            }
            _streamEventDispatcher = context.dispatcher;
            _streamEventDispatcher.AddStreamCallback(context.updateID, ClearCache);
        }
        
        private void UnsubscribeClearCacheCallback() {
            if (_streamEventDispatcher != null) {
                _streamEventDispatcher.RemoveStreamCallback(ClearCache);
                _streamEventDispatcher = null;
            }
        }
        
        #endregion
    }
}
/* ----- End Normal Autogenerated Code ----- */
