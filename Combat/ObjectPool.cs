using Godot;
using System.Collections.Generic;

namespace EchoesAcrossTime.Combat
{
    /// <summary>
    /// Object pooling system for battle VFX and UI elements
    /// Add as Autoload: Project Settings -> Autoload -> ObjectPool
    /// </summary>
    public partial class ObjectPool : Node
    {
        public static ObjectPool Instance { get; private set; }
        
        private Dictionary<string, Queue<CanvasItem>> pools = new();
        private Dictionary<string, PackedScene> scenes = new();
        
        public override void _Ready()
        {
            Instance = this;
            GD.Print("[ObjectPool] Initialized");
        }
        
        /// <summary>
        /// Register a pool with a scene path
        /// </summary>
        public void RegisterPool(string poolId, string scenePath, int preWarmCount = 10)
        {
            var scene = GD.Load<PackedScene>(scenePath);
            if (scene == null)
            {
                GD.PrintErr($"[ObjectPool] Failed to load scene: {scenePath}");
                return;
            }
            
            scenes[poolId] = scene;
            pools[poolId] = new Queue<CanvasItem>();
            
            // Pre-warm pool
            for (int i = 0; i < preWarmCount; i++)
            {
                var obj = scene.Instantiate<CanvasItem>();
                if (obj == null)
                {
                    GD.PrintErr($"[ObjectPool] Scene {scenePath} must instantiate a CanvasItem (Control, Node2D, etc)");
                    return;
                }
                
                obj.Visible = false;
                AddChild(obj);
                pools[poolId].Enqueue(obj);
            }
            
            GD.Print($"[ObjectPool] Registered '{poolId}' with {preWarmCount} pre-warmed instances");
        }
        
        /// <summary>
        /// Spawn an object from the pool
        /// </summary>
        public T Spawn<T>(string poolId, Vector2 position = default) where T : CanvasItem
        {
            if (!pools.ContainsKey(poolId))
            {
                GD.PrintErr($"[ObjectPool] Pool '{poolId}' not registered!");
                return null;
            }
            
            CanvasItem obj;
            if (pools[poolId].Count > 0)
            {
                obj = pools[poolId].Dequeue();
            }
            else
            {
                // Pool empty, create new instance
                obj = scenes[poolId].Instantiate<CanvasItem>();
                AddChild(obj);
                GD.Print($"[ObjectPool] '{poolId}' pool empty, creating new instance (total: {GetChildCount()})");
            }
            
            // Set position if it's a Node2D
            if (obj is Node2D node2d && position != default)
            {
                node2d.GlobalPosition = position;
            }
            
            obj.Visible = true;
            return obj as T;
        }
        
        /// <summary>
        /// Return object to pool
        /// </summary>
        public void Recycle(string poolId, CanvasItem obj)
        {
            if (obj == null) return;
            
            if (!pools.ContainsKey(poolId))
            {
                GD.PrintErr($"[ObjectPool] Cannot recycle to unknown pool '{poolId}'");
                obj.QueueFree();
                return;
            }
            
            obj.Visible = false;
            
            // Reset position if Node2D
            if (obj is Node2D node2d)
            {
                node2d.Position = Vector2.Zero;
            }
            
            pools[poolId].Enqueue(obj);
        }
        
        /// <summary>
        /// Clear a specific pool
        /// </summary>
        public void ClearPool(string poolId)
        {
            if (!pools.ContainsKey(poolId)) return;
            
            while (pools[poolId].Count > 0)
            {
                var obj = pools[poolId].Dequeue();
                obj.QueueFree();
            }
            
            GD.Print($"[ObjectPool] Cleared pool '{poolId}'");
        }
        
        /// <summary>
        /// Clear all pools
        /// </summary>
        public void ClearAllPools()
        {
            foreach (var poolId in pools.Keys)
            {
                ClearPool(poolId);
            }
            
            GD.Print("[ObjectPool] All pools cleared");
        }
    }
}