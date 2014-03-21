using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace CS5643P2 {
    public class Cloth {
        private DynamicVertexBuffer vb;
        public readonly int stride, rows;
        VertexPositionNormalTexture[] verts;
        private IndexBuffer ib;
        public Vector3 this[int vx, int vy] {
            get {
                return verts[vy * stride + vx].Position;
            }
            set {
                verts[vy * stride + vx].Position = value;
                verts[vy * stride + vx + stride * rows].Position = value;
            }
        }

        public Cloth(GraphicsDevice g, int w, int h, Vector2 s) {
            // Scale To Patches
            int u = w, v = h;
            w++; h++;
            stride = w;
            rows = h;

            // Create Vertices
            verts = new VertexPositionNormalTexture[(w * h) << 1];
            int i = 0;
            for(int z = 0; z < h; z++) {
                for(int x = 0; x < w; x++) {
                    verts[i].Position.X = x * s.X;
                    verts[i].Position.Y = 0;
                    verts[i].Position.Z = z * s.Y;
                    verts[i].TextureCoordinate.X = (float)x / (float)u * 0.5f;
                    verts[i].TextureCoordinate.Y = (float)z / (float)v;
                    verts[i].Normal = Vector3.UnitY;
                    i++;
                }
            }
            for(int z = 0; z < h; z++) {
                for(int x = 0; x < w; x++) {
                    verts[i].Position.X = x * s.X;
                    verts[i].Position.Y = 0;
                    verts[i].Position.Z = z * s.Y;
                    verts[i].TextureCoordinate.X = (float)x / (float)u * 0.5f;
                    verts[i].TextureCoordinate.X += 0.5f;
                    verts[i].TextureCoordinate.Y = (float)z / (float)v;
                    verts[i].Normal = -Vector3.UnitY;
                    i++;
                }
            }

            vb = new DynamicVertexBuffer(g, VertexPositionNormalTexture.VertexDeclaration, verts.Length, BufferUsage.WriteOnly);
            vb.SetData(verts);

            int[] inds = new int[((u * v) << 2) * 3];
            int ii1 = 0, ii2 = ((u * v) << 1) * 3;
            int vi1 = 0, vi2 = verts.Length >> 1;
            for(int z = 0; z < v; z++) {
                for(int x = 0; x < u; x++) {
                    inds[ii1++] = vi1 + 0;
                    inds[ii1++] = vi1 + 1;
                    inds[ii1++] = vi1 + w + 0;
                    inds[ii1++] = vi1 + w + 0;
                    inds[ii1++] = vi1 + 1;
                    inds[ii1++] = vi1 + w + 1;
                    vi1++;
                    inds[ii2++] = vi2 + 0;
                    inds[ii2++] = vi2 + w + 0;
                    inds[ii2++] = vi2 + 1;
                    inds[ii2++] = vi2 + 1;
                    inds[ii2++] = vi2 + w + 0;
                    inds[ii2++] = vi2 + w + 1;
                    vi2++;
                }
                vi1++;
                vi2++;
            }

            ib = new IndexBuffer(g, IndexElementSize.ThirtyTwoBits, inds.Length, BufferUsage.WriteOnly);
            ib.SetData(inds);
        }

        public void UpdatePositions() {
            vb.SetData(verts);
        }

        public void SetBuffers(GraphicsDevice g) {
            g.SetVertexBuffer(vb);
            g.Indices = ib;
        }
        public void Draw(GraphicsDevice g) {
            g.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, verts.Length, 0, ib.IndexCount / 3);
        }
    }
}