using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CS5643P2 {
    public class Constraint {
        // The Time That This Constraint Was Generated
        public float Time {
            get;
            set;
        }

        // Stiffness Value Between 0-1;
        public float Stiffness {
            get;
            set;
        }

        // Equality Or Inequality Constraint
        public bool DesiresZero {
            get { return GradientModifier < 0; }
            set { GradientModifier = value ? -1 : 1; }
        }

        // Determines Direction Of Displacement Gradient
        public float GradientModifier {
            get;
            private set;
        }

        public List<int> AffectedParticles;
    }
}