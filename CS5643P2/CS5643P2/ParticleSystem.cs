using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CS5643P2 {
    public class ParticleSystem {
        public void Step(float dt, int iter) {
            float dti = dt / iter;
            while(iter > 0) {
                Physics(dti);
                iter--;
            }
        }
        private void Physics(float dt) {

        }
    }
}