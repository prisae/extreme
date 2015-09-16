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

typedef struct { double re; double im; } complex16;

namespace Native
{
	extern "C"
	{
		DllExport void Zaxpy(const long n, MKL_Complex16 alpha, const void *x, int incX, void *y, int incY)
		{
			cblas_zaxpy(n, &alpha, x, incX, y, incY);
		}

		DllExport void Zcopy(const long n, const void *x, void *y)
		{
			cblas_zcopy(n, x, 1, y, 1);
		}

		DllExport void Zrot(const int n, MKL_Complex16 *x, int incX, MKL_Complex16 *y, int incY, double c, MKL_Complex16 s)
		{
			zrot(&n, x, &incX, y, &incY, &c, &s);
		}

		DllExport void Zrotg(MKL_Complex16 a, MKL_Complex16 b, double* c, MKL_Complex16* s)
		{
			zrotg(&a, &b, c, s);
		}

		DllExport void SimplifiedZtrsv(const int n, const void *a, const MKL_INT lda, void *x)
		{
			cblas_ztrsv(CblasColMajor, CblasUpper, CblasNoTrans, CblasNonUnit, n, a, lda, x, 1);
		}

		DllExport void SimplifiedFgmresZgemv(int n, int jh, void* a, void* input, void* result)
		{
			complex16 alpha = { 1, 0 };
			complex16 beta = { 0, 0 };
			
			cblas_zgemv(CblasColMajor, CblasNoTrans, n, jh, &alpha, a, n, input, 1, &beta, result, 1);
		}
	}
}