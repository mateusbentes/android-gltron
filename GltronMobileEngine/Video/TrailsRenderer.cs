using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using GltronMobileEngine.Interfaces;

namespace GltronMobileEngine.Video;

public class TrailsRenderer
{
    private readonly GraphicsDevice _gd;
    private DynamicVertexBuffer _vb;

    public TrailsRenderer(GraphicsDevice gd)
    {
        _gd = gd;
        _vb = new DynamicVertexBuffer(_gd, typeof(VertexPositionColor), 8192, BufferUsage.WriteOnly);
    }

    public void LoadContent(Microsoft.Xna.Framework.Content.ContentManager content)
    {
        // No content needed yet
    }

    public void DrawTrail(WorldGraphics world, IPlayer p)
    {
        if (p == null) return;
        var verts = new List<VertexPositionColor>();
        for (int i = 0; i <= p.getTrailOffset(); i++)
        {
            var s = p.getTrail(i);
            if (s == null) continue;
            var a = new Vector3(s.vStart.v[0], 0.5f, s.vStart.v[1]);
            var b = a + new Vector3(s.vDirection.v[0], 0, s.vDirection.v[1]);
            var col = p.getPlayerNum() == 0 ? Color.LimeGreen : Color.OrangeRed;
            verts.Add(new VertexPositionColor(a, col));
            verts.Add(new VertexPositionColor(b, col));
        }
        if (verts.Count == 0) return;
        if (_vb.VertexCount < verts.Count)
        {
            _vb.Dispose();
            _vb = new DynamicVertexBuffer(_gd, typeof(VertexPositionColor), verts.Count, BufferUsage.WriteOnly);
        }
        _vb.SetData(verts.ToArray());
        _gd.SetVertexBuffer(_vb);
        foreach (var pass in world.Effect.CurrentTechnique.Passes)
        {
            pass.Apply();
            _gd.DrawPrimitives(PrimitiveType.LineList, 0, verts.Count / 2);
        }
    }
}
