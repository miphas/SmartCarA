using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WinFormKalmanRadar {
    public class KalmanSingle {

        private double vxk1k1;
        private double rpk1k1;
        private double Q;
        private double R;
        /// <summary>
        /// Filter single point with kalman filter
        /// </summary>
        /// <param name="Q"></param>
        /// <param name="R"></param>
        /// <param name="vxk1k1">initial value of state x</param>
        /// <param name="rpk1k1">initial value of p(can't be 0)</param>
        public const int square = 100;
        public KalmanSingle(double Q = 10, double R = 200, double vxk1k1 = 1700, double rpk1k1 = 1) {
            this.Q = Q;
            this.R = R;
            this.vxk1k1 = vxk1k1;
            this.rpk1k1 = rpk1k1;
        }
        /// <summary>
        /// get estimate point
        /// </summary>
        /// <param name="zk"></param>
        /// <returns></returns>
        public int getDistance(int zk) {
            // Prediction
            double vxkk1 = vxk1k1;
            double rpkk1 = rpk1k1 + Q;
            // Observation
            double y = zk - vxkk1;
            double s = rpkk1 + R;
            // Update
            double k = rpkk1 * (1 / s);
            double vxkk = vxkk1 + k * y;
            double rpkk = (1 - k) * rpkk1;
            // Replace
            vxk1k1 = vxkk;
            rpk1k1 = rpkk;

            return (int)vxkk;
        }
    }
}
