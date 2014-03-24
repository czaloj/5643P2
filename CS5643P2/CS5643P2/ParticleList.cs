using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

namespace CS5643P2 {
    public struct ParticleList {
        public readonly Vector3[] positions;
        public readonly Vector3[] velocities;

        public ParticleList(int n) {
            positions = new Vector3[n];
            velocities = new Vector3[n];
        }
    }
}
