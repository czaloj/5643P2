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
        public static SoftBody Parse(Stream s, out VertexPositionNormalTexture[] verts, out int[] inds) {
            SoftBody body = null;
            DataFlags df;
            if(ObjParser.TryParse(s, out verts, out inds, out df, ParsingFlags.ConversionOpenGL)) {
                body = new SoftBody(inds.Length / 3, verts.Length);
                for(int vi = 0, ti = 0; vi < inds.Length; ) {
                    body.tris[ti].P1 = inds[vi++];
                    body.tris[ti].P2 = inds[vi++];
                    body.tris[ti].P3 = inds[vi++];
                    ti++;
                }
                for(int i = 0; i < verts.Length; i++) {
                    body.positions[i] = verts[i].Position;
                }
            }
            return body;
        }


        public readonly Triangle[] tris;
        public readonly Vector3[] positions;

        public readonly List<Constraint> restAngleConstraints;

        public SoftBody(int tc, int vc) {
            tris = new Triangle[tc];
            positions = new Vector3[vc];
            restAngleConstraints = new List<Constraint>();
        }

        public void BuildRestConstraints(Vector3[] pos) {
            restAngleConstraints.Clear();

            // TODO: Create Rest Angle Constraints Here
        }
    }
}