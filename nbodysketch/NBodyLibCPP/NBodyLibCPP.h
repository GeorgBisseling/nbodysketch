// NBodyLibCPP.h

#pragma once

using namespace System;
using namespace VectorLib;

namespace NBodyLibCPP {

	public ref class EulerStateBoost
	{
	public:
		static Vector3^ EulerStateBoost_A_onFirstFromSecond( Vector3^ r1, Vector3^ r2, double m2, double G, double eps2, Vector3^ rdiff_tmp);
	};

}
