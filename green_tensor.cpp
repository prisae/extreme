#include <mkl_service.h>
#include <mkl_vml.h>
#include <mkl.h>

#include <iostream>

#ifdef WINDOWS
#define DllExport __declspec(dllexport) 
#else
#define DllExport 
#endif

using namespace std;


namespace Native
{
	extern "C"
	{
		DllExport void Zaxpy(const long n, MKL_Complex16 alpha, const void *x, void *y, const MKL_INT incy)
		{
			cblas_zaxpy(n, &alpha, x, 1, y, incy);
		}
	}
}