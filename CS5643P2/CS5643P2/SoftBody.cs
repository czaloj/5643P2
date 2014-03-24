using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace CS5643P2 {
    public struct Triangle {
        public int P1, P2, P3;
        public Vector3 Normal;
    }

    public class SoftBody {
        public static SoftBody Parse(Stream s, out VertexPositionNormalTexture[] verts) {
            SoftBody body = null;
            int[] inds;
            DataFlags df;
            if(ObjParser.TryParse(s, out verts, out inds, out df, ParsingFlags.ConversionOpenGL)) {
                body = new SoftBody(inds.Length / 3);
                for(int vi = 0, ti = 0; vi < inds.Length; ) {
                    body.tris[ti].P1 = inds[vi++];
                    body.tris[ti].P2 = inds[vi++];
                    body.tris[ti].P3 = inds[vi++];
                    ti++;
                }
            }
            return body;
        }

        public readonly Triangle[] tris;
        public readonly List<Constraint> restAngleConstraints;

        public SoftBody(int c) {
            tris = new Triangle[c];
            restAngleConstraints = new List<Constraint>();
        }

        public void BuildRestConstraints(Vector3[] pos) {
            restAngleConstraints.Clear();

            // TODO: Create Rest Angle Constraints Here
        }
    }
}