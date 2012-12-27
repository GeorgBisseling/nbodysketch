// This is the main DLL file.

#include "stdafx.h"

#include "NBodyLibCPP.h"

namespace NBodyLibCPP
{
		Vector3^ EulerStateBoost::EulerStateBoost_A_onFirstFromSecond( Vector3^ r1, Vector3^ r2, double m2, double G, double eps2, Vector3^ rdiff_tmp)
        {
            rdiff_tmp->c0 = r2->c0 - r1->c0;
            rdiff_tmp->c1 = r2->c1 - r1->c1;
            rdiff_tmp->c2 = r2->c2 - r1->c2;

			double R2 = rdiff_tmp->c0*rdiff_tmp->c0 + rdiff_tmp->c1*rdiff_tmp->c1 + rdiff_tmp->c2*rdiff_tmp->c2;
            double R = Math::Sqrt(R2);

            double factor = m2 * G / (R * (R2 + eps2));

            rdiff_tmp->c0 *= factor;
            rdiff_tmp->c1 *= factor;
            rdiff_tmp->c2 *= factor;

            return rdiff_tmp;
        }

}
