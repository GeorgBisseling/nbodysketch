// #include "stdafx.h"

volatile double shift = 0.0;

 extern double native(double input)
 {
	 return input + shift;
 }
