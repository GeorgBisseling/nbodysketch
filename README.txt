This is inspired by the fine material about simulating dense stellar
clusters as presented here: 

http://www.artcompsci.org/kali/

As of this writing these integrators are implemented:
Forward Euler
Leapfrog
Runge Kutta 2nd and 4. order
Haruo Yoshida, 6. and 8. order
Multistep (2. order predictor corrector, 4. order, 6. order, 8. order)

Plan is to suggest this project as a tutorial series for interested
programmer-wanna-bees.

Visual Studio Express 2012 and C# were chosen to support a wide
audience without having to introduce everybody to linux.

Some candy like graphical output shall definitely be added.
Currently there is a primitive WPF-Based 2D-Player.

Later on it may be explored if the performance critical parts could
be: parallelized, implemented in C++, OpenCL or CUDA and what not.
