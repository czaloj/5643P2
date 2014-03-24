using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

namespace CS5643P2 {
    public abstract class Constraint {
        // Stiffness Value Between 0-1;
        public float Stiffness {
            get;
            set;
        }

        // Determines Direction Of Displacement Gradient
        public float GradientModifier {
            get;
            private set;
        }

        // Equality Or Inequality Constraint
        public bool DesiresZero {
            get { return GradientModifier < 0; }
            set { GradientModifier = value ? -1 : 1; }
        }

        public abstract void Apply(float dt);
    }

    public class SpringConstraint : Constraint {
        private SoftBody body1, body2;
        private int p1, p2;
        private float restDist;

        // Spring Constant
        private float k;
        public float K {
            get { return k * 2f; }
            set { k = value * 0.5f; }
        }

        public SpringConstraint(SoftBody b1, int _p1, SoftBody b2, int _p2) {
            Stiffness = 0.5f;
            DesiresZero = true;

            // Copy References
            body1 = b1; p1 = _p1;
            body2 = b2; p2 = _p2;

            // Simple Constant
            K = 1;

            // Find Rest Distance Now
            restDist = (body1.positions[p1] - body2.positions[p2]).Length();
        }

        public override void Apply(float dt) {
            // Find Displacement
            Vector3 dir = body1.positions[p1] - body2.positions[p2];
            float d = dir.Length();
            dir /= d;

            // Find Distance From Rest And Energy
            float x = restDist - d;
            float e = k * x * x;

            // Use Spring Constraint Parameters To Modify The Positions
            body1.positions[p1] += dir * (e * Stiffness * GradientModifier) * dt; // TODO: Of Course This Isn't Correct
            body2.positions[p2] -= dir * (e * Stiffness * GradientModifier) * dt;
        }
    }
}