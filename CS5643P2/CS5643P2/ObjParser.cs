using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.IO;
using System.Reflection;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;

namespace Microsoft.Xna.Framework.Graphics {
    // Parsing Options
    public enum ParsingFlags : byte {
        None = 0x00,
        FlipTexCoordV = 0x01,
        FlipTriangleOrder = 0x02,
        ConversionOpenGL = FlipTexCoordV | FlipTriangleOrder,
    }
    public enum DataFlags : byte {
        None = 0x00,
        Position = 0x01,
        UV = 0x02,
        Normal = 0x04
    }

    #region Intermediate Reading Structs
    struct OBJFace {
        public string V1;
        public string V2;
        public string V3;
    }
    struct OBJVert {
        public int Position;
        public int UV;
        public int Normal;

        public OBJVert(int pos, int uv, int norm) {
            Position = pos;
            UV = uv;
            Normal = norm;
        }
    }
    struct OBJDataDictionary {
        public List<Vector3> Positions;
        public List<Vector2> UVs;
        public List<Vector3> Normals;
        public List<OBJFace> Faces;
    }
    class VertDict : IEnumerable<VertDict.Key> {
        public struct Key {
            public int Index;
            public OBJVert Vertex;

            public Key(int i, OBJVert v) {
                Index = i;
                Vertex = v;
            }
        }

        List<Key>[] verts;
        public int Count {
            get;
            private set;
        }

        public VertDict() {
            Count = 0;
            verts = new List<Key>[256];
            for(int i = 0; i < verts.Length; i++) {
                verts[i] = new List<Key>(8);
            }
        }
        ~VertDict() {
            foreach(var l in verts) {
                l.Clear();
            }
            verts = null;
            Count = 0;
        }

        public int Get(OBJVert v) {
            int h = v.GetHashCode() & 0xff;
            for(int i = 0; i < verts[h].Count; i++) {
                if(verts[h][i].Vertex.Equals(v)) return verts[h][i].Index;
            }
            verts[h].Add(new Key(Count, v));
            Count++;
            return Count - 1;
        }

        public IEnumerator<Key> GetEnumerator() {
            for(int h = 0; h < verts.Length; h++) {
                for(int i = 0; i < verts[h].Count; i++) {
                    yield return verts[h][i];
                }
            }
        }
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() {
            return GetEnumerator();
        }
    }
    #endregion

    public static class ObjParser {
        private const string rgxKeyNum = @"([\d\x2d\.eE]+)";
        private static readonly Regex rgxPos = new Regex(string.Join(@"\s+", @"[Vv]", rgxKeyNum, rgxKeyNum, rgxKeyNum));
        private static readonly Regex rgxUV = new Regex(string.Join(@"\s+", @"[Vv][Tt]", rgxKeyNum, rgxKeyNum));
        private static readonly Regex rgxNorm = new Regex(string.Join(@"\s+", @"[Vv][Nn]", rgxKeyNum, rgxKeyNum, rgxKeyNum));
        private const string rgxKeyFPiece = @"([\d\\/\s]+)";
        private static readonly Regex rgxFTri = new Regex(string.Join(@"\s+", @"[Ff]", rgxKeyFPiece, rgxKeyFPiece, rgxKeyFPiece));
        private static readonly Regex rgxFQuad = new Regex(string.Join(@"\s+", @"[Ff]", rgxKeyFPiece, rgxKeyFPiece, rgxKeyFPiece, rgxKeyFPiece));
        private static readonly Regex rgxFPos = new Regex(@"([\d]+)");
        private static readonly Regex rgxFPosUV = new Regex(@"([\d]+)\s*[\\/]\s*([\d]+)");
        private static readonly Regex rgxFPosNorm = new Regex(@"([\d]+)\s*[\\/]\s*[\\/]\s*([\d]+)");
        private static readonly Regex rgxFFull = new Regex(@"([\d]+)\s*[\\/]\s*([\d]+)\s*[\\/]\s*([\d]+)");

        public static DataFlags Parse(Stream s, out VertexPositionNormalTexture[] verts, out int[] inds, ParsingFlags ps = ParsingFlags.None) {
            // Get Data
            OBJDataDictionary dd = ReadData(s, ps);

            // Get Indices
            VertDict vd;
            DataFlags df = ConvertFaces(ref dd, out vd, out inds);

            // Get Vertices
            verts = new VertexPositionNormalTexture[vd.Count];
            if(df.HasFlag(DataFlags.Normal)) {
                if(df.HasFlag(DataFlags.UV)) {
                    foreach(var vert in vd) {
                        verts[vert.Index].Position = dd.Positions[vert.Vertex.Position];
                        verts[vert.Index].TextureCoordinate = dd.UVs[vert.Vertex.UV];
                        verts[vert.Index].Normal = dd.Normals[vert.Vertex.Normal];
                    }
                }
                else {
                    foreach(var vert in vd) {
                        verts[vert.Index].Position = dd.Positions[vert.Vertex.Position];
                        verts[vert.Index].Normal = dd.Normals[vert.Vertex.Normal];
                    }
                }
            }
            else if(df.HasFlag(DataFlags.UV)) {
                foreach(var vert in vd) {
                    verts[vert.Index].Position = dd.Positions[vert.Vertex.Position];
                    verts[vert.Index].TextureCoordinate = dd.UVs[vert.Vertex.UV];
                }
            }
            else {
                foreach(var vert in vd) {
                    verts[vert.Index].Position = dd.Positions[vert.Vertex.Position];
                }
            }

            return df;
        }
        private static OBJDataDictionary ReadData(Stream s, ParsingFlags ps) {
            // Empty Data Dictionary
            OBJDataDictionary dd = new OBJDataDictionary();
            dd.Positions = new List<Vector3>();
            dd.UVs = new List<Vector2>();
            dd.Normals = new List<Vector3>();
            dd.Faces = new List<OBJFace>();

            // Triangle Arrangement
            int t1 = ps.HasFlag(ParsingFlags.FlipTriangleOrder) ? 3 : 1;
            int t2 = ps.HasFlag(ParsingFlags.FlipTriangleOrder) ? 2 : 2;
            int t3 = ps.HasFlag(ParsingFlags.FlipTriangleOrder) ? 1 : 3;
            int q1 = ps.HasFlag(ParsingFlags.FlipTriangleOrder) ? 4 : 1;
            int q2 = ps.HasFlag(ParsingFlags.FlipTriangleOrder) ? 3 : 3;
            int q3 = ps.HasFlag(ParsingFlags.FlipTriangleOrder) ? 1 : 4;

            StreamReader sr = new StreamReader(new BufferedStream(s));
            while(!sr.EndOfStream) {
                // Read Data By Lines
                string line = sr.ReadLine();
                Match m;
                if((m = rgxPos.Match(line)).Success) {
                    Vector3 pos = Vector3.Zero;
                    pos.X = float.Parse(m.Groups[1].Value);
                    pos.Y = float.Parse(m.Groups[2].Value);
                    pos.Z = float.Parse(m.Groups[3].Value);
                    dd.Positions.Add(pos);
                }
                else if((m = rgxUV.Match(line)).Success) {
                    Vector2 uv = Vector2.Zero;
                    uv.X = float.Parse(m.Groups[1].Value);
                    uv.Y = float.Parse(m.Groups[2].Value);
                    dd.UVs.Add(uv);
                }
                else if((m = rgxNorm.Match(line)).Success) {
                    Vector3 norm = Vector3.Zero;
                    norm.X = float.Parse(m.Groups[1].Value);
                    norm.Y = float.Parse(m.Groups[2].Value);
                    norm.Z = float.Parse(m.Groups[3].Value);
                    dd.Normals.Add(norm);
                }
                else if((m = rgxFTri.Match(line)).Success) {
                    OBJFace f = new OBJFace();
                    f.V1 = m.Groups[t1].Value;
                    f.V2 = m.Groups[t2].Value;
                    f.V3 = m.Groups[t3].Value;
                    dd.Faces.Add(f);
                }
                else if((m = rgxFQuad.Match(line)).Success) {
                    OBJFace fbuf = new OBJFace();
                    fbuf.V1 = m.Groups[t1].Value;
                    fbuf.V2 = m.Groups[t2].Value;
                    fbuf.V3 = m.Groups[t3].Value;
                    dd.Faces.Add(fbuf);
                    fbuf.V1 = m.Groups[q1].Value;
                    fbuf.V2 = m.Groups[q2].Value;
                    fbuf.V3 = m.Groups[q3].Value;
                    dd.Faces.Add(fbuf);
                }
            }

            // Flip V Coordinate
            if(ps.HasFlag(ParsingFlags.FlipTexCoordV)) {
                for(int i = 0; i < dd.UVs.Count; i++) {
                    dd.UVs[i] = new Vector2(dd.UVs[i].X, 1f - dd.UVs[i].Y);
                }
            }
            return dd;
        }
        private static DataFlags ConvertFaces(ref OBJDataDictionary d, out VertDict vd, out int[] inds) {
            vd = new VertDict();
            if(d.Positions.Count < 1 || d.Faces.Count < 1)
                throw new ArgumentException("Positions And Faces Must Be Specified");

            // Get Conversion Function
            Action<OBJFace, OBJVert[]> fc;
            DataFlags f = DataFlags.Position;
            if(d.Normals.Count > 1) {
                if(d.UVs.Count > 1) {
                    fc = CreateVertsFull;
                    f |= DataFlags.UV;
                    f |= DataFlags.Normal;
                }
                else {
                    fc = CreateVertsPosNorm;
                    f |= DataFlags.Normal;
                }
            }
            else if(d.UVs.Count > 1) {
                fc = CreateVertsPosUV;
                f |= DataFlags.UV;
            }
            else {
                fc = CreateVertsPos;
            }
            // Create Indices And
            OBJVert[] verts = new OBJVert[3];
            inds = new int[d.Faces.Count * 3];
            int ii = 0;
            for(int i = 0; i < d.Faces.Count; i++) {
                fc(d.Faces[i], verts);
                inds[ii++] = vd.Get(verts[0]);
                inds[ii++] = vd.Get(verts[1]);
                inds[ii++] = vd.Get(verts[2]);
            }
            return f;
        }
        private static void CreateVertsPos(OBJFace f, OBJVert[] v) {
            Match m;
            m = rgxFPos.Match(f.V1);
            v[0] = new OBJVert(
                int.Parse(m.Groups[1].Value) - 1,
                -1,
                -1
                );
            m = rgxFPos.Match(f.V2);
            v[1] = new OBJVert(
                int.Parse(m.Groups[1].Value) - 1,
                -1,
                -1
                );
            m = rgxFPos.Match(f.V3);
            v[2] = new OBJVert(
                int.Parse(m.Groups[1].Value) - 1,
                -1,
                -1
                );
        }
        private static void CreateVertsPosUV(OBJFace f, OBJVert[] v) {
            Match m;
            m = rgxFPosUV.Match(f.V1);
            v[0] = new OBJVert(
                int.Parse(m.Groups[1].Value) - 1,
                int.Parse(m.Groups[2].Value) - 1,
                -1
                );
            m = rgxFPosUV.Match(f.V2);
            v[1] = new OBJVert(
                int.Parse(m.Groups[1].Value) - 1,
                int.Parse(m.Groups[2].Value) - 1,
                -1
                );
            m = rgxFPosUV.Match(f.V3);
            v[2] = new OBJVert(
                int.Parse(m.Groups[1].Value) - 1,
                int.Parse(m.Groups[2].Value) - 1,
                -1
                );
        }
        private static void CreateVertsPosNorm(OBJFace f, OBJVert[] v) {
            Match m;
            m = rgxFPosNorm.Match(f.V1);
            v[0] = new OBJVert(
                int.Parse(m.Groups[1].Value) - 1,
                -1,
                int.Parse(m.Groups[2].Value) - 1
                );
            m = rgxFPosNorm.Match(f.V2);
            v[1] = new OBJVert(
                int.Parse(m.Groups[1].Value) - 1,
                -1,
                int.Parse(m.Groups[2].Value) - 1
                );
            m = rgxFPosNorm.Match(f.V3);
            v[2] = new OBJVert(
                int.Parse(m.Groups[1].Value) - 1,
                -1,
                int.Parse(m.Groups[2].Value) - 1
                );
        }
        private static void CreateVertsFull(OBJFace f, OBJVert[] v) {
            Match m;
            m = rgxFFull.Match(f.V1);
            v[0] = new OBJVert(
                int.Parse(m.Groups[1].Value) - 1,
                int.Parse(m.Groups[2].Value) - 1,
                int.Parse(m.Groups[3].Value) - 1
                );
            m = rgxFFull.Match(f.V2);
            v[1] = new OBJVert(
                int.Parse(m.Groups[1].Value) - 1,
                int.Parse(m.Groups[2].Value) - 1,
                int.Parse(m.Groups[3].Value) - 1
                );
            m = rgxFFull.Match(f.V3);
            v[2] = new OBJVert(
                int.Parse(m.Groups[1].Value) - 1,
                int.Parse(m.Groups[2].Value) - 1,
                int.Parse(m.Groups[3].Value) - 1
                );
        }

        public static bool TryParse(Stream s, out VertexPositionNormalTexture[] verts, out int[] inds, out DataFlags df, ParsingFlags ps = ParsingFlags.None) {
            try {
                df = Parse(s, out verts, out inds, ps);
            }
            catch(Exception) {
                df = DataFlags.None;
                verts = null;
                inds = null;
                return false;
            }
            return true;
        }
    }
}