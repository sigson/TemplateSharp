using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using Godot.Collections;

/// <summary>
/// 2d realization https://stackoverflow.com/questions/2792443/finding-the-centroid-of-a-polygon
/// 3d realization https://stackoverflow.com/questions/22465832/center-point-calculation-for-3d-polygon
/// </summary>
public partial class CenterOfMassAuto : Node
{
    [Export]
	public RigidBody3D rigidBody3D;
    [Export]
    public Node3D marker;
	public List<CollisionObjectProvider> collisionObjects = new List<CollisionObjectProvider>();

    /// <summary>
    /// For perfomance purposes.
    /// </summary>
    [Export]
    public int SkipFrames = 0;
    private int _skippedFrames = 0;
	public override void _Process(double delta)
	{
        #region frameskip
        if (_skippedFrames < SkipFrames)
        {
            _skippedFrames++;
            return;
        }
        _skippedFrames = 0;
        #endregion

        CacheChilds();
        marker.Position = marker.ToLocal(CalcCenterOfAllChilds());
        if(rigidBody3D.CenterOfMassMode != RigidBody3D.CenterOfMassModeEnum.Auto)
        {
            rigidBody3D.CenterOfMass = rigidBody3D.ToLocal(CalcCenterOfAllChilds());
        }
        

    }

    private Vector3 CalcCenterOfAllChilds()
    {
        float sx = 0, sy = 0, sz = 0;
        foreach (var pos in collisionObjects.Select(x => x.GlobalCenterOfShape))
        {
            sx = sx + pos.X;
            sy = sy + pos.Y;
            sz = sz + pos.Z;
        }
        return new Vector3(sx / collisionObjects.Count, sy / collisionObjects.Count, sz / collisionObjects.Count);
    }

    private void CacheChilds()
    {
        var newchilds = rigidBody3D.GetChildCount() != collisionObjects.Count;
        for(int i = 0; i < collisionObjects.Count; i++)
        {
            var child = collisionObjects[i];
            if (!child._Node.IsInsideTree())
            {
                newchilds = true;
                collisionObjects.Remove(child);
                i--;
            }
        }
        if (newchilds)
        {
            for (int i = 0; i < rigidBody3D.GetChildCount(); i++)
            {
                var child = rigidBody3D.GetChild(i);
                if(collisionObjects.Where(x => x._Node == child).Count() == 0 && child is CollisionShape3D)
                {
                    collisionObjects.Add(new CollisionObjectProvider(child as Node3D));
                }
            }
        }
    }
}

public class CollisionObjectProvider
{
	public Node3D _Node;
	public Basis Basis => _Node.Basis;
    public Aabb LocalAABB;
    public Vector3 GlobalCenterOfShape
    {
        get
        {
            return this._Node.GlobalPosition;
            //return LocalAABB.GetCenter();//this._Node.GlobalTransform *
        }
    }
	public Vector3 Size
	{
		get
		{
			if(_Node is CollisionShape3D)
			{
				var collisionShape = _Node as CollisionShape3D;
                if (collisionShape.Shape is BoxShape3D)
                {
                    return (collisionShape.Shape as BoxShape3D).Size * collisionShape.Scale;
                }
                if (collisionShape.Shape is SphereShape3D)
                {
					var radius = (collisionShape.Shape as SphereShape3D).Radius;
                    return new Vector3(radius, radius, radius) * collisionShape.Scale;
                }
                if (collisionShape.Shape is CylinderShape3D)
                {
                    var radius = (collisionShape.Shape as CylinderShape3D).Radius;
                    var height = (collisionShape.Shape as CylinderShape3D).Height;
                    return new Vector3(radius, height, radius) * collisionShape.Scale;
                }
                if (collisionShape.Shape is CapsuleShape3D)
                {
                    var radius = (collisionShape.Shape as CapsuleShape3D).Radius;
                    var height = (collisionShape.Shape as CapsuleShape3D).Height;
                    return new Vector3(radius, height+radius*2, radius) * collisionShape.Scale;
                }
            }
			return new Vector3();
        }
	}
	public CollisionObjectProvider(Node3D collisionObject)
	{
        _Node = collisionObject;
        LocalAABB = new Aabb(this._Node.GlobalPosition, this.Size);
    }
}